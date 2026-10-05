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
                        40).Value =
                        DateTime.Now.DayOfWeek.ToString();

                    con.Open();

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblRouteCount.Text =
                                reader["RouteCount"].ToString();

                            lblScheduleCount.Text =
                                reader["TodayCount"].ToString();
                        }
                    }
                }
            }


            // Villages on the salesman's routes
            string villageQuery = @"
                SELECT COUNT(*)
                FROM RouteVillages rv
                INNER JOIN Routes r
                    ON rv.RouteId = r.RouteId
                INNER JOIN RouteSchedules rs
                    ON rs.RouteId = r.RouteId
                WHERE rs.SalesmanId = @SalesmanId
                AND rs.IsActive = 1
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

                    lblVillageCount.Text =
                        Convert.ToInt32(
                            cmd.ExecuteScalar())
                        .ToString();
                }
            }


            // Vehicle assigned to today's schedule, if any
            string vehicleQuery = @"
                SELECT TOP 1
                    ISNULL(v.VehicleNumber, '-')
                FROM RouteSchedules rs
                LEFT JOIN Vehicles v
                    ON rs.VehicleId = v.VehicleId
                WHERE rs.SalesmanId = @SalesmanId
                AND rs.DayOfWeek = @Today
                AND rs.IsActive = 1;";

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
                        40).Value =
                        DateTime.Now.DayOfWeek.ToString();

                    con.Open();

                    object result =
                        cmd.ExecuteScalar();

                    lblVehicle.Text =
                        result == null || result == DBNull.Value
                            ? "-"
                            : result.ToString();
                }
            }
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
    }
}
