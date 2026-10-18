using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
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

                // DISABLED - the inventory PDF import card is
                // commented out in AddProduct.aspx, so its dropdowns
                // are no longer filled.
                // LoadImportDropdowns();

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

            // DISABLED - the import panel, its preview grid, its
            // Import button and its summary label are all commented
            // out in AddProduct.aspx, so they are null here and must
            // not be touched. Kept for reference.
            //
            // Bulk import creates products, so it had no meaning
            // while an existing one was being edited.
            // pnlImport.Visible = (ProductId == 0);
            //
            // The preview and its Import button followed the rows
            // held in ViewState rather than a Visible flag, so
            // neither was lost on the postback that reads the sheet.
            // List<InventoryPdfRow> pending =
            //     ViewState[ImportRowsKey] as List<InventoryPdfRow>;
            //
            // pnlPreview.Visible =
            //     pending != null && pending.Count > 0;
            //
            // btnImportRows.Visible = pnlPreview.Visible;
            //
            // string summary = ViewState[ImportSummaryKey] as string;
            //
            // if (summary != null)
            // {
            //     litSummary.Text = summary;
            // }
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
                        IsActive,
                        ImagePath
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

                            // Held in ViewState so the preview tile
                            // can still show the stored image on the
                            // postback that saves the form.
                            ViewState["ImagePath"] =
                                reader["ImagePath"] == DBNull.Value
                                    ? null
                                    : reader["ImagePath"].ToString();

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

            // The image is written to disk before the row so the
            // path can go straight into the statement. A failed
            // transaction can only leave an unused file behind, it
            // can never leave a row pointing at a missing one.
            string imagePath;

            try
            {
                imagePath = ApplyImageChange(
                    ViewState["ImagePath"] as string);
            }
            catch (Exception imageError)
            {
                ShowMessage(
                    "Error saving image: " + imageError.Message,
                    false);

                return;
            }

            ViewState["ImagePath"] = imagePath;


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
                                    isActive,
                                    imagePath);
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
                                    isActive,
                                    imagePath);
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


        // Returns the new ProductId, because a variant can only
        // be attached once the product exists. The single existing
        // caller ignores the result.
        private int InsertProduct(
            SqlConnection con,
            SqlTransaction tx,
            string productName,
            string productCode,
            string description,
            int categoryId,
            bool isActive,
            string imagePath)
        {
            string query = @"
                INSERT INTO Products
                (
                    ProductName,
                    ProductCode,
                    Description,
                    CategoryId,
                    IsActive,
                    ImagePath,
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
                    @ImagePath,
                    @CreatedBy,
                    GETDATE()
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                AddParameters(
                    cmd,
                    productName,
                    productCode,
                    description,
                    categoryId,
                    isActive,
                    imagePath);

                cmd.Parameters.Add(
                    "@CreatedBy",
                    SqlDbType.Int).Value = AdminId;

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }


        private void UpdateProduct(
            SqlConnection con,
            SqlTransaction tx,
            string productName,
            string productCode,
            string description,
            int categoryId,
            bool isActive,
            string imagePath)
        {
            string query = @"
                UPDATE Products
                SET
                    ProductName = @ProductName,
                    ProductCode = @ProductCode,
                    Description = @Description,
                    CategoryId = @CategoryId,
                    IsActive = @IsActive,
                    ImagePath = @ImagePath,
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
                    isActive,
                    imagePath);

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
            bool isActive,
            string imagePath)
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

            cmd.Parameters.Add(
                "@ImagePath",
                SqlDbType.NVarChar,
                300).Value =
                string.IsNullOrWhiteSpace(imagePath)
                    ? (object)DBNull.Value
                    : imagePath;
        }


        /* ============================================================
           DISABLED - BULK IMPORT FROM AN INVENTORY PDF

           The import card is commented out in AddProduct.aspx, so
           every control it used (fuInventoryPdf, ddlGodown,
           ddlFallbackCategory, btnReadSheet, btnImportRows,
           pnlPreview, gvPreview, litSummary, lblImportMessage) is
           null at run time. The region is kept for reference and
           stays inside this block comment until those controls come
           back. Delete this opener and the matching closer after
           ShowImport at the bottom of the region to restore the
           feature.

           The sheet is always read first and shown as a preview.
           Nothing is written until the Import button is pressed,
           and every row the import touches is created inside one
           transaction, so a half imported sheet cannot happen.
           ============================================================

        private const string ImportRowsKey = "ImportRows";

        private const string ImportSummaryKey =
            "ImportSummary";

        private const int MaxSheetBytes = 10 * 1024 * 1024;


        private void LoadImportDropdowns()
        {
            string categories = @"
                SELECT CategoryId, CategoryName
                FROM ProductCategories
                WHERE IsActive = 1
                ORDER BY SortOrder, CategoryName";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(categories, con))
                {
                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        ddlFallbackCategory.DataSource = dt;
                        ddlFallbackCategory.DataTextField =
                            "CategoryName";

                        ddlFallbackCategory.DataValueField =
                            "CategoryId";

                        ddlFallbackCategory.DataBind();

                        ddlFallbackCategory.Items.Insert(
                            0,
                            new ListItem("Select Category", ""));
                    }
                }

                // Inventory belongs to a dealer, so the stock the
                // sheet describes has to land on one of them.
                string godowns = @"
                    SELECT UserId, FullName
                    FROM Users
                    WHERE Role = 'Dealer'
                      AND IsActive = 1
                    ORDER BY FullName, UserId";

                using (SqlCommand cmd =
                    new SqlCommand(godowns, con))
                {
                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        ddlGodown.DataSource = dt;
                        ddlGodown.DataTextField = "FullName";
                        ddlGodown.DataValueField = "UserId";
                        ddlGodown.DataBind();

                        ddlGodown.Items.Insert(
                            0,
                            new ListItem("Select Godown", ""));
                    }
                }
            }
        }


        protected void btnReadSheet_Click(object sender, EventArgs e)
        {
            if (!fuInventoryPdf.HasFile)
            {
                ShowImport(
                    "Choose an inventory PDF first.",
                    false);

                return;
            }

            if (fuInventoryPdf.PostedFile == null ||
                fuInventoryPdf.PostedFile.ContentLength == 0)
            {
                ShowImport(
                    "The uploaded file is empty.",
                    false);

                return;
            }

            if (fuInventoryPdf.PostedFile.ContentLength >
                MaxSheetBytes)
            {
                ShowImport(
                    "The sheet is larger than 10 MB.",
                    false);

                return;
            }

            string fileName =
                fuInventoryPdf.FileName ?? "";

            if (!fileName.EndsWith(
                    ".pdf",
                    StringComparison.OrdinalIgnoreCase))
            {
                ShowImport(
                    "Only .pdf sheets can be read.",
                    false);

                return;
            }

            try
            {
                List<InventoryPdfRow> rows =
                    InventoryPdfParser.Parse(
                        fuInventoryPdf.FileBytes);

                Dictionary<string, int> categories =
                    LoadCategoryMap();

                int fallbackId;

                if (!int.TryParse(
                        ddlFallbackCategory.SelectedValue,
                        out fallbackId))
                {
                    fallbackId = 0;
                }

                HashSet<string> existing =
                    LoadProductNames();

                int duplicates = 0;
                int unnamed = 0;

                foreach (InventoryPdfRow row in rows)
                {
                    if (row.ProductName.Length == 0)
                    {
                        unnamed++;
                        row.Include = false;
                    }
                    else if (existing.Contains(row.ProductName))
                    {
                        // Already in the catalogue, so leave the
                        // tick off unless the user wants a second
                        // copy.
                        row.Include = false;
                        duplicates++;
                    }

                    string guess =
                        row.ProductName.Length == 0
                            ? ""
                            : InventoryPdfParser
                                .GuessCategoryName(
                                    row.ProductName);

                    int guessedId;

                    if (guess.Length > 0 &&
                        categories.TryGetValue(
                            guess,
                            out guessedId))
                    {
                        row.CategoryId = guessedId;
                        row.CategoryName = guess;
                    }
                    else
                    {
                        row.CategoryId = fallbackId;
                        row.CategoryName = "";
                    }
                }

                ViewState[ImportRowsKey] = rows;

                BindPreview(rows);

                string summary = BuildSummary(
                    rows,
                    duplicates,
                    unnamed);

                litSummary.Text = summary;
                ViewState[ImportSummaryKey] = summary;

                btnImportRows.Visible = true;

                ShowImport(
                    "Sheet read. Check the rows below, then import.",
                    true);
            }
            catch (Exception ex)
            {
                ShowImport(
                    "Could not read the sheet: " + ex.Message,
                    false);
            }
        }


        protected void btnImportRows_Click(object sender, EventArgs e)
        {
            List<InventoryPdfRow> rows =
                ViewState[ImportRowsKey]
                    as List<InventoryPdfRow>;

            if (rows == null || rows.Count == 0)
            {
                ShowImport("Read a sheet first.", false);
                return;
            }

            int godownId;

            if (!int.TryParse(
                    ddlGodown.SelectedValue,
                    out godownId))
            {
                ShowImport(
                    "Select the godown to stock.",
                    false);

                return;
            }

            int fallbackId;

            if (!int.TryParse(
                    ddlFallbackCategory.SelectedValue,
                    out fallbackId))
            {
                ShowImport(
                    "Select the fallback category.",
                    false);

                return;
            }

            // The tick boxes live in the grid, which keeps its
            // rows in ViewState, so read them back before doing
            // anything with them.
            SyncIncludeFlags(rows);

            foreach (InventoryPdfRow row in rows)
            {
                if (row.CategoryId <= 0)
                {
                    row.CategoryId = fallbackId;
                }
            }

            List<InventoryPdfRow> chosen =
                rows.FindAll(
                    delegate(InventoryPdfRow row)
                    {
                        return row.Include &&
                               row.ProductName.Length > 0;
                    });

            if (chosen.Count == 0)
            {
                ShowImport(
                    "No rows are ticked for import.",
                    false);

                return;
            }

            string reference =
                "PDF-" +
                DateTime.Now.ToString(
                    "yyyyMMdd-HHmmss",
                    CultureInfo.InvariantCulture);

            int created = 0;
            int alreadyThere = 0;

            List<int> variantIds = new List<int>();
            List<int> quantities = new List<int>();
            List<decimal> costs = new List<decimal>();

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
                            HashSet<string> existing =
                                LoadProductNames(con, tx);

                            foreach (InventoryPdfRow row
                                in chosen)
                            {
                                if (existing.Contains(
                                        row.ProductName))
                                {
                                    alreadyThere++;
                                    continue;
                                }

                                int productId =
                                    InsertProduct(
                                        con,
                                        tx,
                                        row.ProductName,
                                        null,
                                        null,
                                        row.CategoryId,
                                        row.IsActive);

                                int variantId =
                                    InsertVariant(
                                        con,
                                        tx,
                                        productId,
                                        row);

                                InsertUnitOptions(
                                    con,
                                    tx,
                                    variantId,
                                    row);

                                InsertPrices(
                                    con,
                                    tx,
                                    variantId,
                                    row);

                                SetGodownStock(
                                    con,
                                    tx,
                                    godownId,
                                    variantId,
                                    row.TotalPacket);

                                existing.Add(row.ProductName);

                                if (row.TotalPacket > 0)
                                {
                                    variantIds.Add(variantId);
                                    quantities.Add(
                                        row.TotalPacket);

                                    costs.Add(
                                        row.PurchasePrice);
                                }

                                created++;
                            }

                            WriteLedger(
                                con,
                                tx,
                                godownId,
                                reference,
                                created,
                                variantIds,
                                quantities,
                                costs);

                            tx.Commit();
                        }
                        catch
                        {
                            tx.Rollback();
                            throw;
                        }
                    }
                }

                // Imported rows now exist, so untick them to make
                // pressing the button twice harmless.
                foreach (InventoryPdfRow row in chosen)
                {
                    row.Include = false;
                }

                BindPreview(rows);

                string summary =
                    "Imported " + created + " product(s) as " +
                    reference + " into " +
                    ddlGodown.SelectedItem.Text + "." +
                    (alreadyThere > 0
                        ? " " + alreadyThere +
                          " were already in the catalogue."
                        : "");

                litSummary.Text = summary;
                ViewState[ImportSummaryKey] = summary;

                ShowImport(
                    "Products imported.",
                    true);
            }
            catch (Exception ex)
            {
                ShowImport(
                    "Import failed, nothing was saved: " +
                    ex.Message,
                    false);
            }
        }


        // ============================================================
        // IMPORT HELPERS
        // ============================================================

        private void BindPreview(List<InventoryPdfRow> rows)
        {
            gvPreview.DataSource = rows;
            gvPreview.DataBind();

            pnlPreview.Visible = true;
        }


        private void SyncIncludeFlags(
            List<InventoryPdfRow> rows)
        {
            for (int i = 0;
                 i < gvPreview.Rows.Count &&
                 i < rows.Count;
                 i++)
            {
                CheckBox box =
                    (CheckBox)gvPreview.Rows[i].FindControl(
                        "chkInclude");

                if (box != null)
                {
                    rows[i].Include = box.Checked;
                }
            }
        }


        private static HashSet<string> NewNameSet()
        {
            return new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);
        }


        private static void FillProductNames(
            SqlConnection con,
            SqlTransaction tx,
            HashSet<string> names)
        {
            string query =
                "SELECT ProductName FROM Products";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                using (SqlDataReader reader =
                    cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        names.Add(
                            reader["ProductName"]
                                .ToString()
                                .Trim());
                    }
                }
            }
        }


        private HashSet<string> LoadProductNames()
        {
            HashSet<string> names = NewNameSet();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                FillProductNames(con, null, names);
            }

            return names;
        }


        private HashSet<string> LoadProductNames(
            SqlConnection con,
            SqlTransaction tx)
        {
            HashSet<string> names = NewNameSet();

            FillProductNames(con, tx, names);

            return names;
        }


        private Dictionary<string, int> LoadCategoryMap()
        {
            Dictionary<string, int> map =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);

            string query = @"
                SELECT CategoryId, CategoryName
                FROM ProductCategories
                WHERE IsActive = 1";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            map[
                                reader["CategoryName"]
                                    .ToString()] =
                                Convert.ToInt32(
                                    reader["CategoryId"]);
                        }
                    }
                }
            }

            return map;
        }


        private int InsertVariant(
            SqlConnection con,
            SqlTransaction tx,
            int productId,
            InventoryPdfRow row)
        {
            string query = @"
                INSERT INTO ProductVariants
                (
                    ProductId,
                    VariantCode,
                    VariantName,
                    PacketWeight,
                    WeightUnit,
                    SortOrder,
                    IsActive,
                    CreatedBy,
                    CreatedAt
                )
                VALUES
                (
                    @ProductId,
                    @VariantCode,
                    @VariantName,
                    @PacketWeight,
                    @WeightUnit,
                    1,
                    @IsActive,
                    @CreatedBy,
                    GETDATE()
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@ProductId",
                    SqlDbType.Int).Value = productId;

                cmd.Parameters.Add(
                    "@VariantCode",
                    SqlDbType.NVarChar,
                    200).Value =
                        string.IsNullOrEmpty(row.VariantCode)
                            ? (object)DBNull.Value
                            : row.VariantCode;

                // VariantName is NOT NULL, so a row with no
                // "15GM*192" spec falls back to its material.
                cmd.Parameters.Add(
                    "@VariantName",
                    SqlDbType.NVarChar,
                    500).Value =
                        string.IsNullOrEmpty(row.VariantCode)
                            ? row.ProductName
                            : row.VariantCode;

                SqlParameter weight =
                    cmd.Parameters.Add(
                        "@PacketWeight",
                        SqlDbType.Decimal);

                weight.Precision = 18;
                weight.Scale = 3;
                weight.Value = row.PacketWeight;

                cmd.Parameters.Add(
                    "@WeightUnit",
                    SqlDbType.NVarChar,
                    40).Value =
                        string.IsNullOrEmpty(row.WeightUnit)
                            ? (object)DBNull.Value
                            : row.WeightUnit;

                cmd.Parameters.Add(
                    "@IsActive",
                    SqlDbType.Bit).Value = row.IsActive;

                cmd.Parameters.Add(
                    "@CreatedBy",
                    SqlDbType.Int).Value = AdminId;

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }


        private void InsertUnitOptions(
            SqlConnection con,
            SqlTransaction tx,
            int variantId,
            InventoryPdfRow row)
        {
            // Packet is always the base unit at one packet each.
            InsertUnitOption(
                con, tx, variantId, "PKT", 1);

            if (row.PacketsPerPatti > 0)
            {
                InsertUnitOption(
                    con,
                    tx,
                    variantId,
                    "PTI",
                    row.PacketsPerPatti);
            }

            if (row.PacketsPerBox > 0)
            {
                InsertUnitOption(
                    con,
                    tx,
                    variantId,
                    "BOX",
                    row.PacketsPerBox);
            }
        }


        private void InsertUnitOption(
            SqlConnection con,
            SqlTransaction tx,
            int variantId,
            string unitCode,
            int packetsPerUnit)
        {
            int unitId = UnitId(con, tx, unitCode);

            if (unitId <= 0)
            {
                return;
            }

            string query = @"
                INSERT INTO ProductUnitOptions
                (
                    ProductVariantId,
                    UnitId,
                    PacketsPerUnit,
                    IsActive,
                    CreatedBy,
                    CreatedAt
                )
                VALUES
                (
                    @ProductVariantId,
                    @UnitId,
                    @PacketsPerUnit,
                    1,
                    @CreatedBy,
                    GETDATE()
                )";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                cmd.Parameters.Add(
                    "@UnitId",
                    SqlDbType.Int).Value = unitId;

                cmd.Parameters.Add(
                    "@PacketsPerUnit",
                    SqlDbType.Int).Value = packetsPerUnit;

                cmd.Parameters.Add(
                    "@CreatedBy",
                    SqlDbType.Int).Value = AdminId;

                cmd.ExecuteNonQuery();
            }
        }


        private void InsertPrices(
            SqlConnection con,
            SqlTransaction tx,
            int variantId,
            InventoryPdfRow row)
        {
            // The sheet prices a packet, so every wider unit is
            // scaled by its own packing. Otherwise a box would
            // sell at packet rates.
            InsertPrice(
                con, tx, variantId, "PKT", 1, row);

            if (row.PacketsPerPatti > 0)
            {
                InsertPrice(
                    con,
                    tx,
                    variantId,
                    "PTI",
                    row.PacketsPerPatti,
                    row);
            }

            if (row.PacketsPerBox > 0)
            {
                InsertPrice(
                    con,
                    tx,
                    variantId,
                    "BOX",
                    row.PacketsPerBox,
                    row);
            }
        }


        private void InsertPrice(
            SqlConnection con,
            SqlTransaction tx,
            int variantId,
            string unitCode,
            int packetsPerUnit,
            InventoryPdfRow row)
        {
            int optionId = OptionId(
                con, tx, variantId, unitCode);

            if (optionId <= 0)
            {
                return;
            }

            decimal purchase = Math.Round(
                row.PurchasePrice * packetsPerUnit,
                2);

            decimal selling = Math.Round(
                row.SalesPrice * packetsPerUnit,
                2);

            // The sheet carries no printed MRP, so the selling
            // price is the closest thing to it.
            decimal printed = selling;

            string query = @"
                INSERT INTO ProductPrices
                (
                    ProductOptionId,
                    PrintedPrice,
                    DealerPurchasePrice,
                    ShopSellingPrice,
                    EffectiveFrom,
                    IsActive,
                    CreatedBy,
                    CreatedAt
                )
                VALUES
                (
                    @ProductOptionId,
                    @PrintedPrice,
                    @DealerPurchasePrice,
                    @ShopSellingPrice,
                    @EffectiveFrom,
                    1,
                    @CreatedBy,
                    GETDATE()
                )";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@ProductOptionId",
                    SqlDbType.Int).Value = optionId;

                cmd.Parameters.Add(
                    "@PrintedPrice",
                    SqlDbType.Decimal).Value = printed;

                cmd.Parameters.Add(
                    "@DealerPurchasePrice",
                    SqlDbType.Decimal).Value = purchase;

                cmd.Parameters.Add(
                    "@ShopSellingPrice",
                    SqlDbType.Decimal).Value = selling;

                cmd.Parameters.Add(
                    "@EffectiveFrom",
                    SqlDbType.Date).Value =
                        DateTime.Today;

                cmd.Parameters.Add(
                    "@CreatedBy",
                    SqlDbType.Int).Value = AdminId;

                cmd.ExecuteNonQuery();
            }
        }


        private void SetGodownStock(
            SqlConnection con,
            SqlTransaction tx,
            int dealerId,
            int variantId,
            int quantity)
        {
            string query = @"
                IF EXISTS
                (
                    SELECT 1
                    FROM Inventory
                    WHERE DealerId = @DealerId
                      AND ProductVariantId = @ProductVariantId
                )
                    UPDATE Inventory
                    SET QuantityPackets = @Quantity,
                        UpdatedBy = @UpdatedBy,
                        UpdatedAt = GETDATE()
                    WHERE DealerId = @DealerId
                      AND ProductVariantId = @ProductVariantId
                ELSE
                    INSERT INTO Inventory
                    (
                        DealerId,
                        ProductVariantId,
                        QuantityPackets,
                        ReorderLevel,
                        UpdatedBy,
                        UpdatedAt
                    )
                    VALUES
                    (
                        @DealerId,
                        @ProductVariantId,
                        @Quantity,
                        0,
                        @UpdatedBy,
                        GETDATE()
                    )";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = dealerId;

                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                cmd.Parameters.Add(
                    "@Quantity",
                    SqlDbType.Int).Value = quantity;

                cmd.Parameters.Add(
                    "@UpdatedBy",
                    SqlDbType.Int).Value = AdminId;

                cmd.ExecuteNonQuery();
            }
        }


        // One ledger header for the whole sheet rather than one
        // per row, with a line per product underneath it.
        private void WriteLedger(
            SqlConnection con,
            SqlTransaction tx,
            int dealerId,
            string reference,
            int count,
            List<int> variantIds,
            List<int> quantities,
            List<decimal> costs)
        {
            if (variantIds.Count == 0)
            {
                return;
            }

            string header = @"
                INSERT INTO StockTransactions
                (
                    DealerId,
                    TransactionType,
                    TransactionDate,
                    ReferenceNo,
                    Remarks,
                    CreatedBy,
                    CreatedAt
                )
                VALUES
                (
                    @DealerId,
                    @TransactionType,
                    GETDATE(),
                    @ReferenceNo,
                    @Remarks,
                    @CreatedBy,
                    GETDATE()
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            int transactionId;

            using (SqlCommand cmd =
                new SqlCommand(header, con, tx))
            {
                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = dealerId;

                cmd.Parameters.Add(
                    "@TransactionType",
                    SqlDbType.NVarChar,
                    80).Value = "Stock In";

                cmd.Parameters.Add(
                    "@ReferenceNo",
                    SqlDbType.NVarChar,
                    200).Value = reference;

                cmd.Parameters.Add(
                    "@Remarks",
                    SqlDbType.NVarChar,
                    1000).Value =
                        "Bulk import from inventory PDF - " +
                        count + " product(s)";

                cmd.Parameters.Add(
                    "@CreatedBy",
                    SqlDbType.Int).Value = AdminId;

                transactionId =
                    Convert.ToInt32(cmd.ExecuteScalar());
            }

            string detail = @"
                INSERT INTO StockTransactionDetails
                (
                    StockTransactionId,
                    ProductVariantId,
                    UnitId,
                    Quantity,
                    QuantityPackets,
                    UnitCostPrice
                )
                VALUES
                (
                    @StockTransactionId,
                    @ProductVariantId,
                    @UnitId,
                    @Quantity,
                    @QuantityPackets,
                    @UnitCostPrice
                )";

            int packetUnitId = UnitId(con, tx, "PKT");

            using (SqlCommand cmd =
                new SqlCommand(detail, con, tx))
            {
                SqlParameter transaction =
                    cmd.Parameters.Add(
                        "@StockTransactionId",
                        SqlDbType.Int);

                SqlParameter variant =
                    cmd.Parameters.Add(
                        "@ProductVariantId",
                        SqlDbType.Int);

                SqlParameter unit =
                    cmd.Parameters.Add(
                        "@UnitId",
                        SqlDbType.Int);

                SqlParameter quantity =
                    cmd.Parameters.Add(
                        "@Quantity",
                        SqlDbType.Int);

                SqlParameter packets =
                    cmd.Parameters.Add(
                        "@QuantityPackets",
                        SqlDbType.Int);

                SqlParameter cost =
                    cmd.Parameters.Add(
                        "@UnitCostPrice",
                        SqlDbType.Decimal);

                cost.Precision = 18;
                cost.Scale = 2;

                transaction.Value = transactionId;
                unit.Value = packetUnitId;

                for (int i = 0; i < variantIds.Count; i++)
                {
                    variant.Value = variantIds[i];
                    quantity.Value = quantities[i];
                    packets.Value = quantities[i];
                    cost.Value = costs[i];

                    cmd.ExecuteNonQuery();
                }
            }
        }


        private int UnitId(
            SqlConnection con,
            SqlTransaction tx,
            string unitCode)
        {
            string query = @"
                SELECT TOP (1) UnitId
                FROM SellingUnits
                WHERE UnitCode = @UnitCode
                  AND IsActive = 1
                ORDER BY UnitId";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@UnitCode",
                    SqlDbType.NVarChar,
                    40).Value = unitCode;

                object result = cmd.ExecuteScalar();

                return result == null ||
                       result == DBNull.Value
                    ? 0
                    : Convert.ToInt32(result);
            }
        }


        private int OptionId(
            SqlConnection con,
            SqlTransaction tx,
            int variantId,
            string unitCode)
        {
            int unitId = UnitId(con, tx, unitCode);

            if (unitId <= 0)
            {
                return 0;
            }

            string query = @"
                SELECT TOP (1) ProductOptionId
                FROM ProductUnitOptions
                WHERE ProductVariantId = @ProductVariantId
                  AND UnitId = @UnitId";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                cmd.Parameters.Add(
                    "@UnitId",
                    SqlDbType.Int).Value = unitId;

                object result = cmd.ExecuteScalar();

                return result == null ||
                       result == DBNull.Value
                    ? 0
                    : Convert.ToInt32(result);
            }
        }


        private static string BuildSummary(
            List<InventoryPdfRow> rows,
            int duplicates,
            int unnamed)
        {
            int withBox = 0;
            int withPatti = 0;
            int inactive = 0;
            List<int> flagged = new List<int>();

            foreach (InventoryPdfRow row in rows)
            {
                if (row.PacketsPerBox > 0)
                {
                    withBox++;
                }

                if (row.PacketsPerPatti > 0)
                {
                    withPatti++;
                }

                if (!row.IsActive)
                {
                    inactive++;
                }

                if (row.Note.Length > 0)
                {
                    flagged.Add(row.Sr);
                }
            }

            StringBuilder text = new StringBuilder();

            text.Append(rows.Count);
            text.Append(" row(s) read. Packing read for ");
            text.Append(withBox);
            text.Append(" by box and ");
            text.Append(withPatti);
            text.Append(" by patti");

            if (rows.Count - withPatti > 0)
            {
                text.Append(" - ");
                text.Append(rows.Count - withPatti);
                text.Append(
                    " row(s) carry no patti stock on the " +
                    "sheet, so they are billable by Packet " +
                    "and Box only");
            }

            text.Append(".");

            if (duplicates > 0)
            {
                text.Append(" ");
                text.Append(duplicates);
                text.Append(
                    " already exist and are unticked.");
            }

            if (unnamed > 0)
            {
                text.Append(" ");
                text.Append(unnamed);
                text.Append(
                    " have no material and are unticked.");
            }

            if (inactive > 0)
            {
                text.Append(" ");
                text.Append(inactive);
                text.Append(
                    " are marked [INACT.] and import inactive.");
            }

            if (flagged.Count > 0)
            {
                text.Append(" Row(s) ");
                text.Append(string.Join(
                    ", ",
                    flagged));
                text.Append(
                    " do not add up to Total Packet - check " +
                    "them before importing.");
            }

            return text.ToString();
        }


        private void ShowImport(
            string message,
            bool success)
        {
            lblImportMessage.Visible = true;
            lblImportMessage.Text = message;
            lblImportMessage.CssClass = success
                ? "alert alert-success d-block"
                : "alert alert-danger d-block";
        }

           ============================================================ */


        // ============================================================
        // PRODUCT IMAGE
        //
        // The image travels on the same postback as the form, so it
        // is written to disk first and only the relative path is
        // stored. Any file that either the file input or the Remove
        // button replaces is deleted afterwards.
        // ============================================================

        private const int MaxImageBytes = 2 * 1024 * 1024;

        private static readonly string[] AllowedImageExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };


        // Read by the markup so the tile can come back painted after
        // a validation error.
        protected bool HasImage
        {
            get
            {
                if (hfRemoveImage.Value == "1" &&
                    !fuProductImage.HasFile)
                {
                    return false;
                }

                return !string.IsNullOrEmpty(
                    ViewState["ImagePath"] as string);
            }
        }


        protected string PreviewUrl
        {
            get
            {
                string path = ViewState["ImagePath"] as string;

                return string.IsNullOrEmpty(path)
                    ? ""
                    : ResolveUrl(path);
            }
        }


        private string ApplyImageChange(string currentPath)
        {
            // Remove was pressed and no replacement was chosen.
            if (hfRemoveImage.Value == "1" &&
                !fuProductImage.HasFile)
            {
                DeleteImage(currentPath);

                return null;
            }

            if (!fuProductImage.HasFile)
            {
                return currentPath;
            }

            return SaveUploadedImage(currentPath);
        }


        private string SaveUploadedImage(string currentPath)
        {
            if (fuProductImage.PostedFile.ContentLength >
                MaxImageBytes)
            {
                throw new InvalidOperationException(
                    "the image is larger than 2 MB.");
            }

            string extension = Path
                .GetExtension(fuProductImage.FileName ?? "")
                .ToLowerInvariant();

            if (Array.IndexOf(
                    AllowedImageExtensions,
                    extension) < 0)
            {
                throw new InvalidOperationException(
                    "only PNG, JPG and WEBP images are allowed.");
            }

            string folder =
                Server.MapPath("~/Uploads/Products/");

            Directory.CreateDirectory(folder);

            string fileName =
                Guid.NewGuid().ToString("N") + extension;

            fuProductImage.SaveAs(
                Path.Combine(folder, fileName));

            // The replaced file is only dropped once the new one is
            // safely on disk.
            DeleteImage(currentPath);

            return "~/Uploads/Products/" + fileName;
        }


        private void DeleteImage(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            try
            {
                string full = Server.MapPath(path);

                if (File.Exists(full))
                {
                    File.Delete(full);
                }
            }
            catch
            {
                // A file left behind is not worth failing a save
                // over.
            }
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
