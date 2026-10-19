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


                ShowFlashMessage();
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

                        litPager.Text = GridPager.Html(gvCategories);
                    }
                }
            }
        }


        protected void txtSearch_TextChanged(
            object sender,
            EventArgs e)
        {
            gvCategories.PageIndex = 0;

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


        // ==============================================
        // FLASH MESSAGE
        //
        // The message is stored in Session, shown once
        // and immediately cleared, so refreshing the
        // page never shows the same message again.
        // ==============================================

        private void ShowFlashMessage()
        {
            object text = Session["CategoriesFlashText"];

            if (text == null)
            {
                return;
            }

            bool success =
                Session["CategoriesFlashOk"] != null &&
                Session["CategoriesFlashOk"].ToString() == "1";

            Session.Remove("CategoriesFlashText");
            Session.Remove("CategoriesFlashOk");

            ShowMessage(text.ToString(), success);
        }


        private void SetFlashMessage(
            string message,
            bool success)
        {
            Session["CategoriesFlashText"] = message;

            Session["CategoriesFlashOk"] = success ? "1" : "0";
        }


        // ==============================================
        // GRID COMMANDS
        // ==============================================

        protected void gvCategories_RowCommand(
            object sender,
            System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (e.CommandName != "DeleteCategory")
            {
                return;
            }

            int categoryId;

            if (!int.TryParse(
                e.CommandArgument.ToString(),
                out categoryId))
            {
                return;
            }

            // When DeleteCategory redirected, the page is
            // already being replaced - do not rebind.
            if (DeleteCategory(categoryId))
            {
                return;
            }

            LoadCategories();
        }


        // ==============================================
        // DELETE CATEGORY
        //
        // Returns true when the page redirected, so the
        // delete always ends in a Post / Redirect / Get.
        // Refreshing afterwards never repeats the action
        // and never shows the message again.
        // ==============================================

        private bool DeleteCategory(int categoryId)
        {
            string categoryName = "";

            try
            {
                using (SqlConnection connection =
                    DatabaseHelper.GetConnection())
                {
                    connection.Open();


                    // --------------------------------------
                    // LOAD THE TARGET CATEGORY
                    // --------------------------------------

                    string loadQuery = @"
                        SELECT CategoryName
                        FROM ProductCategories
                        WHERE CategoryId = @CategoryId";

                    using (SqlCommand command =
                        new SqlCommand(loadQuery, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@CategoryId",
                            categoryId);

                        object name =
                            command.ExecuteScalar();

                        if (name == null ||
                            name == DBNull.Value)
                        {
                            SetFlashMessage(
                                "This category was not found. " +
                                "It may already have been deleted.",
                                false);

                            GoToCategories();

                            return true;
                        }

                        categoryName = name.ToString();
                    }


                    // --------------------------------------
                    // A CATEGORY THAT STILL HAS PRODUCTS CAN
                    // NEVER BE REMOVED, OTHERWISE THE
                    // PRODUCTS WOULD BE LEFT WITHOUT ONE.
                    // --------------------------------------

                    string countQuery = @"
                        SELECT COUNT(*)
                        FROM Products
                        WHERE CategoryId = @CategoryId";

                    using (SqlCommand command =
                        new SqlCommand(countQuery, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@CategoryId",
                            categoryId);

                        int productCount =
                            Convert.ToInt32(
                                command.ExecuteScalar());

                        if (productCount > 0)
                        {
                            SetFlashMessage(
                                "\"" + categoryName + "\" is used by " +
                                productCount +
                                (productCount == 1
                                    ? " product"
                                    : " products") +
                                " and cannot be deleted. " +
                                "Set the category to Inactive instead.",
                                false);

                            GoToCategories();

                            return true;
                        }
                    }


                    // --------------------------------------
                    // DELETE
                    // --------------------------------------

                    string deleteQuery = @"
                        DELETE FROM ProductCategories
                        WHERE CategoryId = @CategoryId";

                    using (SqlCommand command =
                        new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@CategoryId",
                            categoryId);

                        command.ExecuteNonQuery();
                    }
                }

                SetFlashMessage(
                    "\"" + categoryName + "\" deleted successfully.",
                    true);
            }
            catch (SqlException ex)
                when (ex.Number == 547)
            {
                // Foreign key: products still point at this
                // category.
                SetFlashMessage(
                    "\"" + categoryName + "\" is linked to existing " +
                    "records and cannot be deleted. " +
                    "Set the category to Inactive instead.",
                    false);
            }
            catch (Exception ex)
            {
                SetFlashMessage(
                    "Error deleting \"" + categoryName + "\": " +
                    ex.Message,
                    false);
            }

            GoToCategories();

            return true;
        }


        // ==============================================
        // REDIRECT WITH THE MESSAGE IN SESSION
        // ==============================================

        private void GoToCategories()
        {
            Response.Redirect(
                "~/Admin/Categories.aspx",
                false);

            Context.ApplicationInstance.CompleteRequest();
        }


        // ------------------------------------------------------------
        // PAGING
        //
        // The new index has to be applied before the grid is rebound,
        // otherwise the postback lands back on the page it came from.
        // ------------------------------------------------------------

        protected void gvCategories_PageIndexChanging(
            object sender,
            System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            gvCategories.PageIndex = e.NewPageIndex;

            LoadCategories();
        }


        // The pager bar under the grid links back to the grid with a
        // "Page$N" argument. Event validation only accepts arguments that
        // were registered while rendering, which the grid's own (hidden)
        // pager would normally do, so register them here.
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            GridPager.RegisterForEventValidation(gvCategories);

            base.Render(writer);
        }

    }
}
