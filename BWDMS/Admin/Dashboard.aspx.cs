using System;
using System.Data;
using System.Data.SqlClient;
using BWDMS.Data;

namespace BWDMS.Admin
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // ==========================================
            // PREVENT BROWSER CACHE
            // ==========================================

            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            Response.Cache.SetExpires(
                DateTime.UtcNow.AddYears(-1));

            Response.Cache.SetRevalidation(
                System.Web.HttpCacheRevalidation.AllCaches);


            // ==========================================
            // CHECK LOGIN
            // ==========================================

            if (Session["UserId"] == null)
            {
                Response.Redirect(
                    "~/Account/Login.aspx",
                    false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }


            // ==========================================
            // CHECK ADMIN ROLE
            // ==========================================

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


            if (!IsPostBack)
            {
                LoadDashboard();
            }
        }


        // ============================================================
        // DASHBOARD
        // ============================================================

        private void LoadDashboard()
        {
            LoadCounts();
            LoadStatusSummary();
            LoadRecentActivity();
        }


        // ============================================================
        // HEADLINE COUNTS
        //
        // The admin is the only role that reads unscoped data.
        // ============================================================

        private void LoadCounts()
        {
            string query = @"
                SELECT
                    DealerCount =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM Users
                             WHERE Role = 'Dealer'
                               AND IsActive = 1),
                            0),
                    CategoryCount =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM ProductCategories),
                            0),
                    ProductCount =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM Products
                             WHERE IsActive = 1),
                            0),
                    OrderCount =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM Orders),
                            0)";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblDealerCount.Text =
                                reader["DealerCount"].ToString();

                            lblCategoryCount.Text =
                                reader["CategoryCount"].ToString();

                            lblProductCount.Text =
                                reader["ProductCount"].ToString();

                            lblOrderCount.Text =
                                reader["OrderCount"].ToString();
                        }
                    }
                }
            }
        }


        // ============================================================
        // ORDERS BY STATUS - ALL DEALERS
        // ============================================================

        private void LoadStatusSummary()
        {
            string query = @"
                SELECT
                    x.Status,
                    OrderCount =
                        ISNULL(COUNT(o.OrderId), 0),
                    Amount =
                        ISNULL(SUM(o.GrandTotal), 0)
                FROM
                (
                    SELECT Status = 'Pending', SortOrder = 1
                    UNION ALL SELECT 'Confirmed', 2
                    UNION ALL SELECT 'Dispatched', 3
                    UNION ALL SELECT 'Cancelled', 4
                ) x
                LEFT JOIN Orders o
                    ON o.Status = x.Status
                GROUP BY x.Status, x.SortOrder
                ORDER BY x.SortOrder";

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            if (!dt.Columns.Contains("StatusCss"))
            {
                dt.Columns.Add("StatusCss", typeof(string));
                dt.Columns.Add("AmountText", typeof(string));
            }

            int totalOrders = 0;
            decimal totalAmount = 0m;

            foreach (DataRow row in dt.Rows)
            {
                row["StatusCss"] =
                    BWDMS.Dealer.Orders.StatusCss(
                        row["Status"].ToString());

                row["AmountText"] =
                    Convert.ToDecimal(row["Amount"])
                        .ToString("N2");

                totalOrders +=
                    Convert.ToInt32(row["OrderCount"]);

                totalAmount +=
                    Convert.ToDecimal(row["Amount"]);
            }

            lblStatusTotal.Text =
                totalOrders + " orders  |  " +
                totalAmount.ToString("N2");

            gvStatusSummary.DataSource = dt;
            gvStatusSummary.DataBind();
        }


        // ============================================================
        // RECENT DEALER ACTIVITY
        //
        // Latest records by CreatedAt from Orders, a dealer-owned
        // table, with the dealership that produced them.
        // ============================================================

        private void LoadRecentActivity()
        {
            string query = @"
                SELECT TOP (8)
                    CreatedAtText =
                        CONVERT(NVARCHAR(16), o.CreatedAt, 120),
                    DealerName = ISNULL(u.FullName, '-'),
                    o.OrderNumber,
                    ShopName = ISNULL(s.ShopName, '-'),
                    o.GrandTotal,
                    o.Status
                FROM Orders o
                LEFT JOIN Users u
                    ON u.UserId = o.DealerId
                   AND u.Role = 'Dealer'
                LEFT JOIN Shops s
                    ON s.ShopId = o.ShopId
                ORDER BY o.CreatedAt DESC, o.OrderId DESC";

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            if (!dt.Columns.Contains("StatusCss"))
            {
                dt.Columns.Add("StatusCss", typeof(string));
                dt.Columns.Add("GrandTotalText", typeof(string));
            }

            foreach (DataRow row in dt.Rows)
            {
                row["StatusCss"] =
                    BWDMS.Dealer.Orders.StatusCss(
                        row["Status"].ToString());

                row["GrandTotalText"] =
                    Convert.ToDecimal(row["GrandTotal"])
                        .ToString("N2");
            }

            gvRecentActivity.DataSource = dt;
            gvRecentActivity.DataBind();
        }
    }
}
