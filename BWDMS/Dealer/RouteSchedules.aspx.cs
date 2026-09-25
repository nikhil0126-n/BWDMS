
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace BWDMS.Dealer
{
    public partial class RouteSchedules : System.Web.UI.Page
    {
        private readonly string connectionString =
            ConfigurationManager
                .ConnectionStrings["BWDMSConnection"]
                .ConnectionString;

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

            if (!IsPostBack)
            {
                LoadSchedules();
            }
        }

        private void LoadSchedules()
        {
            DataTable dt = new DataTable();

            string query = @"
                SELECT
                    rs.RouteScheduleId,
                    rs.DayOfWeek,
                    rs.IsActive,

                    r.RouteName,

                    ISNULL(v.VehicleNumber, 'Not Assigned')
                        AS VehicleNumber,

                    ISNULL(s.FullName, 'Not Assigned')
                        AS SalesmanName,

                    ISNULL(d.FullName, 'Not Assigned')
                        AS DriverName

                FROM RouteSchedules rs

                INNER JOIN Routes r
                    ON rs.RouteId = r.RouteId

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
                new SqlConnection(connectionString))
            {
                using (SqlCommand command =
                    new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue(
                        "@DayOfWeek",
                        ddlDayFilter.SelectedValue ?? "");

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