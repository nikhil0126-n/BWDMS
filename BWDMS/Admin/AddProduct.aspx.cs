using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Admin
{
    public partial class AddProduct : System.Web.UI.Page
    {
        // ============================================================
        // EDIT ID (0 = Add mode)
        // ============================================================

        private int ProductId
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
                LoadCategoryDropdown();

                if (ProductId > 0)
                {
                    SetEditMode();
                    LoadProduct();
                }
                else
                {
                    SetAddMode();
                }
            }
        }


        private void SetAddMode()
        {
            lblPageTitle.Text = "Add Product";
            lblHeading.Text = "Add Product";
            lblSubHeading.Text =
                "Add a product to the Balaji Wafers catalogue";
            btnSave.Text = "Create Product";
        }


        private void SetEditMode()
        {
            lblPageTitle.Text = "Edit Product";
            lblHeading.Text = "Edit Product";
            lblSubHeading.Text = "Update product details";
            btnSave.Text = "Update Product";
        }


        private void LoadCategoryDropdown()
        {
            string query = @"
                SELECT CategoryId, CategoryName
                FROM ProductCategories
                WHERE IsActive = 1
                ORDER BY SortOrder, CategoryName";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        ddlCategory.DataSource = dt;
                        ddlCategory.DataTextField = "CategoryName";
                        ddlCategory.DataValueField = "CategoryId";
                        ddlCategory.DataBind();

                        ddlCategory.Items.Insert(
                            0,
                            new ListItem("Select Category", ""));
                    }
                }
            }
        }


        private void LoadProduct()
        {
            try
            {
                string query = @"
                    SELECT
                        ProductName,
                        ProductCode,
                        Description,
                        CategoryId,
                        IsActive
                    FROM Products
                    WHERE ProductId = @ProductId";

                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    con.Open();

                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@ProductId",
                            SqlDbType.Int).Value =
                            ProductId;

                        using (SqlDataReader reader =
                            cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                Response.Redirect(
                                    "~/Admin/Products.aspx");
                                return;
                            }

                            SetEditMode();

                            txtProductName.Text =
                                reader["ProductName"].ToString();

                            txtProductCode.Text =
                                reader["ProductCode"] == DBNull.Value
                                    ? ""
                                    : reader["ProductCode"].ToString();

                            txtDescription.Text =
                                reader["Description"] == DBNull.Value
                                    ? ""
                                    : reader["Description"].ToString();

                            SetSelectedValue(
                                ddlCategory,
                                reader["CategoryId"].ToString());

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
                    "Error loading product: " + ex.Message,
                    false);
            }
        }


        private void SetSelectedValue(
            System.Web.UI.WebControls.DropDownList dropdown,
            string value)
        {
            System.Web.UI.WebControls.ListItem item =
                dropdown.Items.FindByValue(value);

            if (item != null)
            {
                dropdown.SelectedValue = value;
            }
            else
            {
                dropdown.SelectedIndex = 0;
            }
        }


        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            string productName =
                txtProductName.Text.Trim();

            string productCode =
                txtProductCode.Text.Trim();

            string description =
                txtDescription.Text.Trim();

            int categoryId =
                Convert.ToInt32(ddlCategory.SelectedValue);

            bool isActive =
                ddlStatus.SelectedValue == "1";

            bool isEdit = ProductId > 0;


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
                            // SOFT DUPLICATE CHECK
                            //
                            // Products has no unique index on name or
                            // code, so this is application level only.
                            // =========================================

                            string checkQuery = @"
                                SELECT COUNT(*)
                                FROM Products
                                WHERE ProductName = @ProductName";

                            if (isEdit)
                            {
                                checkQuery +=
                                    " AND ProductId <> @ProductId";
                            }

                            using (SqlCommand cmd =
                                new SqlCommand(
                                    checkQuery,
                                    con,
                                    tx))
                            {
                                cmd.Parameters.Add(
                                    "@ProductName",
                                    SqlDbType.NVarChar,
                                    500).Value =
                                    productName;

                                if (isEdit)
                                {
                                    cmd.Parameters.Add(
                                        "@ProductId",
                                        SqlDbType.Int).Value =
                                        ProductId;
                                }

                                int count =
                                    Convert.ToInt32(
                                        cmd.ExecuteScalar());

                                if (count > 0)
                                {
                                    tx.Rollback();

                                    ShowMessage(
                                        "A product with this name already exists.",
                                        false);

                                    return;
                                }
                            }


                            if (isEdit)
                            {
                                UpdateProduct(
                                    con,
                                    tx,
                                    productName,
                                    productCode,
                                    description,
                                    categoryId,
                                    isActive);
                            }
                            else
                            {
                                InsertProduct(
                                    con,
                                    tx,
                                    productName,
                                    productCode,
                                    description,
                                    categoryId,
                                    isActive);
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
                    "~/Admin/Products.aspx?saved=1");
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error saving product: " + ex.Message,
                    false);
            }
        }


        private void InsertProduct(
            SqlConnection con,
            SqlTransaction tx,
            string productName,
            string productCode,
            string description,
            int categoryId,
            bool isActive)
        {
            string query = @"
                INSERT INTO Products
                (
                    ProductName,
                    ProductCode,
                    Description,
                    CategoryId,
                    IsActive,
                    CreatedBy,
                    CreatedAt
                )
                VALUES
                (
                    @ProductName,
                    @ProductCode,
                    @Description,
                    @CategoryId,
                    @IsActive,
                    @CreatedBy,
                    GETDATE()
                )";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                AddParameters(
                    cmd,
                    productName,
                    productCode,
                    description,
                    categoryId,
                    isActive);

                cmd.Parameters.Add(
                    "@CreatedBy",
                    SqlDbType.Int).Value = AdminId;

                cmd.ExecuteNonQuery();
            }
        }


        private void UpdateProduct(
            SqlConnection con,
            SqlTransaction tx,
            string productName,
            string productCode,
            string description,
            int categoryId,
            bool isActive)
        {
            string query = @"
                UPDATE Products
                SET
                    ProductName = @ProductName,
                    ProductCode = @ProductCode,
                    Description = @Description,
                    CategoryId = @CategoryId,
                    IsActive = @IsActive,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE ProductId = @ProductId";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                AddParameters(
                    cmd,
                    productName,
                    productCode,
                    description,
                    categoryId,
                    isActive);

                cmd.Parameters.Add(
                    "@UpdatedBy",
                    SqlDbType.Int).Value = AdminId;

                cmd.Parameters.Add(
                    "@ProductId",
                    SqlDbType.Int).Value = ProductId;

                int rows = cmd.ExecuteNonQuery();

                if (rows == 0)
                {
                    throw new InvalidOperationException(
                        "The product could not be updated.");
                }
            }
        }


        private void AddParameters(
            SqlCommand cmd,
            string productName,
            string productCode,
            string description,
            int categoryId,
            bool isActive)
        {
            cmd.Parameters.Add(
                "@ProductName",
                SqlDbType.NVarChar,
                500).Value = productName;

            cmd.Parameters.Add(
                "@ProductCode",
                SqlDbType.NVarChar,
                200).Value =
                string.IsNullOrWhiteSpace(productCode)
                    ? (object)DBNull.Value
                    : productCode;

            cmd.Parameters.Add(
                "@Description",
                SqlDbType.NVarChar,
                1000).Value =
                string.IsNullOrWhiteSpace(description)
                    ? (object)DBNull.Value
                    : description;

            cmd.Parameters.Add(
                "@CategoryId",
                SqlDbType.Int).Value = categoryId;

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
