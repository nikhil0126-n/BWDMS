using System;
using System.Data;
using System.Data.SqlClient;
using BWDMS.Data;

namespace BWDMS.Salesman
{
    public partial class Shops : System.Web.UI.Page
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
                LoadShops();

                if (Request.QueryString["denied"] == "1")
                {
                    ShowMessage(
                        "That shop is not on your routes.",
                        false);
                }
            }
        }


        // ============================================================
        // SHOPS ON THIS SALESMAN'S ROUTES / SCHEDULES
        //
        // A shop can be tied to a route, to a schedule, or to both.
        // The salesman sees it if either points at their work.
        // ============================================================

        private void LoadShops()
        {
            string search = txtSearch.Text.Trim();

            string query = @"
                SELECT
                    s.ShopId,
                    s.ShopCode,
                    s.ShopName,
                    s.OwnerName,
                    s.Phone,
                    s.Address,
                    VillageName = ISNULL(v.VillageName, '-'),
                    VisitText = ISNULL(
                        CASE
                            WHEN rs.DayOfWeek IS NOT NULL
                                THEN rs.DayOfWeek
                            ELSE r.RouteName
                        END, '-'),
                    DealerId = s.DealerId
                FROM Shops s
                LEFT JOIN Villages v
                    ON v.VillageId = s.VillageId
                LEFT JOIN Routes r
                    ON r.RouteId = s.RouteId
                LEFT JOIN RouteSchedules rs
                    ON rs.RouteScheduleId = s.RouteScheduleId
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
                  AND
                  (
                    s.ShopName LIKE @Search
                    OR ISNULL(s.ShopCode, '') LIKE @Search
                    OR ISNULL(s.OwnerName, '') LIKE @Search
                    OR ISNULL(s.Phone, '') LIKE @Search
                  )
                ORDER BY s.ShopName";

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

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        gvShops.DataSource = dt;
                        gvShops.DataBind();
                    }
                }
            }
        }


        protected void txtSearch_TextChanged(
            object sender,
            EventArgs e)
        {
            LoadShops();
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
