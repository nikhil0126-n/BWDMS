using System;
using System.Data;
using System.Data.SqlClient;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    // ============================================================
    // DEALER DASHBOARD
    //
    // Every figure on this page is a live SQL aggregate scoped to
    // the logged-in dealer (Session["UserId"] == DealerId).
    // ============================================================

    public partial class Dashboard : System.Web.UI.Page
    {
        private int DealerId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }


        // English weekday name exactly as stored in
        // Routes.DayOfWeek / RouteSchedules.DayOfWeek.
        private static string TodayName
        {
            get { return DateTime.Now.DayOfWeek.ToString(); }
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
        // LOAD EVERY TILE
        // ============================================================

        private void LoadDashboard()
        {
            LoadHeadlineCounts();
            LoadSalesSummary();
            LoadOrdersByStatus();
            LoadStockSummary();
            LoadStockItems();
            LoadLowStock();
            LoadTodaySchedules();
            LoadVehicleStatus();
            LoadRecentActivity();
            LoadRecentOrders();
        }


        // ============================================================
        // STAT CARDS
        // ============================================================

        private void LoadHeadlineCounts()
        {
            string query = @"
                SELECT
                    ShopCount =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM Shops
                             WHERE DealerId = @DealerId
                               AND IsActive = 1),
                            0),
                    SalesmanCount =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM Users
                             WHERE DealerId = @DealerId
                               AND Role = 'Salesman'
                               AND IsActive = 1),
                            0),
                    VehicleCount =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM Vehicles
                             WHERE DealerId = @DealerId
                               AND IsActive = 1),
                            0),
                    TodayOrderCount =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM Orders
                             WHERE DealerId = @DealerId
                               AND OrderDate = @Today),
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
                                reader["ShopCount"].ToString();

                            lblSalesmanCount.Text =
                                reader["SalesmanCount"].ToString();

                            lblVehicleCount.Text =
                                reader["VehicleCount"].ToString();

                            lblTodayOrders.Text =
                                reader["TodayOrderCount"].ToString();
                        }
                    }
                }
            }
        }


        // ============================================================
        // SALES SUMMARY - ORDERS BY STATUS FOR A PERIOD
        // ============================================================

        private void LoadSalesSummary()
        {
            DateTime from;
            DateTime to;

            GetPeriod(out from, out to);

            DataTable dt = StatusTable(from, to);

            gvSalesSummary.DataSource = dt;
            gvSalesSummary.DataBind();

            int orders = 0;
            decimal amount = 0m;

            foreach (DataRow row in dt.Rows)
            {
                orders += Convert.ToInt32(row["OrderCount"]);

                amount += Convert.ToDecimal(row["Amount"]);
            }

            lblSalesPeriod.Text =
                from.ToString("dd MMM yyyy") + " - " +
                to.AddDays(-1).ToString("dd MMM yyyy");

            lblSalesPeriodOrders.Text =
                orders + (orders == 1 ? " order" : " orders");

            lblSalesPeriodTotal.Text =
                amount.ToString("N2");
        }


        private void GetPeriod(
            out DateTime from,
            out DateTime to)
        {
            DateTime today = DateTime.Today;

            string period = ddlPeriod.SelectedValue;

            if (period == "Week")
            {
                // Week starts on Monday.
                int offset =
                    ((int)today.DayOfWeek + 6) % 7;

                from = today.AddDays(-offset);
                to = from.AddDays(7);
            }
            else if (period == "Year")
            {
                from = new DateTime(today.Year, 1, 1);
                to = from.AddYears(1);
            }
            else
            {
                from = new DateTime(today.Year, today.Month, 1);
                to = from.AddMonths(1);
            }
        }


        // All four statuses, always four rows, never a fake row.
        private DataTable StatusTable(
            DateTime from,
            DateTime to)
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
                    ON o.DealerId = @DealerId
                   AND o.Status = x.Status
                   AND o.OrderDate >= @From
                   AND o.OrderDate < @To
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
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    cmd.Parameters.Add(
                        "@From",
                        SqlDbType.Date).Value = from;

                    cmd.Parameters.Add(
                        "@To",
                        SqlDbType.Date).Value = to;

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

            foreach (DataRow row in dt.Rows)
            {
                row["StatusCss"] = Orders.StatusCss(
                    row["Status"].ToString());

                row["AmountText"] =
                    Convert.ToDecimal(row["Amount"])
                        .ToString("N2");
            }

            return dt;
        }


        // ============================================================
        // ORDERS BY STATUS - ALL TIME
        // ============================================================

        private void LoadOrdersByStatus()
        {
            DataTable dt = StatusTable(
                new DateTime(1, 1, 1),
                new DateTime(9999, 12, 31));

            gvOrdersByStatus.DataSource = dt;
            gvOrdersByStatus.DataBind();
        }


        // ============================================================
        // WAREHOUSE STOCK SUMMARY
        // ============================================================

        private void LoadStockSummary()
        {
            string query = @"
                SELECT
                    VariantCount =
                        ISNULL(COUNT(i.ProductVariantId), 0),
                    TotalPackets =
                        ISNULL(SUM(i.QuantityPackets), 0),
                    StockValue =
                        ISNULL(
                            SUM(
                                i.QuantityPackets *
                                ISNULL(p.Price, 0)),
                            0),
                    LowStockCount =
                        ISNULL(
                            SUM(
                                CASE
                                    WHEN i.QuantityPackets
                                         <= i.ReorderLevel
                                    THEN 1
                                    ELSE 0
                                END),
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
                       AND pp.IsActive = 1
                       AND pp.EffectiveFrom <= CAST(GETDATE() AS DATE)
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
                            lblStockVariants.Text =
                                reader["VariantCount"].ToString();

                            lblStockPackets.Text =
                                Convert.ToInt32(
                                    reader["TotalPackets"])
                                .ToString("N0");

                            lblStockValue.Text =
                                Convert.ToDecimal(
                                    reader["StockValue"])
                                .ToString("N2");

                            lblLowStockCount.Text =
                                reader["LowStockCount"].ToString();
                        }
                    }
                }
            }
        }


        // ============================================================
        // STOCK ITEMS - LOW STOCK FIRST
        // ============================================================

        private void LoadStockItems()
        {
            string query = @"
                SELECT TOP (10)
                    ItemName =
                        p.ProductName + ' - ' + v.VariantName,
                    PacketsText =
                        CONVERT(NVARCHAR(20), i.QuantityPackets)
                        + ' packets (reorder '
                        + CONVERT(NVARCHAR(20), i.ReorderLevel)
                        + ')',
                    StockState =
                        CASE
                            WHEN i.QuantityPackets
                                 <= i.ReorderLevel
                            THEN 'Low'
                            ELSE 'Good'
                        END
                FROM Inventory i
                INNER JOIN ProductVariants v
                    ON v.ProductVariantId = i.ProductVariantId
                INNER JOIN Products p
                    ON p.ProductId = v.ProductId
                WHERE i.DealerId = @DealerId
                ORDER BY
                    CASE
                        WHEN i.QuantityPackets
                             <= i.ReorderLevel
                        THEN 0
                        ELSE 1
                    END,
                    i.QuantityPackets";

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

            if (!dt.Columns.Contains("StockStateCss"))
            {
                dt.Columns.Add(
                    "StockStateCss",
                    typeof(string));
            }

            foreach (DataRow row in dt.Rows)
            {
                row["StockStateCss"] =
                    row["StockState"].ToString() == "Low"
                        ? "badge bg-warning text-dark"
                        : "badge bg-success";
            }

            repStockItems.DataSource = dt;
            repStockItems.DataBind();

            // Repeater cannot render its own empty template, so the
            // caption next to it is toggled here instead.
            lblNoStockItems.Visible = dt.Rows.Count == 0;
        }


        // ============================================================
        // LOW STOCK ALERTS
        // ============================================================

        private void LoadLowStock()
        {
            string query = @"
                SELECT TOP (10)
                    ItemName =
                        p.ProductName + ' - ' + v.VariantName,
                    i.QuantityPackets,
                    i.ReorderLevel
                FROM Inventory i
                INNER JOIN ProductVariants v
                    ON v.ProductVariantId = i.ProductVariantId
                INNER JOIN Products p
                    ON p.ProductId = v.ProductId
                WHERE i.DealerId = @DealerId
                  AND i.QuantityPackets <= i.ReorderLevel
                ORDER BY
                    (i.QuantityPackets - i.ReorderLevel),
                    i.QuantityPackets";

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

            gvLowStock.DataSource = dt;
            gvLowStock.DataBind();
        }


        // ============================================================
        // TODAY'S SCHEDULES
        // ============================================================

        private void LoadTodaySchedules()
        {
            string today = TodayName;

            lblTodayName.Text = today;

            string query = @"
                SELECT
                    r.RouteName,
                    rs.DayOfWeek,
                    VehicleNumber =
                        ISNULL(v.VehicleNumber, '-')
                FROM RouteSchedules rs
                INNER JOIN Routes r
                    ON r.RouteId = rs.RouteId
                   AND r.DealerId = @DealerId
                LEFT JOIN Vehicles v
                    ON v.VehicleId = rs.VehicleId
                WHERE rs.DayOfWeek = @Today
                  AND rs.IsActive = 1
                ORDER BY r.RouteName";

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

                    cmd.Parameters.Add(
                        "@Today",
                        SqlDbType.NVarChar,
                        40).Value = today;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            gvSchedules.DataSource = dt;
            gvSchedules.DataBind();
        }


        // ============================================================
        // VEHICLE LOADING AND RECONCILIATION STATUS
        // ============================================================

        private void LoadVehicleStatus()
        {
            string query = @"
                SELECT
                    LoadedToday =
                        ISNULL(
                            (SELECT SUM(LoadedPackets)
                             FROM VehicleStock
                             WHERE DealerId = @DealerId
                               AND StockDate = @Today),
                            0),
                    ReconciliationComplete =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM VehicleReconciliations
                             WHERE DealerId = @DealerId
                               AND ReconciliationDate = @Today
                               AND ISNULL(IsApproved, 0) = 1),
                            0),
                    ReconciliationPending =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM VehicleReconciliations
                             WHERE DealerId = @DealerId
                               AND ReconciliationDate = @Today
                               AND ISNULL(IsApproved, 0) = 0),
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
                            lblLoadedToday.Text =
                                Convert.ToInt32(
                                    reader["LoadedToday"])
                                .ToString("N0") + " packets";

                            lblReconComplete.Text =
                                reader[
                                    "ReconciliationComplete"]
                                .ToString();

                            lblReconPending.Text =
                                reader[
                                    "ReconciliationPending"]
                                .ToString();
                        }
                    }
                }
            }
        }


        // ============================================================
        // RECENT OPERATIONAL ACTIVITY
        //
        // Latest rows by CreatedAt across the three dealer-owned
        // transactional tables: orders, stock, company receipts.
        // ============================================================

        private void LoadRecentActivity()
        {
            string query = @"
                SELECT TOP (10)
                    ActivityAt,
                    ActivityType,
                    Reference,
                    Details
                FROM
                (
                    SELECT
                        ActivityAt = o.CreatedAt,
                        ActivityType = 'Order',
                        Reference = o.OrderNumber,
                        Details =
                            o.Status
                            + ' | ' + ISNULL(s.ShopName, '-')
                    FROM Orders o
                    LEFT JOIN Shops s
                        ON s.ShopId = o.ShopId
                    WHERE o.DealerId = @DealerId

                    UNION ALL

                    SELECT
                        ActivityAt = st.CreatedAt,
                        ActivityType = 'Stock',
                        Reference =
                            ISNULL(st.ReferenceNo, '-'),
                        Details =
                            st.TransactionType
                    FROM StockTransactions st
                    WHERE st.DealerId = @DealerId

                    UNION ALL

                    SELECT
                        ActivityAt = c.CreatedAt,
                        ActivityType = 'Receipt',
                        Reference = c.ReceiptNumber,
                        Details =
                            ISNULL(c.Status, '-')
                            + ' | '
                            + CONVERT(
                                NVARCHAR(20),
                                c.TotalCost)
                    FROM CompanyStockReceipts c
                    WHERE c.DealerId = @DealerId
                ) a
                ORDER BY ActivityAt DESC, Reference DESC";

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

            if (!dt.Columns.Contains("ActivityAtText"))
            {
                dt.Columns.Add(
                    "ActivityAtText",
                    typeof(string));
            }

            foreach (DataRow row in dt.Rows)
            {
                row["ActivityAtText"] =
                    Convert.ToDateTime(row["ActivityAt"])
                        .ToString("dd MMM HH:mm");
            }

            gvActivity.DataSource = dt;
            gvActivity.DataBind();
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
                    Convert.ToDecimal(row["GrandTotal"])
                        .ToString("N2");
            }

            gvRecentOrders.DataSource = dt;
            gvRecentOrders.DataBind();
        }
    }
}
