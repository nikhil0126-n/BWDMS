using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class RouteHistory : System.Web.UI.Page
    {
        // Dealer's own id lives in UserId (there is no Session["DealerId"]).
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
                // A month of runs is the useful default.
                txtFromDate.Text =
                    DateTime.Today.AddDays(-30)
                    .ToString("yyyy-MM-dd");

                txtToDate.Text =
                    DateTime.Today.ToString("yyyy-MM-dd");

                LoadRoutes();

                LoadRouteHistory();
            }
        }


        private void LoadRoutes()
        {
            string query = @"
                SELECT
                    RouteId,
                    RouteName
                FROM Routes
                WHERE DealerId = @DealerId
                  AND IsActive = 1
                ORDER BY RouteName";

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

                        ddlRoute.DataSource = dt;
                        ddlRoute.DataTextField = "RouteName";
                        ddlRoute.DataValueField = "RouteId";
                        ddlRoute.DataBind();
                    }
                }
            }

            ddlRoute.Items.Insert(
                0,
                new ListItem("All routes", "0"));

            ddlRoute.SelectedIndex = 0;
        }


        // ============================================================
        // THE REPORT
        //
        // A route repeats every week, so history is read back as one
        // row per date a route actually did something: it loaded, it
        // took orders, or its day was closed.
        // ============================================================

        private void LoadRouteHistory()
        {
            DateTime fromDate;

            if (!TryDate(txtFromDate.Text, out fromDate))
            {
                fromDate = DateTime.Today.AddDays(-30);
            }

            DateTime toDate;

            if (!TryDate(txtToDate.Text, out toDate))
            {
                toDate = DateTime.Today;
            }

            if (fromDate > toDate)
            {
                ShowMessage(
                    "The From date must not be after the To date.",
                    false);

                BindRouteHistory(null);

                return;
            }

            int routeId;

            if (!int.TryParse(
                    ddlRoute.SelectedValue,
                    out routeId))
            {
                routeId = 0;
            }

            string query = @"
                WITH routeDays AS
                (
                    SELECT
                        ActivityDate = StockDate,
                        RouteScheduleId
                    FROM VehicleStock
                    WHERE DealerId = @DealerId
                      AND ISNULL(RouteScheduleId, 0) > 0
                      AND StockDate >= @FromDate
                      AND StockDate <= @ToDate
                    GROUP BY StockDate, RouteScheduleId

                    UNION

                    SELECT
                        ActivityDate = OrderDate,
                        RouteScheduleId
                    FROM Orders
                    WHERE DealerId = @DealerId
                      AND ISNULL(RouteScheduleId, 0) > 0
                      AND OrderDate >= @FromDate
                      AND OrderDate <= @ToDate
                    GROUP BY OrderDate, RouteScheduleId

                    UNION

                    SELECT
                        ActivityDate = rs.StockDate,
                        rs.RouteScheduleId
                    FROM RouteSchedules rs
                    INNER JOIN Routes r
                        ON r.RouteId = rs.RouteId
                    WHERE r.DealerId = @DealerId
                      AND rs.StockDate IS NOT NULL
                      AND rs.StockDate >= @FromDate
                      AND rs.StockDate <= @ToDate
                ),
                loadDays AS
                (
                    SELECT
                        StockDate,
                        RouteScheduleId,
                        LoadedPackets = SUM(LoadedPackets),
                        SoldPackets = SUM(SoldPackets)
                    FROM VehicleStock
                    WHERE DealerId = @DealerId
                      AND ISNULL(RouteScheduleId, 0) > 0
                      AND StockDate >= @FromDate
                      AND StockDate <= @ToDate
                    GROUP BY StockDate, RouteScheduleId
                ),
                orderDays AS
                (
                    SELECT
                        OrderDate,
                        RouteScheduleId,
                        OrderCount = COUNT(*),
                        OrderValue = SUM(GrandTotal)
                    FROM Orders
                    WHERE DealerId = @DealerId
                      AND ISNULL(RouteScheduleId, 0) > 0
                      AND OrderDate >= @FromDate
                      AND OrderDate <= @ToDate
                    GROUP BY OrderDate, RouteScheduleId
                )
                SELECT
                    ActivityDateText =
                        CONVERT(NVARCHAR(10), d.ActivityDate, 120),
                    rs.DayOfWeek,
                    r.RouteName,
                    VehicleNumber = ISNULL(v.VehicleNumber, ''),
                    SalesmanName = ISNULL(s.FullName, '-'),
                    DriverName = ISNULL(dr.FullName, '-'),
                    LoadedPackets = ISNULL(ld.LoadedPackets, 0),
                    SoldPackets = ISNULL(ld.SoldPackets, 0),
                    OrderCount = ISNULL(od.OrderCount, 0),
                    OrderValue = ISNULL(od.OrderValue, 0),
                    IsClosed =
                        CASE
                            WHEN rs.IsClosed = 1
                                 AND rs.StockDate = d.ActivityDate
                            THEN 1
                            ELSE 0
                        END
                FROM routeDays d
                INNER JOIN RouteSchedules rs
                    ON rs.RouteScheduleId = d.RouteScheduleId
                INNER JOIN Routes r
                    ON r.RouteId = rs.RouteId
                LEFT JOIN Vehicles v
                    ON v.VehicleId = rs.VehicleId
                LEFT JOIN Users s
                    ON s.UserId = rs.SalesmanId
                LEFT JOIN Users dr
                    ON dr.UserId = rs.DriverId
                LEFT JOIN loadDays ld
                    ON ld.RouteScheduleId = d.RouteScheduleId
                   AND ld.StockDate = d.ActivityDate
                LEFT JOIN orderDays od
                    ON od.RouteScheduleId = d.RouteScheduleId
                   AND od.OrderDate = d.ActivityDate
                WHERE r.DealerId = @DealerId
                  AND (@RouteId = 0 OR rs.RouteId = @RouteId)
                ORDER BY
                    d.ActivityDate DESC,
                    r.RouteName";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    cmd.Parameters.Add(
                        "@FromDate",
                        SqlDbType.Date).Value = fromDate;

                    cmd.Parameters.Add(
                        "@ToDate",
                        SqlDbType.Date).Value = toDate;

                    cmd.Parameters.Add(
                        "@RouteId",
                        SqlDbType.Int).Value = routeId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        BindRouteHistory(dt);
                    }
                }
            }
        }


        private void BindRouteHistory(DataTable dt)
        {
            gvRouteHistory.DataSource = dt;
            gvRouteHistory.DataBind();
        }


        private static bool TryDate(
            string value,
            out DateTime date)
        {
            date = DateTime.MinValue;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return DateTime.TryParse(
                       value.Trim(),
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.None,
                       out date) ||
                   DateTime.TryParse(
                       value.Trim(),
                       out date);
        }


        protected void FilterChanged(
            object sender,
            EventArgs e)
        {
            LoadRouteHistory();
        }


        protected void btnShow_Click(
            object sender,
            EventArgs e)
        {
            LoadRouteHistory();
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

        protected void gvRouteHistory_PageIndexChanging(
            object sender,
            GridViewPageEventArgs e)
        {
            gvRouteHistory.PageIndex = e.NewPageIndex;

            LoadRouteHistory();
        }

    }
}
