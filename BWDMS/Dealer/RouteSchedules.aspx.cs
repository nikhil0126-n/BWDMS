
using System;
using System.Data;
using System.Data.SqlClient;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class RouteSchedules : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Account/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }


            // ========================================================
            // CHECK DEALER ROLE
            //
            // This page previously accepted any logged-in user and
            // returned every dealer's schedules. A dealer must only
            // ever see schedules for routes owned by that dealer.
            // ========================================================

            if (Session["UserRole"] == null ||
                Session["UserRole"].ToString() != "Dealer")
            {
                Response.Redirect(BWDMS.Data.AppAuth.HomeUrl(Session["UserRole"]), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadSchedules();
            }
        }

        private void LoadSchedules()
        {
            DataTable dt = new DataTable();

            int dealerId =
                Convert.ToInt32(Session["UserId"]);


            // DealerId is reached through Routes, which owns the schedule.
            string query = @"
                SELECT
                    rs.RouteScheduleId,
                    rs.DayOfWeek,
                    rs.IsActive,
                    rs.VehicleId,
                    rs.SalesmanId,
                    rs.DriverId,
                    rs.RouteId,

                    r.RouteName,
                    r.RouteCode,

                    ISNULL(v.VehicleNumber, 'Not Assigned')
                        AS VehicleNumber,

                    ISNULL(s.FullName, 'Not Assigned')
                        AS SalesmanName,

                    ISNULL(d.FullName, 'Not Assigned')
                        AS DriverName,

                    rs.IsClosed,
                    ClosedDateText =
                        CASE
                            WHEN rs.IsClosed = 0 THEN '-'
                            ELSE CONVERT(NVARCHAR(10), rs.StockDate, 120)
                        END,

                    (
                        SELECT COUNT(*)
                        FROM RouteVillages rv
                        WHERE rv.RouteId = r.RouteId
                        AND rv.IsActive = 1
                    ) AS VillageCount,

                    (
                        SELECT COUNT(*)
                        FROM RouteVillageSchedules rvs
                        WHERE rvs.RouteScheduleId = rs.RouteScheduleId
                        AND rvs.IsActive = 1
                    ) AS ScheduledVillageCount

                FROM RouteSchedules rs

                INNER JOIN Routes r
                    ON rs.RouteId = r.RouteId
                    AND r.DealerId = @DealerId

                LEFT JOIN Vehicles v
                    ON rs.VehicleId = v.VehicleId

                LEFT JOIN Users s
                    ON rs.SalesmanId = s.UserId

                LEFT JOIN Users d
                    ON rs.DriverId = d.UserId

                WHERE
                    (@DayOfWeek = ''
                    OR rs.DayOfWeek = @DayOfWeek)

                ORDER BY
                    CASE rs.DayOfWeek
                        WHEN 'Monday' THEN 1
                        WHEN 'Tuesday' THEN 2
                        WHEN 'Wednesday' THEN 3
                        WHEN 'Thursday' THEN 4
                        WHEN 'Friday' THEN 5
                        WHEN 'Saturday' THEN 6
                        WHEN 'Sunday' THEN 7
                        ELSE 8
                    END,
                    r.RouteName;
            ";

            using (SqlConnection connection =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand command =
                    new SqlCommand(query, connection))
                {
                    command.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value =
                        dealerId;

                    command.Parameters.Add(
                        "@DayOfWeek",
                        SqlDbType.NVarChar,
                        40).Value =
                        ddlDayFilter.SelectedValue ?? "";

                    using (SqlDataAdapter adapter =
                        new SqlDataAdapter(command))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            gvSchedules.DataSource = dt;

            gvSchedules.DataBind();

            lblTotal.Text = dt.Rows.Count + " schedules";
        }

        protected void ddlDayFilter_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            LoadSchedules();
        }
    }
}
