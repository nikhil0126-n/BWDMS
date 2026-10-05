using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Salesman
{
    public partial class Routes : System.Web.UI.Page
    {
        private int SalesmanId
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
                    "Salesman",
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

                if (Request.QueryString["saved"] == "1")
                {
                    ShowMessage(
                        "Saved successfully.",
                        true);
                }
            }
        }


        // ============================================================
        // ROUTE SCHEDULES THIS SALESMAN WORKS
        //
        // Routes are permanent; a schedule is the repeatable visit.
        // ============================================================

        private void LoadRoutes()
        {
            string search = txtSearch.Text.Trim();
            string status = ddlStatus.SelectedValue;

            string query = @"
                SELECT
                    rs.RouteScheduleId,
                    r.RouteName,
                    ISNULL(r.RouteCode, '') AS RouteCode,
                    ISNULL(rs.DayOfWeek, '-') AS DayOfWeek,
                    ISNULL(r.OrderDispatchDays, '-') AS OrderDispatchDays,
                    ISNULL(r.RouteType, '-') AS RouteType,
                    VillageCount =
                        ISNULL(
                            (
                                SELECT COUNT(*)
                                FROM RouteVillageSchedules rvs
                                WHERE rvs.RouteScheduleId =
                                      rs.RouteScheduleId
                                  AND rvs.IsActive = 1
                            ),
                            0),
                    VehicleNumber = ISNULL(v.VehicleNumber, '-'),
                    rs.IsActive
                FROM RouteSchedules rs
                INNER JOIN Routes r
                    ON r.RouteId = rs.RouteId
                LEFT JOIN Vehicles v
                    ON v.VehicleId = rs.VehicleId
                WHERE rs.SalesmanId = @SalesmanId
                  AND
                  (
                    r.RouteName LIKE @Search
                    OR ISNULL(r.RouteCode, '') LIKE @Search
                    OR ISNULL(rs.DayOfWeek, '') LIKE @Search
                  )
                  AND (@Status = '' OR CONVERT(NVARCHAR(10), rs.IsActive) = @Status)
                ORDER BY r.RouteName, rs.RouteScheduleId";

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@SalesmanId",
                        SqlDbType.Int).Value = SalesmanId;

                    cmd.Parameters.AddWithValue(
                        "@Search",
                        "%" + search + "%");

                    cmd.Parameters.AddWithValue(
                        "@Status",
                        status);

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            gvRoutes.DataSource = dt;
            gvRoutes.DataBind();
        }


        protected void txtSearch_TextChanged(
            object sender,
            EventArgs e)
        {
            LoadRoutes();
        }


        protected void FilterChanged(
            object sender,
            EventArgs e)
        {
            LoadRoutes();
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
    }
}
