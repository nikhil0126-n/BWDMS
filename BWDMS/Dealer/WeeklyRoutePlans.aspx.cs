
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class WeeklyRoutePlans : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // ============================================
            // PREVENT BROWSER CACHE
            // ============================================

            Response.Cache.SetCacheability(
                HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            Response.Cache.SetExpires(
                DateTime.UtcNow.AddDays(-1));

            Response.Cache.SetRevalidation(
                HttpCacheRevalidation.AllCaches);


            // ============================================
            // CHECK LOGIN
            // ============================================

            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }


            // ============================================
            // CHECK DEALER ROLE
            // ============================================

            if (Session["UserRole"] == null ||
                Session["UserRole"].ToString() != "Dealer")
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }


            // ============================================
            // LOAD DATA ONLY ON FIRST PAGE LOAD
            // ============================================

            if (!IsPostBack)
            {
                LoadWeeklyRoutePlans();
            }
        }


        // ============================================
        // LOAD WEEKLY ROUTE PLANS
        // ============================================

        private void LoadWeeklyRoutePlans()
        {
            try
            {
                int dealerId =
                    Convert.ToInt32(Session["UserId"]);


                string search =
                    txtSearch.Text.Trim();


                string query = @"
                    SELECT
                        wrp.WeeklyRoutePlanId,
                        r.RouteCode,
                        r.RouteName,
                        wrp.WeekStartDate,
                        wrp.WeekEndDate,
                        wrp.IsActive
                    FROM WeeklyRoutePlans wrp

                    INNER JOIN Routes r
                        ON wrp.RouteId = r.RouteId

                    WHERE wrp.DealerId = @DealerId

                    AND
                    (
                        r.RouteCode LIKE @Search
                        OR r.RouteName LIKE @Search
                        OR CONVERT(NVARCHAR(20), wrp.WeekStartDate, 105)
                            LIKE @Search
                        OR CONVERT(NVARCHAR(20), wrp.WeekEndDate, 105)
                            LIKE @Search
                    )

                    ORDER BY
                        wrp.WeekStartDate DESC,
                        r.RouteName ASC";


                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    con.Open();


                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@DealerId",
                            SqlDbType.Int).Value =
                            dealerId;


                        cmd.Parameters.Add(
                            "@Search",
                            SqlDbType.NVarChar,
                            200).Value =
                            "%" + search + "%";


                        using (SqlDataAdapter da =
                            new SqlDataAdapter(cmd))
                        {
                            DataTable dt =
                                new DataTable();

                            da.Fill(dt);


                            gvWeeklyPlans.DataSource =
                                dt;

                            gvWeeklyPlans.DataBind();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message =
                    HttpUtility.JavaScriptStringEncode(
                        ex.Message);

                Response.Write(
                    "<script>alert('Error loading weekly route plans: "
                    + message
                    + "');</script>");
            }
        }
    }
}