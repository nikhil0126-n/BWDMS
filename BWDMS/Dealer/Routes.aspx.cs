using System;
using System.Data;
using System.Data.SqlClient;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class Routes : System.Web.UI.Page
    {
        // ============================================================
        // PAGE LOAD
        // ============================================================

        protected void Page_Load(object sender, EventArgs e)
        {
            // --------------------------------------------------------
            // Prevent browser cache
            // --------------------------------------------------------

            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            Response.Cache.SetExpires(
                DateTime.UtcNow.AddDays(-1));

            Response.Cache.SetRevalidation(
                System.Web.HttpCacheRevalidation.AllCaches);


            // --------------------------------------------------------
            // Check login
            // --------------------------------------------------------

            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }


            // --------------------------------------------------------
            // Check Dealer role
            // --------------------------------------------------------

            if (Session["UserRole"] == null ||
                Session["UserRole"].ToString() != "Dealer")
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }


            // --------------------------------------------------------
            // Load all routes only once
            // --------------------------------------------------------

            if (!IsPostBack)
            {
                LoadRoutes();
            }
        }


        // ============================================================
        // LOAD ALL ROUTES FOR CURRENT DEALER
        // ============================================================

        private void LoadRoutes()
        {
            try
            {
                // ----------------------------------------------------
                // Get logged-in dealer ID
                // ----------------------------------------------------

                int dealerId =
                    Convert.ToInt32(Session["UserId"]);


                // ----------------------------------------------------
                // IMPORTANT:
                // No search condition here.
                //
                // We load ALL routes belonging to this dealer.
                // JavaScript will perform live search.
                // ----------------------------------------------------

                string query = @"
                    SELECT
                        RouteId,
                        RouteCode,
                        RouteName,
                        DayOfWeek,
                        IsActive
                    FROM Routes
                    WHERE DealerId = @DealerId
                    ORDER BY
                        CASE DayOfWeek
                            WHEN 'Monday' THEN 1
                            WHEN 'Tuesday' THEN 2
                            WHEN 'Wednesday' THEN 3
                            WHEN 'Thursday' THEN 4
                            WHEN 'Friday' THEN 5
                            WHEN 'Saturday' THEN 6
                            WHEN 'Sunday' THEN 7
                            ELSE 8
                        END,
                        RouteName";


                // ----------------------------------------------------
                // Database connection
                // ----------------------------------------------------

                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    con.Open();


                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        // ------------------------------------------------
                        // Dealer ID
                        // ------------------------------------------------

                        cmd.Parameters.Add(
                            "@DealerId",
                            SqlDbType.Int).Value =
                            dealerId;


                        // ------------------------------------------------
                        // Get data
                        // ------------------------------------------------

                        using (SqlDataAdapter da =
                            new SqlDataAdapter(cmd))
                        {
                            DataTable dt =
                                new DataTable();

                            da.Fill(dt);


                            // ------------------------------------------------
                            // Bind GridView
                            // ------------------------------------------------

                            gvRoutes.DataSource = dt;

                            gvRoutes.DataBind();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Write(
                    "<script>alert('Error loading routes: " +
                    Server.HtmlEncode(ex.Message) +
                    "');</script>");
            }
        }
    }
}