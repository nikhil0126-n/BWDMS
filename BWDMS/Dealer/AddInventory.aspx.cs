using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class AddInventory : System.Web.UI.Page
    {
        // ============================================================
        // IDS
        //
        // ?id=      -> existing Inventory row
        // ?variant= -> a variant that has no Inventory row yet
        // ============================================================

        private int InventoryId
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

        private int VariantFromQuery
        {
            get
            {
                int id;

                if (int.TryParse(
                    Request.QueryString["variant"],
                    out id))
                {
                    return id;
                }

                return 0;
            }
        }

        private int DealerId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
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

                int preset = InventoryId > 0
                    ? ExistingVariantId()
                    : VariantFromQuery;

                if (preset > 0 &&
                    ddlVariant.Items.FindByValue(
                        preset.ToString()) != null)
                {
                    ddlVariant.SelectedValue = preset.ToString();
                }

                ShowCurrentStock();
            }
        }


        // Reads the variant belonging to this row, so a forged
        // ?id= cannot make one dealer edit another's stock.
        private int ExistingVariantId()
        {
            string query = @"
                SELECT ProductVariantId
                FROM Inventory
                WHERE InventoryId = @InventoryId
                  AND DealerId = @DealerId";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@InventoryId",
                        SqlDbType.Int).Value = InventoryId;

                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    object result = cmd.ExecuteScalar();

                    return result == null ||
                           result == DBNull.Value
                        ? 0
                        : Convert.ToInt32(result);
                }
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
        // LIVE CURRENT-STOCK BANNER
        // ============================================================

        private void ShowCurrentStock()
        {
            int variantId = SelectedVariantId();

            if (variantId <= 0)
            {
                lblCurrent.Visible = false;
                return;
            }

            string query = @"
                SELECT
                    ISNULL(QuantityPackets, 0),
                    ISNULL(ReorderLevel, 0)
                FROM Inventory
                WHERE DealerId = @DealerId
                  AND ProductVariantId = @ProductVariantId";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    cmd.Parameters.Add(
                        "@ProductVariantId",
                        SqlDbType.Int).Value = variantId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        if (dt.Rows.Count == 0)
                        {
                            lblCurrent.Text =
                                "No stock recorded for this variant yet. " +
                                "Saving will create the row.";

                            lblCurrent.Visible = true;

                            return;
                        }

                        int qty = Convert.ToInt32(
                            dt.Rows[0][0]);

                        int reorder = Convert.ToInt32(
                            dt.Rows[0][1]);

                        lblCurrent.Text =
                            "Current stock: <strong>" + qty +
                            "</strong> packets &nbsp;|&nbsp; reorder level: <strong>" +
                            reorder + "</strong>";

                        lblCurrent.Visible = true;
                    }
                }
            }
        }


        private int SelectedVariantId()
        {
            int id = 0;

            int.TryParse(ddlVariant.SelectedValue, out id);

            return id;
        }


        protected void ddlVariant_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            ShowCurrentStock();
        }


        // ============================================================
        // SAVE
        //
        // 1. Upsert the Inventory row (UQ_Inventory_Dealer_Variant)
        // 2. If the quantity moved, write the ledger entry
        //
        // Both happen in one transaction.
        // ============================================================

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
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

            int quantity = 0;

            if (!int.TryParse(
                txtQuantity.Text.Trim(),
                out quantity) || quantity < 0)
            {
                ShowMessage(
                    "Quantity must be zero or more.",
                    false);

                return;
            }

            int reorder = 0;

            int.TryParse(txtReorder.Text.Trim(), out reorder);

            if (reorder < 0)
            {
                ShowMessage(
                    "Reorder level cannot be negative.",
                    false);

                return;
            }

            string remarks = txtRemarks.Text.Trim();
            string reference = txtReference.Text.Trim();


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
                            // ------------------------------------
                            // Current quantity for this dealer +
                            // variant (0 when no row exists).
                            //
                            // UPDLOCK holds the row for the rest
                            // of this transaction, so the balance
                            // read here is the same one the
                            // guarded write below acts on: two
                            // saves editing the same variant are
                            // serialised on the lock instead of
                            // both working from a stale value.
                            // ------------------------------------

                            int current = 0;
                            bool exists = false;

                            string readQuery = @"
                                SELECT InventoryId,
                                       QuantityPackets
                                FROM Inventory WITH (UPDLOCK, ROWLOCK)
                                WHERE DealerId = @DealerId
                                  AND ProductVariantId = @ProductVariantId";

                            using (SqlCommand cmd =
                                new SqlCommand(
                                    readQuery,
                                    con,
                                    tx))
                            {
                                cmd.Parameters.Add(
                                    "@DealerId",
                                    SqlDbType.Int).Value =
                                    DealerId;

                                cmd.Parameters.Add(
                                    "@ProductVariantId",
                                    SqlDbType.Int).Value =
                                    variantId;

                                using (SqlDataReader reader =
                                    cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        exists = true;
                                        current =
                                            Convert.ToInt32(
                                                reader["QuantityPackets"]);
                                    }
                                }
                            }


                            // ------------------------------------
                            // Upsert the stock level
                            //
                            // The typed figure is absolute, so the
                            // write repeats the quantity read
                            // under the lock as an optimistic
                            // guard: if another save moved the row
                            // first, the WHERE clause matches
                            // nothing and this request is refused
                            // instead of overwriting that save.
                            // ------------------------------------

                            if (exists)
                            {
                                WriteStockLevel(
                                    con,
                                    tx,
                                    variantId,
                                    quantity,
                                    reorder,
                                    current);
                            }
                            else
                            {
                                try
                                {
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
                                            @ReorderLevel,
                                            @UpdatedBy,
                                            GETDATE()
                                        )";

                                    using (SqlCommand cmd =
                                        new SqlCommand(
                                            insert,
                                            con,
                                            tx))
                                    {
                                        cmd.Parameters.Add(
                                            "@DealerId",
                                            SqlDbType.Int).Value =
                                            DealerId;

                                        cmd.Parameters.Add(
                                            "@ProductVariantId",
                                            SqlDbType.Int).Value =
                                            variantId;

                                        cmd.Parameters.Add(
                                            "@QuantityPackets",
                                            SqlDbType.Int).Value =
                                            quantity;

                                        cmd.Parameters.Add(
                                            "@ReorderLevel",
                                            SqlDbType.Int).Value =
                                            reorder;

                                        cmd.Parameters.Add(
                                            "@UpdatedBy",
                                            SqlDbType.Int).Value =
                                            DealerId;

                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                catch (SqlException ex)
                                {
                                    // UQ_Inventory_Dealer_Variant
                                    // fired because a concurrent
                                    // save created the row first:
                                    // fall back to one guarded
                                    // write against the quantity
                                    // this transaction read (0).
                                    if (ex.Number != 2601 &&
                                        ex.Number != 2627)
                                    {
                                        throw;
                                    }

                                    WriteStockLevel(
                                        con,
                                        tx,
                                        variantId,
                                        quantity,
                                        reorder,
                                        0);
                                }
                            }


                            // ------------------------------------
                            // Ledger entry when the level moved
                            // ------------------------------------

                            int delta = quantity - current;

                            if (delta != 0)
                            {
                                WriteTransaction(
                                    con,
                                    tx,
                                    variantId,
                                    delta,
                                    reference,
                                    remarks);
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
                    "~/Dealer/Inventory.aspx?saved=1");
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error saving stock: " + ex.Message,
                    false);
            }
        }


        // ============================================================
        // GUARDED STOCK WRITE
        //
        // Absolute write guarded by the quantity this transaction
        // locked and read. Zero rows means another save moved the
        // balance first, which is never silently overwritten.
        // ============================================================

        private void WriteStockLevel(
            SqlConnection con,
            SqlTransaction tx,
            int variantId,
            int quantity,
            int reorder,
            int expected)
        {
            string update = @"
                UPDATE Inventory
                SET QuantityPackets = @QuantityPackets,
                    ReorderLevel = @ReorderLevel,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE DealerId = @DealerId
                  AND ProductVariantId = @ProductVariantId
                  AND QuantityPackets = @Expected";

            using (SqlCommand cmd =
                new SqlCommand(update, con, tx))
            {
                cmd.Parameters.Add(
                    "@QuantityPackets",
                    SqlDbType.Int).Value = quantity;

                cmd.Parameters.Add(
                    "@ReorderLevel",
                    SqlDbType.Int).Value = reorder;

                cmd.Parameters.Add(
                    "@Expected",
                    SqlDbType.Int).Value = expected;

                cmd.Parameters.Add(
                    "@UpdatedBy",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                if (cmd.ExecuteNonQuery() == 0)
                {
                    throw new InvalidOperationException(
                        "Stock changed while you were saving. " +
                        "Please reload and try again.");
                }
            }
        }


        // ============================================================
        // LEDGER ENTRY
        // ============================================================

        private void WriteTransaction(
            SqlConnection con,
            SqlTransaction tx,
            int variantId,
            int delta,
            string reference,
            string remarks)
        {
            string type = delta > 0
                ? "Stock In"
                : "Stock Out";

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
                SELECT SCOPE_IDENTITY();";

            int transactionId;

            using (SqlCommand cmd =
                new SqlCommand(header, con, tx))
            {
                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@TransactionType",
                    SqlDbType.NVarChar,
                    80).Value = type;

                cmd.Parameters.Add(
                    "@ReferenceNo",
                    SqlDbType.NVarChar,
                    200).Value =
                    string.IsNullOrWhiteSpace(reference)
                        ? (object)DBNull.Value
                        : reference;

                cmd.Parameters.Add(
                    "@Remarks",
                    SqlDbType.NVarChar,
                    1000).Value =
                    string.IsNullOrWhiteSpace(remarks)
                        ? (object)DBNull.Value
                        : remarks;

                cmd.Parameters.Add(
                    "@CreatedBy",
                    SqlDbType.Int).Value = DealerId;

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
                    QuantityPackets
                )
                VALUES
                (
                    @StockTransactionId,
                    @ProductVariantId,
                    @UnitId,
                    @Quantity,
                    @QuantityPackets
                )";

            using (SqlCommand cmd =
                new SqlCommand(detail, con, tx))
            {
                cmd.Parameters.Add(
                    "@StockTransactionId",
                    SqlDbType.Int).Value = transactionId;

                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                cmd.Parameters.Add(
                    "@UnitId",
                    SqlDbType.Int).Value = PacketUnitId(con, tx);

                cmd.Parameters.Add(
                    "@Quantity",
                    SqlDbType.Int).Value = delta;

                cmd.Parameters.Add(
                    "@QuantityPackets",
                    SqlDbType.Int).Value = delta;

                cmd.ExecuteNonQuery();
            }
        }


        // Stock is counted in base packets, so the detail row is
        // tagged with the Packet selling unit when one exists.
        private object PacketUnitId(
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

                return result == null ||
                       result == DBNull.Value
                    ? (object)DBNull.Value
                    : result;
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
