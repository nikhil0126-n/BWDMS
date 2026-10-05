using System;
using System.Data;
using System.Data.SqlClient;
using BWDMS.Data;

namespace BWDMS.Admin
{
    public partial class Categories : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
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
                    BWDMS.Data.AppAuth.HomeUrl(
                        Session["UserRole"]),
                    false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }


            if (!IsPostBack)
            {
                LoadCategories();

                if (Request.QueryString["saved"] == "1")
                {
                    ShowMessage(
                        "Category saved successfully.",
                        true);
                }
            }
        }


        // ==============================================
        // LOAD CATEGORIES (with product count)
        // ==============================================

        private void LoadCategories()
        {
            string search = txtSearch.Text.Trim();

            string query = @"
                SELECT
                    c.CategoryId,
                    c.CategoryName,
                    c.Description,
                    c.SortOrder,
                    c.IsActive,
                    ProductCount =
                    (
                        SELECT COUNT(*)
                        FROM Products p
                        WHERE p.CategoryId = c.CategoryId
                    )
                FROM ProductCategories c
                WHERE c.CategoryName LIKE @Search
                   OR ISNULL(c.Description, '') LIKE @Search
                ORDER BY c.SortOrder, c.CategoryName";

            using (SqlConnection connection =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand command =
                    new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue(
                        "@Search",
                        "%" + search + "%");

                    using (SqlDataAdapter adapter =
                        new SqlDataAdapter(command))
                    {
                        DataTable table = new DataTable();

                        adapter.Fill(table);

                        gvCategories.DataSource = table;
                        gvCategories.DataBind();
                    }
                }
            }
        }


        protected void txtSearch_TextChanged(
            object sender,
            EventArgs e)
        {
            LoadCategories();
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
