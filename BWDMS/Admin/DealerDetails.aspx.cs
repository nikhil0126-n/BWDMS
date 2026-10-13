using System;
using System.Data;
using System.Data.SqlClient;
using BWDMS.Data;

namespace BWDMS.Admin
{
    // ================================================================
    // Admin > Dealer Details
    //
    // One dealer, everything about them on a single page:
    //   - account card (owner, contact, location, join date)
    //   - Overview: salesman count, shop count, orders this month and
    //     total sales
    //   - Recent Orders
    //
    // The dealer id comes from ?id=. Everything on the page is scoped
    // to that one dealership, so two dealers' numbers never mix.
    // ================================================================

    public partial class DealerDetails : System.Web.UI.Page
    {
        private int DealerId
        {
            get { return _dealerId; }
        }

        private int _dealerId;


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
                    "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect(
                    BWDMS.Data.AppAuth.HomeUrl(
                        Session["UserRole"]),
                    false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }


            int.TryParse(
                Request.QueryString["id"],
                out _dealerId);


            if (!IsPostBack)
            {
                if (!LoadDealer())
                {
                    return;
                }

                LoadOverview();
                LoadRecentOrders();
            }
        }


        // ============================================================
        // ACCOUNT CARD
        //
        // Returns false when the id is missing or is not a dealer,
        // and shows the message instead of the details panel.
        // ============================================================

        private bool LoadDealer()
        {
            if (DealerId <= 0)
            {
                return ShowError("Dealer not found.");
            }

            // Location comes from the first shop the dealer has, so
            // the same dealer is never shown twice for the same id.
            string query = @"
                SELECT TOP (1)
                    u.FullName,
                    u.Email,
                    u.Phone,
                    u.IsActive,
                    u.CreatedAt,
                    Location =
                    (
                        SELECT TOP (1)
                            Location =
                                ISNULL(v.VillageName, '')
                                + CASE
                                    WHEN v.Taluka IS NULL
                                        THEN ''
                                    ELSE ', ' + v.Taluka
                                  END
                        FROM Shops s
                        LEFT JOIN Villages v
                            ON v.VillageId = s.VillageId
                        WHERE s.DealerId = u.UserId
                        ORDER BY s.ShopId
                    )
                FROM Users u
                WHERE u.UserId = @UserId
                  AND u.Role = 'Dealer'";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@UserId",
                        SqlDbType.Int).Value = DealerId;

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return ShowError(
                                "Dealer not found.");
                        }

                        string fullName =
                            reader["FullName"].ToString();

                        lblDealerName.Text = fullName;

                        // The account name doubles as the owner.
                        lblOwner.Text = fullName;

                        lblEmail.Text =
                            string.IsNullOrWhiteSpace(
                                reader["Email"].ToString())
                                ? "-"
                                : reader["Email"].ToString();

                        lblPhone.Text =
                            string.IsNullOrWhiteSpace(
                                reader["Phone"].ToString())
                                ? "-"
                                : reader["Phone"].ToString();

                        string location =
                            reader["Location"].ToString();

                        lblLocation.Text =
                            string.IsNullOrWhiteSpace(location)
                                ? "-"
                                : location;

                        lblJoined.Text =
                            Convert.ToDateTime(
                                reader["CreatedAt"])
                                .ToString("dd MMM yyyy");

                        bool active =
                            Convert.ToBoolean(
                                reader["IsActive"]);

                        lblStatusBadge.Text =
                            active
                                ? "Active"
                                : "Inactive";

                        lblStatusBadge.CssClass =
                            active
                                ? "badge bg-success"
                                : "badge bg-secondary";

                        lnkEditDealer.NavigateUrl =
                            "AddDealer.aspx?id=" + DealerId;
                    }
                }
            }

            return true;
        }


        private bool ShowError(string message)
        {
            pnlDetails.Visible = false;

            lblMessage.Text = message;

            lblMessage.CssClass =
                "alert alert-danger d-block";

            lblMessage.Visible = true;

            return false;
        }


        // ============================================================
        // OVERVIEW STATISTICS
        //
        // All four numbers come back in one round trip.
        // ============================================================

        private void LoadOverview()
        {
            string query = @"
                SELECT
                    SalesmanCount =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM Users
                             WHERE Role = 'Salesman'
                               AND DealerId = @DealerId),
                            0),
                    ShopCount =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM Shops
                             WHERE DealerId = @DealerId
                               AND IsActive = 1),
                            0),
                    OrdersThisMonth =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM Orders
                             WHERE DealerId = @DealerId
                               AND MONTH(CreatedAt) =
                                   MONTH(GETDATE())
                               AND YEAR(CreatedAt) =
                                   YEAR(GETDATE())),
                            0),
                    TotalSales =
                        ISNULL(
                            (SELECT SUM(GrandTotal)
                             FROM Orders
                             WHERE DealerId = @DealerId
                               AND Status <> 'Cancelled'),
                            0)";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblSalesmanCount.Text =
                                reader["SalesmanCount"]
                                    .ToString();

                            lblShopCount.Text =
                                reader["ShopCount"]
                                    .ToString();

                            lblOrdersThisMonth.Text =
                                reader["OrdersThisMonth"]
                                    .ToString();

                            lblTotalSales.Text =
                                Convert.ToDecimal(
                                    reader["TotalSales"])
                                    .ToString("N2");
                        }
                    }
                }
            }
        }


        // ============================================================
        // RECENT ORDERS
        // ============================================================

        private void LoadRecentOrders()
        {
            string query = @"
                SELECT TOP (20)
                    o.OrderNumber,
                    DateText =
                        CONVERT(
                            NVARCHAR(10),
                            o.CreatedAt,
                            103),
                    ShopName = ISNULL(s.ShopName, '-'),
                    o.GrandTotal,
                    o.Status
                FROM Orders o
                LEFT JOIN Shops s
                    ON s.ShopId = o.ShopId
                WHERE o.DealerId = @DealerId
                ORDER BY o.CreatedAt DESC, o.OrderId DESC";

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

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

            if (!dt.Columns.Contains("StatusCss"))
            {
                dt.Columns.Add(
                    "StatusCss",
                    typeof(string));

                dt.Columns.Add(
                    "GrandTotalText",
                    typeof(string));
            }

            foreach (DataRow row in dt.Rows)
            {
                row["StatusCss"] =
                    BWDMS.Dealer.Orders.StatusCss(
                        row["Status"].ToString());

                row["GrandTotalText"] =
                    "Rs. " +
                    Convert.ToDecimal(row["GrandTotal"])
                        .ToString("N2");
            }

            gvRecentOrders.DataSource = dt;
            gvRecentOrders.DataBind();
        }


        protected void gvRecentOrders_PageIndexChanging(
            object sender,
            System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            gvRecentOrders.PageIndex = e.NewPageIndex;

            LoadRecentOrders();
        }
    }
}
