using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    // ============================================================
    // DEALER DASHBOARD
    //
    // Every figure on this page is a live SQL aggregate scoped to
    // the logged-in dealer (Session["UserId"] == DealerId).
    //
    // Nothing here invents a column: where the reference design
    // asks for a figure the schema cannot answer, the tile shows
    // the closest real number instead (see Today's Dispatched and
    // the Vehicle Status breakdown below).
    // ============================================================

    public partial class Dashboard : System.Web.UI.Page
    {
        // Indian grouping (1,24,500) is what the design uses.
        private static readonly CultureInfo Indian =
            new CultureInfo("en-IN");


        private int DealerId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            // Prevent browser caching
            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            Response.Cache.SetExpires(
                DateTime.UtcNow.AddYears(-1));

            Response.Cache.SetRevalidation(
                System.Web.HttpCacheRevalidation.AllCaches);


            // Check login
            if (Session["UserId"] == null)
            {
                Response.Redirect(
                    "~/Account/Login.aspx",
                    false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }


            // Check Dealer role
            if (Session["UserRole"] == null ||
                Session["UserRole"].ToString() != "Dealer")
            {
                Response.Redirect(
                    BWDMS.Data.AppAuth.HomeUrl(
                        Session["UserRole"]),
                    false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }


            LoadDashboard();
        }


        // ============================================================
        // LOAD EVERY PANEL
        // ============================================================

        private void LoadDashboard()
        {
            LoadHeadlineTiles();

            LoadSalesChart();

            LoadTopProducts();

            LoadVehicleStatus();

            LoadStockValue();

            LoadRecentOrders();
        }


        // ============================================================
        // HEADLINE TILES
        // ============================================================

        private void LoadHeadlineTiles()
        {
            // "Today's Collections" has no source: BWDMS records no
            // shop payments anywhere, so the fourth tile reports the
            // value of the orders that actually left for delivery
            // today, which is a real, computable figure.
            string query = @"
                SELECT
                    ShopCount =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM Shops
                             WHERE DealerId = @DealerId
                               AND IsActive = 1),
                            0),
                    TodayOrderCount =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM Orders
                             WHERE DealerId = @DealerId
                               AND OrderDate = @Today),
                            0),
                    TodaySales =
                        ISNULL(
                            (SELECT SUM(GrandTotal)
                             FROM Orders
                             WHERE DealerId = @DealerId
                               AND OrderDate = @Today
                               AND Status <> 'Cancelled'),
                            0),
                    TodayDispatched =
                        ISNULL(
                            (SELECT SUM(GrandTotal)
                             FROM Orders
                             WHERE DealerId = @DealerId
                               AND OrderDate = @Today
                               AND Status IN
                                   ('Dispatched', 'Delivered')),
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

                    cmd.Parameters.Add(
                        "@Today",
                        SqlDbType.Date).Value =
                            DateTime.Today;

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblShopCount.Text =
                                Convert.ToInt32(
                                    reader["ShopCount"])
                                .ToString("N0");

                            lblTodayOrders.Text =
                                Convert.ToInt32(
                                    reader["TodayOrderCount"])
                                .ToString("N0");

                            lblTodaySales.Text =
                                "&#8377;" +
                                Convert.ToDecimal(
                                    reader["TodaySales"])
                                .ToString("N2", Indian);

                            lblTodayDispatched.Text =
                                "&#8377;" +
                                Convert.ToDecimal(
                                    reader["TodayDispatched"])
                                .ToString("N2", Indian);
                        }
                    }
                }
            }
        }


        // ============================================================
        // SALES OVERVIEW - ONE POINT PER DAY THIS MONTH
        //
        // Rendered as a self-contained SVG so the page needs no
        // chart library and no client script.
        // ============================================================

        private void LoadSalesChart()
        {
            DateTime first = new DateTime(
                DateTime.Today.Year,
                DateTime.Today.Month,
                1);

            string query = @"
                SELECT
                    SaleDate = CAST(OrderDate AS DATE),
                    Amount   = SUM(GrandTotal)
                FROM Orders
                WHERE DealerId = @DealerId
                  AND OrderDate >= @First
                  AND OrderDate <= @Today
                  AND Status <> 'Cancelled'
                GROUP BY CAST(OrderDate AS DATE)
                ORDER BY CAST(OrderDate AS DATE)";

            Dictionary<int, decimal> byDay =
                new Dictionary<int, decimal>();

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

                    cmd.Parameters.Add(
                        "@First",
                        SqlDbType.Date).Value = first;

                    cmd.Parameters.Add(
                        "@Today",
                        SqlDbType.Date).Value =
                            DateTime.Today;

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime day =
                                Convert.ToDateTime(
                                    reader["SaleDate"]);

                            byDay[day.Day] =
                                Convert.ToDecimal(
                                    reader["Amount"]);
                        }
                    }
                }
            }

            litChart.Text = BuildChart(byDay);
        }


        private string BuildChart(Dictionary<int, decimal> byDay)
        {
            int lastDay = DateTime.Today.Day;

            if (lastDay < 2)
            {
                return "<p class=\"text-muted small mb-0\">" +
                       "Not enough days yet to plot this month.</p>";
            }

            List<decimal> values = new List<decimal>();

            decimal max = 0;

            for (int day = 1; day <= lastDay; day++)
            {
                decimal amount;

                if (!byDay.TryGetValue(day, out amount))
                {
                    amount = 0;
                }

                values.Add(amount);

                if (amount > max)
                {
                    max = amount;
                }
            }


            // --- geometry -------------------------------------------------
            const int W = 760;
            const int H = 250;

            const int PadLeft = 66;
            const int PadRight = 16;
            const int PadTop = 14;
            const int PadBottom = 30;

            int plotW = W - PadLeft - PadRight;
            int plotH = H - PadTop - PadBottom;

            if (max <= 0)
            {
                max = 1000;
            }

            // round the top of the scale up to something readable
            max = RoundUp(max);

            StringBuilder svg = new StringBuilder();

            svg.Append("<svg viewBox=\"0 0 ")
               .Append(W).Append(' ').Append(H)
               .Append("\" role=\"img\" ")
               .Append("aria-label=\"Sales by day this month\" ")
               .Append("preserveAspectRatio=\"none\">");


            // --- gridlines + y labels ------------------------------------
            for (int i = 0; i <= 4; i++)
            {
                double y = PadTop + (plotH * i / 4.0);

                decimal label = max - (max * i / 4m);

                svg.Append("<line x1=\"").Append(PadLeft)
                   .Append("\" y1=\"").Append(y.ToString("0.##"))
                   .Append("\" x2=\"").Append(W - PadRight)
                   .Append("\" y2=\"").Append(y.ToString("0.##"))
                   .Append("\" class=\"grid\" />");

                svg.Append("<text x=\"").Append(PadLeft - 10)
                   .Append("\" y=\"")
                   .Append((y + 4).ToString("0.##"))
                   .Append("\" class=\"axis\" text-anchor=\"end\">")
                   .Append(ShortMoney(label))
                   .Append("</text>");
            }


            // --- the line ------------------------------------------------
            StringBuilder points = new StringBuilder();

            for (int i = 0; i < values.Count; i++)
            {
                double x = PadLeft +
                           (plotW * i / (double)(values.Count - 1));

                double y = PadTop + plotH -
                           (double)(values[i] / max * (decimal)plotH);

                if (i > 0)
                {
                    points.Append(' ');
                }

                points.Append(x.ToString("0.##"))
                      .Append(',')
                      .Append(y.ToString("0.##"));
            }

            svg.Append("<polyline fill=\"none\" ")
               .Append("stroke=\"#2563eb\" stroke-width=\"2.5\" ")
               .Append("stroke-linejoin=\"round\" ")
               .Append("stroke-linecap=\"round\" ")
               .Append("points=\"")
               .Append(points)
               .Append("\" />");


            // --- points --------------------------------------------------
            int everyN = values.Count > 12 ? 2 : 1;

            for (int i = 0; i < values.Count; i++)
            {
                double x = PadLeft +
                           (plotW * i / (double)(values.Count - 1));

                double y = PadTop + plotH -
                           (double)(values[i] / max * (decimal)plotH);

                svg.Append("<circle cx=\"").Append(x.ToString("0.##"))
                   .Append("\" cy=\"").Append(y.ToString("0.##"))
                   .Append("\" r=\"3.5\" class=\"dot\" />");

                if (i % everyN == 0 || i == values.Count - 1)
                {
                    svg.Append("<text x=\"")
                       .Append(x.ToString("0.##"))
                       .Append("\" y=\"").Append(H - 10)
                       .Append("\" class=\"axis\" ")
                       .Append("text-anchor=\"middle\">")
                       .Append(i + 1)
                       .Append("</text>");
                }
            }

            svg.Append("</svg>");

            return svg.ToString();
        }


        private static decimal RoundUp(decimal value)
        {
            decimal[] steps =
            {
                1000, 5000, 10000, 25000, 50000,
                100000, 250000, 500000, 1000000,
                2500000, 5000000, 10000000
            };

            foreach (decimal step in steps)
            {
                if (value <= step)
                {
                    return step;
                }
            }

            return Math.Ceiling(value / 1000000m) * 1000000m;
        }


        private static string ShortMoney(decimal value)
        {
            if (value >= 10000000m)
            {
                return "&#8377;" +
                       (value / 10000000m).ToString("0.#") + "Cr";
            }

            if (value >= 100000m)
            {
                return "&#8377;" +
                       (value / 100000m).ToString("0.#") + "L";
            }

            if (value >= 1000m)
            {
                return "&#8377;" +
                       (value / 1000m).ToString("0.#") + "K";
            }

            return "&#8377;" + value.ToString("N0");
        }


        // ============================================================
        // TOP PRODUCTS
        // ============================================================

        private void LoadTopProducts()
        {
            DateTime first = new DateTime(
                DateTime.Today.Year,
                DateTime.Today.Month,
                1);

            string query = @"
                SELECT TOP (5)
                    p.ProductName,
                    Packets = ISNULL(
                                  SUM(od.QuantityPackets), 0)
                FROM OrderDetails od
                INNER JOIN Orders o
                    ON o.OrderId = od.OrderId
                INNER JOIN ProductVariants pv
                    ON pv.ProductVariantId =
                       od.ProductVariantId
                INNER JOIN Products p
                    ON p.ProductId = pv.ProductId
                WHERE o.DealerId = @DealerId
                  AND o.OrderDate >= @First
                  AND o.OrderDate <= @Today
                  AND o.Status <> 'Cancelled'
                GROUP BY p.ProductId, p.ProductName
                ORDER BY SUM(od.QuantityPackets) DESC";

            DataTable table = new DataTable();

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

                    cmd.Parameters.Add(
                        "@First",
                        SqlDbType.Date).Value = first;

                    cmd.Parameters.Add(
                        "@Today",
                        SqlDbType.Date).Value =
                            DateTime.Today;

                    using (SqlDataAdapter adapter =
                        new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(table);
                    }
                }
            }

            if (!table.Columns.Contains("QuantityText"))
            {
                table.Columns.Add(
                    "QuantityText",
                    typeof(string));
            }

            foreach (DataRow row in table.Rows)
            {
                row["QuantityText"] =
                    Convert.ToInt32(row["Packets"])
                        .ToString("N0");
            }

            bool empty = table.Rows.Count == 0;

            rptTopProducts.Visible = !empty;

            lblNoProducts.Visible = empty;

            rptTopProducts.DataSource = table;

            rptTopProducts.DataBind();
        }


        // ============================================================
        // VEHICLE STATUS
        //
        // Available  - active vehicles with no dispatch today
        // Loading    - dispatches still being loaded
        // Running    - dispatches out on the road
        // Returning  - vehicles that have brought stock back today
        // ============================================================

        private void LoadVehicleStatus()
        {
            string query = @"
                SELECT
                    TotalVehicles =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM Vehicles
                             WHERE DealerId = @DealerId
                               AND IsActive = 1),
                            0),
                    BusyToday =
                        ISNULL(
                            (SELECT COUNT(DISTINCT VehicleId)
                             FROM Dispatches
                             WHERE DealerId = @DealerId
                               AND DispatchDate = @Today
                               AND VehicleId IS NOT NULL),
                            0),
                    LoadingToday =
                        ISNULL(
                            (SELECT COUNT(DISTINCT VehicleId)
                             FROM Dispatches
                             WHERE DealerId = @DealerId
                               AND DispatchDate = @Today
                               AND Status = 'Loading'
                               AND VehicleId IS NOT NULL),
                            0),
                    RunningToday =
                        ISNULL(
                            (SELECT COUNT(DISTINCT VehicleId)
                             FROM Dispatches
                             WHERE DealerId = @DealerId
                               AND DispatchDate = @Today
                               AND Status = 'Dispatched'
                               AND VehicleId IS NOT NULL),
                            0),
                    ReturningToday =
                        ISNULL(
                            (SELECT COUNT(DISTINCT VehicleId)
                             FROM VehicleStock
                             WHERE DealerId = @DealerId
                               AND StockDate = @Today
                               AND ReturnedPackets > 0),
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

                    cmd.Parameters.Add(
                        "@Today",
                        SqlDbType.Date).Value =
                            DateTime.Today;

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int total =
                                Convert.ToInt32(
                                    reader["TotalVehicles"]);

                            int busy =
                                Convert.ToInt32(
                                    reader["BusyToday"]);

                            lblVsAvailable.Text =
                                Math.Max(0, total - busy)
                                    .ToString("N0");

                            lblVsRunning.Text =
                                Convert.ToInt32(
                                    reader["RunningToday"])
                                .ToString("N0");

                            lblVsLoading.Text =
                                Convert.ToInt32(
                                    reader["LoadingToday"])
                                .ToString("N0");

                            lblVsReturning.Text =
                                Convert.ToInt32(
                                    reader["ReturningToday"])
                                .ToString("N0");
                        }
                    }
                }
            }
        }


        // ============================================================
        // WAREHOUSE STOCK VALUE
        // ============================================================

        private void LoadStockValue()
        {
            string query = @"
                SELECT
                    StockValue =
                        ISNULL(
                            SUM(
                                i.QuantityPackets *
                                ISNULL(p.Price, 0)),
                            0)
                FROM Inventory i
                OUTER APPLY
                (
                    SELECT TOP (1)
                        Price = pp.DealerPurchasePrice
                    FROM ProductUnitOptions uo
                    INNER JOIN SellingUnits u
                        ON u.UnitId = uo.UnitId
                       AND u.UnitCode = 'PKT'
                    INNER JOIN ProductPrices pp
                        ON pp.ProductOptionId =
                           uo.ProductOptionId
                       -- Same rule the order screens use
                       -- (AddOrder / CreateOrder / OrderSync):
                       -- the newest row already in force. The
                       -- IsActive flag alone is not enough --
                       -- in this data the only active PKT row
                       -- is dated 2030, which would blank the
                       -- tile out.
                       AND pp.EffectiveFrom <=
                           CAST(GETDATE() AS DATE)
                    WHERE uo.ProductVariantId =
                          i.ProductVariantId
                      AND uo.IsActive = 1
                    ORDER BY
                        pp.EffectiveFrom DESC,
                        pp.PriceId DESC
                ) p
                WHERE i.DealerId = @DealerId";

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
                            lblStockValue.Text =
                                Convert.ToDecimal(
                                    reader["StockValue"])
                                .ToString("N2", Indian);
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
                SELECT TOP (5)
                    o.OrderId,
                    o.OrderNumber,
                    o.Status,
                    ShopName = ISNULL(s.ShopName, '-'),
                    SalesmanName = ISNULL(u.FullName, '-'),
                    o.GrandTotal
                FROM Orders o
                LEFT JOIN Shops s
                    ON s.ShopId = o.ShopId
                LEFT JOIN Users u
                    ON u.UserId = o.SalesmanId
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
                dt.Columns.Add("StatusCss", typeof(string));
                dt.Columns.Add("GrandTotalText", typeof(string));
            }

            foreach (DataRow row in dt.Rows)
            {
                row["StatusCss"] = Orders.StatusCss(
                    row["Status"].ToString());

                row["GrandTotalText"] =
                    "&#8377;" +
                    Convert.ToDecimal(row["GrandTotal"])
                        .ToString("N2", Indian);
            }

            gvRecentOrders.DataSource = dt;

            gvRecentOrders.DataBind();
        }


        // ------------------------------------------------------------
        // PAGING
        //
        // The new index has to be applied before the grid is rebound,
        // otherwise the postback lands back on the page it came from.
        // ------------------------------------------------------------

        protected void gvRecentOrders_PageIndexChanging(
            object sender,
            GridViewPageEventArgs e)
        {
            gvRecentOrders.PageIndex = e.NewPageIndex;

            LoadRecentOrders();
        }
    }
}
