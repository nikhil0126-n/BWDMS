using System;
using System.Data;
using System.Data.SqlClient;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class Salesmen : System.Web.UI.Page
    {
        // ============================================================
        // PAGE LOAD
        // ============================================================

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            Response.Cache.SetExpires(
                DateTime.UtcNow.AddDays(-1));

            Response.Cache.SetRevalidation(
                System.Web.HttpCacheRevalidation.AllCaches);


            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            if (Session["UserRole"] == null ||
                Session["UserRole"].ToString() != "Dealer")
            {
                Response.Redirect(
                    BWDMS.Data.AppAuth.HomeUrl(Session["UserRole"]));
                return;
            }


            if (!IsPostBack)
            {
                LoadSalesmen();

                if (Request.QueryString["saved"] == "1")
                {
                    ShowMessage("Account saved successfully.", true);
                }
            }
        }


        // ============================================================
        // LOAD SALESMEN + DRIVERS BELONGING TO THIS DEALER
        //
        // Driver is a supported role even though no Driver row
        // exists yet, so drivers created here show up correctly.
        // ============================================================

        private void LoadSalesmen()
        {
            try
            {
                int dealerId =
                    Convert.ToInt32(Session["UserId"]);

                string query = @"
                    SELECT
                        u.UserId,
                        u.FullName,
                        u.Email,
                        u.Phone,
                        u.Role,
                        u.IsActive,
                        ScheduleCount =
                        (
                            SELECT COUNT(*)
                            FROM RouteSchedules rs
                            WHERE rs.SalesmanId = u.UserId
                               OR rs.DriverId = u.UserId
                        )
                    FROM Users u
                    WHERE u.DealerId = @DealerId
                    AND u.Role = 'Salesman'
                    ORDER BY u.FullName";

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

                        using (SqlDataAdapter da =
                            new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();

                            da.Fill(dt);

                            gvSalesmen.DataSource = dt;
                            gvSalesmen.DataBind();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error loading accounts: " +
                    ex.Message,
                    false);
            }
        }


        private void ShowMessage(
            string message,
            bool success)
        {
            pnlMessage.Visible = true;

            lblMessage.Text = message;

            pnlMessage.CssClass = success
                ? "alert alert-success mb-4"
                : "alert alert-danger mb-4";
        }
    }
}
