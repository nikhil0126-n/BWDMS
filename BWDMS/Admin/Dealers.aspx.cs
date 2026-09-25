using System;
using System.Data;
using System.Data.SqlClient;
using BWDMS.Data;

namespace BWDMS.Admin
{
    public partial class Dealers : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // ==========================================
            // PREVENT BROWSER CACHE
            // ==========================================

            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            Response.Cache.SetExpires(
                DateTime.UtcNow.AddYears(-1));

            Response.Cache.SetRevalidation(
                System.Web.HttpCacheRevalidation.AllCaches);


            // ==========================================
            // CHECK LOGIN
            // ==========================================

            if (Session["UserId"] == null)
            {
                Response.Redirect(
                    "~/Account/Login.aspx",
                    false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }


            // ==========================================
            // ONLY ADMIN CAN ACCESS
            // ==========================================

            if (Session["UserRole"] == null ||
                !Session["UserRole"].ToString().Equals(
                    "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect(
                    "~/Account/Login.aspx",
                    false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }


            // ==========================================
            // LOAD DEALERS
            // ==========================================

            if (!IsPostBack)
            {
                LoadDealers();
            }
        }


        // ==============================================
        // LOAD DEALERS
        // ==============================================

        private void LoadDealers()
        {
            string search =
                txtSearch.Text.Trim();


            using (SqlConnection connection =
                   DatabaseHelper.GetConnection())
            {
                string query = @"
                    SELECT
                        UserId,
                        FullName,
                        Email,
                        Phone,
                        IsActive
                    FROM Users
                    WHERE Role = 'Dealer'
                    AND
                    (
                        FullName LIKE @Search
                        OR Email LIKE @Search
                        OR Phone LIKE @Search
                    )
                    ORDER BY UserId DESC";


                using (SqlCommand command =
                       new SqlCommand(
                           query,
                           connection))
                {
                    command.Parameters.AddWithValue(
                        "@Search",
                        "%" + search + "%");


                    using (SqlDataAdapter adapter =
                           new SqlDataAdapter(command))
                    {
                        DataTable table =
                            new DataTable();


                        adapter.Fill(table);


                        gvDealers.DataSource =
                            table;

                        gvDealers.DataBind();
                    }
                }
            }
        }


        // ==============================================
        // SEARCH
        // ==============================================

        protected void txtSearch_TextChanged(
            object sender,
            EventArgs e)
        {
            LoadDealers();
        }


        // ==============================================
        // GRIDVIEW COMMAND
        // ==============================================

        protected void gvDealers_RowCommand(
            object sender,
            System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ToggleStatus")
            {
                int userId =
                    Convert.ToInt32(
                        e.CommandArgument);


                ToggleDealerStatus(userId);


                LoadDealers();
            }
        }


        // ==============================================
        // TOGGLE DEALER STATUS
        // ==============================================

        private void ToggleDealerStatus(
            int userId)
        {
            using (SqlConnection connection =
                   DatabaseHelper.GetConnection())
            {
                string query = @"
                    UPDATE Users
                    SET IsActive =
                        CASE
                            WHEN IsActive = 1 THEN 0
                            ELSE 1
                        END
                    WHERE UserId = @UserId
                    AND Role = 'Dealer'";


                using (SqlCommand command =
                       new SqlCommand(
                           query,
                           connection))
                {
                    command.Parameters.AddWithValue(
                        "@UserId",
                        userId);


                    connection.Open();

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}