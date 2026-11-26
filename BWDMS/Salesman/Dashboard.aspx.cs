using System;
using System.Data;
using System.Data.SqlClient;
using BWDMS.Data;

namespace BWDMS.Salesman
{
    // ============================================================
    // SALESMAN DASHBOARD
    //
    // Shows the beat the salesman has been assigned to today.
    //
    // A Salesman is a Users row with Role = 'Salesman' whose
    // DealerId points at the dealer that employs them. The
    // salesman is matched against RouteSchedules.SalesmanId.
    // ============================================================

    public partial class Dashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // --------------------------------------------------------
            // Prevent browser caching
            // --------------------------------------------------------

            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            Response.Cache.SetExpires(
                DateTime.UtcNow.AddYears(-1));

            Response.Cache.SetRevalidation(
                System.Web.HttpCacheRevalidation.AllCaches);


            // --------------------------------------------------------
            // Check login
            // --------------------------------------------------------

            if (Session["UserId"] == null)
            {
                Response.Redirect(
                    "~/Account/Login.aspx",
                    false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }


            // --------------------------------------------------------
            // Check Salesman role
            //
            // Without this check any logged-in user could read
            // another salesman's beat.
            // --------------------------------------------------------

            if (Session["UserRole"] == null ||
                Session["UserRole"].ToString() != "Salesman")
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
                LoadSummary();
                LoadShopStatus();
                LoadVehicleStock();
                LoadTodayRoute();
                LoadSalesSummary();
                LoadTodaysBeat();
            }
        }


        // ============================================================
        // CURRENT SALESMAN ID
        // ============================================================

        private int SalesmanId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }


        // ============================================================
        // SUMMARY CARDS
        // ============================================================

        private void LoadSummary()
        {
            string today = DateTime.Now.DayOfWeek.ToString();

            string query = @"
                SELECT
                    COUNT(DISTINCT r.RouteId) AS RouteCount,
                    COUNT(DISTINCT
                        CASE WHEN rs.DayOfWeek = @Today
                        THEN rs.RouteScheduleId END) AS TodayCount
                FROM RouteSchedules rs
                INNER JOIN Routes r
                    ON rs.RouteId = r.RouteId
                WHERE rs.SalesmanId = @SalesmanId
                AND rs.IsActive = 1
                AND r.IsActive = 1;";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@SalesmanId",
                        SqlDbType.Int).Value =
                        SalesmanId;

                    cmd.Parameters.Add(
                        "@Today",
                        SqlDbType.NVarChar,
                        40).Value = today;

                    con.Open();

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblRouteCount.Text =
                                Convert.ToString(reader["RouteCount"]);

                            lblScheduleCount.Text =
                                Convert.ToString(reader["TodayCount"]);
                        }
                    }
                }
            }


            // Villages on the salesman's active routes
            string villageQuery = @"
                SELECT COUNT(DISTINCT rv.RouteVillageId)
                FROM RouteVillages rv
                INNER JOIN Routes r
                    ON rv.RouteId = r.RouteId
                INNER JOIN RouteSchedules rs
                    ON rs.RouteId = r.RouteId
                WHERE rs.SalesmanId = @SalesmanId
                AND rs.IsActive = 1
                AND r.IsActive = 1
                AND rv.IsActive = 1;";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(villageQuery, con))
                {
                    cmd.Parameters.Add(
                        "@SalesmanId",
                        SqlDbType.Int).Value =
                        SalesmanId;

                    con.Open();

                    object result = cmd.ExecuteScalar();
                    lblVillageCount.Text =
                        result == null || result == DBNull.Value
                            ? "0"
                            : Convert.ToInt32(result).ToString();
                }
            }


            // Vehicle assigned to today's schedule, if any
            string vehicleQuery = @"
                SELECT TOP 1
                    ISNULL(NULLIF(LTRIM(RTRIM(v.VehicleNumber)), ''), '-')
                FROM RouteSchedules rs
                LEFT JOIN Vehicles v
                    ON rs.VehicleId = v.VehicleId
                WHERE rs.SalesmanId = @SalesmanId
                AND rs.DayOfWeek = @Today
                AND rs.IsActive = 1
                ORDER BY rs.RouteScheduleId;";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(vehicleQuery, con))
                {
                    cmd.Parameters.Add(
                        "@SalesmanId",
                        SqlDbType.Int).Value =
                        SalesmanId;

                    cmd.Parameters.Add(
                        "@Today",
                        SqlDbType.NVarChar,
                        40).Value = today;

                    con.Open();

                    object result = cmd.ExecuteScalar();
                    lblVehicle.Text =
                        result == null || result == DBNull.Value
                            ? "-"
                            : Convert.ToString(result);
                }
            }
        }


        // ============================================================
        // TODAY'S SHOP STATUS
        // ============================================================

        private void LoadShopStatus()
        {
            string today = DateTime.Now.DayOfWeek.ToString();
            DateTime todayDate = DateTime.Today;

            string query = @"
                ;WITH TodaySchedules AS
                (
                    SELECT rs.RouteScheduleId, rs.RouteId
                    FROM RouteSchedules rs
                    INNER JOIN Routes r
                        ON r.RouteId = rs.RouteId
                    WHERE rs.SalesmanId = @SalesmanId
                      AND rs.DayOfWeek = @Today
                      AND rs.IsActive = 1
                      AND r.IsActive = 1
                ),
                BeatShops AS
                (
                    SELECT DISTINCT s.ShopId
                    FROM Shops s
                    WHERE s.IsActive = 1
                      AND
                      (
                        s.RouteScheduleId IN
                            (SELECT RouteScheduleId FROM TodaySchedules)
                        OR s.RouteId IN
                            (SELECT RouteId FROM TodaySchedules)
                      )
                ),
                Visited AS
                (
                    SELECT DISTINCT o.ShopId
                    FROM Orders o
                    INNER JOIN BeatShops b
                        ON b.ShopId = o.ShopId
                    WHERE o.SalesmanId = @SalesmanId
                      AND CAST(o.OrderDate AS date) = @TodayDate
                )
                SELECT
                    (SELECT COUNT(*) FROM BeatShops) AS TotalShops,
                    (SELECT COUNT(*) FROM Visited) AS VisitedShops;";

            int total = 0;
            int visited = 0;

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@SalesmanId",
                        SqlDbType.Int).Value = SalesmanId;

                    cmd.Parameters.Add(
                        "@Today",
                        SqlDbType.NVarChar,
                        40).Value = today;

                    cmd.Parameters.Add(
                        "@TodayDate",
                        SqlDbType.Date).Value = todayDate;

                    con.Open();

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            total = ToInt(reader["TotalShops"]);
                            visited = ToInt(reader["VisitedShops"]);
                        }
                    }
                }
            }

            int pending = total - visited;
            if (pending < 0)
            {
                pending = 0;
            }

            lblVisitedCount.Text = visited.ToString();
            lblPendingCount.Text = pending.ToString();
            lblTotalShops.Text = total.ToString();
        }


        // ============================================================
        // VEHICLE STOCK
        // ============================================================

        private void LoadVehicleStock()
        {
            string query = @"
                ;WITH Latest AS
                (
                    SELECT MAX(StockDate) AS StockDate
                    FROM VehicleStock
                    WHERE SalesmanId = @SalesmanId
                )
                SELECT ISNULL(SUM(
                    vs.LoadedPackets
                    - vs.SoldPackets
                    - vs.ReturnedPackets
                    - vs.DamagedPackets
                ), 0) AS RemainingPackets
                FROM VehicleStock vs
                INNER JOIN Latest l
                    ON l.StockDate = vs.StockDate
                WHERE vs.SalesmanId = @SalesmanId;";

            int remaining = 0;

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@SalesmanId",
                        SqlDbType.Int).Value = SalesmanId;

                    con.Open();

                    object result = cmd.ExecuteScalar();
                    remaining = ToInt(result);
                }
            }

            if (remaining < 0)
            {
                remaining = 0;
            }

            lblStockRemaining.Text =
                remaining.ToString("N0") + " Box";
        }


        // ============================================================
        // TODAY'S ROUTE
        // ============================================================

        private void LoadTodayRoute()
        {
            string today = DateTime.Now.DayOfWeek.ToString();

            string query = @"
                SELECT TOP 1
                    r.RouteName,
                    ISNULL(NULLIF(LTRIM(RTRIM(v.VehicleNumber)), ''), '') AS VehicleNumber
                FROM RouteSchedules rs
                INNER JOIN Routes r
                    ON r.RouteId = rs.RouteId
                LEFT JOIN Vehicles v
                    ON v.VehicleId = rs.VehicleId
                WHERE rs.SalesmanId = @SalesmanId
                  AND rs.DayOfWeek = @Today
                  AND rs.IsActive = 1
                  AND r.IsActive = 1
                ORDER BY r.RouteName, rs.RouteScheduleId;";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@SalesmanId",
                        SqlDbType.Int).Value = SalesmanId;

                    cmd.Parameters.Add(
                        "@Today",
                        SqlDbType.NVarChar,
                        40).Value = today;

                    con.Open();

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string routeName =
                                Convert.ToString(reader["RouteName"]);
                            string vehicle =
                                Convert.ToString(reader["VehicleNumber"]);

                            lblTodayRoute.Text =
                                string.IsNullOrWhiteSpace(routeName)
                                    ? "No route assigned"
                                    : routeName;

                            lblRouteSubtext.Text =
                                string.IsNullOrWhiteSpace(vehicle)
                                    ? "Assigned beat"
                                    : "Vehicle: " + vehicle;

                            return;
                        }
                    }
                }
            }

            lblTodayRoute.Text = "No route assigned";
            lblRouteSubtext.Text = "No beat assigned today";
        }


        // ============================================================
        // SALES SUMMARY
        // ============================================================

        private void LoadSalesSummary()
        {
            DateTime todayDate = DateTime.Today;
            DateTime yesterdayDate = todayDate.AddDays(-1);

            string query = @"
                SELECT
                    ISNULL(SUM(CASE
                        WHEN CAST(OrderDate AS date) = @TodayDate
                        THEN GrandTotal ELSE 0 END), 0) AS TodayTotal,
                    ISNULL(SUM(CASE
                        WHEN CAST(OrderDate AS date) = @YesterdayDate
                        THEN GrandTotal ELSE 0 END), 0) AS YesterdayTotal
                FROM Orders
                WHERE SalesmanId = @SalesmanId
                  AND CAST(OrderDate AS date) IN (@TodayDate, @YesterdayDate);";

            decimal todayTotal = 0m;
            decimal yesterdayTotal = 0m;

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@SalesmanId",
                        SqlDbType.Int).Value = SalesmanId;

                    cmd.Parameters.Add(
                        "@TodayDate",
                        SqlDbType.Date).Value = todayDate;

                    cmd.Parameters.Add(
                        "@YesterdayDate",
                        SqlDbType.Date).Value = yesterdayDate;

                    con.Open();

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            todayTotal = ToDecimal(reader["TodayTotal"]);
                            yesterdayTotal = ToDecimal(reader["YesterdayTotal"]);
                        }
                    }
                }
            }

            lblTodaySales.Text = FormatCurrency(todayTotal);
            lblPreviousSales.Text = FormatCurrency(yesterdayTotal);
        }


        private static int ToInt(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return 0;
            }

            int number;
            if (int.TryParse(Convert.ToString(value), out number))
            {
                return number;
            }

            decimal decimalNumber;
            if (decimal.TryParse(
                Convert.ToString(value),
                out decimalNumber))
            {
                return Convert.ToInt32(decimalNumber);
            }

            return 0;
        }


        private static decimal ToDecimal(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return 0m;
            }

            decimal number;
            if (decimal.TryParse(
                Convert.ToString(value),
                out number))
            {
                return number;
            }

            return 0m;
        }


        private static string FormatCurrency(decimal amount)
        {
            return "\u20B9" + amount.ToString("N0");
        }


        // ============================================================
        // TODAY'S BEAT - VILLAGES IN VISIT ORDER
        // ============================================================

        private void LoadTodaysBeat()
        {
            string today =
                DateTime.Now.DayOfWeek.ToString();

            lblTodayName.Text = today;

            string query = @"
                SELECT
                    rvs.VisitSequence,
                    v.VillageName,
                    v.Taluka,
                    v.District,
                    r.RouteName,
                    rs.DayOfWeek
                FROM RouteVillageSchedules rvs

                INNER JOIN RouteSchedules rs
                    ON rvs.RouteScheduleId = rs.RouteScheduleId
                    AND rs.SalesmanId = @SalesmanId
                    AND rs.DayOfWeek = @Today
                    AND rs.IsActive = 1

                INNER JOIN RouteVillages rv
                    ON rvs.RouteVillageId = rv.RouteVillageId
                    AND rv.IsActive = 1

                INNER JOIN Villages v
                    ON rv.VillageId = v.VillageId

                INNER JOIN Routes r
                    ON rv.RouteId = r.RouteId
                    AND r.IsActive = 1

                WHERE rvs.IsActive = 1

                ORDER BY
                    r.RouteName,
                    rvs.VisitSequence;";

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@SalesmanId",
                        SqlDbType.Int).Value =
                        SalesmanId;

                    cmd.Parameters.Add(
                        "@Today",
                        SqlDbType.NVarChar,
                        40).Value =
                        today;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            gvBeat.DataSource = dt;

            gvBeat.DataBind();
        }

        // ------------------------------------------------------------
        // PAGING
        //
        // The new index has to be applied before the grid is rebound,
        // otherwise the postback lands back on the page it came from.
        // ------------------------------------------------------------

        protected void gvBeat_PageIndexChanging(
            object sender,
            System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            gvBeat.PageIndex = e.NewPageIndex;

            LoadTodaysBeat();
        }

    }
}
