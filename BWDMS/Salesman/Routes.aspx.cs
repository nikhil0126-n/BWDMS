using System;
using System.Data;
using System.Data.SqlClient;
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
                LoadRoute();

                LoadStops();

                if (Request.QueryString["saved"] == "1")
                {
                    ShowMessage(
                        "Saved successfully.",
                        true);
                }
            }
        }


        // ============================================================
        // ROUTE SUMMARY
        //
        // The salesman's first route schedule drives the header card.
        // Routes are permanent; a schedule is the repeatable visit.
        // ============================================================

        private void LoadRoute()
        {
            string query = @"
                SELECT TOP 1
                    r.RouteName,
                    ISNULL(v.VehicleNumber, '-') AS VehicleNumber,
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
                    rs.IsActive
                FROM RouteSchedules rs
                INNER JOIN Routes r
                    ON r.RouteId = rs.RouteId
                LEFT JOIN Vehicles v
                    ON v.VehicleId = rs.VehicleId
                WHERE rs.SalesmanId = @SalesmanId
                ORDER BY r.RouteName, rs.RouteScheduleId";

            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@SalesmanId",
                        SqlDbType.Int).Value = SalesmanId;

                    DataTable dt = new DataTable();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }

                    if (dt.Rows.Count == 0)
                    {
                        lblRouteName.Text = "No route assigned";
                        lblRouteStatus.Text = "Unassigned";
                        lblVillages.Text = "0 villages";
                        lblVehicle.Text = "-";

                        return;
                    }

                    DataRow row = dt.Rows[0];

                    bool isActive =
                        row["IsActive"] != DBNull.Value &&
                        Convert.ToBoolean(row["IsActive"]);

                    int villageCount = Convert.ToInt32(row["VillageCount"]);

                    lblRouteName.Text =
                        Convert.ToString(row["RouteName"]);

                    lblVehicle.Text =
                        Convert.ToString(row["VehicleNumber"]);

                    lblRouteStatus.Text =
                        isActive ? "On Route" : "Inactive";

                    lblVillages.Text =
                        villageCount == 1
                            ? "1 village"
                            : villageCount + " villages";
                }
            }
        }


        // ============================================================
        // ROUTE STOPS
        //
        // Shops that sit on this salesman's routes / schedules.
        // A shop can be tied to a route, a schedule, or both.
        // ============================================================

        private void LoadStops()
        {
            string query = @"
                SELECT
                    s.ShopId,
                    s.ShopName,
                    ISNULL(vi.VillageName, '-') AS VillageName
                FROM Shops s
                LEFT JOIN Villages vi
                    ON vi.VillageId = s.VillageId
                WHERE s.IsActive = 1
                  AND
                  (
                    s.RouteScheduleId IN
                    (
                        SELECT rs2.RouteScheduleId
                        FROM RouteSchedules rs2
                        WHERE rs2.SalesmanId = @SalesmanId
                    )
                    OR s.RouteId IN
                    (
                        SELECT rs3.RouteId
                        FROM RouteSchedules rs3
                        WHERE rs3.SalesmanId = @SalesmanId
                    )
                  )
                ORDER BY s.ShopName";

            DataTable dt = new DataTable();

            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@SalesmanId",
                        SqlDbType.Int).Value = SalesmanId;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            gvRoutes.DataSource = dt;
            gvRoutes.DataBind();

            lblStopCount.Text = dt.Rows.Count.ToString();
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

        protected void gvRoutes_PageIndexChanging(
            object sender,
            System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            gvRoutes.PageIndex = e.NewPageIndex;

            LoadStops();
        }

    }
}
