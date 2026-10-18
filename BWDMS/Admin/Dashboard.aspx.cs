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
            LoadProductOverview();
            LoadRecentDealers();

            // Recent Dealer Activity is hidden on the page but kept
            // for later; its loader is commented out with the rest of
            // the feature below.
            //
            // LoadRecentActivity();
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
        // PRODUCT OVERVIEW - ACTIVE PRODUCTS BY CATEGORY
        //
        // Feeds the donut chart and the colour-coded legend on the
        // right of it. Categories with no active product are left
        // out of the chart but the query still returns them, so the
        // zero rows are dropped here.
        // ============================================================

        private static readonly string[] OverviewColors =
        {
            "#4361ee",
            "#2a9d8f",
            "#f77f00",
            "#e63946",
            "#7209b7",
            "#06d6a0",
            "#118ab2",
            "#ff8fab"
        };


        protected string ProductOverviewJson { get; private set; } = "[]";


        private void LoadProductOverview()
        {
            string query = @"
                SELECT
                    CategoryName =
                        ISNULL(pc.CategoryName, 'Uncategorised'),
                    ProductCount =
                        COUNT(p.ProductId)
                FROM ProductCategories pc
                LEFT JOIN Products p
                    ON p.CategoryId = pc.CategoryId
                   AND p.IsActive = 1
                GROUP BY ISNULL(pc.CategoryName, 'Uncategorised')
                HAVING COUNT(p.ProductId) > 0
                ORDER BY ProductCount DESC, CategoryName ASC";

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

            int total = 0;

            foreach (DataRow row in dt.Rows)
            {
                total += Convert.ToInt32(row["ProductCount"]);
            }

            if (dt.Rows.Count == 0)
            {
                // Products may still exist with no category row at
                // all - show them rather than an empty circle.
                string orphanQuery = @"
                    SELECT
                        CategoryName = 'Uncategorised',
                        ProductCount = COUNT(*)
                    FROM Products
                    WHERE IsActive = 1";

                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    con.Open();

                    using (SqlCommand cmd =
                        new SqlCommand(orphanQuery, con))
                    {
                        int orphan =
                            Convert.ToInt32(cmd.ExecuteScalar());

                        if (orphan == 0)
                        {
                            divOverview.Visible = false;

                            lblOverviewMessage.Text =
                                "No active products yet.";

                            lblOverviewMessage.Visible = true;

                            ProductOverviewJson = "[]";

                            return;
                        }

                        dt.Rows.Add("Uncategorised", orphan);

                        total = orphan;
                    }
                }
            }

            if (!dt.Columns.Contains("Color"))
            {
                dt.Columns.Add("Color", typeof(string));
                dt.Columns.Add("CountText", typeof(string));
                dt.Columns.Add("PercentText", typeof(string));
            }

            System.Text.StringBuilder json =
                new System.Text.StringBuilder();

            json.Append("[");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];

                int count =
                    Convert.ToInt32(row["ProductCount"]);

                string color =
                    OverviewColors[
                        i % OverviewColors.Length];

                double percent =
                    total > 0
                        ? Math.Round(
                            count * 100.0 / total,
                            1)
                        : 0;

                row["Color"] = color;

                row["CountText"] = count.ToString();

                row["PercentText"] =
                    percent.ToString("0.#") + "%";

                if (i > 0)
                {
                    json.Append(",");
                }

                json.Append("{\"label\":\"")
                    .Append(EscapeJson(
                        row["CategoryName"].ToString()))
                    .Append("\",\"value\":")
                    .Append(count)
                    .Append(",\"color\":\"")
                    .Append(color)
                    .Append("\"}");
            }

            json.Append("]");

            ProductOverviewJson = json.ToString();

            lblOverviewTotal.Text = total.ToString();

            rptOverview.DataSource = dt;
            rptOverview.DataBind();
        }


        // The chart script reads this string as raw JavaScript, so
        // quotes and backslashes in a category name have to go.
        private static string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "")
                .Replace("\n", " ");
        }


        // ============================================================
        // RECENT DEALERS
        //
        // The newest dealer accounts. A dealer is a Users row and has
        // no location column of its own, so the district of the
        // dealer's first shop village stands in for it - a dealer
        // without a shop yet shows a dash.
        // ============================================================

        private void LoadRecentDealers()
        {
            string query = @"
                SELECT TOP (5)
                    DealerName = ISNULL(u.FullName, '-'),
                    Location = ISNULL(
                        (SELECT TOP (1) v.District
                         FROM Shops sh
                         INNER JOIN Villages v
                             ON v.VillageId = sh.VillageId
                         WHERE sh.DealerId = u.UserId
                           AND ISNULL(v.District, '') <> ''
                         ORDER BY sh.ShopId),
                        '-'),
                    u.IsActive,
                    JoinedOn =
                        CONVERT(NVARCHAR(11), u.CreatedAt, 106)
                FROM Users u
                WHERE u.Role = 'Dealer'
                ORDER BY u.CreatedAt DESC, u.UserId DESC";

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

            gvRecentDealers.DataSource = dt;
            gvRecentDealers.DataBind();
        }


        /* ============================================================
           RECENT DEALER ACTIVITY - HIDDEN, NOT DELETED.
           The panel was taken off the dashboard on request. The loader
           and its pager handler are kept here so the feature can be
           restored later; the matching markup and the call inside
           LoadDashboard are commented out in the same way.

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

        // ------------------------------------------------------------
        // PAGING
        //
        // The new index has to be applied before the grid is rebound,
        // otherwise the postback lands back on the page it came from.
        // ------------------------------------------------------------

        protected void gvRecentActivity_PageIndexChanging(
            object sender,
            System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            gvRecentActivity.PageIndex = e.NewPageIndex;

            LoadRecentActivity();
        }

        ============ end of hidden Recent Dealer Activity ============ */

    }
}
