using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class AddDispatch : System.Web.UI.Page
    {
        private int DealerId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            Response.Cache.SetExpires(
                DateTime.UtcNow.AddYears(-1));

            Response.Cache.SetRevalidation(
                System.Web.HttpCacheRevalidation.AllCaches);


            if (Session["UserId"] == null)
            {
                Response.Redirect(
                    "~/Account/Login.aspx",
                    false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }

            if (Session["UserRole"] == null ||
                !Session["UserRole"].ToString().Equals(
                    "Dealer",
                    StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect(
                    BWDMS.Data.AppAuth.HomeUrl(Session["UserRole"]),
                    false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }


            if (!IsPostBack)
            {
                LoadVehicles();
                LoadRouteSchedules();
                LoadSalesmen();

                txtDispatchDate.Text =
                    DateTime.Today.ToString("yyyy-MM-dd");

                LoadOrders();

                // ?order=N comes from an order screen - the tick is
                // only applied when the order really is in this
                // dealer's dispatchable list.
                PreselectOrderFromQuery();
            }
        }


        // ============================================================
        // DROPDOWNS + ORDER PICKER
        // ============================================================

        private void LoadVehicles()
        {
            string query = @"
                SELECT
                    VehicleId,
                    VehicleNumber
                FROM Vehicles
                WHERE IsActive = 1
                  AND (DealerId = @DealerId
                       OR DealerId IS NULL)
                ORDER BY VehicleNumber";

            LoadDropDown(
                ddlVehicle,
                query,
                "VehicleNumber",
                "VehicleId",
                "Select Vehicle");
        }


        private void LoadRouteSchedules()
        {
            string query = @"
                SELECT
                    rs.RouteScheduleId,
                    Label = r.RouteName
                            + ' - '
                            + ISNULL(rs.DayOfWeek, '')
                            + CASE
                                  WHEN rs.VehicleId IS NULL
                                      THEN ''
                                      ELSE ' (' + v.VehicleNumber + ')'
                              END
                FROM RouteSchedules rs
                INNER JOIN Routes r
                    ON r.RouteId = rs.RouteId
                       AND r.DealerId = @DealerId
                LEFT JOIN Vehicles v
                    ON v.VehicleId = rs.VehicleId
                WHERE rs.IsActive = 1
                ORDER BY r.RouteName";

            LoadDropDown(
                ddlRouteSchedule,
                query,
                "Label",
                "RouteScheduleId",
                "None");
        }


        private void LoadSalesmen()
        {
            string query = @"
                SELECT
                    UserId,
                    FullName
                FROM Users
                WHERE Role = 'Salesman'
                  AND IsActive = 1
                  AND (DealerId = @DealerId
                       OR DealerId IS NULL)
                ORDER BY FullName";

            LoadDropDown(
                ddlSalesman,
                query,
                "FullName",
                "UserId",
                "None");
        }


        private void LoadDropDown(
            DropDownList dropdown,
            string query,
            string textField,
            string valueField,
            string firstItemText)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            dropdown.DataSource = dt;
            dropdown.DataTextField = textField;
            dropdown.DataValueField = valueField;
            dropdown.DataBind();

            dropdown.Items.Insert(
                0,
                new ListItem(firstItemText, ""));
        }


        private void LoadOrders()
        {
            string query = @"
                SELECT
                    o.OrderId,
                    o.OrderNumber,
                    s.ShopName,
                    o.GrandTotal,
                    LineCount =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM OrderDetails d
                             WHERE d.OrderId = o.OrderId),
                            0)
                FROM Orders o
                INNER JOIN Shops s
                    ON s.ShopId = o.ShopId
                WHERE o.DealerId = @DealerId
                  AND o.Status IN ('Pending', 'Confirmed')
                ORDER BY o.OrderDate DESC, o.OrderId DESC";

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            if (!dt.Columns.Contains("GrandTotalText"))
            {
                dt.Columns.Add(
                    "GrandTotalText",
                    typeof(string));
            }

            foreach (DataRow row in dt.Rows)
            {
                row["GrandTotalText"] =
                    Convert.ToDecimal(row["GrandTotal"])
                        .ToString("N2");
            }

            gvOrders.DataSource = dt;
            gvOrders.DataBind();
        }


        private void PreselectOrderFromQuery()
        {
            int orderId;

            if (!int.TryParse(
                    Request.QueryString["order"],
                    out orderId) ||
                orderId <= 0)
            {
                return;
            }

            foreach (GridViewRow row in gvOrders.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow)
                {
                    continue;
                }

                CheckBox chk =
                    row.FindControl("chkSelect") as CheckBox;

                if (chk == null)
                {
                    continue;
                }

                if (Convert.ToInt32(
                        gvOrders.DataKeys[row.RowIndex].Value)
                    == orderId)
                {
                    chk.Checked = true;
                    return;
                }
            }

            // Not in this dealer's list: say nothing when the order
            // belongs to someone else, only when it is simply not
            // dispatchable any more.
            if (OrderBelongsToDealer(orderId))
            {
                ShowMessage(
                    "That order is not available for dispatch.",
                    false);
            }
        }


        private bool OrderBelongsToDealer(int orderId)
        {
            string query = @"
                SELECT COUNT(*)
                FROM Orders
                WHERE OrderId = @OrderId
                  AND DealerId = @DealerId";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@OrderId",
                        SqlDbType.Int).Value = orderId;

                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    return Convert.ToInt32(
                        cmd.ExecuteScalar()) > 0;
                }
            }
        }


        protected void cvOrders_ServerValidate(
            object source,
            ServerValidateEventArgs args)
        {
            args.IsValid = SelectedOrderIds().Count > 0;
        }


        private List<int> SelectedOrderIds()
        {
            List<int> orderIds = new List<int>();

            foreach (GridViewRow row in gvOrders.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow)
                {
                    continue;
                }

                CheckBox chk =
                    row.FindControl("chkSelect") as CheckBox;

                if (chk == null || !chk.Checked)
                {
                    continue;
                }

                int orderId =
                    Convert.ToInt32(
                        gvOrders.DataKeys[row.RowIndex].Value);

                if (!orderIds.Contains(orderId))
                {
                    orderIds.Add(orderId);
                }
            }

            return orderIds;
        }


        // ============================================================
        // SAVE
        // ============================================================

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                ShowMessage(
                    "Please correct the highlighted fields.",
                    false);

                return;
            }

            List<int> orderIds = SelectedOrderIds();

            if (orderIds.Count == 0)
            {
                ShowMessage(
                    "Select at least one order to dispatch.",
                    false);

                return;
            }

            DateTime dispatchDate;

            if (!TryDate(
                    txtDispatchDate.Text.Trim(),
                    out dispatchDate))
            {
                ShowMessage(
                    "Enter a valid dispatch date (YYYY-MM-DD).",
                    false);

                return;
            }

            int vehicleId = SelectedId(ddlVehicle);
            int scheduleId = SelectedId(ddlRouteSchedule);
            int salesmanId = SelectedId(ddlSalesman);

            string remarks =
                string.IsNullOrWhiteSpace(txtRemarks.Text.Trim())
                    ? null
                    : txtRemarks.Text.Trim();

            try
            {
                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    con.Open();

                    using (SqlTransaction tx =
                        con.BeginTransaction())
                    {
                        try
                        {
                            // A forged id from another dealership
                            // never reaches the write.
                            ValidateOwnership(
                                con,
                                tx,
                                vehicleId,
                                scheduleId,
                                salesmanId);

                            string dispatchNumber =
                                NextDispatchNumber(con, tx);

                            int dispatchId = InsertDispatch(
                                con,
                                tx,
                                dispatchNumber,
                                dispatchDate,
                                vehicleId,
                                scheduleId,
                                salesmanId,
                                remarks);

                            // Packets needed, summed across every
                            // order that really goes out.
                            Dictionary<int, int> packets =
                                new Dictionary<int, int>();

                            int dispatchedCount = 0;
                            int skippedCount = 0;

                            foreach (int orderId in orderIds)
                            {
                                string oldStatus =
                                    ReadOrderStatus(
                                        con,
                                        tx,
                                        orderId);

                                if (oldStatus == "Dispatched" ||
                                    oldStatus == "Cancelled")
                                {
                                    // Already out on the road (or
                                    // cancelled) - never deduct twice.
                                    skippedCount++;
                                    continue;
                                }

                                CopyOrderDetails(
                                    con,
                                    tx,
                                    dispatchId,
                                    orderId,
                                    packets);

                                MarkOrderDispatched(
                                    con,
                                    tx,
                                    orderId);

                                dispatchedCount++;
                            }

                            if (dispatchedCount == 0)
                            {
                                throw new InvalidOperationException(
                                    "No order was dispatched - " +
                                    skippedCount +
                                    " selected order(s) are already dispatched or cancelled.");
                            }

                            DeductStock(con, tx, packets);

                            WriteLedger(
                                con,
                                tx,
                                packets,
                                dispatchNumber,
                                remarks);

                            tx.Commit();
                        }
                        catch
                        {
                            tx.Rollback();
                            throw;
                        }
                    }
                }

                Response.Redirect(
                    "~/Dealer/Dispatches.aspx?saved=1");
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error saving dispatch: " + ex.Message,
                    false);
            }
        }


        // ============================================================
        // HEADER
        // ============================================================

        private void ValidateOwnership(
            SqlConnection con,
            SqlTransaction tx,
            int vehicleId,
            int scheduleId,
            int salesmanId)
        {
            if (vehicleId > 0 &&
                !RowExists(
                    con,
                    tx,
                    @"SELECT COUNT(*)
                      FROM Vehicles
                      WHERE VehicleId = @Id
                        AND IsActive = 1
                        AND (DealerId = @DealerId
                             OR DealerId IS NULL)",
                    vehicleId))
            {
                throw new InvalidOperationException(
                    "The selected vehicle does not belong to this dealership.");
            }

            if (scheduleId > 0 &&
                !RowExists(
                    con,
                    tx,
                    @"SELECT COUNT(*)
                      FROM RouteSchedules rs
                      INNER JOIN Routes r
                          ON r.RouteId = rs.RouteId
                             AND r.DealerId = @DealerId
                      WHERE rs.RouteScheduleId = @Id
                        AND rs.IsActive = 1",
                    scheduleId))
            {
                throw new InvalidOperationException(
                    "The selected route schedule does not belong to this dealership.");
            }

            if (salesmanId > 0 &&
                !RowExists(
                    con,
                    tx,
                    @"SELECT COUNT(*)
                      FROM Users
                      WHERE UserId = @Id
                        AND Role = 'Salesman'
                        AND IsActive = 1
                        AND (DealerId = @DealerId
                             OR DealerId IS NULL)",
                    salesmanId))
            {
                throw new InvalidOperationException(
                    "The selected salesman does not belong to this dealership.");
            }
        }


        private bool RowExists(
            SqlConnection con,
            SqlTransaction tx,
            string query,
            int id)
        {
            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@Id",
                    SqlDbType.Int).Value = id;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                return Convert.ToInt32(
                    cmd.ExecuteScalar()) > 0;
            }
        }


        private string NextDispatchNumber(
            SqlConnection con,
            SqlTransaction tx)
        {
            string prefix =
                "DSP-" +
                DateTime.Now.ToString("yyyyMMdd-HHmmss");

            string candidate = prefix;
            int suffix = 2;

            while (DispatchNumberExists(con, tx, candidate))
            {
                candidate = prefix + "-" + suffix++;
            }

            return candidate;
        }


        private bool DispatchNumberExists(
            SqlConnection con,
            SqlTransaction tx,
            string number)
        {
            string query = @"
                SELECT COUNT(*)
                FROM Dispatches
                WHERE DealerId = @DealerId
                  AND DispatchNumber = @DispatchNumber";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@DispatchNumber",
                    SqlDbType.NVarChar,
                    50).Value = number;

                return Convert.ToInt32(
                    cmd.ExecuteScalar()) > 0;
            }
        }


        private int InsertDispatch(
            SqlConnection con,
            SqlTransaction tx,
            string dispatchNumber,
            DateTime dispatchDate,
            int vehicleId,
            int scheduleId,
            int salesmanId,
            string remarks)
        {
            string query = @"
                INSERT INTO Dispatches
                (
                    DealerId,
                    DispatchNumber,
                    DispatchDate,
                    VehicleId,
                    SalesmanId,
                    RouteScheduleId,
                    Status,
                    Remarks,
                    CreatedBy,
                    CreatedAt
                )
                VALUES
                (
                    @DealerId,
                    @DispatchNumber,
                    @DispatchDate,
                    @VehicleId,
                    @SalesmanId,
                    @RouteScheduleId,
                    @Status,
                    @Remarks,
                    @CreatedBy,
                    GETDATE()
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@DispatchNumber",
                    SqlDbType.NVarChar,
                    50).Value = dispatchNumber;

                cmd.Parameters.Add(
                    "@DispatchDate",
                    SqlDbType.Date).Value = dispatchDate;

                cmd.Parameters.Add(
                    "@VehicleId",
                    SqlDbType.Int).Value =
                    vehicleId > 0
                        ? (object)vehicleId
                        : DBNull.Value;

                cmd.Parameters.Add(
                    "@SalesmanId",
                    SqlDbType.Int).Value =
                    salesmanId > 0
                        ? (object)salesmanId
                        : DBNull.Value;

                cmd.Parameters.Add(
                    "@RouteScheduleId",
                    SqlDbType.Int).Value =
                    scheduleId > 0
                        ? (object)scheduleId
                        : DBNull.Value;

                cmd.Parameters.Add(
                    "@Status",
                    SqlDbType.NVarChar,
                    30).Value = "Dispatched";

                cmd.Parameters.Add(
                    "@Remarks",
                    SqlDbType.NVarChar,
                    500).Value =
                    remarks == null
                        ? (object)DBNull.Value
                        : remarks;

                cmd.Parameters.Add(
                    "@CreatedBy",
                    SqlDbType.Int).Value = DealerId;

                return Convert.ToInt32(
                    cmd.ExecuteScalar());
            }
        }


        // ============================================================
        // ORDERS + LINES
        // ============================================================

        // Read inside the transaction with an update lock so a
        // double submit cannot dispatch (and therefore deduct
        // stock for) the same order twice.
        private string ReadOrderStatus(
            SqlConnection con,
            SqlTransaction tx,
            int orderId)
        {
            string query = @"
                SELECT Status
                FROM Orders WITH (UPDLOCK, ROWLOCK)
                WHERE OrderId = @OrderId
                  AND DealerId = @DealerId";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@OrderId",
                    SqlDbType.Int).Value = orderId;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                object result = cmd.ExecuteScalar();

                if (result == null ||
                    result == DBNull.Value)
                {
                    throw new InvalidOperationException(
                        "Order #" + orderId +
                        " was not found for this dealership.");
                }

                return result.ToString();
            }
        }


        private void CopyOrderDetails(
            SqlConnection con,
            SqlTransaction tx,
            int dispatchId,
            int orderId,
            Dictionary<int, int> packets)
        {
            List<int[]> lines = new List<int[]>();

            string read = @"
                SELECT
                    ProductVariantId,
                    UnitId,
                    Quantity,
                    QuantityPackets
                FROM OrderDetails
                WHERE OrderId = @OrderId
                ORDER BY OrderDetailId";

            using (SqlCommand cmd =
                new SqlCommand(read, con, tx))
            {
                cmd.Parameters.Add(
                    "@OrderId",
                    SqlDbType.Int).Value = orderId;

                using (SqlDataReader reader =
                    cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lines.Add(new int[]
                        {
                            Convert.ToInt32(reader[0]),
                            Convert.ToInt32(reader[1]),
                            Convert.ToInt32(reader[2]),
                            Convert.ToInt32(reader[3])
                        });
                    }
                }
            }

            string insert = @"
                INSERT INTO DispatchDetails
                (
                    DispatchId,
                    OrderId,
                    ProductVariantId,
                    UnitId,
                    Quantity,
                    QuantityPackets
                )
                VALUES
                (
                    @DispatchId,
                    @OrderId,
                    @ProductVariantId,
                    @UnitId,
                    @Quantity,
                    @QuantityPackets
                )";

            foreach (int[] line in lines)
            {
                using (SqlCommand cmd =
                    new SqlCommand(insert, con, tx))
                {
                    cmd.Parameters.Add(
                        "@DispatchId",
                        SqlDbType.Int).Value = dispatchId;

                    cmd.Parameters.Add(
                        "@OrderId",
                        SqlDbType.Int).Value = orderId;

                    cmd.Parameters.Add(
                        "@ProductVariantId",
                        SqlDbType.Int).Value = line[0];

                    cmd.Parameters.Add(
                        "@UnitId",
                        SqlDbType.Int).Value = line[1];

                    cmd.Parameters.Add(
                        "@Quantity",
                        SqlDbType.Int).Value = line[2];

                    cmd.Parameters.Add(
                        "@QuantityPackets",
                        SqlDbType.Int).Value = line[3];

                    cmd.ExecuteNonQuery();
                }

                int variantId = line[0];
                int needed = line[3];

                packets[variantId] =
                    packets.ContainsKey(variantId)
                        ? packets[variantId] + needed
                        : needed;
            }
        }


        private void MarkOrderDispatched(
            SqlConnection con,
            SqlTransaction tx,
            int orderId)
        {
            string query = @"
                UPDATE Orders
                SET Status = 'Dispatched',
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE OrderId = @OrderId
                  AND DealerId = @DealerId";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@UpdatedBy",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@OrderId",
                    SqlDbType.Int).Value = orderId;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                cmd.ExecuteNonQuery();
            }
        }


        // ============================================================
        // STOCK IMPACT
        // ============================================================

        // One deduction per variant for the whole dispatch - the
        // godown is never touched once per order.
        private void DeductStock(
            SqlConnection con,
            SqlTransaction tx,
            Dictionary<int, int> packets)
        {
            foreach (int variantId in SortedVariants(packets))
            {
                int needed = packets[variantId];

                if (needed <= 0)
                {
                    continue;
                }

                string label = VariantLabel(con, tx, variantId);

                int current = 0;

                string read = @"
                    SELECT ISNULL(QuantityPackets, 0)
                    FROM Inventory WITH (UPDLOCK, ROWLOCK)
                    WHERE DealerId = @DealerId
                      AND ProductVariantId = @ProductVariantId";

                using (SqlCommand cmd =
                    new SqlCommand(read, con, tx))
                {
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    cmd.Parameters.Add(
                        "@ProductVariantId",
                        SqlDbType.Int).Value = variantId;

                    object result = cmd.ExecuteScalar();

                    if (result != null &&
                        result != DBNull.Value)
                    {
                        current = Convert.ToInt32(result);
                    }
                }

                if (current < needed)
                {
                    throw new InvalidOperationException(
                        "Not enough godown stock for " + label +
                        ": only " + current +
                        " packet(s) available, this dispatch needs " +
                        needed + ".");
                }

                string write = @"
                    UPDATE Inventory
                    SET QuantityPackets = QuantityPackets - @Quantity,
                        UpdatedBy = @UpdatedBy,
                        UpdatedAt = GETDATE()
                    WHERE DealerId = @DealerId
                      AND ProductVariantId = @ProductVariantId";

                int affected;

                using (SqlCommand cmd =
                    new SqlCommand(write, con, tx))
                {
                    cmd.Parameters.Add(
                        "@Quantity",
                        SqlDbType.Int).Value = needed;

                    cmd.Parameters.Add(
                        "@UpdatedBy",
                        SqlDbType.Int).Value = DealerId;

                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    cmd.Parameters.Add(
                        "@ProductVariantId",
                        SqlDbType.Int).Value = variantId;

                    affected = cmd.ExecuteNonQuery();

                    if (affected == 0)
                    {
                        // Another session may have created the row
                        // between the read and this write - one
                        // retry settles the unique key race.
                        affected = cmd.ExecuteNonQuery();
                    }

                    if (affected == 0)
                    {
                        string insert = @"
                            INSERT INTO Inventory
                            (
                                DealerId,
                                ProductVariantId,
                                QuantityPackets,
                                ReorderLevel,
                                UpdatedBy,
                                UpdatedAt
                            )
                            VALUES
                            (
                                @DealerId,
                                @ProductVariantId,
                                @QuantityPackets,
                                0,
                                @UpdatedBy,
                                GETDATE()
                            )";

                        using (SqlCommand ins =
                            new SqlCommand(
                                insert,
                                con,
                                tx))
                        {
                            ins.Parameters.Add(
                                "@DealerId",
                                SqlDbType.Int).Value =
                                DealerId;

                            ins.Parameters.Add(
                                "@ProductVariantId",
                                SqlDbType.Int).Value =
                                variantId;

                            ins.Parameters.Add(
                                "@QuantityPackets",
                                SqlDbType.Int).Value =
                                current - needed;

                            ins.Parameters.Add(
                                "@UpdatedBy",
                                SqlDbType.Int).Value =
                                DealerId;

                            ins.ExecuteNonQuery();
                        }
                    }
                }
            }
        }


        // One 'Stock Out' header for the dispatch, one detail row
        // per variant. Quantities are written as the signed stock
        // movement, so Stock Out rows carry a negative quantity -
        // exactly what the existing ledger rows do.
        private void WriteLedger(
            SqlConnection con,
            SqlTransaction tx,
            Dictionary<int, int> packets,
            string dispatchNumber,
            string remarks)
        {
            List<int> variants = SortedVariants(packets);

            if (variants.Count == 0)
            {
                return;
            }

            string header = @"
                INSERT INTO StockTransactions
                (
                    DealerId,
                    TransactionType,
                    TransactionDate,
                    ReferenceNo,
                    Remarks,
                    CreatedBy,
                    CreatedAt
                )
                VALUES
                (
                    @DealerId,
                    @TransactionType,
                    GETDATE(),
                    @ReferenceNo,
                    @Remarks,
                    @CreatedBy,
                    GETDATE()
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int transactionId;

            using (SqlCommand cmd =
                new SqlCommand(header, con, tx))
            {
                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@TransactionType",
                    SqlDbType.NVarChar,
                    80).Value = "Stock Out";

                cmd.Parameters.Add(
                    "@ReferenceNo",
                    SqlDbType.NVarChar,
                    200).Value = dispatchNumber;

                cmd.Parameters.Add(
                    "@Remarks",
                    SqlDbType.NVarChar,
                    1000).Value =
                    remarks == null
                        ? (object)("Dispatch " + dispatchNumber)
                        : remarks;

                cmd.Parameters.Add(
                    "@CreatedBy",
                    SqlDbType.Int).Value = DealerId;

                transactionId =
                    Convert.ToInt32(cmd.ExecuteScalar());
            }

            int unitId = PacketUnitId(con, tx);

            if (unitId == 0)
            {
                throw new InvalidOperationException(
                    "No packet (PKT) selling unit is configured.");
            }

            string detail = @"
                INSERT INTO StockTransactionDetails
                (
                    StockTransactionId,
                    ProductVariantId,
                    UnitId,
                    Quantity,
                    QuantityPackets
                )
                VALUES
                (
                    @StockTransactionId,
                    @ProductVariantId,
                    @UnitId,
                    @Quantity,
                    @QuantityPackets
                )";

            foreach (int variantId in variants)
            {
                using (SqlCommand cmd =
                    new SqlCommand(detail, con, tx))
                {
                    cmd.Parameters.Add(
                        "@StockTransactionId",
                        SqlDbType.Int).Value =
                        transactionId;

                    cmd.Parameters.Add(
                        "@ProductVariantId",
                        SqlDbType.Int).Value =
                        variantId;

                    cmd.Parameters.Add(
                        "@UnitId",
                        SqlDbType.Int).Value = unitId;

                    // Stored as the signed stock movement, so
                    // Stock Out rows carry a negative quantity.
                    cmd.Parameters.Add(
                        "@Quantity",
                        SqlDbType.Int).Value =
                        -packets[variantId];

                    cmd.Parameters.Add(
                        "@QuantityPackets",
                        SqlDbType.Int).Value =
                        -packets[variantId];

                    cmd.ExecuteNonQuery();
                }
            }
        }


        private static List<int> SortedVariants(
            Dictionary<int, int> packets)
        {
            List<int> variants =
                new List<int>(packets.Keys);

            variants.Sort();

            return variants;
        }


        private int PacketUnitId(
            SqlConnection con,
            SqlTransaction tx)
        {
            string query = @"
                SELECT TOP (1) UnitId
                FROM SellingUnits
                WHERE UnitCode = 'PKT'
                  AND IsActive = 1
                ORDER BY UnitId";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                object result = cmd.ExecuteScalar();

                return result == null ||
                       result == DBNull.Value
                    ? 0
                    : Convert.ToInt32(result);
            }
        }


        private string VariantLabel(
            SqlConnection con,
            SqlTransaction tx,
            int variantId)
        {
            string query = @"
                SELECT p.ProductName + ' - ' + v.VariantName
                FROM ProductVariants v
                INNER JOIN Products p
                    ON p.ProductId = v.ProductId
                WHERE v.ProductVariantId = @ProductVariantId";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                object result = cmd.ExecuteScalar();

                return result == null ||
                       result == DBNull.Value
                    ? "variant #" + variantId
                    : result.ToString();
            }
        }


        // ============================================================
        // HELPERS
        // ============================================================

        private static int SelectedId(DropDownList dropdown)
        {
            int id = 0;
            int.TryParse(dropdown.SelectedValue, out id);
            return id;
        }

        private static bool TryDate(
            string text,
            out DateTime value)
        {
            return DateTime.TryParse(
                text,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out value);
        }

        private void ShowMessage(
            string message,
            bool success)
        {
            lblMessage.Visible = true;
            lblMessage.Text = message;
            lblMessage.CssClass = success
                ? "alert alert-success d-block"
                : "alert alert-danger d-block";
        }
    }
}
