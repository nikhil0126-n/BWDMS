using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    /// <summary>
    /// Route report history.
    ///
    /// No table is invented for this: every row is a (date, route-day) pair
    /// that actually happened, taken from the union of the three places a
    /// route day leaves a trace — a vehicle load, an order, or a closure.
    /// </summary>
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
                LoadRoutes();

                LoadRouteHistory();
            }
        }


        // ------------------------------------------------------------
        // FILTERS
        // ------------------------------------------------------------

        private void LoadRoutes()
        {
            ddlRoute.Items.Clear();

            ListItem all = new ListItem("All routes", "");
            all.Selected = true;

            ddlRoute.Items.Add(all);

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT
                        r.RouteId,
                        r.RouteName
                    FROM Routes r
                    WHERE r.DealerId = @DealerId
                      AND r.IsActive = 1
                    ORDER BY r.RouteName;", con))
                {
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ddlRoute.Items.Add(
                                new ListItem(
                                    reader["RouteName"].ToString(),
                                    reader["RouteId"].ToString()));
                        }
                    }
                }
            }
        }


        protected void FilterChanged(object sender, EventArgs e)
        {
            LoadRouteHistory();
        }


        protected void btnShow_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            LoadRouteHistory();
        }


        // ------------------------------------------------------------
        // ROUTE RUNS
        // ------------------------------------------------------------

        private void LoadRouteHistory()
        {
            DateTime? from = ParseDate(txtFromDate.Text);

            DateTime? to = ParseDate(txtToDate.Text);

            object routeId =
                string.IsNullOrWhiteSpace(ddlRoute.SelectedValue)
                    ? (object)DBNull.Value
                    : Convert.ToInt32(ddlRoute.SelectedValue);

            // A route day is "closed" only for the date it was closed on.
            // The route repeats weekly, so a closure for last Monday must
            // not paint every past Monday as closed.
            string query = @"
                ;WITH Activity AS
                (
                    SELECT
                        vs.StockDate  AS ActivityDate,
                        vs.RouteScheduleId
                    FROM VehicleStock vs
                    WHERE vs.DealerId = @DealerId
                      AND vs.RouteScheduleId IS NOT NULL

                    UNION

                    SELECT
                        o.OrderDate   AS ActivityDate,
                        o.RouteScheduleId
                    FROM Orders o
                    WHERE o.DealerId = @DealerId
                      AND o.RouteScheduleId IS NOT NULL

                    UNION

                    SELECT
                        rs.StockDate  AS ActivityDate,
                        rs.RouteScheduleId
                    FROM RouteSchedules rs
                    INNER JOIN Routes r
                        ON r.RouteId = rs.RouteId
                    WHERE r.DealerId = @DealerId
                      AND rs.StockDate IS NOT NULL
                )
                SELECT
                    a.ActivityDate,
                    ActivityDateText =
                        CONVERT(NVARCHAR(10), a.ActivityDate, 103),
                    DayOfWeek        = DATENAME(WEEKDAY, a.ActivityDate),
                    r.RouteName,
                    v.VehicleNumber,
                    SalesmanName     = ISNULL(s.FullName, '-'),
                    DriverName       = ISNULL(d.FullName, '-'),
                    LoadedPackets    = ISNULL(load.LoadedPackets, 0),
                    SoldPackets      = ISNULL(load.SoldPackets, 0),
                    OrderCount       = ISNULL(ord.OrderCount, 0),
                    OrderValue       = ISNULL(ord.OrderValue, 0),
                    IsClosed         =
                        CASE
                            WHEN rs.IsClosed = 1
                                 AND rs.StockDate = a.ActivityDate
                                THEN 1
                            ELSE 0
                        END
                FROM Activity a
                INNER JOIN RouteSchedules rs
                    ON rs.RouteScheduleId = a.RouteScheduleId
                INNER JOIN Routes r
                    ON r.RouteId = rs.RouteId
                LEFT JOIN Vehicles v
                    ON v.VehicleId = rs.VehicleId
                LEFT JOIN Users s
                    ON s.UserId = rs.SalesmanId
                LEFT JOIN Users d
                    ON d.UserId = rs.DriverId
                OUTER APPLY
                (
                    SELECT
                        LoadedPackets = SUM(vs.LoadedPackets),
                        SoldPackets   = SUM(vs.SoldPackets)
                    FROM VehicleStock vs
                    WHERE vs.RouteScheduleId = rs.RouteScheduleId
                      AND vs.StockDate       = a.ActivityDate
                ) load
                OUTER APPLY
                (
                    SELECT
                        OrderCount = COUNT(*),
                        OrderValue = SUM(o.GrandTotal)
                    FROM Orders o
                    WHERE o.RouteScheduleId = rs.RouteScheduleId
                      AND o.OrderDate        = a.ActivityDate
                ) ord
                WHERE r.DealerId = @DealerId
                  AND
                  (
                        @RouteId IS NULL
                     OR r.RouteId = @RouteId
                  )
                  AND
                  (
                        @FromDate IS NULL
                     OR a.ActivityDate >= @FromDate
                  )
                  AND
                  (
                        @ToDate IS NULL
                     OR a.ActivityDate <= @ToDate
                  )
                ORDER BY
                    a.ActivityDate DESC,
                    r.RouteName;";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    cmd.Parameters.Add(
                        "@RouteId",
                        SqlDbType.Int).Value = routeId;

                    cmd.Parameters.Add(
                        "@FromDate",
                        SqlDbType.Date).Value =
                            from.HasValue
                                ? (object)from.Value
                                : DBNull.Value;

                    cmd.Parameters.Add(
                        "@ToDate",
                        SqlDbType.Date).Value =
                            to.HasValue
                                ? (object)to.Value
                                : DBNull.Value;

                    using (SqlDataAdapter adapter =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable table = new DataTable();

                        adapter.Fill(table);

                        gvRouteHistory.DataSource = table;

                        gvRouteHistory.DataBind();
                    }
                }
            }
        }


        private DateTime? ParseDate(string value)
        {
            DateTime parsed;

            if (!string.IsNullOrWhiteSpace(value) &&
                DateTime.TryParse(value.Trim(), out parsed))
            {
                return parsed.Date;
            }

            return null;
        }


        protected void gvRouteHistory_PageIndexChanging(
            object sender,
            GridViewPageEventArgs e)
        {
            gvRouteHistory.PageIndex = e.NewPageIndex;

            LoadRouteHistory();
        }


        // ------------------------------------------------------------
        // MESSAGE
        // ------------------------------------------------------------

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
