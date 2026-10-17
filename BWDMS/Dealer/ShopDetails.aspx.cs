using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    // ============================================================
    // SHOP DETAILS
    //
    // ~/Dealer/ShopDetails.aspx?id=N shows one shop of the logged-in
    // dealer together with a summary and its recent orders. The shop
    // is always re-checked against the logged-in dealer, so a forged
    // ?id= can never expose another dealership's shop.
    // ============================================================

    public partial class ShopDetails : System.Web.UI.Page
    {
        // Dealer's own id lives in UserId (there is no Session["DealerId"]).
        private int DealerId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }

        // ?id=<ShopId>; 0 when the value is missing or malformed.
        private int ShopId
        {
            get
            {
                int id;

                if (int.TryParse(Request.QueryString["id"], out id))
                {
                    return id;
                }

                return 0;
            }
        }

        // True only after the shop has been proven to belong to
        // this dealer. No other query runs while it is false.
        private bool shopLoaded;


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


            if (ShopId <= 0)
            {
                Response.Redirect("~/Dealer/Shops.aspx");

                return;
            }

            shopLoaded = LoadShopHeader();

            if (!shopLoaded)
            {
                Response.Redirect("~/Dealer/Shops.aspx");

                return;
            }


            if (!IsPostBack)
            {
                LoadSummary();

                LoadOrders();
            }
        }


        // ============================================================
        // OWNERSHIP CHECK - runs before anything else is queried
        // ============================================================

        private bool LoadShopHeader()
        {
            string query = @"
                SELECT
                    s.ShopName,
                    s.ShopCode,
                    s.OwnerName,
                    s.Phone,
                    s.Email,
                    s.Address,
                    s.OpeningBalance,
                    s.CreditLimit,
                    s.IsActive,
                    s.CreatedAt,
                    VillageName = ISNULL(v.VillageName, '-'),
                    RouteName = ISNULL(
                        CASE
                            WHEN r.RouteCode IS NULL
                                 OR r.RouteCode = '' THEN r.RouteName
                            ELSE r.RouteCode + ' - ' + r.RouteName
                        END, '-')
                FROM Shops s
                LEFT JOIN Villages v
                    ON v.VillageId = s.VillageId
                LEFT JOIN Routes r
                    ON r.RouteId = s.RouteId
                WHERE s.ShopId = @ShopId
                  AND s.DealerId = @DealerId";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@ShopId",
                        SqlDbType.Int).Value = ShopId;

                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return false;
                        }

                        lblShopName.Text =
                            reader["ShopName"].ToString();

                        bool isActive =
                            Convert.ToBoolean(reader["IsActive"]);

                        lblStatus.Text =
                            isActive ? "Active" : "Inactive";

                        lblStatus.CssClass = isActive
                            ? "badge bg-success"
                            : "badge bg-secondary";

                        lblShopCode.Text =
                            TextOrDash(reader["ShopCode"]);

                        lblOwnerName.Text =
                            TextOrDash(reader["OwnerName"]);

                        lblPhone.Text =
                            TextOrDash(reader["Phone"]);

                        lblEmail.Text =
                            TextOrDash(reader["Email"]);

                        lblAddress.Text =
                            TextOrDash(reader["Address"]);

                        lblVillage.Text =
                            reader["VillageName"].ToString();

                        lblRoute.Text =
                            reader["RouteName"].ToString();

                        lblOpeningBalance.Text =
                            Convert.ToDecimal(
                                reader["OpeningBalance"])
                                .ToString("N2");

                        lblCreditLimit.Text =
                            reader["CreditLimit"] == DBNull.Value
                                ? "-"
                                : Convert.ToDecimal(
                                      reader["CreditLimit"])
                                      .ToString("N2");

                        lblCreatedAt.Text =
                            Convert.ToDateTime(
                                reader["CreatedAt"])
                                .ToString("dd-MM-yyyy");

                        return true;
                    }
                }
            }
        }


        // ============================================================
        // SHOP SUMMARY - only real aggregates, no payments table
        // exists in this schema, so there is no "Outstanding" or
        // "Total Payments" tile.
        // ============================================================

        private void LoadSummary()
        {
            if (!shopLoaded)
            {
                return;
            }

            DateTime monthStart =
                new DateTime(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1);

            DateTime monthEnd = monthStart.AddMonths(1);

            string query = @"
                SELECT
                    TotalOrders =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM Orders o
                             WHERE o.ShopId = @ShopId
                               AND o.DealerId = @DealerId),
                            0),
                    TotalSales =
                        ISNULL(
                            (SELECT SUM(o.GrandTotal)
                             FROM Orders o
                             WHERE o.ShopId = @ShopId
                               AND o.DealerId = @DealerId
                               AND o.Status NOT IN
                                   ('Cancelled', 'Returned')),
                            0),
                    MonthSales =
                        ISNULL(
                            (SELECT SUM(o.GrandTotal)
                             FROM Orders o
                             WHERE o.ShopId = @ShopId
                               AND o.DealerId = @DealerId
                               AND o.Status NOT IN
                                   ('Cancelled', 'Returned')
                               AND o.OrderDate >= @From
                               AND o.OrderDate < @To),
                            0),
                    ReturnOrders =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM SalesReturns sr
                             WHERE sr.ShopId = @ShopId
                               AND sr.DealerId = @DealerId),
                            0)";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@ShopId",
                        SqlDbType.Int).Value = ShopId;

                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    cmd.Parameters.Add(
                        "@From",
                        SqlDbType.Date).Value = monthStart;

                    cmd.Parameters.Add(
                        "@To",
                        SqlDbType.Date).Value = monthEnd;

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return;
                        }

                        lblTotalOrders.Text =
                            Convert.ToInt32(
                                reader["TotalOrders"])
                                .ToString();

                        lblTotalSales.Text =
                            Convert.ToDecimal(
                                reader["TotalSales"])
                                .ToString("N2");

                        lblMonthSales.Text =
                            Convert.ToDecimal(
                                reader["MonthSales"])
                                .ToString("N2");

                        lblReturnOrders.Text =
                            Convert.ToInt32(
                                reader["ReturnOrders"])
                                .ToString();
                    }
                }
            }
        }


        // ============================================================
        // RECENT ORDERS
        // ============================================================

        private void LoadOrders()
        {
            if (!shopLoaded)
            {
                return;
            }

            string query = @"
                SELECT
                    o.OrderNumber,
                    OrderDateText =
                        CONVERT(NVARCHAR(10), o.OrderDate, 120),
                    o.GrandTotal,
                    o.Status
                FROM Orders o
                WHERE o.ShopId = @ShopId
                  AND o.DealerId = @DealerId
                ORDER BY o.OrderDate DESC, o.OrderId DESC";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@ShopId",
                        SqlDbType.Int).Value = ShopId;

                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        gvOrders.DataSource = dt;
                        gvOrders.DataBind();
                    }
                }
            }
        }


        private static string TextOrDash(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return "-";
            }

            string text = value.ToString();

            return string.IsNullOrWhiteSpace(text) ? "-" : text;
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

        // ------------------------------------------------------------
        // PAGING
        //
        // The new index has to be applied before the grid is rebound,
        // otherwise the postback lands back on the page it came from.
        // ------------------------------------------------------------

        protected void gvOrders_PageIndexChanging(
            object sender,
            System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            gvOrders.PageIndex = e.NewPageIndex;

            LoadOrders();
        }

    }
}
