using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using BWDMS.Data;

namespace BWDMS.Admin
{
    public partial class AddVariant : System.Web.UI.Page
    {
        // ============================================================
        // IDS
        // ============================================================

        private int VariantId
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

        private int ProductId
        {
            get
            {
                int id;

                if (int.TryParse(
                    Request.QueryString["product"],
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


        // The product this variant belongs to. Resolved from the
        // variant row in edit mode so a forged ?product= value
        // cannot redirect the save.
        private int EffectiveProductId
        {
            get
            {
                int stored = 0;

                if (ViewState["ProductId"] != null)
                {
                    stored =
                        Convert.ToInt32(ViewState["ProductId"]);
                }

                return stored;
            }
            set { ViewState["ProductId"] = value; }
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


            if (VariantId > 0)
            {
                // Edit mode: the product comes from the row itself.
                if (ViewState["ProductId"] == null)
                {
                    if (!LoadVariantHeader())
                    {
                        RedirectToVariants();
                        return;
                    }
                }
            }
            else
            {
                if (ProductId <= 0)
                {
                    RedirectToVariants();
                    return;
                }

                if (ViewState["ProductId"] == null)
                {
                    if (!ProductExists(ProductId))
                    {
                        RedirectToVariants();
                        return;
                    }

                    EffectiveProductId = ProductId;
                }
            }


            if (!IsPostBack)
            {
                SetFormMode();

                BindUnits();
            }
        }


        private void RedirectToVariants()
        {
            Response.Redirect(
                "~/Admin/Products.aspx",
                false);

            Context.ApplicationInstance.CompleteRequest();
        }


        private void SetFormMode()
        {
            bool isEdit = VariantId > 0;

            lblPageTitle.Text =
                isEdit ? "Edit Variant" : "Add Variant";

            lblHeading.Text =
                isEdit ? "Edit Variant" : "Add Variant";

            btnSave.Text =
                isEdit ? "Update Variant" : "Create Variant";

            lnkBack.NavigateUrl =
                "~/Admin/ProductVariants.aspx?product=" +
                EffectiveProductId;

            lnkCancel.NavigateUrl =
                lnkBack.NavigateUrl;
        }


        private bool ProductExists(int productId)
        {
            string query = @"
                SELECT ProductName
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
                        SqlDbType.Int).Value = productId;

                    object result = cmd.ExecuteScalar();

                    if (result == null ||
                        result == DBNull.Value)
                    {
                        return false;
                    }

                    lblSubHeading.Text = result.ToString();

                    return true;
                }
            }
        }


        // ============================================================
        // EDIT MODE: LOAD VARIANT HEADER + RESOLVE OWN PRODUCT
        // ============================================================

        private bool LoadVariantHeader()
        {
            string query = @"
                SELECT
                    v.ProductId,
                    v.VariantName,
                    v.VariantCode,
                    v.PacketWeight,
                    v.WeightUnit,
                    v.SortOrder,
                    v.IsActive,
                    p.ProductName
                FROM ProductVariants v
                INNER JOIN Products p
                    ON p.ProductId = v.ProductId
                WHERE v.ProductVariantId = @VariantId";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@VariantId",
                        SqlDbType.Int).Value = VariantId;

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return false;
                        }

                        EffectiveProductId =
                            Convert.ToInt32(reader["ProductId"]);

                        lblSubHeading.Text =
                            reader["ProductName"].ToString();

                        txtVariantName.Text =
                            reader["VariantName"].ToString();

                        txtVariantCode.Text =
                            reader["VariantCode"] == DBNull.Value
                                ? ""
                                : reader["VariantCode"].ToString();

                        if (reader["PacketWeight"] !=
                            DBNull.Value)
                        {
                            txtPacketWeight.Text =
                                Convert.ToDecimal(
                                    reader["PacketWeight"],
                                    CultureInfo.InvariantCulture)
                                .ToString(CultureInfo.InvariantCulture);
                        }

                        if (reader["WeightUnit"] !=
                            DBNull.Value)
                        {
                            SelectByValue(
                                ddlWeightUnit,
                                reader["WeightUnit"].ToString());
                        }

                        txtSortOrder.Text =
                            reader["SortOrder"] == DBNull.Value
                                ? "0"
                                : reader["SortOrder"].ToString();

                        ddlStatus.SelectedValue =
                            Convert.ToBoolean(reader["IsActive"])
                                ? "1"
                                : "0";

                        return true;
                    }
                }
            }
        }


        private void SelectByValue(
            System.Web.UI.WebControls.DropDownList dropdown,
            string value)
        {
            if (dropdown.Items.FindByValue(value) != null)
            {
                dropdown.SelectedValue = value;
            }
        }


        // ============================================================
        // BIND THE UNIT / PRICE GRID
        //
        // Called only on the first load. On postback the repeater
        // is rebuilt from view state so typed values survive.
        // ============================================================

        private void BindUnits()
        {
            string unitsQuery = @"
                SELECT UnitId, UnitCode, UnitName, SortOrder
                FROM SellingUnits
                WHERE IsActive = 1
                ORDER BY SortOrder, UnitName";

            DataTable units = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(unitsQuery, con))
                {
                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(units);
                    }
                }


                // --------------------------------------------
                // Existing unit options + prices for this
                // variant (edit mode returns rows; add mode
                // returns nothing).
                // --------------------------------------------

                string optionsQuery = @"
                    SELECT
                        uo.UnitId,
                        uo.ProductOptionId,
                        uo.PacketsPerUnit,
                        uo.IsActive AS OptionActive,
                        pp.PrintedPrice,
                        pp.DealerPurchasePrice,
                        pp.ShopSellingPrice
                    FROM ProductUnitOptions uo
                    LEFT JOIN ProductPrices pp
                        ON pp.ProductOptionId = uo.ProductOptionId
                    WHERE uo.ProductVariantId = @VariantId";

                DataTable options = new DataTable();

                using (SqlCommand cmd =
                    new SqlCommand(optionsQuery, con))
                {
                    cmd.Parameters.Add(
                        "@VariantId",
                        SqlDbType.Int).Value =
                        VariantId > 0 ? VariantId : 0;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(options);
                    }
                }


                // --------------------------------------------
                // Merge into one table the repeater binds to.
                // --------------------------------------------

                units.Columns.Add(
                    "Checked",
                    typeof(bool));

                units.Columns.Add(
                    "PacketsPerUnit",
                    typeof(string));

                units.Columns.Add(
                    "PrintedPrice",
                    typeof(string));

                units.Columns.Add(
                    "DealerPrice",
                    typeof(string));

                units.Columns.Add(
                    "ShopPrice",
                    typeof(string));

                foreach (DataRow unit in units.Rows)
                {
                    int unitId =
                        Convert.ToInt32(unit["UnitId"]);

                    DataRow match = null;

                    foreach (DataRow opt in options.Rows)
                    {
                        if (Convert.ToInt32(
                            opt["UnitId"]) == unitId)
                        {
                            match = opt;
                            break;
                        }
                    }

                    if (match == null)
                    {
                        // New option: Packet is the base unit so it
                        // is ticked by default with 1 packet = 1.
                        bool isBase =
                            unit["UnitCode"].ToString()
                                .Equals(
                                    "PKT",
                                    StringComparison.OrdinalIgnoreCase);

                        unit["Checked"] = isBase;
                        unit["PacketsPerUnit"] = isBase ? "1" : "";
                        unit["PrintedPrice"] = "";
                        unit["DealerPrice"] = "";
                        unit["ShopPrice"] = "";
                    }
                    else
                    {
                        unit["Checked"] =
                            Convert.ToBoolean(
                                match["OptionActive"]);

                        unit["PacketsPerUnit"] =
                            Convert.ToInt32(
                                match["PacketsPerUnit"])
                                .ToString();

                        unit["PrintedPrice"] =
                            FormatPrice(match["PrintedPrice"]);

                        unit["DealerPrice"] =
                            FormatPrice(
                                match["DealerPurchasePrice"]);

                        unit["ShopPrice"] =
                            FormatPrice(
                                match["ShopSellingPrice"]);
                    }
                }
            }

            repUnits.DataSource = units;
            repUnits.DataBind();
        }


        private static string FormatPrice(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return "";
            }

            return Convert.ToDecimal(
                value,
                CultureInfo.InvariantCulture)
                .ToString(
                    "0.##",
                    CultureInfo.InvariantCulture);
        }


        // ============================================================
        // SAVE
        // ============================================================

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }


            // ========================================================
            // READ THE UNIT GRID
            // ========================================================

            var selectedUnits = new List<UnitRow>();

            foreach (System.Web.UI.WebControls.RepeaterItem item
                in repUnits.Items)
            {
                var chk =
                    (System.Web.UI.WebControls.CheckBox)
                    item.FindControl("chkEnable");

                var hid =
                    (System.Web.UI.WebControls.HiddenField)
                    item.FindControl("hidUnitId");

                var ppu =
                    (System.Web.UI.WebControls.TextBox)
                    item.FindControl("txtPacketsPerUnit");

                var printed =
                    (System.Web.UI.WebControls.TextBox)
                    item.FindControl("txtPrintedPrice");

                var dealer =
                    (System.Web.UI.WebControls.TextBox)
                    item.FindControl("txtDealerPrice");

                var shop =
                    (System.Web.UI.WebControls.TextBox)
                    item.FindControl("txtShopPrice");

                selectedUnits.Add(new UnitRow
                {
                    UnitId = Convert.ToInt32(hid.Value),
                    Enabled = chk.Checked,
                    PacketsPerUnitRaw = ppu.Text.Trim(),
                    PrintedRaw = printed.Text.Trim(),
                    DealerRaw = dealer.Text.Trim(),
                    ShopRaw = shop.Text.Trim()
                });
            }


            // ========================================================
            // VALIDATION
            // ========================================================

            bool anyEnabled = false;

            foreach (UnitRow row in selectedUnits)
            {
                if (!row.Enabled)
                {
                    continue;
                }

                anyEnabled = true;

                int ppu;

                if (!int.TryParse(row.PacketsPerUnitRaw,
                    out ppu) || ppu < 1)
                {
                    ShowMessage(
                        "Every enabled unit needs a 'Packets per Unit' of 1 or more.",
                        false);

                    return;
                }

                row.PacketsPerUnit = ppu;

                decimal printed, dealer, shop;

                if (!TryPrice(row.PrintedRaw, out printed) ||
                    !TryPrice(row.DealerRaw, out dealer) ||
                    !TryPrice(row.ShopRaw, out shop))
                {
                    ShowMessage(
                        "Every enabled unit needs all three prices " +
                        "(printed, dealer and shop), each 0 or more.",
                        false);

                    return;
                }

                if (shop < dealer)
                {
                    ShowMessage(
                        "Shop price cannot be lower than the dealer price.",
                        false);

                    return;
                }

                row.Printed = printed;
                row.Dealer = dealer;
                row.Shop = shop;
            }

            if (!anyEnabled)
            {
                ShowMessage(
                    "Tick at least one selling unit for this variant.",
                    false);

                return;
            }


            string variantName =
                txtVariantName.Text.Trim();

            string variantCode =
                txtVariantCode.Text.Trim();

            decimal? packetWeight = null;

            if (!string.IsNullOrWhiteSpace(
                txtPacketWeight.Text))
            {
                decimal parsed;

                if (!decimal.TryParse(
                    txtPacketWeight.Text.Trim(),
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out parsed) || parsed < 0)
                {
                    ShowMessage(
                        "Enter a valid packet weight.",
                        false);

                    return;
                }

                packetWeight = parsed;
            }

            int sortOrder = 0;

            int.TryParse(txtSortOrder.Text.Trim(), out sortOrder);

            bool isActive =
                ddlStatus.SelectedValue == "1";


            // ========================================================
            // SAVE INSIDE ONE TRANSACTION
            // ========================================================

            try
            {
                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    con.Open();

                    using (SqlTransaction tx =
                        con.BeginTransaction())
                    {
                        int savedVariantId;

                        try
                        {
                            // ------------------------------------
                            // 1. Variant header
                            // ------------------------------------

                            if (VariantId > 0)
                            {
                                savedVariantId = VariantId;

                                UpdateVariant(
                                    con,
                                    tx,
                                    variantName,
                                    variantCode,
                                    packetWeight,
                                    sortOrder,
                                    isActive);
                            }
                            else
                            {
                                savedVariantId =
                                    InsertVariant(
                                        con,
                                        tx,
                                        variantName,
                                        variantCode,
                                        packetWeight,
                                        sortOrder,
                                        isActive);
                            }

                            // ------------------------------------
                            // 2. Unit options + prices
                            // ------------------------------------

                            SaveUnitOptions(
                                con,
                                tx,
                                savedVariantId,
                                selectedUnits);

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
                    "~/Admin/ProductVariants.aspx?product=" +
                    EffectiveProductId +
                    "&saved=1");
            }
            catch (SqlException ex)
                when (ex.Number == 2601 ||
                      ex.Number == 2627)
            {
                ShowMessage(
                    "That variant code is already used.",
                    false);
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error saving variant: " + ex.Message,
                    false);
            }
        }


        private static bool TryPrice(
            string text,
            out decimal value)
        {
            value = 0;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            if (!decimal.TryParse(
                text,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out value))
            {
                return false;
            }

            return value >= 0;
        }


        private int InsertVariant(
            SqlConnection con,
            SqlTransaction tx,
            string variantName,
            string variantCode,
            decimal? packetWeight,
            int sortOrder,
            bool isActive)
        {
            string query = @"
                INSERT INTO ProductVariants
                (
                    ProductId,
                    VariantName,
                    VariantCode,
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
                    @VariantName,
                    @VariantCode,
                    @PacketWeight,
                    @WeightUnit,
                    @SortOrder,
                    @IsActive,
                    @CreatedBy,
                    GETDATE()
                );
                SELECT SCOPE_IDENTITY();";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@ProductId",
                    SqlDbType.Int).Value =
                    EffectiveProductId;

                cmd.Parameters.Add(
                    "@VariantName",
                    SqlDbType.NVarChar,
                    500).Value = variantName;

                cmd.Parameters.Add(
                    "@VariantCode",
                    SqlDbType.NVarChar,
                    200).Value =
                    string.IsNullOrWhiteSpace(variantCode)
                        ? (object)DBNull.Value
                        : variantCode;

                cmd.Parameters.Add(
                    "@PacketWeight",
                    SqlDbType.Decimal).Value =
                    packetWeight.HasValue
                        ? (object)packetWeight.Value
                        : DBNull.Value;

                cmd.Parameters.Add(
                    "@WeightUnit",
                    SqlDbType.NVarChar,
                    40).Value =
                    string.IsNullOrWhiteSpace(
                        ddlWeightUnit.SelectedValue)
                        ? (object)DBNull.Value
                        : ddlWeightUnit.SelectedValue;

                cmd.Parameters.Add(
                    "@SortOrder",
                    SqlDbType.Int).Value = sortOrder;

                cmd.Parameters.Add(
                    "@IsActive",
                    SqlDbType.Bit).Value = isActive;

                cmd.Parameters.Add(
                    "@CreatedBy",
                    SqlDbType.Int).Value = AdminId;

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }


        private void UpdateVariant(
            SqlConnection con,
            SqlTransaction tx,
            string variantName,
            string variantCode,
            decimal? packetWeight,
            int sortOrder,
            bool isActive)
        {
            string query = @"
                UPDATE ProductVariants
                SET
                    VariantName = @VariantName,
                    VariantCode = @VariantCode,
                    PacketWeight = @PacketWeight,
                    WeightUnit = @WeightUnit,
                    SortOrder = @SortOrder,
                    IsActive = @IsActive,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE ProductVariantId = @ProductVariantId";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@VariantName",
                    SqlDbType.NVarChar,
                    500).Value = variantName;

                cmd.Parameters.Add(
                    "@VariantCode",
                    SqlDbType.NVarChar,
                    200).Value =
                    string.IsNullOrWhiteSpace(variantCode)
                        ? (object)DBNull.Value
                        : variantCode;

                cmd.Parameters.Add(
                    "@PacketWeight",
                    SqlDbType.Decimal).Value =
                    packetWeight.HasValue
                        ? (object)packetWeight.Value
                        : DBNull.Value;

                cmd.Parameters.Add(
                    "@WeightUnit",
                    SqlDbType.NVarChar,
                    40).Value =
                    string.IsNullOrWhiteSpace(
                        ddlWeightUnit.SelectedValue)
                        ? (object)DBNull.Value
                        : ddlWeightUnit.SelectedValue;

                cmd.Parameters.Add(
                    "@SortOrder",
                    SqlDbType.Int).Value = sortOrder;

                cmd.Parameters.Add(
                    "@IsActive",
                    SqlDbType.Bit).Value = isActive;

                cmd.Parameters.Add(
                    "@UpdatedBy",
                    SqlDbType.Int).Value = AdminId;

                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = VariantId;

                int rows = cmd.ExecuteNonQuery();

                if (rows == 0)
                {
                    throw new InvalidOperationException(
                        "The variant could not be updated.");
                }
            }
        }


        // ============================================================
        // UNIT OPTIONS + PRICES
        //
        // UQ_POU_Variant_Unit  : one option per (variant, unit)
        // UQ_ProductPrices_Option : one price row per option
        //
        // Disabled units keep their row but are switched off so no
        // history is destroyed.
        // ============================================================

        private void SaveUnitOptions(
            SqlConnection con,
            SqlTransaction tx,
            int variantId,
            List<UnitRow> rows)
        {
            foreach (UnitRow row in rows)
            {
                int optionId =
                    GetOptionId(
                        con,
                        tx,
                        variantId,
                        row.UnitId);

                if (!row.Enabled)
                {
                    // Switch off but keep the row.
                    if (optionId > 0)
                    {
                        SetOptionActive(
                            con,
                            tx,
                            optionId,
                            false);
                    }

                    continue;
                }


                // --------------------------------------------
                // Option row
                // --------------------------------------------

                if (optionId > 0)
                {
                    UpdateOption(
                        con,
                        tx,
                        optionId,
                        row.PacketsPerUnit,
                        true);
                }
                else
                {
                    optionId = InsertOption(
                        con,
                        tx,
                        variantId,
                        row.UnitId,
                        row.PacketsPerUnit);
                }


                // --------------------------------------------
                // Price row (exactly one per option)
                // --------------------------------------------

                int priceId =
                    GetPriceId(
                        con,
                        tx,
                        optionId);

                if (priceId > 0)
                {
                    UpdatePrice(
                        con,
                        tx,
                        priceId,
                        row.Printed,
                        row.Dealer,
                        row.Shop);
                }
                else
                {
                    InsertPrice(
                        con,
                        tx,
                        optionId,
                        row.Printed,
                        row.Dealer,
                        row.Shop);
                }
            }
        }


        private int GetOptionId(
            SqlConnection con,
            SqlTransaction tx,
            int variantId,
            int unitId)
        {
            string query = @"
                SELECT ProductOptionId
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


        private void SetOptionActive(
            SqlConnection con,
            SqlTransaction tx,
            int optionId,
            bool isActive)
        {
            string query = @"
                UPDATE ProductUnitOptions
                SET IsActive = @IsActive,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE ProductOptionId = @ProductOptionId";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@IsActive",
                    SqlDbType.Bit).Value = isActive;

                cmd.Parameters.Add(
                    "@UpdatedBy",
                    SqlDbType.Int).Value = AdminId;

                cmd.Parameters.Add(
                    "@ProductOptionId",
                    SqlDbType.Int).Value = optionId;

                cmd.ExecuteNonQuery();
            }
        }


        private void UpdateOption(
            SqlConnection con,
            SqlTransaction tx,
            int optionId,
            int packetsPerUnit,
            bool isActive)
        {
            string query = @"
                UPDATE ProductUnitOptions
                SET PacketsPerUnit = @PacketsPerUnit,
                    IsActive = @IsActive,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE ProductOptionId = @ProductOptionId";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@PacketsPerUnit",
                    SqlDbType.Int).Value = packetsPerUnit;

                cmd.Parameters.Add(
                    "@IsActive",
                    SqlDbType.Bit).Value = isActive;

                cmd.Parameters.Add(
                    "@UpdatedBy",
                    SqlDbType.Int).Value = AdminId;

                cmd.Parameters.Add(
                    "@ProductOptionId",
                    SqlDbType.Int).Value = optionId;

                cmd.ExecuteNonQuery();
            }
        }


        private int InsertOption(
            SqlConnection con,
            SqlTransaction tx,
            int variantId,
            int unitId,
            int packetsPerUnit)
        {
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
                );
                SELECT SCOPE_IDENTITY();";

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

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }


        // Which price row is the one in force today. A unit option can carry
            // several prices over time, so the one used for editing is the
            // newest that is already in force - not just any row.
            private int GetPriceId(
                SqlConnection con,
                SqlTransaction tx,
                int optionId)
            {
                string query = @"
                    SELECT TOP (1) PriceId
                    FROM ProductPrices
                    WHERE ProductOptionId = @ProductOptionId
                    ORDER BY EffectiveFrom DESC";

                using (SqlCommand cmd =
                    new SqlCommand(query, con, tx))
                {
                    cmd.Parameters.Add(
                        "@ProductOptionId",
                        SqlDbType.Int).Value = optionId;

                    object result = cmd.ExecuteScalar();

                    return result == null ||
                           result == DBNull.Value
                        ? 0
                        : Convert.ToInt32(result);
                }
            }


        private void InsertPrice(
            SqlConnection con,
            SqlTransaction tx,
            int optionId,
            decimal printed,
            decimal dealer,
            decimal shop)
        {
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
                    CAST(GETDATE() AS DATE),
                    1,
                    @CreatedBy,
                    GETDATE()
                )";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                AddPriceParameters(
                    cmd,
                    optionId,
                    printed,
                    dealer,
                    shop);

                cmd.Parameters.Add(
                    "@CreatedBy",
                    SqlDbType.Int).Value = AdminId;

                cmd.ExecuteNonQuery();
            }
        }


        // A price change starts a NEW price row dated today rather than
            // overwriting the old one. Orders already placed keep the price
            // they were billed at (their own snapshot), and a bill raised
            // before today still resolves to the older price. Editing the
            // same day's row in place is still allowed so a typo can be
            // corrected without littering the history.
            private void UpdatePrice(
                SqlConnection con,
                SqlTransaction tx,
                int priceId,
                decimal printed,
                decimal dealer,
                decimal shop)
            {
                string closeHistory = @"
                    UPDATE ProductPrices
                    SET IsActive = 0,
                        UpdatedBy = @UpdatedBy,
                        UpdatedAt = GETDATE()
                    WHERE PriceId = @PriceId";

                string reopen = @"
                    UPDATE ProductPrices
                    SET PrintedPrice = @PrintedPrice,
                        DealerPurchasePrice = @DealerPurchasePrice,
                        ShopSellingPrice = @ShopSellingPrice,
                        IsActive = 1,
                        UpdatedBy = @UpdatedBy,
                        UpdatedAt = GETDATE()
                    WHERE PriceId = @PriceId";

                string insertNew = @"
                    INSERT INTO ProductPrices
                    (
                        ProductOptionId,
                        EffectiveFrom,
                        PrintedPrice,
                        DealerPurchasePrice,
                        ShopSellingPrice,
                        IsActive,
                        CreatedBy,
                        CreatedAt
                    )
                    VALUES
                    (
                        @ProductOptionId,
                        @EffectiveFrom,
                        @PrintedPrice,
                        @DealerPurchasePrice,
                        @ShopSellingPrice,
                        1,
                        @UpdatedBy,
                        GETDATE()
                    )";

                // Same effective date -> amend that row in place.
                bool sameDay = false;
                int optionId = 0;

                string read = @"
                    SELECT ProductOptionId,
                           DATEDIFF(day, EffectiveFrom, @Today) = 0
                    FROM ProductPrices
                    WHERE PriceId = @PriceId";

                using (SqlCommand cmd =
                    new SqlCommand(read, con, tx))
                {
                    cmd.Parameters.Add(
                        "@PriceId",
                        SqlDbType.Int).Value = priceId;

                    cmd.Parameters.Add(
                        "@Today",
                        SqlDbType.Date).Value = DateTime.Today;

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            sameDay = Convert.ToBoolean(
                                reader[1]);

                            optionId = Convert.ToInt32(
                                reader[0]);
                        }
                    }
                }

                if (sameDay)
                {
                    using (SqlCommand cmd =
                        new SqlCommand(reopen, con, tx))
                    {
                        AddPriceParameters(
                            cmd,
                            priceId,
                            printed,
                            dealer,
                            shop);

                        cmd.Parameters.Add(
                            "@UpdatedBy",
                            SqlDbType.Int).Value = AdminId;

                        cmd.ExecuteNonQuery();
                    }

                    return;
                }

                // Different day: close the old row and start a new one.
                using (SqlCommand cmd =
                    new SqlCommand(closeHistory, con, tx))
                {
                    cmd.Parameters.Add(
                        "@PriceId",
                        SqlDbType.Int).Value = priceId;

                    cmd.Parameters.Add(
                        "@UpdatedBy",
                        SqlDbType.Int).Value = AdminId;

                    cmd.ExecuteNonQuery();
                }

                using (SqlCommand cmd =
                    new SqlCommand(insertNew, con, tx))
                {
                    cmd.Parameters.Add(
                        "@ProductOptionId",
                        SqlDbType.Int).Value = optionId;

                    cmd.Parameters.Add(
                        "@EffectiveFrom",
                        SqlDbType.Date).Value = DateTime.Today;

                    cmd.Parameters.Add(
                        "@PrintedPrice",
                        SqlDbType.Decimal).Value = printed;

                    cmd.Parameters.Add(
                        "@DealerPurchasePrice",
                        SqlDbType.Decimal).Value = dealer;

                    cmd.Parameters.Add(
                        "@ShopSellingPrice",
                        SqlDbType.Decimal).Value = shop;

                    cmd.Parameters.Add(
                        "@UpdatedBy",
                        SqlDbType.Int).Value = AdminId;

                    cmd.ExecuteNonQuery();
                }
            }


        private void AddPriceParameters(
            SqlCommand cmd,
            int keyId,
            decimal printed,
            decimal dealer,
            decimal shop)
        {
            string keyName =
                cmd.CommandText.IndexOf("UPDATE") >= 0
                    ? "@PriceId"
                    : "@ProductOptionId";

            cmd.Parameters.Add(
                keyName,
                SqlDbType.Int).Value = keyId;

            cmd.Parameters.Add(
                "@PrintedPrice",
                SqlDbType.Decimal).Value = printed;

            cmd.Parameters.Add(
                "@DealerPurchasePrice",
                SqlDbType.Decimal).Value = dealer;

            cmd.Parameters.Add(
                "@ShopSellingPrice",
                SqlDbType.Decimal).Value = shop;
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


        // ============================================================
        // WORK MODEL FOR ONE GRID ROW
        // ============================================================

        private sealed class UnitRow
        {
            public int UnitId;
            public bool Enabled;

            public string PacketsPerUnitRaw;
            public string PrintedRaw;
            public string DealerRaw;
            public string ShopRaw;

            public int PacketsPerUnit;
            public decimal Printed;
            public decimal Dealer;
            public decimal Shop;
        }
    }
}
