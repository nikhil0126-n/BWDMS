using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using BWDMS.Data;

namespace BWDMS.Handlers
{
    // ================================================================
    // Offline order synchronisation.
    //
    // The salesman captures orders while out of coverage and the browser
    // queues them in IndexedDB. When connectivity returns it POSTs the
    // queue here.
    //
    // Rules this endpoint enforces, all of them server-side:
    //   * the caller must be a logged-in salesman;
    //   * the shop must sit on one of HIS routes;
    //   * the route day must not already be closed;
    //   * the product variant and unit must still be active;
    //   * the price must still be the current one - a changed price is
    //     reported, never silently accepted;
    //   * the vehicle must still carry the stock.
    //
    // Idempotency: the browser generates ClientOrderId and there is a
    // UNIQUE index on it, so posting the same order again returns the
    // original order instead of creating a second one.
    //
    // Nothing is reported as accepted unless it was actually written.
    // ================================================================

    // IRequiresSessionState is mandatory here: without it
    // HttpContext.Session is always null for a handler, and every order
    // would be rejected as "not logged in".
    public class OrderSync : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.Cache.SetCacheability(
                HttpCacheability.NoCache);

            // Without this, IIS swaps our JSON body for its own HTML
            // error page whenever the status code is 4xx/5xx, and the
            // browser cannot tell "rejected" from "server broken".
            context.Response.TrySkipIisCustomErrors = true;

            if (!string.Equals(
                context.Request.HttpMethod,
                "POST",
                StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 405;
                Write(context, Fail("METHOD", "Only POST is accepted."));

                return;
            }

            int salesmanId;

            if (!ReadSession(context, out salesmanId))
            {
                context.Response.StatusCode = 401;
                Write(context, Fail(
                    "NOT_AUTHORIZED",
                    "Log in again before syncing."));

                return;
            }

            Payload payload;

            try
            {
                payload = ReadBody(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 400;
                Write(context, Fail("BAD_REQUEST", ex.Message));

                return;
            }

            if (payload == null ||
                payload.Orders == null ||
                payload.Orders.Count == 0)
            {
                context.Response.StatusCode = 400;
                Write(context, Fail(
                    "BAD_REQUEST",
                    "No orders were submitted."));

                return;
            }

            var results = new List<Result>();

            // One order at a time, each in its own transaction, so a bad
            // order cannot block the rest of the queue.
            foreach (QueuedOrder order in payload.Orders)
            {
                Result result = ProcessOne(salesmanId, order);

                results.Add(result);
            }

            var response = new Dictionary<string, object>
            {
                { "results", results },
                { "serverTime", DateTime.Now.ToString("o") }
            };

            Write(context, response);
        }


        // ============================================================
        // ONE ORDER
        // ============================================================

        private Result ProcessOne(
            int salesmanId,
            QueuedOrder order)
        {
            string clientOrderId = Trim(order.ClientOrderId);

            if (clientOrderId.Length == 0)
            {
                return new Result
                {
                    ClientOrderId = "",
                    Status = "failed",
                    Code = "INVALID",
                    Message = "The order has no client reference."
                };
            }

            SqlConnection con = null;

            try
            {
                con = DatabaseHelper.GetConnection();
                con.Open();

                // ---- already synced? return the original ----
                int existingOrderId;
                string existingNumber;

                if (FindByClientOrderId(
                    con,
                    clientOrderId,
                    out existingOrderId,
                    out existingNumber))
                {
                    return new Result
                    {
                        ClientOrderId = clientOrderId,
                        Status = "duplicate",
                        OrderId = existingOrderId,
                        OrderNumber = existingNumber,
                        Message = "Already received - not created again."
                    };
                }

                using (SqlTransaction tx =
                    con.BeginTransaction())
                {
                    try
                    {
                        Result failure;

                        if (!ValidateOrder(
                            con,
                            tx,
                            salesmanId,
                            order,
                            out failure))
                        {
                            tx.Rollback();

                            return failure;
                        }

                        int dealerId;
                        int shopId;
                        int scheduleId;
                        DateTime orderDate;

                        ResolveOrder(
                            con,
                            tx,
                            salesmanId,
                            order,
                            out dealerId,
                            out shopId,
                            out scheduleId,
                            out orderDate);

                        string orderNumber =
                            NextOrderNumber(con, tx, dealerId);

                        int orderId =
                            InsertOrder(
                                con,
                                tx,
                                dealerId,
                                orderNumber,
                                shopId,
                                scheduleId,
                                salesmanId,
                                orderDate,
                                order,
                                clientOrderId);

                        var applied = new List<LineResult>();

                        foreach (QueuedLine line in order.Lines)
                        {
                            LineResult lr = ApplyLine(
                                con,
                                tx,
                                dealerId,
                                scheduleId,
                                salesmanId,
                                orderDate,
                                orderNumber,
                                line);

                            applied.Add(lr);
                        }

                        tx.Commit();

                        return new Result
                        {
                            ClientOrderId = clientOrderId,
                            Status = "synced",
                            OrderId = orderId,
                            OrderNumber = orderNumber,
                            Lines = applied,
                            Message = "Received."
                        };
                    }
                    catch (SqlException ex)
                    {
                        tx.Rollback();

                        // 2601/2627 = unique violation. Almost always the
                        // ClientOrderId index, i.e. a retry that raced.
                        if (ex.Number == 2601 || ex.Number == 2627)
                        {
                            int id;
                            string number;

                            if (FindByClientOrderId(
                                con,
                                clientOrderId,
                                out id,
                                out number))
                            {
                                return new Result
                                {
                                    ClientOrderId = clientOrderId,
                                    Status = "duplicate",
                                    OrderId = id,
                                    OrderNumber = number,
                                    Message = "Already received."
                                };
                            }
                        }

                        return new Result
                        {
                            ClientOrderId = clientOrderId,
                            Status = "failed",
                            Code = "SERVER_ERROR",
                            Message =
                                "The server could not save this order. " +
                                ex.Message
                        };
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();

                        // A validation failure travels as CODE:detail so
                        // the browser can explain it in plain words.
                        Result mapped = MapFailure(
                            clientOrderId,
                            ex.Message);

                        if (mapped != null)
                        {
                            return mapped;
                        }

                        return new Result
                        {
                            ClientOrderId = clientOrderId,
                            Status = "failed",
                            Code = "SERVER_ERROR",
                            Message = ex.Message
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new Result
                {
                    ClientOrderId = clientOrderId,
                    Status = "failed",
                    Code = "SERVER_ERROR",
                    Message = ex.Message
                };
            }
            finally
            {
                if (con != null)
                {
                    con.Dispose();
                }
            }
        }


        // Turns "CODE:detail" exceptions into a failed Result so the browser can
        // say something useful. Returns null when the message is not one of
        // our codes, which means a genuine server fault.
        private static Result MapFailure(
            string clientOrderId,
            string message)
        {
            string[] codes =
            {
                "NOT_AUTHORIZED",
                "SHOP_NOT_FOUND",
                "SHOP_INACTIVE",
                "ROUTE_CLOSED",
                "NO_VEHICLE",
                "PRODUCT_UNAVAILABLE",
                "PRICE_CHANGED",
                "INSUFFICIENT_STOCK"
            };

            if (string.IsNullOrEmpty(message))
            {
                return null;
            }

            foreach (string code in codes)
            {
                if (message.StartsWith(
                    code,
                    StringComparison.Ordinal))
                {
                    return new Result
                    {
                        ClientOrderId = clientOrderId,
                        Status = "failed",
                        Code = code,
                        Message = message
                    };
                }
            }

            return null;
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        private bool ValidateOrder(
            SqlConnection con,
            SqlTransaction tx,
            int salesmanId,
            QueuedOrder order,
            out Result failure)
        {
            failure = null;

            if (order.Lines == null || order.Lines.Count == 0)
            {
                failure = Failed(order, "INVALID",
                    "The order has no lines.");
                return false;
            }

            int dealerId;
            int shopId;
            int scheduleId;
            DateTime orderDate;

            ResolveOrder(
                con,
                tx,
                salesmanId,
                order,
                out dealerId,
                out shopId,
                out scheduleId,
                out orderDate);

            // ---- shop still there, still active, still on his route ----
            string shopQuery = @"
                SELECT
                    s.ShopId,
                    s.IsActive,
                    s.DealerId,
                    OnHisRoute =
                        CASE
                            WHEN EXISTS
                            (
                                SELECT 1
                                FROM RouteSchedules rs2
                                WHERE rs2.SalesmanId = @SalesmanId
                                  AND rs2.RouteScheduleId = @RouteScheduleId
                                  AND rs2.IsActive = 1
                            )
                            THEN 1
                            ELSE 0
                        END
                FROM Shops s
                WHERE s.ShopId = @ShopId
                  AND s.DealerId = @DealerId";

            using (SqlCommand cmd =
                new SqlCommand(shopQuery, con, tx))
            {
                cmd.Parameters.Add(
                    "@ShopId",
                    SqlDbType.Int).Value = shopId;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = dealerId;

                cmd.Parameters.Add(
                    "@SalesmanId",
                    SqlDbType.Int).Value = salesmanId;

                cmd.Parameters.Add(
                    "@RouteScheduleId",
                    SqlDbType.Int).Value = scheduleId;

                using (SqlDataReader reader =
                    cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        failure = Failed(order, "SHOP_NOT_FOUND",
                            "That shop is not on your account.");
                        return false;
                    }

                    if (!Convert.ToBoolean(reader["IsActive"]))
                    {
                        failure = Failed(order, "SHOP_INACTIVE",
                            "That shop has been deactivated.");
                        return false;
                    }

                    if (!Convert.ToBoolean(reader["OnHisRoute"]))
                    {
                        failure = Failed(order, "NOT_AUTHORIZED",
                            "That route is no longer assigned to you.");
                        return false;
                    }
                }
            }

            // ---- route day closed? ----
            string closedQuery = @"
                SELECT TOP (1) IsClosed
                FROM RouteSchedules
                WHERE RouteScheduleId = @RouteScheduleId
                ORDER BY
                    CASE
                        WHEN StockDate = @StockDate THEN 0
                        ELSE 1
                    END";

            using (SqlCommand cmd =
                new SqlCommand(closedQuery, con, tx))
            {
                cmd.Parameters.Add(
                    "@RouteScheduleId",
                    SqlDbType.Int).Value = scheduleId;

                cmd.Parameters.Add(
                    "@StockDate",
                    SqlDbType.Date).Value = orderDate;

                object value = cmd.ExecuteScalar();

                if (value != null &&
                    value != DBNull.Value &&
                    Convert.ToBoolean(value))
                {
                    failure = Failed(order, "ROUTE_CLOSED",
                        "The dealer has closed this route day.");
                    return false;
                }
            }

            // ---- vehicle ----
            int vehicleId;

            using (SqlCommand cmd =
                new SqlCommand(
                    "SELECT ISNULL(VehicleId, 0) FROM RouteSchedules " +
                    "WHERE RouteScheduleId = @RouteScheduleId",
                    con,
                    tx))
            {
                cmd.Parameters.Add(
                    "@RouteScheduleId",
                    SqlDbType.Int).Value = scheduleId;

                object value = cmd.ExecuteScalar();

                vehicleId = value == null ||
                            value == DBNull.Value
                    ? 0
                    : Convert.ToInt32(value);
            }

            if (vehicleId <= 0)
            {
                failure = Failed(order, "NO_VEHICLE",
                    "That route has no vehicle, so stock cannot be issued.");
                return false;
            }

            return true;
        }


        // ============================================================
        // ONE LINE - variant, price and vehicle stock
        // ============================================================

        private LineResult ApplyLine(
            SqlConnection con,
            SqlTransaction tx,
            int dealerId,
            int scheduleId,
            int salesmanId,
            DateTime orderDate,
            string orderNumber,
            QueuedLine line)
        {
            int variantId = ToInt(line.ProductVariantId);
            int unitId = ToInt(line.UnitId);
            int quantity = ToInt(line.Quantity);

            if (quantity <= 0)
            {
                throw new InvalidOperationException(
                    "Quantity must be greater than zero.");
            }

            // ---- variant + unit still available? ----
            string optionQuery = @"
                SELECT
                    po.PacketsPerUnit,
                    p.ProductName + ' - ' + v.VariantName AS Label,
                    CurrentPrice =
                        ISNULL(pr.ShopSellingPrice, 0)
                FROM ProductUnitOptions po
                INNER JOIN ProductVariants v
                    ON v.ProductVariantId = po.ProductVariantId
                INNER JOIN Products p
                    ON p.ProductId = v.ProductId
                OUTER APPLY
                (
                    SELECT TOP (1) pr.ShopSellingPrice
                    FROM   ProductPrices pr
                    WHERE  pr.ProductOptionId = po.ProductOptionId
                      AND  pr.EffectiveFrom <= @PriceDate
                    ORDER BY pr.EffectiveFrom DESC
                ) pr
                WHERE po.ProductVariantId = @ProductVariantId
                  AND po.UnitId = @UnitId
                  AND po.IsActive = 1
                  AND v.IsActive = 1
                  AND p.IsActive = 1";

            int packetsPerUnit = 0;
            string label = "";
            decimal currentPrice = 0;

            using (SqlCommand cmd =
                new SqlCommand(optionQuery, con, tx))
            {
                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                cmd.Parameters.Add(
                    "@UnitId",
                    SqlDbType.Int).Value = unitId;

                cmd.Parameters.Add(
                    "@PriceDate",
                    SqlDbType.Date).Value = orderDate;

                using (SqlDataReader reader =
                    cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new InvalidOperationException(
                            "PRODUCT_UNAVAILABLE");
                    }

                    packetsPerUnit = Convert.ToInt32(
                        reader["PacketsPerUnit"]);

                    label = Convert.ToString(reader["Label"]);

                    currentPrice = Convert.ToDecimal(
                        reader["CurrentPrice"]);
                }
            }

            // ---- price changed while the salesman was offline? ----
            decimal claimed = Convert.ToDecimal(line.UnitPrice);

            if (currentPrice > 0 &&
                Math.Abs(claimed - currentPrice) > 0.001M)
            {
                throw new InvalidOperationException(
                    "PRICE_CHANGED:" + label + ":" +
                    currentPrice.ToString("0.####"));
            }

            int packets = packetsPerUnit <= 0
                ? quantity
                : quantity * packetsPerUnit;

            // ---- stock on the vehicle ----
            string stockQuery = @"
                SELECT
                    ISNULL(LoadedPackets, 0)
                    - ISNULL(SoldPackets, 0)
                    - ISNULL(DamagedPackets, 0)
                    + ISNULL(ReturnedPackets, 0)
                FROM VehicleStock WITH (UPDLOCK, ROWLOCK)
                WHERE DealerId = @DealerId
                  AND RouteScheduleId = @RouteScheduleId
                  AND StockDate = @StockDate
                  AND ProductVariantId = @ProductVariantId";

            string vehicleQuery = @"
                SELECT ISNULL(VehicleId, 0)
                FROM RouteSchedules
                WHERE RouteScheduleId = @RouteScheduleId";

            int vehicleId;

            using (SqlCommand cmd =
                new SqlCommand(vehicleQuery, con, tx))
            {
                cmd.Parameters.Add(
                    "@RouteScheduleId",
                    SqlDbType.Int).Value = scheduleId;

                object value = cmd.ExecuteScalar();

                vehicleId = value == null ||
                            value == DBNull.Value
                    ? 0
                    : Convert.ToInt32(value);
            }

            int available = 0;

            using (SqlCommand cmd =
                new SqlCommand(stockQuery, con, tx))
            {
                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = dealerId;

                cmd.Parameters.Add(
                    "@VehicleId",
                    SqlDbType.Int).Value = vehicleId;

                cmd.Parameters.Add(
                    "@RouteScheduleId",
                    SqlDbType.Int).Value = scheduleId;

                cmd.Parameters.Add(
                    "@StockDate",
                    SqlDbType.Date).Value = orderDate;

                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                object value = cmd.ExecuteScalar();

                if (value != null && value != DBNull.Value)
                {
                    available = Convert.ToInt32(value);
                }
            }

            if (available < packets)
            {
                throw new InvalidOperationException(
                    "INSUFFICIENT_STOCK:" + label + ":" +
                    available + ":" + packets);
            }

            // ---- write the order line ----
            string detail = @"
                INSERT INTO OrderDetails
                (
                    OrderId,
                    ProductVariantId,
                    UnitId,
                    Quantity,
                    QuantityPackets,
                    UnitPrice,
                    LineTotal
                )
                VALUES
                (
                    @OrderId,
                    @ProductVariantId,
                    @UnitId,
                    @Quantity,
                    @QuantityPackets,
                    @UnitPrice,
                    @LineTotal
                )";

            using (SqlCommand cmd =
                new SqlCommand(detail, con, tx))
            {
                cmd.Parameters.Add(
                    "@OrderId",
                    SqlDbType.Int).Value = OrderIdFor(
                        con,
                        tx,
                        orderNumber);

                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                cmd.Parameters.Add(
                    "@UnitId",
                    SqlDbType.Int).Value = unitId;

                cmd.Parameters.Add(
                    "@Quantity",
                    SqlDbType.Int).Value = quantity;

                cmd.Parameters.Add(
                    "@QuantityPackets",
                    SqlDbType.Int).Value = packets;

                cmd.Parameters.Add(
                    "@UnitPrice",
                    SqlDbType.Decimal).Value =
                        claimed > 0 ? claimed : currentPrice;

                cmd.Parameters.Add(
                    "@LineTotal",
                    SqlDbType.Decimal).Value =
                        (claimed > 0 ? claimed : currentPrice) *
                        quantity;

                cmd.ExecuteNonQuery();
            }

            // ---- take it off the vehicle ----
            string issue = @"
                UPDATE VehicleStock
                SET SoldPackets = SoldPackets + @SoldPackets,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE DealerId = @DealerId
                  AND VehicleId = @VehicleId
                  AND RouteScheduleId = @RouteScheduleId
                  AND StockDate = @StockDate
                  AND ProductVariantId = @ProductVariantId";

            using (SqlCommand cmd =
                new SqlCommand(issue, con, tx))
            {
                cmd.Parameters.Add(
                    "@SoldPackets",
                    SqlDbType.Int).Value = packets;

                cmd.Parameters.Add(
                    "@UpdatedBy",
                    SqlDbType.Int).Value = salesmanId;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = dealerId;

                cmd.Parameters.Add(
                    "@VehicleId",
                    SqlDbType.Int).Value = vehicleId;

                cmd.Parameters.Add(
                    "@RouteScheduleId",
                    SqlDbType.Int).Value = scheduleId;

                cmd.Parameters.Add(
                    "@StockDate",
                    SqlDbType.Date).Value = orderDate;

                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                cmd.ExecuteNonQuery();
            }

            return new LineResult
            {
                ProductVariantId = variantId,
                Label = label,
                QuantityPackets = packets,
                Status = "issued"
            };
        }


        private static int OrderIdFor(
            SqlConnection con,
            SqlTransaction tx,
            string orderNumber)
        {
            using (SqlCommand cmd =
                new SqlCommand(
                    "SELECT OrderId FROM Orders WHERE OrderNumber = @n",
                    con,
                    tx))
            {
                cmd.Parameters.Add(
                    "@n",
                    SqlDbType.NVarChar,
                    60).Value = orderNumber;

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }


        // ============================================================
        // HELPERS
        // ============================================================

        private void ResolveOrder(
            SqlConnection con,
            SqlTransaction tx,
            int salesmanId,
            QueuedOrder order,
            out int dealerId,
            out int shopId,
            out int scheduleId,
            out DateTime orderDate)
        {
            shopId = ToInt(order.ShopId);
            scheduleId = ToInt(order.RouteScheduleId);
            orderDate = ParseDate(order.OrderDate);

            string query = @"
                SELECT TOP (1) r.DealerId
                FROM RouteSchedules rs
                INNER JOIN Routes r
                    ON r.RouteId = rs.RouteId
                WHERE rs.RouteScheduleId = @RouteScheduleId
                  AND rs.SalesmanId = @SalesmanId
                  AND rs.IsActive = 1";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@RouteScheduleId",
                    SqlDbType.Int).Value = scheduleId;

                cmd.Parameters.Add(
                    "@SalesmanId",
                    SqlDbType.Int).Value = salesmanId;

                object value = cmd.ExecuteScalar();

                if (value == null || value == DBNull.Value)
                {
                    throw new InvalidOperationException(
                        "NOT_AUTHORIZED");
                }

                dealerId = Convert.ToInt32(value);
            }
        }


        private static int InsertOrder(
            SqlConnection con,
            SqlTransaction tx,
            int dealerId,
            string orderNumber,
            int shopId,
            int scheduleId,
            int salesmanId,
            DateTime orderDate,
            QueuedOrder order,
            string clientOrderId)
        {
            decimal subTotal = 0;

            foreach (QueuedLine line in order.Lines)
            {
                subTotal += Convert.ToDecimal(line.UnitPrice) *
                            ToInt(line.Quantity);
            }

            string remarks = Trim(order.Remarks);

            string query = @"
                INSERT INTO Orders
                (
                    DealerId,
                    OrderNumber,
                    ShopId,
                    OrderType,
                    OrderDate,
                    DeliveryDate,
                    RouteScheduleId,
                    SalesmanId,
                    Status,
                    OrderSource,
                    SubTotal,
                    Discount,
                    GrandTotal,
                    Remarks,
                    ClientOrderId,
                    IsSynced,
                    CreatedBy,
                    CreatedAt
                )
                VALUES
                (
                    @DealerId,
                    @OrderNumber,
                    @ShopId,
                    'Order Taking',
                    @OrderDate,
                    @DeliveryDate,
                    @RouteScheduleId,
                    @SalesmanId,
                    'Pending',
                    'Beat',
                    @SubTotal,
                    0,
                    @SubTotal,
                    @Remarks,
                    @ClientOrderId,
                    1,
                    @CreatedBy,
                    GETDATE()
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = dealerId;

                cmd.Parameters.Add(
                    "@OrderNumber",
                    SqlDbType.NVarChar,
                    60).Value = orderNumber;

                cmd.Parameters.Add(
                    "@ShopId",
                    SqlDbType.Int).Value = shopId;

                cmd.Parameters.Add(
                    "@OrderDate",
                    SqlDbType.Date).Value = orderDate;

                cmd.Parameters.Add(
                    "@DeliveryDate",
                    SqlDbType.Date).Value =
                        ParseDate(order.DeliveryDate);

                cmd.Parameters.Add(
                    "@RouteScheduleId",
                    SqlDbType.Int).Value = scheduleId;

                cmd.Parameters.Add(
                    "@SalesmanId",
                    SqlDbType.Int).Value = salesmanId;

                cmd.Parameters.Add(
                    "@SubTotal",
                    SqlDbType.Decimal).Value = subTotal;

                cmd.Parameters.Add(
                    "@Remarks",
                    SqlDbType.NVarChar,
                    1000).Value =
                        remarks.Length == 0
                            ? (object)DBNull.Value
                            : remarks;

                cmd.Parameters.Add(
                    "@ClientOrderId",
                    SqlDbType.NVarChar,
                    80).Value = clientOrderId;

                cmd.Parameters.Add(
                    "@CreatedBy",
                    SqlDbType.Int).Value = salesmanId;

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }


        private static string NextOrderNumber(
            SqlConnection con,
            SqlTransaction tx,
            int dealerId)
        {
            string prefix = "ORD-" +
                DateTime.Now.ToString("yyyyMMdd") + "-";

            string query = @"
                SELECT TOP (1) OrderNumber
                FROM Orders
                WHERE DealerId = @DealerId
                  AND OrderNumber LIKE @Prefix
                ORDER BY OrderNumber DESC";

            long next = 1;

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = dealerId;

                cmd.Parameters.Add(
                    "@Prefix",
                    SqlDbType.NVarChar,
                    80).Value = prefix + "%";

                object value = cmd.ExecuteScalar();

                if (value != null && value != DBNull.Value)
                {
                    string tail = value.ToString().Substring(
                        prefix.Length);

                    long parsed;

                    if (long.TryParse(tail, out parsed))
                    {
                        next = parsed + 1;
                    }
                }
            }

            return prefix + next.ToString("000000");
        }


        private static bool FindByClientOrderId(
            SqlConnection con,
            string clientOrderId,
            out int orderId,
            out string orderNumber)
        {
            string query = @"
                SELECT TOP (1) OrderId, OrderNumber
                FROM Orders
                WHERE ClientOrderId = @ClientOrderId";

            using (SqlCommand cmd =
                new SqlCommand(query, con))
            {
                cmd.Parameters.Add(
                    "@ClientOrderId",
                    SqlDbType.NVarChar,
                    80).Value = clientOrderId;

                using (SqlDataReader reader =
                    cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        orderId = Convert.ToInt32(reader["OrderId"]);
                        orderNumber =
                            Convert.ToString(reader["OrderNumber"]);

                        return true;
                    }
                }
            }

            orderId = 0;
            orderNumber = null;

            return false;
        }


        private static bool ReadSession(
            HttpContext context,
            out int salesmanId)
        {
            salesmanId = 0;

            if (context.Session == null ||
                context.Session["UserId"] == null)
            {
                return false;
            }

            string role = Convert.ToString(
                context.Session["UserRole"]);

            if (!string.Equals(
                role,
                "Salesman",
                StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            salesmanId = Convert.ToInt32(
                context.Session["UserId"]);

            return true;
        }


        private static Payload ReadBody(HttpContext context)
        {
            string body;

            using (StreamReader reader =
                new StreamReader(
                    context.Request.InputStream,
                    Encoding.UTF8))
            {
                body = reader.ReadToEnd();
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            var serializer = new JavaScriptSerializer();

            serializer.MaxJsonLength = 4 * 1024 * 1024;

            return serializer.Deserialize<Payload>(body);
        }


        private static void Write(
            HttpContext context,
            object value)
        {
            var serializer = new JavaScriptSerializer();

            context.Response.Write(
                serializer.Serialize(value));
        }


        private static Dictionary<string, object> Fail(
            string code,
            string message)
        {
            var results = new List<Result>
            {
                new Result
                {
                    Status = "failed",
                    Code = code,
                    Message = message
                }
            };

            return new Dictionary<string, object>
            {
                { "results", results }
            };
        }


        private static Result Failed(
            QueuedOrder order,
            string code,
            string message)
        {
            return new Result
            {
                ClientOrderId =
                    order == null ? "" : Trim(order.ClientOrderId),
                Status = "failed",
                Code = code,
                Message = message
            };
        }


        private static string Trim(string value)
        {
            return value == null ? "" : value.Trim();
        }


        private static int ToInt(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return 0;
            }

            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return 0;
            }
        }


        private static DateTime ParseDate(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return DateTime.Today;
            }

            DateTime parsed;

            if (value is DateTime)
            {
                return ((DateTime)value).Date;
            }

            if (DateTime.TryParse(
                Convert.ToString(value),
                out parsed))
            {
                return parsed.Date;
            }

            return DateTime.Today;
        }


        // ============================================================
        // CONTRACT
        // ============================================================

        public class Payload
        {
            public List<QueuedOrder> Orders { get; set; }
        }

        public class QueuedOrder
        {
            public string ClientOrderId { get; set; }
            public object ShopId { get; set; }
            public object RouteScheduleId { get; set; }
            public string OrderDate { get; set; }
            public string DeliveryDate { get; set; }
            public string Remarks { get; set; }
            public List<QueuedLine> Lines { get; set; }
        }

        public class QueuedLine
        {
            public object ProductVariantId { get; set; }
            public object UnitId { get; set; }
            public object Quantity { get; set; }
            public object UnitPrice { get; set; }
        }

        public class Result
        {
            public string ClientOrderId { get; set; }
            public string Status { get; set; }
            public string Code { get; set; }
            public string Message { get; set; }
            public int OrderId { get; set; }
            public string OrderNumber { get; set; }
            public List<LineResult> Lines { get; set; }
        }

        public class LineResult
        {
            public int ProductVariantId { get; set; }
            public string Label { get; set; }
            public int QuantityPackets { get; set; }
            public string Status { get; set; }
        }
    }
}
