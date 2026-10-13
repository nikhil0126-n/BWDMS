using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    /// <summary>
    /// Route run history.
    ///
    /// A route-day only gets a date stamp when something actually happens to
    /// it - stock loaded, orders taken, or the day closed at reconciliation.
    /// An open, untouched route-day carries no date anywhere, so it cannot be
    /// listed here. The report therefore unions the three dated sources and
    /// shows one row per (route, date) that has at least one of them.
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
        // FILTER DROPDOWN
        //
        // A filter control, so no validation is attached to it.
        // ------------------------------------------------------------

        private void LoadRoutes()
        {
            string query = @"
                SELECT
                    r.RouteId,
                    r.RouteName
                FROM dbo.Routes r
                WHERE r.DealerId = @DealerId
                ORDER BY r.RouteName";

            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        ddlRoute.Items.Clear();

                        ddlRoute.Items.Add(
                            new ListItem("All routes", "0"));

                        while (dr.Read())
                        {
                            ddlRoute.Items.Add(
                                new ListItem(
                                    dr["RouteName"].ToString(),
                                    dr["RouteId"].ToString()));
                        }
                    }
                }
            }
        }


        // ------------------------------------------------------------
        // REPORT
        // ------------------------------------------------------------

        private void LoadRouteHistory()
        {
            DateTime fromDate;
            DateTime toDate;

            bool hasFrom = TryDate(txtFromDate.Text, out fromDate);
            bool hasTo = TryDate(txtToDate.Text, out toDate);

            if (hasFrom && hasTo && fromDate > toDate)
            {
                ShowMessage(
                    "The From date cannot be later than the To date.",
                    false);

                gvRouteHistory.DataSource = null;

                gvRouteHistory.DataBind();

                return;
            }


            int routeId = 0;

            int.TryParse(ddlRoute.SelectedValue, out routeId);


            string query = @"
                /* Dates the route carried stock onto a vehicle. */

                ;WITH dayLoad AS
                (
                    SELECT
                        rs.RouteId,
                        ActivityDate = CONVERT(DATE, vs.StockDate),
                        VehicleId    = MAX(vs.VehicleId),
                        LoadedPackets = SUM(vs.LoadedPackets),
                        SoldPackets   = SUM(vs.SoldPackets)
                    FROM dbo.VehicleStock vs
                    INNER JOIN dbo.RouteSchedules rs
                        ON rs.RouteScheduleId = vs.RouteScheduleId
                    WHERE vs.DealerId = @DealerId
                      AND vs.RouteScheduleId IS NOT NULL
                    GROUP BY rs.RouteId, CONVERT(DATE, vs.StockDate)
                ),

                /* Dates the route collected orders. */

                dayOrders AS
                (
                    SELECT
                        rs.RouteId,
                        ActivityDate = CONVERT(DATE, o.OrderDate),
                        OrderCount = COUNT(o.OrderId),
                        OrderValue = SUM(o.GrandTotal)
                    FROM dbo.Orders o
                    INNER JOIN dbo.RouteSchedules rs
                        ON rs.RouteScheduleId = o.RouteScheduleId
                    WHERE o.DealerId = @DealerId
                      AND o.RouteScheduleId IS NOT NULL
                    GROUP BY rs.RouteId, CONVERT(DATE, o.OrderDate)
                ),

                /* Dates a route-day was stamped closed. StockDate is only
                   written when the day is closed, so it doubles as the
                   closure date - see SetRouteDayClosed. */

                dayClosed AS
                (
                    SELECT
                        rs.RouteId,
                        ActivityDate = rs.StockDate,
                        DayOfWeek  = MAX(rs.DayOfWeek),
                        IsClosed   = MAX(
                                        CASE WHEN rs.IsClosed = 1
                                             THEN 1 ELSE 0 END)
                    FROM dbo.RouteSchedules rs
                    INNER JOIN dbo.Routes r
                        ON r.RouteId = rs.RouteId
                    WHERE r.DealerId = @DealerId
                      AND rs.StockDate IS NOT NULL
                    GROUP BY rs.RouteId, rs.StockDate
                ),

                activity AS
                (
                    SELECT RouteId, ActivityDate FROM dayLoad
                    UNION
                    SELECT RouteId, ActivityDate FROM dayOrders
                    UNION
                    SELECT RouteId, ActivityDate FROM dayClosed
                )

                SELECT
                    ActivityDateText =
                        CONVERT(NVARCHAR(10), a.ActivityDate, 120),
                    DayOfWeek =
                        ISNULL(c.DayOfWeek,
                               DATENAME(WEEKDAY, a.ActivityDate)),
                    RouteName = r.RouteName,
                    VehicleNumber =
                        ISNULL(v.VehicleNumber, '-'),
                    SalesmanName =
                        ISNULL(su.FullName, '-'),
                    DriverName =
                        ISNULL(du.FullName, '-'),
                    LoadedPackets = ISNULL(dl.LoadedPackets, 0),
                    SoldPackets   = ISNULL(dl.SoldPackets, 0),
                    OrderCount    = ISNULL(dord.OrderCount, 0),
                    OrderValue    = ISNULL(dord.OrderValue, 0),
                    IsClosed      = ISNULL(c.IsClosed, 0)
                FROM activity a
                INNER JOIN dbo.Routes r
                    ON r.RouteId = a.RouteId
                    AND r.DealerId = @DealerId
                LEFT JOIN dayLoad dl
                    ON dl.RouteId = a.RouteId
                   AND dl.ActivityDate = a.ActivityDate
                LEFT JOIN dayOrders dord
                    ON dord.RouteId = a.RouteId
                   AND dord.ActivityDate = a.ActivityDate
                LEFT JOIN dayClosed c
                    ON c.RouteId = a.RouteId
                   AND c.ActivityDate = a.ActivityDate
                LEFT JOIN dbo.Vehicles v
                    ON v.VehicleId = ISNULL(
                         dl.VehicleId,
                         ISNULL(r.DefaultVehicleId, 0))
                LEFT JOIN dbo.Users su
                    ON su.UserId = r.PreferredSalesmanId
                LEFT JOIN dbo.Users du
                    ON du.UserId = r.PreferredDriverId
                WHERE (@RouteId = 0
                       OR r.RouteId = @RouteId)
                  AND (@FromDate IS NULL
                       OR a.ActivityDate >= @FromDate)
                  AND (@ToDate IS NULL
                       OR a.ActivityDate <= @ToDate)
                ORDER BY a.ActivityDate DESC,
                         r.RouteName";


            using (SqlConnection con = DatabaseHelper.GetConnection())
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
                        SqlDbType.Date).Value = hasFrom
                            ? (object)fromDate
                            : DBNull.Value;

                    cmd.Parameters.Add(
                        "@ToDate",
                        SqlDbType.Date).Value = hasTo
                            ? (object)toDate
                            : DBNull.Value;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        if (dt.Rows.Count == 0)
                        {
                            ShowMessage(
                                "No route runs found for the selected filters.",
                                true);
                        }
                        else
                        {
                            lblMessage.Visible = false;
                        }

                        gvRouteHistory.DataSource = dt;

                        gvRouteHistory.DataBind();
                    }
                }
            }
        }


        // ------------------------------------------------------------
        // FILTER EVENTS
        // ------------------------------------------------------------

        private static bool TryDate(string text, out DateTime value)
        {
            return DateTime.TryParse(
                (text ?? string.Empty).Trim(),
                out value);
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
            System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            gvRouteHistory.PageIndex = e.NewPageIndex;

            LoadRouteHistory();
        }

    }
}
