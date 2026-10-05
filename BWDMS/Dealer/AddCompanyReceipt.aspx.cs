using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class AddCompanyReceipt : System.Web.UI.Page
    {
        // ============================================================
        // STATE
        //
        // ?id= absent  -> new receipt
        // ?id=n        -> existing receipt (read-only when Posted)
        // ============================================================

        // Cached so a brand-new receipt can be given its id the
        // moment the header row is inserted.
        private int _receiptId = -1;

        private int ReceiptId
        {
            get
            {
                if (_receiptId < 0)
                {
                    int id;

                    _receiptId = int.TryParse(
                        Request.QueryString["id"],
                        out id)
                        ? id
                        : 0;
                }

                return _receiptId;
            }
            set { _receiptId = value; }
        }

        private int DealerId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }

        private bool Editing
        {
            get { return ReceiptId > 0; }
        }


        // A Posted receipt is read-only. The flag is kept in
        // ViewState (MAC protected) so every postback of the same
        // page load sees the same answer.
        private bool IsPosted
        {
            get
            {
                if (!Editing)
                {
                    return false;
                }

                object saved = ViewState["IsPosted"];

                if (saved == null)
                {
                    saved = ReadStatus() == "Posted";

                    ViewState["IsPosted"] = saved;
                }

                return (bool)saved;
            }
        }


        // The line list survives postbacks so the builder, the
        // grid and the save all work from one source of truth.
        private DataTable Lines
        {
            get
            {
                DataTable dt =
                    ViewState["Lines"] as DataTable;

                if (dt == null)
                {
                    dt = NewLineTable();
                    ViewState["Lines"] = dt;
                }

                return dt;
            }
            set { ViewState["Lines"] = value; }
        }

        private static DataTable NewLineTable()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("Index", typeof(int));
            dt.Columns.Add("ProductVariantId", typeof(int));
            dt.Columns.Add("VariantLabel", typeof(string));
            dt.Columns.Add("Quantity", typeof(int));
            dt.Columns.Add("UnitCostPrice", typeof(decimal));
            dt.Columns.Add("LineTotal", typeof(decimal));
            dt.Columns.Add("UnitCostText", typeof(string));
            dt.Columns.Add("LineTotalText", typeof(string));

            return dt;
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            Response.Cache.SetExpires(
                DateTime.UtcNow.AddYears(-1));

            Response.Cache.SetRevalidation(
                System.Web.HttpCacheRevalidation.AllCaches);


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
                    "Dealer",
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
                LoadVariants();

                if (Editing)
                {
                    if (!LoadReceipt())
                    {
                        Response.Redirect(
                            "~/Dealer/CompanyReceipts.aspx",
                            false);

                        Context.ApplicationInstance.CompleteRequest();

                        return;
                    }
                }
                else
                {
                    txtReceiptDate.Text =
                        DateTime.Today.ToString("yyyy-MM-dd");

                    txtReceiptNumber.Text = NextReceiptNumber();

                    BindLines();
                }
            }

            ApplyReadOnlyMode();
        }


        // ============================================================
        // READ-ONLY MODE FOR A POSTED RECEIPT
        // ============================================================

        private void ApplyReadOnlyMode()
        {
            if (!IsPosted)
            {
                return;
            }

            lblHeading.Text = "Company Receipt";
            lblPageTitle.Text = "Company Receipt";

            lblPostedNotice.Visible = true;

            txtReceiptNumber.Enabled = false;
            txtReceiptDate.Enabled = false;
            txtCompanyInvoiceNo.Enabled = false;
            txtNotes.Enabled = false;

            pnlLineBuilder.Visible = false;
            pnlButtons.Visible = false;

            // Hide the Remove column - the lines are frozen too.
            if (gvLines.Columns.Count > 0)
            {
                gvLines.Columns[gvLines.Columns.Count - 1].Visible =
                    false;
            }
        }


        private void LoadVariants()
        {
            string query = @"
                SELECT
                    v.ProductVariantId,
                    VariantLabel =
                        p.ProductName
                        + ' - '
                        + v.VariantName
                        + ISNULL(
                            ' ('
                            + CONVERT(NVARCHAR(20), v.PacketWeight)
                            + ' ' + v.WeightUnit + ')',
                            '')
                FROM ProductVariants v
                INNER JOIN Products p
                    ON p.ProductId = v.ProductId
                WHERE v.IsActive = 1
                  AND p.IsActive = 1
                ORDER BY p.ProductName, v.SortOrder, v.VariantName";

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

                        ddlVariant.DataSource = dt;
                        ddlVariant.DataTextField = "VariantLabel";
                        ddlVariant.DataValueField = "ProductVariantId";
                        ddlVariant.DataBind();

                        ddlVariant.Items.Insert(
                            0,
                            new ListItem("Select Variant", ""));
                    }
                }
            }
        }


        // ============================================================
        // LOAD EXISTING RECEIPT
        // ============================================================

        private bool LoadReceipt()
        {
            string query = @"
                SELECT
                    r.ReceiptNumber,
                    ReceiptDate =
                        CONVERT(NVARCHAR(10), r.ReceiptDate, 120),
                    CompanyInvoiceNo =
                        ISNULL(r.CompanyInvoiceNo, ''),
                    Notes = ISNULL(r.Notes, ''),
                    r.Status
                FROM CompanyStockReceipts r
                WHERE r.ReceiptId = @ReceiptId
                  AND r.DealerId = @DealerId";

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@ReceiptId",
                        SqlDbType.Int).Value = ReceiptId;

                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            if (dt.Rows.Count == 0)
            {
                return false;
            }

            DataRow row = dt.Rows[0];
            string status = row["Status"].ToString();

            lblHeading.Text = "Company Receipt";
            lblPageTitle.Text = "Company Receipt";

            txtReceiptNumber.Text =
                row["ReceiptNumber"].ToString();
            txtReceiptDate.Text =
                row["ReceiptDate"].ToString();
            txtCompanyInvoiceNo.Text =
                row["CompanyInvoiceNo"].ToString();
            txtNotes.Text = row["Notes"].ToString();

            ViewState["IsPosted"] =
                status.Equals(
                    "Posted",
                    StringComparison.OrdinalIgnoreCase);

            LoadReceiptLines();
            BindLines();

            return true;
        }


        private void LoadReceiptLines()
        {
            string query = @"
                SELECT
                    d.ProductVariantId,
                    d.Quantity,
                    UnitCostPrice = ISNULL(d.UnitCostPrice, 0),
                    VariantLabel =
                        p.ProductName + ' - ' + v.VariantName
                FROM CompanyStockReceiptDetails d
                INNER JOIN ProductVariants v
                    ON v.ProductVariantId = d.ProductVariantId
                INNER JOIN Products p
                    ON p.ProductId = v.ProductId
                WHERE d.ReceiptId = @ReceiptId
                ORDER BY d.ReceiptDetailId";

            DataTable loaded = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@ReceiptId",
                        SqlDbType.Int).Value = ReceiptId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(loaded);
                    }
                }
            }

            DataTable dt = NewLineTable();

            int i = 0;

            foreach (DataRow r in loaded.Rows)
            {
                DataRow row = dt.NewRow();
                int quantity = Convert.ToInt32(r["Quantity"]);
                decimal cost =
                    Convert.ToDecimal(r["UnitCostPrice"]);

                row["Index"] = i++;
                row["ProductVariantId"] =
                    Convert.ToInt32(r["ProductVariantId"]);
                row["VariantLabel"] =
                    r["VariantLabel"].ToString();
                row["Quantity"] = quantity;
                row["UnitCostPrice"] = cost;
                row["LineTotal"] = quantity * cost;

                dt.Rows.Add(row);
            }

            Lines = dt;
        }


        private string ReadStatus()
        {
            string query = @"
                SELECT Status
                FROM CompanyStockReceipts
                WHERE ReceiptId = @ReceiptId
                  AND DealerId = @DealerId";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@ReceiptId",
                        SqlDbType.Int).Value = ReceiptId;

                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    object result = cmd.ExecuteScalar();

                    return result == null ||
                           result == DBNull.Value
                        ? ""
                        : result.ToString();
                }
            }
        }


        // ============================================================
        // LINES
        // ============================================================

        protected void btnAddLine_Click(
            object sender,
            EventArgs e)
        {
            if (IsPosted)
            {
                return;
            }

            Page.Validate("LineAdd");

            if (!Page.IsValid)
            {
                ShowMessage(
                    "Please correct the highlighted line fields.",
                    false);

                return;
            }

            int variantId = SelectedVariantId();

            if (variantId <= 0)
            {
                ShowMessage(
                    "Select a product variant.",
                    false);

                return;
            }

            int quantity;

            if (!int.TryParse(
                    txtQuantity.Text.Trim(),
                    out quantity) || quantity <= 0)
            {
                ShowMessage(
                    "Quantity must be a whole number above zero.",
                    false);

                return;
            }

            decimal unitCost;

            if (!TryDecimal(
                    txtUnitCost.Text.Trim(),
                    out unitCost) || unitCost < 0)
            {
                ShowMessage(
                    "Unit cost must be zero or more.",
                    false);

                return;
            }

            DataTable dt = Lines;

            // One line per variant - the details table is unique
            // on (ReceiptId, ProductVariantId), so a repeated
            // variant grows the line that is already there.
            DataRow existing = null;

            foreach (DataRow row in dt.Rows)
            {
                if (Convert.ToInt32(
                        row["ProductVariantId"]) == variantId)
                {
                    existing = row;
                    break;
                }
            }

            if (existing != null)
            {
                int newQty =
                    Convert.ToInt32(existing["Quantity"]) +
                    quantity;

                existing["Quantity"] = newQty;
                existing["UnitCostPrice"] = unitCost;
                existing["LineTotal"] = newQty * unitCost;

                ShowMessage(
                    "That variant is already on the receipt - its quantity was increased.",
                    true);
            }
            else
            {
                DataRow row = dt.NewRow();

                row["Index"] = dt.Rows.Count;
                row["ProductVariantId"] = variantId;
                row["VariantLabel"] = VariantLabel(variantId);
                row["Quantity"] = quantity;
                row["UnitCostPrice"] = unitCost;
                row["LineTotal"] = quantity * unitCost;

                dt.Rows.Add(row);

                ShowMessage("Line added.", true);
            }

            Lines = dt;

            BindLines();
        }


        protected void gvLines_RowCommand(
            object sender,
            GridViewCommandEventArgs e)
        {
            if (IsPosted ||
                e.CommandName != "Remove")
            {
                return;
            }

            int index;

            if (!int.TryParse(
                e.CommandArgument.ToString(),
                out index))
            {
                return;
            }

            DataTable dt = Lines;

            if (index < 0 || index >= dt.Rows.Count)
            {
                return;
            }

            dt.Rows.RemoveAt(index);
            Lines = dt;

            BindLines();
        }


        private void BindLines()
        {
            DataTable dt = Lines;

            int i = 0;

            foreach (DataRow row in dt.Rows)
            {
                row["Index"] = i++;

                decimal cost =
                    Convert.ToDecimal(row["UnitCostPrice"]);

                row["UnitCostText"] = cost.ToString("N2");
                row["LineTotalText"] =
                    Convert.ToDecimal(row["LineTotal"])
                        .ToString("N2");
            }

            gvLines.DataSource = dt;
            gvLines.DataBind();

            // The grand total is always rebuilt from the rows -
            // nothing comes back from the browser.
            decimal grandTotal = 0m;

            foreach (DataRow row in dt.Rows)
            {
                int quantity =
                    Convert.ToInt32(row["Quantity"]);

                decimal unitCost =
                    Convert.ToDecimal(row["UnitCostPrice"]);

                row["LineTotal"] = quantity * unitCost;

                grandTotal += quantity * unitCost;
            }

            lblGrandTotal.Text = grandTotal.ToString("N2");
        }


        private int SelectedVariantId()
        {
            int id = 0;

            int.TryParse(ddlVariant.SelectedValue, out id);

            return id;
        }


        private string VariantLabel(int variantId)
        {
            string query = @"
                SELECT p.ProductName + ' - ' + v.VariantName
                FROM ProductVariants v
                INNER JOIN Products p
                    ON p.ProductId = v.ProductId
                WHERE v.ProductVariantId = @ProductVariantId";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@ProductVariantId",
                        SqlDbType.Int).Value = variantId;

                    object result = cmd.ExecuteScalar();

                    return result == null ||
                           result == DBNull.Value
                        ? "variant #" + variantId
                        : result.ToString();
                }
            }
        }


        // ============================================================
        // SAVE
        //
        // One transaction for everything: reject a duplicate
        // number, upsert the header, replace the details and -
        // when posting - move the stock and write the ledger.
        // ============================================================

        protected void btnSaveDraft_Click(
            object sender,
            EventArgs e)
        {
            Save(false);
        }


        protected void btnPost_Click(
            object sender,
            EventArgs e)
        {
            Save(true);
        }


        private void Save(bool post)
        {
            if (IsPosted)
            {
                ShowMessage(
                    "This receipt is already posted and cannot be edited.",
                    false);

                return;
            }

            Page.Validate("Receipt");

            if (!Page.IsValid)
            {
                ShowMessage(
                    "Please correct the highlighted fields.",
                    false);

                return;
            }

            string number = txtReceiptNumber.Text.Trim();

            if (number.Length == 0)
            {
                ShowMessage(
                    "Receipt number is required.",
                    false);

                return;
            }

            DateTime receiptDate;

            if (!TryDate(
                    txtReceiptDate.Text.Trim(),
                    out receiptDate))
            {
                ShowMessage(
                    "Receipt date is required and must be a valid date (YYYY-MM-DD).",
                    false);

                return;
            }

            DataTable lines = Lines;

            if (lines.Rows.Count == 0)
            {
                ShowMessage(
                    "Add at least one receipt line.",
                    false);

                return;
            }

            // Grand total recalculated on the server for every
            // line - the value the browser shows is never trusted.
            decimal totalCost = 0m;

            foreach (DataRow row in lines.Rows)
            {
                int quantity =
                    Convert.ToInt32(row["Quantity"]);

                decimal unitCost =
                    Convert.ToDecimal(row["UnitCostPrice"]);

                row["LineTotal"] = quantity * unitCost;

                totalCost += quantity * unitCost;
            }

            string invoice =
                string.IsNullOrWhiteSpace(
                    txtCompanyInvoiceNo.Text.Trim())
                    ? null
                    : txtCompanyInvoiceNo.Text.Trim();

            string notes =
                string.IsNullOrWhiteSpace(
                    txtNotes.Text.Trim())
                    ? null
                    : txtNotes.Text.Trim();

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
                            // ----------------------------
                            // (a) one number per dealer
                            // ----------------------------

                            if (ReceiptNumberExists(
                                    con, tx, number))
                            {
                                throw new InvalidOperationException(
                                    "Receipt number '" + number +
                                    "' already exists for your dealership.");
                            }

                            int unitId =
                                PacketUnitId(con, tx);

                            // ----------------------------
                            // (b) header + details
                            // ----------------------------

                            if (Editing)
                            {
                                string update = @"
                                    UPDATE CompanyStockReceipts
                                    SET ReceiptNumber = @ReceiptNumber,
                                        ReceiptDate = @ReceiptDate,
                                        CompanyInvoiceNo = @CompanyInvoiceNo,
                                        TotalCost = @TotalCost,
                                        Notes = @Notes,
                                        UpdatedBy = @UpdatedBy,
                                        UpdatedAt = GETDATE()
                                    WHERE ReceiptId = @ReceiptId
                                      AND DealerId = @DealerId
                                      AND Status = 'Draft'";

                                using (SqlCommand cmd =
                                    new SqlCommand(
                                        update,
                                        con,
                                        tx))
                                {
                                    AddHeaderParameters(
                                        cmd,
                                        number,
                                        receiptDate,
                                        invoice,
                                        totalCost,
                                        notes);

                                    cmd.Parameters.Add(
                                        "@ReceiptId",
                                        SqlDbType.Int).Value =
                                        ReceiptId;

                                    cmd.Parameters.Add(
                                        "@DealerId",
                                        SqlDbType.Int).Value =
                                        DealerId;

                                    cmd.Parameters.Add(
                                        "@UpdatedBy",
                                        SqlDbType.Int).Value =
                                        DealerId;

                                    if (cmd.ExecuteNonQuery() != 1)
                                    {
                                        bool alreadyPosted =
                                            ReadStatus(
                                                con,
                                                tx) == "Posted";

                                        throw new InvalidOperationException(
                                            post && alreadyPosted
                                                ? "This receipt was already posted."
                                                : "This receipt is already posted and cannot be edited.");
                                    }
                                }
                            }
                            else
                            {
                                string insert = @"
                                    INSERT INTO CompanyStockReceipts
                                    (
                                        DealerId,
                                        ReceiptNumber,
                                        ReceiptDate,
                                        CompanyInvoiceNo,
                                        Status,
                                        TotalCost,
                                        Notes,
                                        CreatedBy
                                    )
                                    VALUES
                                    (
                                        @DealerId,
                                        @ReceiptNumber,
                                        @ReceiptDate,
                                        @CompanyInvoiceNo,
                                        'Draft',
                                        @TotalCost,
                                        @Notes,
                                        @CreatedBy
                                    );
                                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                                using (SqlCommand cmd =
                                    new SqlCommand(
                                        insert,
                                        con,
                                        tx))
                                {
                                    AddHeaderParameters(
                                        cmd,
                                        number,
                                        receiptDate,
                                        invoice,
                                        totalCost,
                                        notes);

                                    cmd.Parameters.Add(
                                        "@DealerId",
                                        SqlDbType.Int).Value =
                                        DealerId;

                                    cmd.Parameters.Add(
                                        "@CreatedBy",
                                        SqlDbType.Int).Value =
                                        DealerId;

                                    ReceiptId =
                                        Convert.ToInt32(
                                            cmd.ExecuteScalar());
                                }
                            }

                            // Replace the details

                            string clear = @"
                                DELETE FROM CompanyStockReceiptDetails
                                WHERE ReceiptId = @ReceiptId";

                            using (SqlCommand cmd =
                                new SqlCommand(
                                    clear,
                                    con,
                                    tx))
                            {
                                cmd.Parameters.Add(
                                    "@ReceiptId",
                                    SqlDbType.Int).Value =
                                    ReceiptId;

                                cmd.ExecuteNonQuery();
                            }

                            foreach (DataRow row in lines.Rows)
                            {
                                string detail = @"
                                    INSERT INTO CompanyStockReceiptDetails
                                    (
                                        ReceiptId,
                                        ProductVariantId,
                                        UnitId,
                                        Quantity,
                                        UnitCostPrice,
                                        TotalCost
                                    )
                                    VALUES
                                    (
                                        @ReceiptId,
                                        @ProductVariantId,
                                        @UnitId,
                                        @Quantity,
                                        @UnitCostPrice,
                                        @TotalCost
                                    )";

                                using (SqlCommand cmd =
                                    new SqlCommand(
                                        detail,
                                        con,
                                        tx))
                                {
                                    int quantity =
                                        Convert.ToInt32(
                                            row["Quantity"]);

                                    decimal unitCost =
                                        Convert.ToDecimal(
                                            row["UnitCostPrice"]);

                                    SqlParameter unitCostParameter;

                                    cmd.Parameters.Add(
                                        "@ReceiptId",
                                        SqlDbType.Int).Value =
                                        ReceiptId;

                                    cmd.Parameters.Add(
                                        "@ProductVariantId",
                                        SqlDbType.Int).Value =
                                        Convert.ToInt32(
                                            row["ProductVariantId"]);

                                    cmd.Parameters.Add(
                                        "@UnitId",
                                        SqlDbType.Int).Value =
                                        unitId;

                                    cmd.Parameters.Add(
                                        "@Quantity",
                                        SqlDbType.Int).Value =
                                        quantity;

                                    unitCostParameter =
                                        cmd.Parameters.Add(
                                            "@UnitCostPrice",
                                            SqlDbType.Decimal);

                                    unitCostParameter.Precision = 18;
                                    unitCostParameter.Scale = 2;
                                    unitCostParameter.Value = unitCost;

                                    SqlParameter totalParameter =
                                        cmd.Parameters.Add(
                                            "@TotalCost",
                                            SqlDbType.Decimal);

                                    totalParameter.Precision = 18;
                                    totalParameter.Scale = 2;
                                    totalParameter.Value =
                                        quantity * unitCost;

                                    cmd.ExecuteNonQuery();
                                }
                            }

                            // ----------------------------
                            // (c) posting: move the stock
                            // ----------------------------

                            if (post)
                            {
                                PostReceipt(
                                    con,
                                    tx,
                                    DealerId,
                                    ReceiptId);
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
                    post
                        ? "~/Dealer/CompanyReceipts.aspx?posted=1"
                        : "~/Dealer/CompanyReceipts.aspx?saved=1");
            }
            catch (InvalidOperationException ex)
            {
                ShowMessage(ex.Message, false);
            }
            catch (SqlException ex)
            {
                ShowMessage(
                    DbErrorMessage(ex, number),
                    false);
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error saving receipt: " + ex.Message,
                    false);
            }
        }


        private void AddHeaderParameters(
            SqlCommand cmd,
            string number,
            DateTime receiptDate,
            string invoice,
            decimal totalCost,
            string notes)
        {
            cmd.Parameters.Add(
                "@ReceiptNumber",
                SqlDbType.NVarChar,
                100).Value = number;

            cmd.Parameters.Add(
                "@ReceiptDate",
                SqlDbType.Date).Value = receiptDate;

            cmd.Parameters.Add(
                "@CompanyInvoiceNo",
                SqlDbType.NVarChar,
                200).Value =
                invoice == null
                    ? (object)DBNull.Value
                    : invoice;

            SqlParameter totalParameter =
                cmd.Parameters.Add(
                    "@TotalCost",
                    SqlDbType.Decimal);

            totalParameter.Precision = 18;
            totalParameter.Scale = 2;
            totalParameter.Value = totalCost;

            cmd.Parameters.Add(
                "@Notes",
                SqlDbType.NVarChar,
                2000).Value =
                notes == null
                    ? (object)DBNull.Value
                    : notes;
        }


        private string DbErrorMessage(
            SqlException ex,
            string number)
        {
            if (ex.Number == 2601 || ex.Number == 2627)
            {
                if (ex.Message.Contains(
                    "UQ_CompanyStockReceipts_Dealer_Number"))
                {
                    return "Receipt number '" + number +
                           "' already exists for your dealership.";
                }
            }

            return "Error saving receipt: " + ex.Message;
        }


        private bool ReceiptNumberExists(
            SqlConnection con,
            SqlTransaction tx,
            string number)
        {
            string query = @"
                SELECT COUNT(*)
                FROM CompanyStockReceipts
                WHERE DealerId = @DealerId
                  AND ReceiptNumber = @ReceiptNumber
                  AND ReceiptId <> @ReceiptId";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@ReceiptNumber",
                    SqlDbType.NVarChar,
                    100).Value = number;

                cmd.Parameters.Add(
                    "@ReceiptId",
                    SqlDbType.Int).Value = ReceiptId;

                return Convert.ToInt32(
                    cmd.ExecuteScalar()) > 0;
            }
        }


        private string ReadStatus(
            SqlConnection con,
            SqlTransaction tx)
        {
            string query = @"
                SELECT Status
                FROM CompanyStockReceipts
                WHERE ReceiptId = @ReceiptId
                  AND DealerId = @DealerId";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@ReceiptId",
                    SqlDbType.Int).Value = ReceiptId;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                object result = cmd.ExecuteScalar();

                return result == null ||
                       result == DBNull.Value
                    ? ""
                    : result.ToString();
            }
        }


        // Suggested next number: RCP-yyyymmdd-#### counting the
        // receipts already taken for today.
        private string NextReceiptNumber()
        {
            string prefix =
                "RCP-" +
                DateTime.Today.ToString("yyyyMMdd") +
                "-";

            string query = @"
                SELECT ISNULL(
                    MAX(
                        TRY_CONVERT(
                            INT,
                            RIGHT(
                                ReceiptNumber,
                                LEN(ReceiptNumber) -
                                LEN(@Prefix)))),
                    0)
                FROM CompanyStockReceipts
                WHERE DealerId = @DealerId
                  AND ReceiptNumber LIKE @Prefix + '%'";

            int last = 0;

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@Prefix",
                        SqlDbType.NVarChar,
                        100).Value = prefix;

                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    last = Convert.ToInt32(
                        cmd.ExecuteScalar());
                }
            }

            return prefix + (last + 1).ToString("D4");
        }


        // ============================================================
        // POST ROUTINE
        //
        // ONE routine, shared by "Save & Post" here and by the
        // Post link on CompanyReceipts, so the two entry points
        // can never drift apart. It runs inside the caller's
        // transaction: flip the status, raise the godown stock and
        // write the ledger - all or nothing.
        // ============================================================

        internal static void PostReceipt(
            SqlConnection con,
            SqlTransaction tx,
            int dealerId,
            int receiptId)
        {
            // ------------------------------------
            // Race-safe flip: the row must still
            // be a Draft, or nothing happens.
            // ------------------------------------

            string flip = @"
                UPDATE CompanyStockReceipts
                SET Status = 'Posted',
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE ReceiptId = @ReceiptId
                  AND DealerId = @DealerId
                  AND Status = 'Draft'";

            int affected;

            using (SqlCommand cmd =
                new SqlCommand(flip, con, tx))
            {
                cmd.Parameters.Add(
                    "@UpdatedBy",
                    SqlDbType.Int).Value = dealerId;

                cmd.Parameters.Add(
                    "@ReceiptId",
                    SqlDbType.Int).Value = receiptId;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = dealerId;

                affected = cmd.ExecuteNonQuery();
            }

            if (affected != 1)
            {
                throw new InvalidOperationException(
                    "This receipt was already posted.");
            }


            // ------------------------------------
            // Header
            // ------------------------------------

            string number;
            string notes;

            string header = @"
                SELECT
                    ReceiptNumber,
                    ISNULL(Notes, '')
                FROM CompanyStockReceipts
                WHERE ReceiptId = @ReceiptId
                  AND DealerId = @DealerId";

            using (SqlCommand cmd =
                new SqlCommand(header, con, tx))
            {
                cmd.Parameters.Add(
                    "@ReceiptId",
                    SqlDbType.Int).Value = receiptId;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = dealerId;

                using (SqlDataReader reader =
                    cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new InvalidOperationException(
                            "Receipt not found.");
                    }

                    number = reader.GetString(0);
                    notes = reader.IsDBNull(1)
                        ? ""
                        : reader.GetString(1);
                }
            }


            // ------------------------------------
            // Lines
            // ------------------------------------

            List<ReceiptLine> lines =
                new List<ReceiptLine>();

            string detailQuery = @"
                SELECT
                    d.ProductVariantId,
                    d.Quantity,
                    UnitCostPrice = ISNULL(d.UnitCostPrice, 0)
                FROM CompanyStockReceiptDetails d
                WHERE d.ReceiptId = @ReceiptId
                ORDER BY d.ReceiptDetailId";

            using (SqlCommand cmd =
                new SqlCommand(detailQuery, con, tx))
            {
                cmd.Parameters.Add(
                    "@ReceiptId",
                    SqlDbType.Int).Value = receiptId;

                using (SqlDataReader reader =
                    cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lines.Add(
                            new ReceiptLine(
                                Convert.ToInt32(
                                    reader["ProductVariantId"]),
                                Convert.ToInt32(
                                    reader["Quantity"]),
                                Convert.ToDecimal(
                                    reader["UnitCostPrice"])));
                    }
                }
            }

            if (lines.Count == 0)
            {
                throw new InvalidOperationException(
                    "This receipt has no lines to post.");
            }


            // ------------------------------------
            // (c) into the godown, one line at a
            //     time under a row lock
            // ------------------------------------

            foreach (ReceiptLine line in lines)
            {
                IncreaseGodown(
                    con,
                    tx,
                    dealerId,
                    line.VariantId,
                    line.Quantity);
            }


            // ------------------------------------
            // (d) one ledger header + the lines
            // ------------------------------------

            WriteReceiptLedger(
                con,
                tx,
                dealerId,
                number,
                notes,
                lines);
        }


        // Stock is counted in base packets, so every line of a
        // receipt adds its quantity to the godown.
        private static void IncreaseGodown(
            SqlConnection con,
            SqlTransaction tx,
            int dealerId,
            int variantId,
            int quantity)
        {
            // Lock the row when it exists so two receipts posting
            // at the same moment cannot lose an update.
            string read = @"
                SELECT QuantityPackets
                FROM Inventory WITH (UPDLOCK, ROWLOCK)
                WHERE DealerId = @DealerId
                  AND ProductVariantId = @ProductVariantId";

            using (SqlCommand cmd =
                new SqlCommand(read, con, tx))
            {
                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = dealerId;

                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                cmd.ExecuteScalar();
            }

            string update = @"
                UPDATE Inventory
                SET QuantityPackets =
                        QuantityPackets + @QuantityPackets,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE DealerId = @DealerId
                  AND ProductVariantId = @ProductVariantId";

            if (ExecuteIncrease(
                    con,
                    tx,
                    update,
                    dealerId,
                    variantId,
                    quantity) > 0)
            {
                return;
            }

            // No Inventory row yet - create it with this quantity.
            string insert = @"
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
                    @QuantityPackets,
                    0,
                    @UpdatedBy,
                    GETDATE()
                )";

            using (SqlCommand cmd =
                new SqlCommand(insert, con, tx))
            {
                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = dealerId;

                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                cmd.Parameters.Add(
                    "@QuantityPackets",
                    SqlDbType.Int).Value = quantity;

                cmd.Parameters.Add(
                    "@UpdatedBy",
                    SqlDbType.Int).Value = dealerId;

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    // UQ_Inventory_Dealer_Variant lost the race -
                    // the row exists now, so retry the increase
                    // exactly once.
                    if (!IsDuplicateKey(ex))
                    {
                        throw;
                    }

                    if (ExecuteIncrease(
                            con,
                            tx,
                            update,
                            dealerId,
                            variantId,
                            quantity) == 0)
                    {
                        throw new InvalidOperationException(
                            "Could not increase the godown stock for variant #" +
                            variantId + ".");
                    }
                }
            }
        }


        private static int ExecuteIncrease(
            SqlConnection con,
            SqlTransaction tx,
            string update,
            int dealerId,
            int variantId,
            int quantity)
        {
            using (SqlCommand cmd =
                new SqlCommand(update, con, tx))
            {
                cmd.Parameters.Add(
                    "@QuantityPackets",
                    SqlDbType.Int).Value = quantity;

                cmd.Parameters.Add(
                    "@UpdatedBy",
                    SqlDbType.Int).Value = dealerId;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = dealerId;

                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                return cmd.ExecuteNonQuery();
            }
        }


        private static void WriteReceiptLedger(
            SqlConnection con,
            SqlTransaction tx,
            int dealerId,
            string number,
            string notes,
            List<ReceiptLine> lines)
        {
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
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

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
                    200).Value = number;

                cmd.Parameters.Add(
                    "@Remarks",
                    SqlDbType.NVarChar,
                    1000).Value =
                    string.IsNullOrWhiteSpace(notes)
                        ? (object)("Company receipt " + number)
                        : (object)notes;

                cmd.Parameters.Add(
                    "@CreatedBy",
                    SqlDbType.Int).Value = dealerId;

                transactionId =
                    Convert.ToInt32(cmd.ExecuteScalar());
            }

            int unitId = PacketUnitId(con, tx);

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

            foreach (ReceiptLine line in lines)
            {
                using (SqlCommand cmd =
                    new SqlCommand(detail, con, tx))
                {
                    SqlParameter costParameter;

                    cmd.Parameters.Add(
                        "@StockTransactionId",
                        SqlDbType.Int).Value =
                        transactionId;

                    cmd.Parameters.Add(
                        "@ProductVariantId",
                        SqlDbType.Int).Value =
                        line.VariantId;

                    cmd.Parameters.Add(
                        "@UnitId",
                        SqlDbType.Int).Value = unitId;

                    cmd.Parameters.Add(
                        "@Quantity",
                        SqlDbType.Int).Value =
                        line.Quantity;

                    cmd.Parameters.Add(
                        "@QuantityPackets",
                        SqlDbType.Int).Value =
                        line.Quantity;

                    costParameter =
                        cmd.Parameters.Add(
                            "@UnitCostPrice",
                            SqlDbType.Decimal);

                    costParameter.Precision = 18;
                    costParameter.Scale = 2;
                    costParameter.Value = line.UnitCost;

                    cmd.ExecuteNonQuery();
                }
            }
        }


        // Stock is counted in base packets, so the detail rows are
        // tagged with the Packet selling unit.
        private static int PacketUnitId(
            SqlConnection con,
            SqlTransaction tx)
        {
            string query = @"
                SELECT TOP (1) UnitId
                FROM SellingUnits
                WHERE UnitCode = 'PKT'
                  AND IsActive = 1
                ORDER BY UnitId";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                object result = cmd.ExecuteScalar();

                int unitId =
                    result == null ||
                    result == DBNull.Value
                        ? 0
                        : Convert.ToInt32(result);

                if (unitId <= 0)
                {
                    throw new InvalidOperationException(
                        "The packet selling unit (PKT) is missing from the unit master.");
                }

                return unitId;
            }
        }


        private static bool IsDuplicateKey(SqlException ex)
        {
            return ex.Number == 2601 ||
                   ex.Number == 2627;
        }


        private sealed class ReceiptLine
        {
            public ReceiptLine(
                int variantId,
                int quantity,
                decimal unitCost)
            {
                VariantId = variantId;
                Quantity = quantity;
                UnitCost = unitCost;
            }

            public int VariantId { get; private set; }
            public int Quantity { get; private set; }
            public decimal UnitCost { get; private set; }
        }


        // ============================================================
        // HELPERS
        // ============================================================

        private static bool TryDecimal(
            string text,
            out decimal value)
        {
            return decimal.TryParse(
                text,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out value);
        }

        private static bool TryDate(
            string text,
            out DateTime value)
        {
            return DateTime.TryParse(
                text,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out value);
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
