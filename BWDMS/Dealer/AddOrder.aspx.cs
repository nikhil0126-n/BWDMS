using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class AddOrder : System.Web.UI.Page
    {
        // ============================================================
        // STATE
        // ============================================================

        // Cached so a brand-new order can be given its id the
        // moment the header row is inserted.
        private int _orderId = -1;

        private int OrderId
        {
            get
            {
                if (_orderId < 0)
                {
                    int id;

                    _orderId = int.TryParse(
                        Request.QueryString["id"],
                        out id)
                        ? id
                        : 0;
                }

                return _orderId;
            }
            set { _orderId = value; }
        }

        private int DealerId
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
                LoadShops();
                LoadSchedules();
                LoadSalesmen();
                LoadVariants();

                txtOrderDate.Text =
                    DateTime.Today.ToString("yyyy-MM-dd");

                if (OrderId > 0)
                {
                    if (!LoadOrder())
                    {
                        Response.Redirect(
                            "~/Dealer/Orders.aspx",
                            false);

                        Context.ApplicationInstance.CompleteRequest();

                        return;
                    }
                }
                else
                {
                    LoadUnits(0);
                    BindLines();
                }
            }
        }


        // ============================================================
        // DROPDOWNS
        // ============================================================

        private void LoadShops()
        {
            string query = @"
                SELECT
                    ShopId,
                    Label = ShopCode + ' - ' + ShopName,
                    RouteScheduleId
                FROM Shops
                WHERE DealerId = @DealerId
                  AND IsActive = 1
                ORDER BY ShopName";

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


        private void LoadSchedules()
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
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        ddlSchedule.DataSource = dt;
                        ddlSchedule.DataTextField = "Label";
                        ddlSchedule.DataValueField =
                            "RouteScheduleId";
                        ddlSchedule.DataBind();
                    }
                }
            }

            ddlSchedule.Items.Insert(
                0,
                new ListItem("None", ""));
        }


        private void LoadSalesmen()
        {
            string query = @"
                SELECT
                    UserId,
                    Label = FullName
                            + ' ('
                            + Role
                            + ')'
                FROM Users
                WHERE Role IN ('Salesman', 'Driver')
                  AND IsActive = 1
                ORDER BY FullName";

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

                        ddlSalesman.DataSource = dt;
                        ddlSalesman.DataTextField = "Label";
                        ddlSalesman.DataValueField = "UserId";
                        ddlSalesman.DataBind();
                    }
                }
            }

            ddlSalesman.Items.Insert(
                0,
                new ListItem("None", ""));
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
            // force on the ORDER date. Without the APPLY this join would
            // return one row per historic price and the result arbitrary.
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
                        DataTable dt = new DataTable();
                        da.Fill(dt);

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
                }
            }
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

            int unitId = SelectedUnitId();

            int packets;
            decimal price;

            if (GetVariantUnitInfo(
                variantId,
                unitId,
                out packets,
                out price))
            {
                txtUnitPrice.Text =
                    price.ToString(
                        "0.00",
                        CultureInfo.InvariantCulture);

                lblLineInfo.Visible = true;
                lblLineInfo.Text =
                    "1 unit = <strong>" + packets +
                    "</strong> base packet(s).";
            }
            else
            {
                lblLineInfo.Visible = false;
            }
        }


        // Choosing a shop pulls in that shop's route schedule and
        // the salesman assigned to it.
        protected void ddlShop_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            int shopId = SelectedShopId();

            if (shopId <= 0)
            {
                return;
            }

            string query = @"
                SELECT
                    ISNULL(RouteScheduleId, 0)
                FROM Shops
                WHERE ShopId = @ShopId
                  AND DealerId = @DealerId";

            int scheduleId = 0;

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@ShopId",
                        SqlDbType.Int).Value = shopId;

                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    object result = cmd.ExecuteScalar();

                    if (result != null &&
                        result != DBNull.Value)
                    {
                        scheduleId = Convert.ToInt32(result);
                    }
                }
            }

            if (scheduleId > 0)
            {
                if (ddlSchedule.Items.FindByValue(
                    scheduleId.ToString()) != null)
                {
                    ddlSchedule.SelectedValue =
                        scheduleId.ToString();
                }

                int salesmanId = ScheduleSalesmanId(scheduleId);

                if (salesmanId > 0 &&
                    ddlSalesman.Items.FindByValue(
                        salesmanId.ToString()) != null)
                {
                    ddlSalesman.SelectedValue =
                        salesmanId.ToString();
                }
            }
        }


        private int ScheduleSalesmanId(int scheduleId)
        {
            string query = @"
                SELECT ISNULL(SalesmanId, 0)
                FROM RouteSchedules
                WHERE RouteScheduleId = @RouteScheduleId";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@RouteScheduleId",
                        SqlDbType.Int).Value = scheduleId;

                    object result = cmd.ExecuteScalar();

                    return result == null ||
                           result == DBNull.Value
                        ? 0
                        : Convert.ToInt32(result);
                }
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

            if (variantId <= 0)
            {
                ShowMessage(
                    "Select a product variant.",
                    false);

                return;
            }

            if (unitId <= 0)
            {
                ShowMessage(
                    "Select a selling unit.",
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
                existing["LineTotal"] =
                    newQty * price;
            }
            else
            {
                DataRow row = dt.NewRow();

                row["Index"] = dt.Rows.Count;
                row["ProductVariantId"] = variantId;
                row["ProductName"] =
                    ProductName(variantId);
                row["VariantName"] =
                    VariantName(variantId);
                row["UnitId"] = unitId;
                row["UnitCode"] = UnitCode(unitId);
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
            lblGrandTotal.Text =
                Math.Max(0m, subTotal - discount)
                    .ToString("N2");
        }


        private string ProductName(int variantId)
        {
            return ScalarText(@"
                SELECT p.ProductName
                FROM ProductVariants v
                INNER JOIN Products p
                    ON p.ProductId = v.ProductId
                WHERE v.ProductVariantId = @ProductVariantId",
                variantId);
        }


        private string VariantName(int variantId)
        {
            return ScalarText(@"
                SELECT VariantName
                FROM ProductVariants
                WHERE ProductVariantId = @ProductVariantId",
                variantId);
        }


        private string UnitCode(int unitId)
        {
            return ScalarText(@"
                SELECT UnitCode
                FROM SellingUnits
                WHERE UnitId = @UnitId",
                unitId);
        }


        private string ScalarText(
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
                        "@ProductVariantId",
                        SqlDbType.Int).Value = id;

                    cmd.Parameters.Add(
                        "@UnitId",
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
        // LOAD EXISTING ORDER
        // ============================================================

        private bool LoadOrder()
        {
            string query = @"
                SELECT
                    o.OrderNumber,
                    o.ShopId,
                    o.OrderType,
                    o.Status,
                    OrderDate =
                        CONVERT(NVARCHAR(10), o.OrderDate, 120),
                    DeliveryDate =
                        CASE
                            WHEN o.DeliveryDate IS NULL THEN ''
                            ELSE CONVERT(NVARCHAR(10), o.DeliveryDate, 120)
                        END,
                    RouteScheduleId =
                        ISNULL(o.RouteScheduleId, 0),
                    SalesmanId =
                        ISNULL(o.SalesmanId, 0),
                    o.Discount,
                    Remarks = ISNULL(o.Remarks, ''),
                    OrderSource =
                        ISNULL(o.OrderSource, '')
                FROM Orders o
                WHERE o.OrderId = @OrderId
                  AND o.DealerId = @DealerId";

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@OrderId",
                        SqlDbType.Int).Value = OrderId;

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

            if (dt.Rows.Count == 0)
            {
                return false;
            }

            DataRow row = dt.Rows[0];

            lblHeading.Text = "Edit Order";
            lblPageTitle.Text = "Edit Order";
            lblOrderNo.Text = row["OrderNumber"].ToString();

            SetSelected(ddlShop, row["ShopId"]);
            SetSelected(
                ddlSchedule,
                row["RouteScheduleId"]);
            SetSelected(
                ddlSalesman,
                row["SalesmanId"]);

            SelectByText(
                ddlOrderType,
                row["OrderType"].ToString());

            SelectByText(
                ddlStatus,
                row["Status"].ToString());

            // An order saved before the source column existed
            // falls back to Counter.
            if (string.IsNullOrEmpty(
                row["OrderSource"].ToString()))
            {
                ddlSource.SelectedValue = "Counter";
            }
            else
            {
                SelectByText(
                    ddlSource,
                    row["OrderSource"].ToString());
            }


            txtOrderDate.Text = row["OrderDate"].ToString();
            txtDeliveryDate.Text =
                row["DeliveryDate"].ToString();

            txtDiscount.Text =
                Convert.ToDecimal(row["Discount"])
                    .ToString(
                        "0.00",
                        CultureInfo.InvariantCulture);

            txtRemarks.Text = row["Remarks"].ToString();

            LoadOrderLines();
            BindLines();

            return true;
        }


        private void LoadOrderLines()
        {
            string query = @"
                SELECT
                    d.ProductVariantId,
                    d.UnitId,
                    d.Quantity,
                    d.QuantityPackets,
                    d.UnitPrice,
                    p.ProductName,
                    v.VariantName,
                    UnitCode = ISNULL(u.UnitCode, '')
                FROM OrderDetails d
                INNER JOIN ProductVariants v
                    ON v.ProductVariantId = d.ProductVariantId
                INNER JOIN Products p
                    ON p.ProductId = v.ProductId
                LEFT JOIN SellingUnits u
                    ON u.UnitId = d.UnitId
                WHERE d.OrderId = @OrderId
                ORDER BY d.OrderDetailId";

            DataTable loaded = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@OrderId",
                        SqlDbType.Int).Value = OrderId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(loaded);
                    }
                }
            }

            DataTable dt = NewLineTable();

            int i = 0;

            foreach (DataRow r in loaded.Rows)
            {
                DataRow row = dt.NewRow();

                row["Index"] = i++;
                row["ProductVariantId"] =
                    Convert.ToInt32(r["ProductVariantId"]);
                row["UnitId"] =
                    Convert.ToInt32(r["UnitId"]);
                row["Quantity"] =
                    Convert.ToInt32(r["Quantity"]);
                row["QuantityPackets"] =
                    Convert.ToInt32(r["QuantityPackets"]);
                row["UnitPrice"] =
                    Convert.ToDecimal(r["UnitPrice"]);
                row["LineTotal"] =
                    Convert.ToInt32(r["Quantity"]) *
                    Convert.ToDecimal(r["UnitPrice"]);
                row["ProductName"] =
                    r["ProductName"].ToString();
                row["VariantName"] =
                    r["VariantName"].ToString();
                row["UnitCode"] =
                    r["UnitCode"].ToString();

                dt.Rows.Add(row);
            }

            Lines = dt;
        }


        private void SetSelected(
            DropDownList list,
            object value)
        {
            string text =
                value == null || value == DBNull.Value
                    ? ""
                    : value.ToString();

            if (string.IsNullOrEmpty(text) ||
                text == "0")
            {
                list.SelectedValue = "";
                return;
            }

            if (list.Items.FindByValue(text) != null)
            {
                list.SelectedValue = text;
            }
        }


        private void SelectByText(
            DropDownList list,
            string text)
        {
            ListItem item =
                list.Items.FindByText(text);

            if (item != null)
            {
                list.SelectedValue = item.Value;
            }
        }


        // ============================================================
        // STATUS TRANSITIONS
        //
        //   Pending    -> Confirmed, Cancelled
        //   Confirmed  -> Dispatched, Cancelled
        //   Dispatched -> terminal (only a Returns record may follow)
        //   Cancelled  -> terminal
        //
        // Every status change this page performs goes through
        // IsTransitionAllowed; an illegal step is refused with a
        // message that names both statuses.
        // ============================================================

        private static bool IsTransitionAllowed(
            string from,
            string to)
        {
            string reason;

            return IsTransitionAllowed(from, to, out reason);
        }


        private static bool IsTransitionAllowed(
            string from,
            string to,
            out string reason)
        {
            reason = null;

            if (string.IsNullOrEmpty(from))
            {
                from = "Pending";
            }

            if (from == "Pending" &&
                (to == "Pending" ||
                 to == "Confirmed" ||
                 to == "Cancelled"))
            {
                return true;
            }

            if (from == "Confirmed" &&
                (to == "Confirmed" ||
                 to == "Dispatched" ||
                 to == "Cancelled"))
            {
                return true;
            }

            if (from == "Pending" && to == "Dispatched")
            {
                reason =
                    "A Pending order cannot go straight to " +
                    "Dispatched. Confirm it first.";

                return false;
            }

            if (from == "Dispatched" || from == "Cancelled")
            {
                reason =
                    "A " + from + " order is closed and cannot " +
                    "become " + to + ". " +
                    (from == "Dispatched"
                        ? "Record a return instead."
                        : "Create a new order instead.");

                return false;
            }

            reason =
                "A " + from + " order cannot become " + to + ".";

            return false;
        }


        // ============================================================
        // SAVE
        //
        // One transaction: upsert header, replace details, apply the
        // stock impact and write the ledger.
        //
        // Stock only leaves the godown when an order is Dispatched.
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

            int shopId = SelectedShopId();

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

            Dictionary<int, int> newPackets =
                new Dictionary<int, int>();

            foreach (DataRow row in lines.Rows)
            {
                subTotal +=
                    Convert.ToDecimal(row["LineTotal"]);

                int variantId =
                    Convert.ToInt32(row["ProductVariantId"]);

                int packets =
                    Convert.ToInt32(row["QuantityPackets"]);

                if (newPackets.ContainsKey(variantId))
                {
                    newPackets[variantId] += packets;
                }
                else
                {
                    newPackets[variantId] = packets;
                }
            }

            if (discount > subTotal)
            {
                ShowMessage(
                    "Discount cannot be greater than the sub total.",
                    false);

                return;
            }

            decimal grandTotal = subTotal - discount;

            string status = ddlStatus.SelectedValue;
            string orderType = ddlOrderType.SelectedValue;
            string source = ddlSource.SelectedValue;

            int scheduleId =
                string.IsNullOrEmpty(ddlSchedule.SelectedValue)
                    ? 0
                    : Convert.ToInt32(ddlSchedule.SelectedValue);

            int salesmanId =
                string.IsNullOrEmpty(ddlSalesman.SelectedValue)
                    ? 0
                    : Convert.ToInt32(ddlSalesman.SelectedValue);

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
                            string orderNumber;
                            string oldStatus = null;
                            Dictionary<int, int> oldPackets =
                                new Dictionary<int, int>();

                            bool editing = false;


                            // ----------------------------
                            // Read the previous state so a
                            // Dispatched <-> Pending switch
                            // can be reversed correctly.
                            // ----------------------------

                            if (OrderId > 0)
                            {
                                string read = @"
                                    SELECT OrderNumber, Status
                                    FROM Orders
                                    WHERE OrderId = @OrderId
                                      AND DealerId = @DealerId";

                                using (SqlCommand cmd =
                                    new SqlCommand(
                                        read,
                                        con,
                                        tx))
                                {
                                    cmd.Parameters.Add(
                                        "@OrderId",
                                        SqlDbType.Int).Value =
                                        OrderId;

                                    cmd.Parameters.Add(
                                        "@DealerId",
                                        SqlDbType.Int).Value =
                                        DealerId;

                                    using (SqlDataReader reader =
                                        cmd.ExecuteReader())
                                    {
                                        if (!reader.Read())
                                        {
                                            throw new InvalidOperationException(
                                                "Order not found.");
                                        }

                                        editing = true;
                                        orderNumber =
                                            reader["OrderNumber"]
                                                .ToString();
                                        oldStatus =
                                            reader["Status"]
                                                .ToString();
                                    }
                                }

                                string detailRead = @"
                                    SELECT
                                        ProductVariantId,
                                        SUM(QuantityPackets)
                                    FROM OrderDetails
                                    WHERE OrderId = @OrderId
                                    GROUP BY ProductVariantId";

                                using (SqlCommand cmd =
                                    new SqlCommand(
                                        detailRead,
                                        con,
                                        tx))
                                {
                                    cmd.Parameters.Add(
                                        "@OrderId",
                                        SqlDbType.Int).Value =
                                        OrderId;

                                    using (SqlDataReader reader =
                                        cmd.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            oldPackets[
                                                Convert.ToInt32(
                                                    reader[0])] =
                                                Convert.ToInt32(
                                                    reader[1]);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                orderNumber =
                                    NextOrderNumber(con, tx);
                            }


                            // ----------------------------
                            // Status rules
                            //
                            // The document is editable only while
                            // it is Pending or Confirmed, and the
                            // requested status must be a legal
                            // step in the transition table.
                            // Both are checked against the status
                            // read inside this transaction.
                            // ----------------------------

                            if (editing &&
                                oldStatus != "Pending" &&
                                oldStatus != "Confirmed")
                            {
                                throw new InvalidOperationException(
                                    oldStatus == "Dispatched"
                                        ? "A dispatched order can no longer be edited. Record a return instead."
                                        : "A cancelled order can no longer be edited. Create a new order instead.");
                            }

                            string fromStatus =
                                editing ? oldStatus : "Pending";

                            string reason;

                            if (!IsTransitionAllowed(
                                    fromStatus,
                                    status,
                                    out reason))
                            {
                                throw new InvalidOperationException(
                                    reason);
                            }


                            // ----------------------------
                            // Upsert the header
                            // ----------------------------

                            if (editing)
                            {
                                string update = @"
                                    UPDATE Orders
                                    SET ShopId = @ShopId,
                                        OrderType = @OrderType,
                                        OrderDate = @OrderDate,
                                        DeliveryDate = @DeliveryDate,
                                        RouteScheduleId = @RouteScheduleId,
                                        SalesmanId = @SalesmanId,
                                        Status = @Status,
                                        OrderSource = @OrderSource,
                                        SubTotal = @SubTotal,
                                        Discount = @Discount,
                                        GrandTotal = @GrandTotal,
                                        Remarks = @Remarks,
                                        UpdatedBy = @UpdatedBy,
                                        UpdatedAt = GETDATE()
                                    WHERE OrderId = @OrderId
                                      AND DealerId = @DealerId";

                                using (SqlCommand cmd =
                                    new SqlCommand(
                                        update,
                                        con,
                                        tx))
                                {
                                    AddOrderParameters(
                                        cmd,
                                        shopId,
                                        orderType,
                                        orderDate,
                                        deliveryDate,
                                        scheduleId,
                                        salesmanId,
                                        status,
                                        source,
                                        subTotal,
                                        discount,
                                        grandTotal,
                                        remarks);

                                    cmd.Parameters.Add(
                                        "@OrderId",
                                        SqlDbType.Int).Value =
                                        OrderId;

                                    cmd.Parameters.Add(
                                        "@DealerId",
                                        SqlDbType.Int).Value =
                                        DealerId;

                                    cmd.Parameters.Add(
                                        "@UpdatedBy",
                                        SqlDbType.Int).Value =
                                        DealerId;

                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
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
                                        @Status,
                                        @OrderSource,
                                        @SubTotal,
                                        @Discount,
                                        @GrandTotal,
                                        @Remarks,
                                        @CreatedBy,
                                        GETDATE()
                                    );
                                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                                using (SqlCommand cmd =
                                    new SqlCommand(
                                        insert,
                                        con,
                                        tx))
                                {
                                    AddOrderParameters(
                                        cmd,
                                        shopId,
                                        orderType,
                                        orderDate,
                                        deliveryDate,
                                        scheduleId,
                                        salesmanId,
                                        status,
                                        source,
                                        subTotal,
                                        discount,
                                        grandTotal,
                                        remarks);

                                    cmd.Parameters.Add(
                                        "@DealerId",
                                        SqlDbType.Int).Value =
                                        DealerId;

                                    cmd.Parameters.Add(
                                        "@OrderNumber",
                                        SqlDbType.NVarChar,
                                        100).Value =
                                        orderNumber;

                                    cmd.Parameters.Add(
                                        "@CreatedBy",
                                        SqlDbType.Int).Value =
                                        DealerId;

                                    OrderId = Convert.ToInt32(
                                        cmd.ExecuteScalar());
                                }

                                lblOrderNo.Text = orderNumber;
                            }


                            // ----------------------------
                            // Replace the details
                            // ----------------------------

                            string clear = @"
                                DELETE FROM OrderDetails
                                WHERE OrderId = @OrderId";

                            using (SqlCommand cmd =
                                new SqlCommand(
                                    clear,
                                    con,
                                    tx))
                            {
                                cmd.Parameters.Add(
                                    "@OrderId",
                                    SqlDbType.Int).Value =
                                    OrderId;

                                cmd.ExecuteNonQuery();
                            }

                            foreach (DataRow row in lines.Rows)
                            {
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
                                    new SqlCommand(
                                        detail,
                                        con,
                                        tx))
                                {
                                    cmd.Parameters.Add(
                                        "@OrderId",
                                        SqlDbType.Int).Value =
                                        OrderId;

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
                            // Stock impact
                            // ----------------------------

                            bool wasDispatched =
                                oldStatus == "Dispatched";

                            bool isDispatched =
                                status == "Dispatched";

                            Dictionary<int, int> changes =
                                new Dictionary<int, int>();

                            List<int> variants =
                                new List<int>();

                            foreach (int v in oldPackets.Keys)
                            {
                                variants.Add(v);
                            }

                            foreach (int v in newPackets.Keys)
                            {
                                if (!variants.Contains(v))
                                {
                                    variants.Add(v);
                                }
                            }

                            foreach (int v in variants)
                            {
                                int before =
                                    wasDispatched &&
                                    oldPackets.ContainsKey(v)
                                        ? oldPackets[v]
                                        : 0;

                                int after =
                                    isDispatched &&
                                    newPackets.ContainsKey(v)
                                        ? newPackets[v]
                                        : 0;

                                // Positive = packets coming back
                                // to the godown.
                                int change = before - after;

                                if (change != 0)
                                {
                                    changes[v] = change;
                                }
                            }

                            if (changes.Count > 0)
                            {
                                ApplyStockChanges(
                                    con,
                                    tx,
                                    changes,
                                    orderNumber,
                                    status);
                            }

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
                    "~/Dealer/Orders.aspx?saved=1");
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error saving order: " + ex.Message,
                    false);
            }
        }


        private void AddOrderParameters(
            SqlCommand cmd,
            int shopId,
            string orderType,
            DateTime orderDate,
            DateTime? deliveryDate,
            int scheduleId,
            int salesmanId,
            string status,
            string source,
            decimal subTotal,
            decimal discount,
            decimal grandTotal,
            string remarks)
        {
            cmd.Parameters.Add(
                "@ShopId",
                SqlDbType.Int).Value = shopId;

            cmd.Parameters.Add(
                "@OrderType",
                SqlDbType.NVarChar,
                60).Value = orderType;

            cmd.Parameters.Add(
                "@OrderDate",
                SqlDbType.Date).Value = orderDate;

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
                salesmanId > 0
                    ? (object)salesmanId
                    : DBNull.Value;

            cmd.Parameters.Add(
                "@Status",
                SqlDbType.NVarChar,
                60).Value = status;

            cmd.Parameters.Add(
                "@OrderSource",
                SqlDbType.NVarChar,
                20).Value = source;

            cmd.Parameters.Add(
                "@SubTotal",
                SqlDbType.Decimal).Value = subTotal;

            cmd.Parameters.Add(
                "@Discount",
                SqlDbType.Decimal).Value = discount;

            cmd.Parameters.Add(
                "@GrandTotal",
                SqlDbType.Decimal).Value = grandTotal;

            cmd.Parameters.Add(
                "@Remarks",
                SqlDbType.NVarChar,
                1000).Value =
                remarks == null
                    ? (object)DBNull.Value
                    : remarks;
        }


        // ============================================================
        // STOCK IMPACT
        // ============================================================

        private void ApplyStockChanges(
            SqlConnection con,
            SqlTransaction tx,
            Dictionary<int, int> changes,
            string orderNumber,
            string status)
        {
            List<LedgerLine> ledger =
                new List<LedgerLine>();

            foreach (KeyValuePair<int, int> pair in changes)
            {
                int variantId = pair.Key;
                int change = pair.Value;

                string label = VariantLabel(
                    con,
                    tx,
                    variantId);

                // UPDLOCK holds this row until the transaction
                // ends, so the balance read here is the balance
                // the write below acts on: two saves racing for
                // the same variant queue up on the lock instead
                // of both passing the sufficiency check on a
                // stale value.
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

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            current = Convert.ToInt32(reader[0]);
                        }
                    }
                }

                int after = current + change;

                if (after < 0)
                {
                    throw new InvalidOperationException(
                        "Not enough stock for " + label +
                        ": only " + current +
                        " packet(s) available, the order needs " +
                        Math.Abs(change) + ".");
                }

                // The write is relative and carries its own
                // sufficiency guard, so a row that appears or
                // moves between the read above and this statement
                // can never take the balance below zero packets.
                if (ConditionalStockUpdate(
                    con,
                    tx,
                    variantId,
                    change))
                {
                    ledger.Add(
                        new LedgerLine(
                            variantId,
                            change,
                            label));

                    continue;
                }

                // 0 rows: either there is no Inventory row yet or
                // the guard refused the movement. Re-read under
                // the same lock to tell the two apart.
                if (StockRowExists(con, tx, variantId))
                {
                    throw new InvalidOperationException(
                        "Not enough stock for " + label +
                        ": only " + current +
                        " packet(s) available, the order needs " +
                        Math.Abs(change) + ".");
                }

                // No row: the variant has never been stocked for
                // this dealer, so only a stock-in can be written
                // (a stock-out was already refused above).
                try
                {
                    InsertStock(
                        con,
                        tx,
                        variantId,
                        current + change);
                }
                catch (SqlException ex)
                {
                    // UQ_Inventory_Dealer_Variant fired because a
                    // concurrent save created the row first:
                    // fall back to exactly one retry of the
                    // conditional update.
                    if (ex.Number != 2601 && ex.Number != 2627)
                    {
                        throw;
                    }

                    if (!ConditionalStockUpdate(
                        con,
                        tx,
                        variantId,
                        change))
                    {
                        throw new InvalidOperationException(
                            "Not enough stock for " + label +
                            ": only " + current +
                            " packet(s) available, the order needs " +
                            Math.Abs(change) + ".");
                    }
                }

                ledger.Add(
                    new LedgerLine(
                        variantId,
                        change,
                        label));
            }

            WriteLedger(
                con,
                tx,
                ledger,
                orderNumber,
                status);
        }


        // Relative write with the guard inside the statement:
        // returns false when the row is missing or when the
        // movement would take the balance below zero.
        private bool ConditionalStockUpdate(
            SqlConnection con,
            SqlTransaction tx,
            int variantId,
            int delta)
        {
            string write = @"
                UPDATE Inventory
                SET QuantityPackets = QuantityPackets + @Delta,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE DealerId = @DealerId
                  AND ProductVariantId = @ProductVariantId
                  AND QuantityPackets + @Delta >= 0";

            using (SqlCommand cmd =
                new SqlCommand(write, con, tx))
            {
                cmd.Parameters.Add(
                    "@Delta",
                    SqlDbType.Int).Value = delta;

                cmd.Parameters.Add(
                    "@UpdatedBy",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                return cmd.ExecuteNonQuery() > 0;
            }
        }


        // Re-read with the same lock the update uses, so the
        // "no row" and "guard refused" cases cannot be confused.
        private bool StockRowExists(
            SqlConnection con,
            SqlTransaction tx,
            int variantId)
        {
            string read = @"
                SELECT COUNT(*)
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

                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }


        private void InsertStock(
            SqlConnection con,
            SqlTransaction tx,
            int variantId,
            int quantity)
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
                new SqlCommand(insert, con, tx))
            {
                ins.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                ins.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                ins.Parameters.Add(
                    "@QuantityPackets",
                    SqlDbType.Int).Value = quantity;

                ins.Parameters.Add(
                    "@UpdatedBy",
                    SqlDbType.Int).Value = DealerId;

                ins.ExecuteNonQuery();
            }
        }


        private sealed class LedgerLine
        {
            public LedgerLine(
                int variantId,
                int change,
                string label)
            {
                VariantId = variantId;
                Change = change;
                Label = label;
            }

            public int VariantId { get; private set; }
            public int Change { get; private set; }
            public string Label { get; private set; }
        }


        // Positive change = packets returning to the godown.
        // One header is written per direction so the ledger stays
        // unambiguous.
        private void WriteLedger(
            SqlConnection con,
            SqlTransaction tx,
            List<LedgerLine> lines,
            string orderNumber,
            string status)
        {
            WriteLedgerGroup(
                con,
                tx,
                lines,
                true,
                orderNumber,
                status);

            WriteLedgerGroup(
                con,
                tx,
                lines,
                false,
                orderNumber,
                status);
        }


        private void WriteLedgerGroup(
            SqlConnection con,
            SqlTransaction tx,
            List<LedgerLine> lines,
            bool positive,
            string orderNumber,
            string status)
        {
            string type = positive ? "Stock In" : "Stock Out";

            bool any = false;

            foreach (LedgerLine line in lines)
            {
                if ((line.Change > 0) == positive)
                {
                    any = true;
                    break;
                }
            }

            if (!any)
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
                    80).Value = type;

                cmd.Parameters.Add(
                    "@ReferenceNo",
                    SqlDbType.NVarChar,
                    200).Value = orderNumber;

                cmd.Parameters.Add(
                    "@Remarks",
                    SqlDbType.NVarChar,
                    1000).Value =
                    "Order " + orderNumber +
                    " (" + status + ")";

                cmd.Parameters.Add(
                    "@CreatedBy",
                    SqlDbType.Int).Value = DealerId;

                transactionId =
                    Convert.ToInt32(cmd.ExecuteScalar());
            }

            int unitId = PacketUnitId(con, tx);

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

            foreach (LedgerLine line in lines)
            {
                if ((line.Change > 0) != positive)
                {
                    continue;
                }

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
                        line.VariantId;

                    cmd.Parameters.Add(
                        "@UnitId",
                        SqlDbType.Int).Value = unitId;

                    // Stored as the signed stock movement, so
                    // Stock Out rows carry a negative quantity.
                    cmd.Parameters.Add(
                        "@Quantity",
                        SqlDbType.Int).Value =
                        line.Change;

                    cmd.Parameters.Add(
                        "@QuantityPackets",
                        SqlDbType.Int).Value =
                        line.Change;

                    cmd.ExecuteNonQuery();
                }
            }
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

        private int SelectedShopId()
        {
            int id = 0;
            int.TryParse(ddlShop.SelectedValue, out id);
            return id;
        }

        // The price that applies is the one in force on the order date; an
        // unreadable date falls back to today so the lookup still works.
        private DateTime OrderDateForPricing()
        {
            DateTime date;

            string raw = txtOrderDate == null
                ? ""
                : txtOrderDate.Text.Trim();

            if (DateTime.TryParse(
                raw,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date))
            {
                return date;
            }

            if (DateTime.TryParse(raw, out date))
            {
                return date;
            }

            return DateTime.Today;
        }


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

        private string NextOrderNumber(
            SqlConnection con,
            SqlTransaction tx)
        {
            string prefix =
                "ORD-" +
                DateTime.Now.ToString("yyyyMMdd-HHmmss");

            string candidate = prefix;
            int suffix = 2;

            while (OrderNumberExists(con, tx, candidate))
            {
                candidate = prefix + "-" + suffix++;
            }

            return candidate;
        }

        private bool OrderNumberExists(
            SqlConnection con,
            SqlTransaction tx,
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
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@OrderNumber",
                    SqlDbType.NVarChar,
                    100).Value = number;

                return Convert.ToInt32(
                    cmd.ExecuteScalar()) > 0;
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
