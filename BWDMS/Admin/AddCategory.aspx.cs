using System;
using System.Data;
using System.Data.SqlClient;
using BWDMS.Data;

namespace BWDMS.Admin
{
    public partial class AddCategory : System.Web.UI.Page
    {
        // ============================================================
        // EDIT ID (0 = Add mode)
        // ============================================================

        private int CategoryId
        {
            get
            {
                int id;

                if (int.TryParse(
                    Request.QueryString["id"],
                    out id))
                {
                    return id;
                }

                return 0;
            }
        }


        private int AdminId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

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
                    "Admin",
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
                if (CategoryId > 0)
                {
                    SetEditMode();
                    LoadCategory();
                }
                else
                {
                    SetAddMode();
                }
            }
        }


        private void SetAddMode()
        {
            lblPageTitle.Text = "Add Category";
            lblHeading.Text = "Add Category";
            lblSubHeading.Text =
                "Create a product category for the catalogue";
            btnSave.Text = "Create Category";
        }


        private void SetEditMode()
        {
            lblPageTitle.Text = "Edit Category";
            lblHeading.Text = "Edit Category";
            lblSubHeading.Text = "Update category details";
            btnSave.Text = "Update Category";
        }


        private void LoadCategory()
        {
            try
            {
                string query = @"
                    SELECT
                        CategoryName,
                        Description,
                        SortOrder,
                        IsActive
                    FROM ProductCategories
                    WHERE CategoryId = @CategoryId";

                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    con.Open();

                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@CategoryId",
                            SqlDbType.Int).Value =
                            CategoryId;

                        using (SqlDataReader reader =
                            cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                Response.Redirect(
                                    "~/Admin/Categories.aspx");
                                return;
                            }

                            SetEditMode();

                            txtCategoryName.Text =
                                reader["CategoryName"].ToString();

                            txtDescription.Text =
                                reader["Description"] == DBNull.Value
                                    ? ""
                                    : reader["Description"].ToString();

                            txtSortOrder.Text =
                                reader["SortOrder"] == DBNull.Value
                                    ? "0"
                                    : reader["SortOrder"].ToString();

                            ddlStatus.SelectedValue =
                                Convert.ToBoolean(reader["IsActive"])
                                    ? "1"
                                    : "0";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error loading category: " + ex.Message,
                    false);
            }
        }


        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            string categoryName =
                txtCategoryName.Text.Trim();

            string description =
                txtDescription.Text.Trim();

            int sortOrder = 0;

            int.TryParse(txtSortOrder.Text.Trim(), out sortOrder);

            bool isActive =
                ddlStatus.SelectedValue == "1";

            bool isEdit = CategoryId > 0;

            int currentUserId = AdminId;


            try
            {
                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    con.Open();

                    using (SqlTransaction tx =
                        con.BeginTransaction())
                    {
                        try
                        {
                            // =========================================
                            // DUPLICATE CHECK
                            // UQ_ProductCategories_Name is unique.
                            // =========================================

                            string checkQuery = @"
                                SELECT COUNT(*)
                                FROM ProductCategories
                                WHERE CategoryName = @CategoryName";

                            if (isEdit)
                            {
                                checkQuery +=
                                    " AND CategoryId <> @CategoryId";
                            }

                            using (SqlCommand cmd =
                                new SqlCommand(
                                    checkQuery,
                                    con,
                                    tx))
                            {
                                cmd.Parameters.Add(
                                    "@CategoryName",
                                    SqlDbType.NVarChar,
                                    300).Value =
                                    categoryName;

                                if (isEdit)
                                {
                                    cmd.Parameters.Add(
                                        "@CategoryId",
                                        SqlDbType.Int).Value =
                                        CategoryId;
                                }

                                int count =
                                    Convert.ToInt32(
                                        cmd.ExecuteScalar());

                                if (count > 0)
                                {
                                    tx.Rollback();

                                    ShowMessage(
                                        "A category with this name already exists.",
                                        false);

                                    return;
                                }
                            }


                            if (isEdit)
                            {
                                UpdateCategory(
                                    con,
                                    tx,
                                    categoryName,
                                    description,
                                    sortOrder,
                                    isActive,
                                    currentUserId);
                            }
                            else
                            {
                                InsertCategory(
                                    con,
                                    tx,
                                    categoryName,
                                    description,
                                    sortOrder,
                                    isActive,
                                    currentUserId);
                            }

                            tx.Commit();
                        }
                        catch
                        {
                            tx.Rollback();
                            throw;
                        }
                    }
                }

                Response.Redirect(
                    "~/Admin/Categories.aspx?saved=1");
            }
            catch (SqlException ex)
                when (ex.Number == 2601 || ex.Number == 2627)
            {
                ShowMessage(
                    "A category with this name already exists.",
                    false);
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error saving category: " + ex.Message,
                    false);
            }
        }


        private void InsertCategory(
            SqlConnection con,
            SqlTransaction tx,
            string categoryName,
            string description,
            int sortOrder,
            bool isActive,
            int userId)
        {
            string query = @"
                INSERT INTO ProductCategories
                (
                    CategoryName,
                    Description,
                    SortOrder,
                    IsActive,
                    CreatedBy,
                    CreatedAt
                )
                VALUES
                (
                    @CategoryName,
                    @Description,
                    @SortOrder,
                    @IsActive,
                    @CreatedBy,
                    GETDATE()
                )";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                AddParameters(
                    cmd,
                    categoryName,
                    description,
                    sortOrder,
                    isActive);

                cmd.Parameters.Add(
                    "@CreatedBy",
                    SqlDbType.Int).Value = userId;

                cmd.ExecuteNonQuery();
            }
        }


        private void UpdateCategory(
            SqlConnection con,
            SqlTransaction tx,
            string categoryName,
            string description,
            int sortOrder,
            bool isActive,
            int userId)
        {
            string query = @"
                UPDATE ProductCategories
                SET
                    CategoryName = @CategoryName,
                    Description = @Description,
                    SortOrder = @SortOrder,
                    IsActive = @IsActive,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE CategoryId = @CategoryId";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                AddParameters(
                    cmd,
                    categoryName,
                    description,
                    sortOrder,
                    isActive);

                cmd.Parameters.Add(
                    "@UpdatedBy",
                    SqlDbType.Int).Value = userId;

                cmd.Parameters.Add(
                    "@CategoryId",
                    SqlDbType.Int).Value = CategoryId;

                int rows = cmd.ExecuteNonQuery();

                if (rows == 0)
                {
                    throw new InvalidOperationException(
                        "The category could not be updated.");
                }
            }
        }


        private void AddParameters(
            SqlCommand cmd,
            string categoryName,
            string description,
            int sortOrder,
            bool isActive)
        {
            cmd.Parameters.Add(
                "@CategoryName",
                SqlDbType.NVarChar,
                300).Value = categoryName;

            cmd.Parameters.Add(
                "@Description",
                SqlDbType.NVarChar,
                1000).Value =
                string.IsNullOrWhiteSpace(description)
                    ? (object)DBNull.Value
                    : description;

            cmd.Parameters.Add(
                "@SortOrder",
                SqlDbType.Int).Value = sortOrder;

            cmd.Parameters.Add(
                "@IsActive",
                SqlDbType.Bit).Value = isActive;
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
