using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Salesman
{
    public partial class CreateOrder : System.Web.UI.Page
    {
        private int SalesmanId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }


        // The line list survives postbacks so the builder, the
        // repeater and the save all work from one source of truth.
        private DataTable Lines
        {
            get
            {
                DataTable dt =
                    ViewState["Lines"] as DataTable;

                if (dt == null)
                {
                    dt = NewLineTable();
                    ViewState["Lines"] = dt;
                }

                return dt;
            }
            set { ViewState["Lines"] = value; }
        }

        private static DataTable NewLineTable()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("Index", typeof(int));
            dt.Columns.Add("ProductVariantId", typeof(int));
            dt.Columns.Add("ProductName", typeof(string));
            dt.Columns.Add("VariantName", typeof(string));
            dt.Columns.Add("UnitId", typeof(int));
            dt.Columns.Add("UnitCode", typeof(string));
            dt.Columns.Add("Quantity", typeof(int));
            dt.Columns.Add("QuantityPackets", typeof(int));
            dt.Columns.Add("UnitPrice", typeof(decimal));
            dt.Columns.Add("LineTotal", typeof(decimal));

            return dt;
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
                    "Salesman",
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
                LoadShops();
                LoadVariants();
                LoadUnits(0);

                txtOrderDate.Text =
                    DateTime.Today.ToString("yyyy-MM-dd");

                BindLines();
            }
        }


        // ============================================================
        // DROPDOWNS
        // ============================================================

        private void LoadShops()
        {
            string query = @"
                SELECT
                    s.ShopId,
                    Label = s.ShopCode + ' - ' + s.ShopName,
                    ISNULL(s.RouteScheduleId, 0) AS RouteScheduleId
                FROM Shops s
                WHERE s.IsActive = 1
                  AND
                  (
                    s.RouteScheduleId IN
                    (
                        SELECT rs2.RouteScheduleId
                        FROM RouteSchedules rs2
                        WHERE rs2.SalesmanId = @SalesmanId
                    )
                    OR s.RouteId IN
                    (
                        SELECT rs3.RouteId
                        FROM RouteSchedules rs3
                        WHERE rs3.SalesmanId = @SalesmanId
                    )
                  )
                ORDER BY s.ShopName";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@SalesmanId",
                        SqlDbType.Int).Value = SalesmanId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        ddlShop.DataSource = dt;
                        ddlShop.DataTextField = "Label";
                        ddlShop.DataValueField = "ShopId";
                        ddlShop.DataBind();
                    }
                }
            }

            ddlShop.Items.Insert(
                0,
                new ListItem("Select Shop", ""));
        }


        private void LoadVariants()
        {
            string query = @"
                SELECT
                    v.ProductVariantId,
                    Label = p.ProductName
                            + ' - '
                            + v.VariantName
                            + ISNULL(
                                ' ('
                                + CONVERT(NVARCHAR(20), v.PacketWeight)
                                + ' ' + v.WeightUnit + ')',
                                '')
                FROM ProductVariants v
                INNER JOIN Products p
                    ON p.ProductId = v.ProductId
                WHERE v.IsActive = 1
                  AND p.IsActive = 1
                ORDER BY p.ProductName, v.SortOrder, v.VariantName";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        ddlVariant.DataSource = dt;
                        ddlVariant.DataTextField = "Label";
                        ddlVariant.DataValueField =
                            "ProductVariantId";
                        ddlVariant.DataBind();
                    }
                }
            }

            ddlVariant.Items.Insert(
                0,
                new ListItem("Select Variant", ""));
        }


        private void LoadUnits(int variantId)
        {
            DataTable dt = new DataTable();

            if (variantId > 0)
            {
                string query = @"
                    SELECT
                        po.UnitId,
                        Label = u.UnitCode
                                + ' ('
                                + CONVERT(NVARCHAR(20), po.PacketsPerUnit)
                                + ' pkt)'
                    FROM ProductUnitOptions po
                    INNER JOIN SellingUnits u
                        ON u.UnitId = po.UnitId
                    WHERE po.ProductVariantId = @ProductVariantId
                      AND po.IsActive = 1
                      AND u.IsActive = 1
                    ORDER BY u.SortOrder, u.UnitId";

                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@ProductVariantId",
                            SqlDbType.Int).Value = variantId;

                        using (SqlDataAdapter da =
                            new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
            }

            string keep = ddlUnit.SelectedValue;

            ddlUnit.DataSource = dt;
            ddlUnit.DataTextField = "Label";
            ddlUnit.DataValueField = "UnitId";
            ddlUnit.DataBind();

            ddlUnit.Items.Insert(
                0,
                new ListItem("Select Unit", ""));

            if (!string.IsNullOrEmpty(keep) &&
                ddlUnit.Items.FindByValue(keep) != null)
            {
                ddlUnit.SelectedValue = keep;
            }
        }


        // Packets per unit + shop selling price for the pair.
        private bool GetVariantUnitInfo(
            int variantId,
            int unitId,
            out int packetsPerUnit,
            out decimal price)
        {
            packetsPerUnit = 0;
            price = 0m;

            if (variantId <= 0 || unitId <= 0)
            {
                return false;
            }

            // The catalogue keeps a price history, so the row used is the one in
            // force on the ORDER date - the newest price that had already
            // started. Without the APPLY this join would return one row
            // per historic price and the result would be arbitrary.
            string query = @"
                SELECT
                    po.PacketsPerUnit,
                    Price = ISNULL(p.ShopSellingPrice, 0)
                FROM ProductUnitOptions po
                OUTER APPLY
                (
                    SELECT TOP (1)
                           pp.ShopSellingPrice,
                           pp.EffectiveFrom
                    FROM   ProductPrices pp
                    WHERE  pp.ProductOptionId = po.ProductOptionId
                      AND  pp.EffectiveFrom <= @PriceDate
                    ORDER BY pp.EffectiveFrom DESC
                ) p
                WHERE po.ProductVariantId = @ProductVariantId
                  AND po.UnitId = @UnitId
                  AND po.IsActive = 1";

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@ProductVariantId",
                        SqlDbType.Int).Value = variantId;

                    cmd.Parameters.Add(
                        "@UnitId",
                        SqlDbType.Int).Value = unitId;

                    cmd.Parameters.Add(
                        "@PriceDate",
                        SqlDbType.Date).Value =
                        OrderDateForPricing();

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            if (dt.Rows.Count == 0)
            {
                return false;
            }

            packetsPerUnit = Convert.ToInt32(
                dt.Rows[0]["PacketsPerUnit"]);

            price = Convert.ToDecimal(
                dt.Rows[0]["Price"]);

            return true;
        }


        protected void LineInputsChanged(
            object sender,
            EventArgs e)
        {
            int variantId = SelectedVariantId();

            if (variantId <= 0)
            {
                LoadUnits(0);
                txtUnitPrice.Text = "0.00";
                return;
            }

            LoadUnits(variantId);

            int packets;
            decimal price;

            if (GetVariantUnitInfo(
                variantId,
                SelectedUnitId(),
                out packets,
                out price))
            {
                txtUnitPrice.Text =
                    price.ToString(
                        "0.00",
                        CultureInfo.InvariantCulture);
            }
        }


        protected void DiscountChanged(
            object sender,
            EventArgs e)
        {
            BindLines();
        }


        // ============================================================
        // LINES
        // ============================================================

        protected void btnAddLine_Click(
            object sender,
            EventArgs e)
        {
            Page.Validate("LineAdd");

            if (!Page.IsValid)
            {
                return;
            }

            int variantId = SelectedVariantId();
            int unitId = SelectedUnitId();

            if (variantId <= 0 || unitId <= 0)
            {
                ShowMessage(
                    "Select a product variant and a unit.",
                    false);

                return;
            }

            int qty;

            if (!int.TryParse(
                    txtQuantity.Text.Trim(),
                    out qty) || qty <= 0)
            {
                ShowMessage(
                    "Quantity must be a whole number above zero.",
                    false);

                return;
            }

            decimal price;

            if (!TryDecimal(
                    txtUnitPrice.Text.Trim(),
                    out price) || price < 0)
            {
                ShowMessage(
                    "Unit price must be zero or more.",
                    false);

                return;
            }

            int packetsPerUnit;
            decimal masterPrice;

            if (!GetVariantUnitInfo(
                variantId,
                unitId,
                out packetsPerUnit,
                out masterPrice) || packetsPerUnit <= 0)
            {
                ShowMessage(
                    "That unit is not available for the selected variant.",
                    false);

                return;
            }

            DataTable dt = Lines;

            // Same variant + unit already on the order: grow it
            // instead of adding a second row.
            DataRow existing = null;

            foreach (DataRow row in dt.Rows)
            {
                if (Convert.ToInt32(
                        row["ProductVariantId"]) == variantId &&
                    Convert.ToInt32(row["UnitId"]) == unitId)
                {
                    existing = row;
                    break;
                }
            }

            if (existing != null)
            {
                int newQty =
                    Convert.ToInt32(existing["Quantity"]) + qty;

                existing["Quantity"] = newQty;
                existing["QuantityPackets"] =
                    newQty * packetsPerUnit;
                existing["UnitPrice"] = price;
                existing["LineTotal"] = newQty * price;
            }
            else
            {
                DataRow row = dt.NewRow();

                row["Index"] = dt.Rows.Count;
                row["ProductVariantId"] = variantId;
                row["ProductName"] = LookupText(
                    @"SELECT p.ProductName
                      FROM ProductVariants v
                      INNER JOIN Products p
                          ON p.ProductId = v.ProductId
                      WHERE v.ProductVariantId = @Id",
                    variantId);
                row["VariantName"] = LookupText(
                    @"SELECT VariantName
                      FROM ProductVariants
                      WHERE ProductVariantId = @Id",
                    variantId);
                row["UnitId"] = unitId;
                row["UnitCode"] = LookupText(
                    @"SELECT UnitCode
                      FROM SellingUnits
                      WHERE UnitId = @Id",
                    unitId);
                row["Quantity"] = qty;
                row["QuantityPackets"] =
                    qty * packetsPerUnit;
                row["UnitPrice"] = price;
                row["LineTotal"] = qty * price;

                dt.Rows.Add(row);
            }

            Lines = dt;

            BindLines();

            ShowMessage("Line added.", true);
        }


        protected void rpLines_ItemCommand(
            object source,
            RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Remove")
            {
                return;
            }

            int index;

            if (!int.TryParse(
                e.CommandArgument.ToString(),
                out index))
            {
                return;
            }

            DataTable dt = Lines;

            if (index < 0 || index >= dt.Rows.Count)
            {
                return;
            }

            dt.Rows.RemoveAt(index);
            Lines = dt;

            BindLines();
        }


        private void BindLines()
        {
            DataTable dt = Lines;

            int i = 0;

            foreach (DataRow row in dt.Rows)
            {
                row["Index"] = i++;
            }

            rpLines.DataSource = dt;
            rpLines.DataBind();

            pnlNoLines.Visible = dt.Rows.Count == 0;
            pnlLines.Visible = dt.Rows.Count > 0;

            decimal subTotal = 0m;

            foreach (DataRow row in dt.Rows)
            {
                subTotal +=
                    Convert.ToDecimal(row["LineTotal"]);
            }

            decimal discount;

            if (!TryDecimal(
                txtDiscount.Text.Trim(),
                out discount))
            {
                discount = 0m;
            }

            lblSubTotal.Text = subTotal.ToString("N2");
            lblDiscount.Text = discount.ToString("N2");
            lblGrandTotal.Text =
                Math.Max(0m, subTotal - discount)
                    .ToString("N2");
        }


        private string LookupText(
            string query,
            int id)
        {
            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@Id",
                        SqlDbType.Int).Value = id;

                    object result = cmd.ExecuteScalar();

                    return result == null ||
                           result == DBNull.Value
                        ? ""
                        : result.ToString();
                }
            }
        }


        // ============================================================
        // SAVE
        //
        // A beat order is a real sale: the goods came off the vehicle
        // when it was loaded and are issued from the vehicle when the
        // bill is made, so the end-of-day count can be reconciled
        // against what was actually billed. The godown is NOT touched
        // again here - it was reduced once, at load time.
        // ============================================================

        protected void btnSave_Click(object sender, EventArgs e)
        {
            Page.Validate("Order");

            if (!Page.IsValid)
            {
                ShowMessage(
                    "Please correct the highlighted fields.",
                    false);

                return;
            }

            DataTable lines = Lines;

            if (lines.Rows.Count == 0)
            {
                ShowMessage(
                    "Add at least one order line.",
                    false);

                return;
            }

            int shopId = 0;
            int.TryParse(ddlShop.SelectedValue, out shopId);

            if (shopId <= 0)
            {
                ShowMessage(
                    "Select a shop.",
                    false);

                return;
            }

            DateTime orderDate;

            if (!TryDate(
                txtOrderDate.Text.Trim(),
                out orderDate))
            {
                ShowMessage(
                    "Order date is required and must be a valid date (YYYY-MM-DD).",
                    false);

                return;
            }

            DateTime? deliveryDate = null;
            DateTime parsedDelivery;

            if (!string.IsNullOrWhiteSpace(
                txtDeliveryDate.Text.Trim()))
            {
                if (!TryDate(
                    txtDeliveryDate.Text.Trim(),
                    out parsedDelivery))
                {
                    ShowMessage(
                        "Delivery date must be a valid date (YYYY-MM-DD).",
                        false);

                    return;
                }

                deliveryDate = parsedDelivery;
            }

            decimal discount;

            if (!TryDecimal(
                txtDiscount.Text.Trim(),
                out discount) || discount < 0)
            {
                ShowMessage(
                    "Discount must be zero or more.",
                    false);

                return;
            }

            decimal subTotal = 0m;

            foreach (DataRow row in lines.Rows)
            {
                subTotal +=
                    Convert.ToDecimal(row["LineTotal"]);
            }

            if (discount > subTotal)
            {
                ShowMessage(
                    "Discount cannot be greater than the sub total.",
                    false);

                return;
            }

            decimal grandTotal = subTotal - discount;

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
                            int dealerId;
                            int scheduleId;

                            // The order belongs to the shop's
                            // dealer, and the shop must sit on
                            // this salesman's work.
                            if (!ShopForSalesman(
                                con,
                                tx,
                                shopId,
                                out dealerId,
                                out scheduleId))
                            {
                                throw new InvalidOperationException(
                                    "That shop is not on your routes.");
                            }

                            string orderNumber =
                                NextOrderNumber(
                                    con,
                                    tx,
                                    dealerId);

                            int orderId;

                            string insert = @"
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
                                    CreatedBy,
                                    CreatedAt
                                )
                                VALUES
                                (
                                    @DealerId,
                                    @OrderNumber,
                                    @ShopId,
                                    @OrderType,
                                    @OrderDate,
                                    @DeliveryDate,
                                    @RouteScheduleId,
                                    @SalesmanId,
                                    'Pending',
                                    'Beat',
                                    @SubTotal,
                                    @Discount,
                                    @GrandTotal,
                                    @Remarks,
                                    @CreatedBy,
                                    GETDATE()
                                );
                                SELECT CAST(SCOPE_IDENTITY() AS INT);";

                            using (SqlCommand cmd =
                                new SqlCommand(insert, con, tx))
                            {
                                cmd.Parameters.Add(
                                    "@DealerId",
                                    SqlDbType.Int).Value =
                                    dealerId;

                                cmd.Parameters.Add(
                                    "@OrderNumber",
                                    SqlDbType.NVarChar,
                                    100).Value = orderNumber;

                                cmd.Parameters.Add(
                                    "@ShopId",
                                    SqlDbType.Int).Value =
                                    shopId;

                                cmd.Parameters.Add(
                                    "@OrderType",
                                    SqlDbType.NVarChar,
                                    60).Value = "Order Taking";

                                cmd.Parameters.Add(
                                    "@OrderDate",
                                    SqlDbType.Date).Value =
                                    orderDate;

                                cmd.Parameters.Add(
                                    "@DeliveryDate",
                                    SqlDbType.Date).Value =
                                    deliveryDate.HasValue
                                        ? (object)deliveryDate.Value
                                        : DBNull.Value;

                                cmd.Parameters.Add(
                                    "@RouteScheduleId",
                                    SqlDbType.Int).Value =
                                    scheduleId > 0
                                        ? (object)scheduleId
                                        : DBNull.Value;

                                cmd.Parameters.Add(
                                    "@SalesmanId",
                                    SqlDbType.Int).Value =
                                    SalesmanId;

                                cmd.Parameters.Add(
                                    "@SubTotal",
                                    SqlDbType.Decimal).Value =
                                    subTotal;

                                cmd.Parameters.Add(
                                    "@Discount",
                                    SqlDbType.Decimal).Value =
                                    discount;

                                cmd.Parameters.Add(
                                    "@GrandTotal",
                                    SqlDbType.Decimal).Value =
                                    grandTotal;

                                cmd.Parameters.Add(
                                    "@Remarks",
                                    SqlDbType.NVarChar,
                                    1000).Value =
                                    remarks == null
                                        ? (object)DBNull.Value
                                        : remarks;

                                cmd.Parameters.Add(
                                    "@CreatedBy",
                                    SqlDbType.Int).Value =
                                    SalesmanId;

                                orderId = Convert.ToInt32(
                                    cmd.ExecuteScalar());
                            }

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

                            foreach (DataRow row in lines.Rows)
                            {
                                using (SqlCommand cmd =
                                    new SqlCommand(detail, con, tx))
                                {
                                    cmd.Parameters.Add(
                                        "@OrderId",
                                        SqlDbType.Int).Value =
                                        orderId;

                                    cmd.Parameters.Add(
                                        "@ProductVariantId",
                                        SqlDbType.Int).Value =
                                        Convert.ToInt32(
                                            row["ProductVariantId"]);

                                    cmd.Parameters.Add(
                                        "@UnitId",
                                        SqlDbType.Int).Value =
                                        Convert.ToInt32(
                                            row["UnitId"]);

                                    cmd.Parameters.Add(
                                        "@Quantity",
                                        SqlDbType.Int).Value =
                                        Convert.ToInt32(
                                            row["Quantity"]);

                                    cmd.Parameters.Add(
                                        "@QuantityPackets",
                                        SqlDbType.Int).Value =
                                        Convert.ToInt32(
                                            row["QuantityPackets"]);

                                    cmd.Parameters.Add(
                                        "@UnitPrice",
                                        SqlDbType.Decimal).Value =
                                        Convert.ToDecimal(
                                            row["UnitPrice"]);

                                    cmd.Parameters.Add(
                                        "@LineTotal",
                                        SqlDbType.Decimal).Value =
                                        Convert.ToDecimal(
                                            row["LineTotal"]);

                                    cmd.ExecuteNonQuery();
                                }
                            }


                            // ----------------------------
                            // THE SALE LEAVES THE VEHICLE
                            //
                            // Goods went from the godown onto the
                            // vehicle at LOAD time, so nothing is
                            // deducted from Inventory again here.
                            // What the salesman bills now comes off
                            // the vehicle, which is exactly what the
                            // end-of-day count is checked against.
                            // ----------------------------

                            IssueFromVehicle(
                                con,
                                tx,
                                dealerId,
                                scheduleId,
                                orderDate,
                                orderNumber,
                                lines);

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
                    "~/Salesman/Orders.aspx?saved=1");
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error saving order: " + ex.Message,
                    false);
            }
        }


        // Returns false when the shop is not on this salesman's
        // routes or schedules - orders are never created for
        // shops they cannot visit.
        private bool ShopForSalesman(
            SqlConnection con,
            SqlTransaction tx,
            int shopId,
            out int dealerId,
            out int scheduleId)
        {
            dealerId = 0;
            scheduleId = 0;

            string query = @"
                SELECT
                    s.DealerId,
                    ISNULL(s.RouteScheduleId, 0)
                FROM Shops s
                WHERE s.ShopId = @ShopId
                  AND s.IsActive = 1
                  AND
                  (
                    s.RouteScheduleId IN
                    (
                        SELECT rs2.RouteScheduleId
                        FROM RouteSchedules rs2
                        WHERE rs2.SalesmanId = @SalesmanId
                    )
                    OR s.RouteId IN
                    (
                        SELECT rs3.RouteId
                        FROM RouteSchedules rs3
                        WHERE rs3.SalesmanId = @SalesmanId
                    )
                  )";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@ShopId",
                    SqlDbType.Int).Value = shopId;

                cmd.Parameters.Add(
                    "@SalesmanId",
                    SqlDbType.Int).Value = SalesmanId;

                using (SqlDataReader reader =
                    cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return false;
                    }

                    dealerId = Convert.ToInt32(reader[0]);
                    scheduleId = Convert.ToInt32(reader[1]);

                    return dealerId > 0;
                }
            }
        }


        private string NextOrderNumber(
            SqlConnection con,
            SqlTransaction tx,
            int dealerId)
        {
            string prefix =
                "ORD-" +
                DateTime.Now.ToString("yyyyMMdd-HHmmss");

            string candidate = prefix;
            int suffix = 2;

            while (OrderNumberExists(
                con, tx, dealerId, candidate))
            {
                candidate = prefix + "-" + suffix++;
            }

            return candidate;
        }


        private bool OrderNumberExists(
            SqlConnection con,
            SqlTransaction tx,
            int dealerId,
            string number)
        {
            string query = @"
                SELECT COUNT(*)
                FROM Orders
                WHERE DealerId = @DealerId
                  AND OrderNumber = @OrderNumber";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = dealerId;

                cmd.Parameters.Add(
                    "@OrderNumber",
                    SqlDbType.NVarChar,
                    100).Value = number;

                return Convert.ToInt32(
                    cmd.ExecuteScalar()) > 0;
            }
        }


        // ============================================================
        // HELPERS
        // ============================================================

        private int SelectedVariantId()
        {
            int id = 0;
            int.TryParse(ddlVariant.SelectedValue, out id);
            return id;
        }

        private int SelectedUnitId()
        {
            int id = 0;
            int.TryParse(ddlUnit.SelectedValue, out id);
            return id;
        }

        private static bool TryDecimal(
            string text,
            out decimal value)
        {
            return decimal.TryParse(
                text,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out value);
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

        // ============================================================
        // ISSUE THE SALE FROM THE VEHICLE
        //
        // Available on the vehicle for this route + day =
        //     LoadedPackets
        //   - SoldPackets      (earlier bills today)
        //   - DamagedPackets
        //   + ReturnedPackets
        //
        // Any shortfall is rejected BEFORE the transaction commits, so a
        // bill can never be raised for stock the vehicle never carried.
        // That is also what makes the end-of-day count meaningful: a
        // variance at close is goods that left without a bill.
        // ============================================================

        private void IssueFromVehicle(
            SqlConnection con,
            SqlTransaction tx,
            int dealerId,
            int scheduleId,
            DateTime orderDate,
            string orderNumber,
            DataTable lines)
        {
            // The route owns the vehicle; a schedule without one cannot
            // carry stock, so there is nothing to issue from.
            string vehicleQuery = @"
                SELECT ISNULL(VehicleId, 0)
                FROM RouteSchedules
                WHERE RouteScheduleId = @RouteScheduleId";

            int vehicleId = 0;

            using (SqlCommand cmd =
                new SqlCommand(vehicleQuery, con, tx))
            {
                cmd.Parameters.Add(
                    "@RouteScheduleId",
                    SqlDbType.Int).Value = scheduleId;

                object result = cmd.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    vehicleId = Convert.ToInt32(result);
                }
            }

            if (vehicleId <= 0)
            {
                throw new InvalidOperationException(
                    "This route has no vehicle assigned, so stock cannot " +
                    "be issued from it. Ask the dealer to assign a vehicle.");
            }

            // A counted and approved route day is finished.
            if (IsRouteDayClosed(con, tx, scheduleId, orderDate))
            {
                throw new InvalidOperationException(
                    "This route's day for " +
                    orderDate.ToString("yyyy-MM-dd") +
                    " has already been closed by the dealer. " +
                    "Nothing was saved - ask the dealer to reopen it " +
                    "if this bill is genuine.");
            }


            // One issue per variant even when the bill has several lines
            // for it, so the read-modify-write below is unambiguous.
            Dictionary<int, int> needed =
                new Dictionary<int, int>();

            foreach (DataRow row in lines.Rows)
            {
                int variantId = Convert.ToInt32(
                    row["ProductVariantId"]);

                int packets = Convert.ToInt32(
                    row["QuantityPackets"]);

                if (packets <= 0)
                {
                    continue;
                }

                if (needed.ContainsKey(variantId))
                {
                    needed[variantId] += packets;
                }
                else
                {
                    needed.Add(variantId, packets);
                }
            }


            string read = @"
                SELECT
                    ISNULL(LoadedPackets, 0)
                    - ISNULL(SoldPackets, 0)
                    - ISNULL(DamagedPackets, 0)
                    + ISNULL(ReturnedPackets, 0)
                FROM VehicleStock WITH (UPDLOCK, ROWLOCK)
                WHERE DealerId = @DealerId
                  AND VehicleId = @VehicleId
                  AND RouteScheduleId = @RouteScheduleId
                  AND StockDate = @StockDate
                  AND ProductVariantId = @ProductVariantId";

            string update = @"
                UPDATE VehicleStock
                SET SoldPackets = SoldPackets + @SoldPackets,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE DealerId = @DealerId
                  AND VehicleId = @VehicleId
                  AND RouteScheduleId = @RouteScheduleId
                  AND StockDate = @StockDate
                  AND ProductVariantId = @ProductVariantId";

            string upsert = @"
                SELECT VehicleStockId
                FROM VehicleStock WITH (UPDLOCK, ROWLOCK)
                WHERE DealerId = @DealerId
                  AND VehicleId = @VehicleId
                  AND RouteScheduleId = @RouteScheduleId
                  AND StockDate = @StockDate
                  AND ProductVariantId = @ProductVariantId";

            foreach (KeyValuePair<int, int> pair in needed)
            {
                int variantId = pair.Key;
                int want = pair.Value;

                int available = 0;

                using (SqlCommand cmd =
                    new SqlCommand(read, con, tx))
                {
                    AddVehicleStockParameters(
                        cmd,
                        dealerId,
                        vehicleId,
                        scheduleId,
                        orderDate,
                        variantId);

                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        available = Convert.ToInt32(result);
                    }
                }

                if (available < want)
                {
                    throw new InvalidOperationException(
                        "Not enough stock on the vehicle for " +
                        VariantLabel(variantId) +
                        ": only " + available +
                        " packet(s) on board, this bill needs " + want +
                        ". The rest of the bill was not saved.");
                }

                using (SqlCommand cmd =
                    new SqlCommand(upsert, con, tx))
                {
                    AddVehicleStockParameters(
                        cmd,
                        dealerId,
                        vehicleId,
                        scheduleId,
                        orderDate,
                        variantId);

                    bool exists =
                        cmd.ExecuteScalar() != null;

                    if (!exists)
                    {
                        // A bill can only be raised against stock that
                        // was loaded. No vehicle row for this variant and
                        // day means it was never loaded, so this is a
                        // shortfall of the whole quantity.
                        throw new InvalidOperationException(
                            "Nothing on the vehicle for " +
                            VariantLabel(variantId) +
                            " on this route today - nothing was loaded " +
                            "for it. The rest of the bill was not saved.");
                    }
                }

                using (SqlCommand cmd =
                    new SqlCommand(update, con, tx))
                {
                    cmd.Parameters.Add(
                        "@SoldPackets",
                        SqlDbType.Int).Value = want;

                    cmd.Parameters.Add(
                        "@UpdatedBy",
                        SqlDbType.Int).Value = SalesmanId;

                    AddVehicleStockParameters(
                        cmd,
                        dealerId,
                        vehicleId,
                        scheduleId,
                        orderDate,
                        variantId);

                    cmd.ExecuteNonQuery();
                }

                WriteVehicleIssueLedger(
                    con,
                    tx,
                    dealerId,
                    vehicleId,
                    scheduleId,
                    orderDate,
                    orderNumber,
                    variantId,
                    want);
            }
        }


        // A route day closed by the dealer cannot be billed against. The
        // schedule's StockDate is set the first time a day is closed, so a
        // recurring schedule is only blocked for that specific day.
        private static bool IsRouteDayClosed(
            SqlConnection con,
            SqlTransaction tx,
            int scheduleId,
            DateTime date)
        {
            string query = @"
                SELECT TOP (1) IsClosed
                FROM RouteSchedules
                WHERE RouteScheduleId = @RouteScheduleId
                ORDER BY
                    CASE
                        WHEN StockDate = @StockDate THEN 0
                        ELSE 1
                    END";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@RouteScheduleId",
                    SqlDbType.Int).Value = scheduleId;

                cmd.Parameters.Add(
                    "@StockDate",
                    SqlDbType.Date).Value = date;

                object result = cmd.ExecuteScalar();

                return result != null &&
                       result != DBNull.Value &&
                       Convert.ToBoolean(result);
            }
        }


        private void AddVehicleStockParameters(
            SqlCommand cmd,
            int dealerId,
            int vehicleId,
            int scheduleId,
            DateTime stockDate,
            int variantId)
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
                SqlDbType.Date).Value = stockDate;

            cmd.Parameters.Add(
                "@ProductVariantId",
                SqlDbType.Int).Value = variantId;
        }


        // Keeps the vehicle movement auditable from the stock ledger as
        // well as from VehicleStock, exactly like every other movement.
        private void WriteVehicleIssueLedger(
            SqlConnection con,
            SqlTransaction tx,
            int dealerId,
            int vehicleId,
            int scheduleId,
            DateTime stockDate,
            string orderNumber,
            int variantId,
            int packets)
        {
            int unitId = PacketUnitId(con, tx);

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
                    'Vehicle Sale',
                    GETDATE(),
                    @ReferenceNo,
                    @Remarks,
                    @CreatedBy,
                    GETDATE()
                );
                SELECT SCOPE_IDENTITY();";

            int transactionId;

            using (SqlCommand cmd =
                new SqlCommand(header, con, tx))
            {
                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = dealerId;

                cmd.Parameters.Add(
                    "@ReferenceNo",
                    SqlDbType.NVarChar,
                    200).Value = orderNumber;

                cmd.Parameters.Add(
                    "@Remarks",
                    SqlDbType.NVarChar,
                    1000).Value =
                    "Sold from vehicle on route schedule " +
                    scheduleId.ToString() + " for " +
                    stockDate.ToString("yyyy-MM-dd");

                cmd.Parameters.Add(
                    "@CreatedBy",
                    SqlDbType.Int).Value = SalesmanId;

                transactionId =
                    Convert.ToInt32(cmd.ExecuteScalar());
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

            using (SqlCommand cmd =
                new SqlCommand(detail, con, tx))
            {
                cmd.Parameters.Add(
                    "@StockTransactionId",
                    SqlDbType.Int).Value = transactionId;

                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                cmd.Parameters.Add(
                    "@UnitId",
                    SqlDbType.Int).Value = unitId;

                cmd.Parameters.Add(
                    "@Quantity",
                    SqlDbType.Int).Value = packets;

                cmd.Parameters.Add(
                    "@QuantityPackets",
                    SqlDbType.Int).Value = packets;

                cmd.ExecuteNonQuery();
            }
        }


        // The price that applies is the one in force on the date the bill is
        // made. If the date on the form is unreadable, today is used so the
        // lookup still returns something sensible.
        private DateTime OrderDateForPricing()
        {
            DateTime date;

            if (TryDate(txtOrderDate.Text.Trim(), out date))
            {
                return date;
            }

            return DateTime.Today;
        }


        private string VariantLabel(int variantId)
        {
            string query = @"
                SELECT
                    p.ProductName + ' - ' + v.VariantName
                FROM ProductVariants v
                INNER JOIN Products p
                    ON p.ProductId = v.ProductId
                WHERE v.ProductVariantId = @Id";

            return LookupText(query, variantId);
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
