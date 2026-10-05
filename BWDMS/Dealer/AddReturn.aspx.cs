using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class AddReturn : System.Web.UI.Page
    {
        // ============================================================
        // IDS
        //
        // ?returnId= -> read-only view of an already recorded return
        // ============================================================

        private int ReturnId
        {
            get
            {
                int id;

                if (int.TryParse(
                    Request.QueryString["returnId"],
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
                LoadOrders();

                txtReturnDate.Text =
                    DateTime.Today.ToString("yyyy-MM-dd");

                if (ReturnId > 0)
                {
                    if (!LoadReturn())
                    {
                        // A forged ?returnId (another dealer's
                        // return, or none at all) never renders.
                        ShowMessage(
                            "Return not found.",
                            false);

                        pnlForm.Visible = false;

                        return;
                    }
                }
                else
                {
                    BindLines(0);
                }
            }
            else if (ReturnId <= 0)
            {
                // Rebuild the line editor for the posted selection
                // before post data is applied, so the save handler
                // can read the quantities the user typed.
                BindLines(SelectedOrderId());
            }
        }


        // ============================================================
        // DROPDOWNS
        // ============================================================

        private void LoadOrders()
        {
            // Only dispatched goods can come back from a shop, so
            // no other status is ever listed here.
            string query = @"
                SELECT
                    o.OrderId,
                    Label = o.OrderNumber
                            + ' - '
                            + s.ShopName
                            + ' - '
                            + CONVERT(NVARCHAR(10), o.OrderDate, 120)
                FROM Orders o
                INNER JOIN Shops s
                    ON s.ShopId = o.ShopId
                WHERE o.DealerId = @DealerId
                  AND o.Status = 'Dispatched'
                ORDER BY o.OrderId DESC";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        ddlOrder.DataSource = dt;
                        ddlOrder.DataTextField = "Label";
                        ddlOrder.DataValueField = "OrderId";
                        ddlOrder.DataBind();
                    }
                }
            }

            ddlOrder.Items.Insert(
                0,
                new ListItem("Select a dispatched order", ""));
        }


        private int SelectedOrderId()
        {
            int id = 0;

            int.TryParse(ddlOrder.SelectedValue, out id);

            return id;
        }


        protected void ddlOrder_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            BindLines(SelectedOrderId());
        }


        // ============================================================
        // LINE EDITOR
        // ============================================================

        private static DataTable NewLineTable()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("ProductVariantId", typeof(int));
            dt.Columns.Add("VariantLabel", typeof(string));
            dt.Columns.Add("Dispatched", typeof(int));
            dt.Columns.Add("Returned", typeof(int));
            dt.Columns.Add("Condition", typeof(string));

            return dt;
        }


        private void BindLines(int orderId)
        {
            DataTable dt = NewLineTable();

            if (orderId > 0)
            {
                string query = @"
                    SELECT
                        d.ProductVariantId,
                        VariantLabel =
                            p.ProductName + ' - ' + v.VariantName,
                        Dispatched =
                            ISNULL(SUM(d.QuantityPackets), 0)
                    FROM OrderDetails d
                    INNER JOIN ProductVariants v
                        ON v.ProductVariantId =
                           d.ProductVariantId
                    INNER JOIN Products p
                        ON p.ProductId = v.ProductId
                    WHERE d.OrderId = @OrderId
                    GROUP BY
                        d.ProductVariantId,
                        p.ProductName,
                        v.VariantName
                    ORDER BY p.ProductName, v.VariantName";

                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@OrderId",
                            SqlDbType.Int).Value = orderId;

                        using (SqlDataAdapter da =
                            new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }

                foreach (DataRow row in dt.Rows)
                {
                    row["Returned"] = 0;
                    row["Condition"] = "Good";
                }
            }

            BindLineTable(dt, orderId > 0);
        }


        // A recorded return is shown read-only: the quantities and
        // conditions come from SalesReturnDetails.
        private void BindViewLines(int salesReturnId, int orderId)
        {
            DataTable dt = NewLineTable();

            string query = @"
                SELECT
                    d.ProductVariantId,
                    VariantLabel =
                        p.ProductName + ' - ' + v.VariantName,
                    Dispatched =
                        ISNULL(
                            (
                                SELECT SUM(od.QuantityPackets)
                                FROM OrderDetails od
                                WHERE od.OrderId = @OrderId
                                  AND od.ProductVariantId =
                                      d.ProductVariantId
                            ),
                            0),
                    Returned = d.QuantityPackets,
                    d.Condition
                FROM SalesReturnDetails d
                INNER JOIN ProductVariants v
                    ON v.ProductVariantId = d.ProductVariantId
                INNER JOIN Products p
                    ON p.ProductId = v.ProductId
                WHERE d.SalesReturnId = @SalesReturnId
                ORDER BY p.ProductName, v.VariantName";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@SalesReturnId",
                        SqlDbType.Int).Value = salesReturnId;

                    cmd.Parameters.Add(
                        "@OrderId",
                        SqlDbType.Int).Value = orderId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            BindLineTable(dt, true);
        }


        private void BindLineTable(DataTable dt, bool hasOrder)
        {
            rpLines.DataSource = dt;
            rpLines.DataBind();

            bool hasLines = dt.Rows.Count > 0;

            pnlLines.Visible = hasLines;
            pnlNoLines.Visible = !hasLines;

            if (!hasLines)
            {
                lblNoLines.Text = hasOrder
                    ? "This order has no lines to return."
                    : "Select a dispatched order above to list the goods available for return.";
            }
        }


        protected void rpLines_ItemDataBound(
            object sender,
            RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item &&
                e.Item.ItemType != ListItemType.AlternatingItem)
            {
                return;
            }

            DropDownList ddlCondition =
                (DropDownList)e.Item.FindControl("ddlCondition");

            if (ddlCondition != null)
            {
                string condition = Convert.ToString(
                    DataBinder.Eval(
                        e.Item.DataItem,
                        "Condition"));

                if (condition == "Damaged" &&
                    ddlCondition.Items.FindByValue(
                        "Damaged") != null)
                {
                    ddlCondition.SelectedValue = "Damaged";
                }
            }

            // A recorded return can never be edited again.
            if (ReturnId > 0)
            {
                TextBox txtQty =
                    (TextBox)e.Item.FindControl("txtQty");

                if (txtQty != null)
                {
                    txtQty.ReadOnly = true;
                }

                if (ddlCondition != null)
                {
                    ddlCondition.Enabled = false;
                }
            }
        }


        // ============================================================
        // VIEW MODE
        // ============================================================

        private bool LoadReturn()
        {
            string query = @"
                SELECT
                    r.ReturnNumber,
                    r.OrderId,
                    r.ReturnDate,
                    r.Reason,
                    r.Status,
                    OrderLabel =
                        ISNULL(o.OrderNumber, '-')
                        + ISNULL(' - ' + s.ShopName, '')
                        + ISNULL(
                              ' - '
                              + CONVERT(
                                    NVARCHAR(10),
                                    o.OrderDate,
                                    120),
                              '')
                FROM SalesReturns r
                LEFT JOIN Orders o
                    ON o.OrderId = r.OrderId
                LEFT JOIN Shops s
                    ON s.ShopId = r.ShopId
                WHERE r.SalesReturnId = @SalesReturnId
                  AND r.DealerId = @DealerId";

            string returnNumber = null;
            string status = null;
            string reason = null;
            string orderLabel = null;
            int orderId = 0;
            DateTime returnDate = DateTime.MinValue;

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@SalesReturnId",
                        SqlDbType.Int).Value = ReturnId;

                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return false;
                        }

                        returnNumber =
                            reader["ReturnNumber"].ToString();

                        orderId = reader["OrderId"] ==
                                  DBNull.Value
                            ? 0
                            : Convert.ToInt32(
                                reader["OrderId"]);

                        returnDate =
                            Convert.ToDateTime(
                                reader["ReturnDate"]);

                        reason =
                            reader["Reason"].ToString();

                        status =
                            reader["Status"].ToString();

                        orderLabel =
                            reader["OrderLabel"].ToString();
                    }
                }
            }

            lblPageTitle.Text = "View Return";
            lblHeading.Text = "View Return";
            lblSubHeading.Text =
                "Returns are read-only once recorded - the stock and ledger entries were written with them.";

            lblReturnNo.Text = returnNumber;

            lblViewInfo.Text =
                "Viewing return <strong>" +
                returnNumber +
                "</strong> &middot; " + status +
                " &middot; order " + orderLabel;

            lblViewInfo.Visible = true;

            txtReturnDate.Text =
                returnDate.ToString("yyyy-MM-dd");

            txtReason.Text = reason;

            ddlOrder.Visible = false;
            ddlOrder.AutoPostBack = false;
            lblOrderView.Text = orderLabel;
            lblOrderView.Visible = true;

            txtReturnDate.ReadOnly = true;
            txtReason.ReadOnly = true;

            rfvOrder.Enabled = false;
            rfvReturnDate.Enabled = false;
            revReturnDate.Enabled = false;
            rfvReason.Enabled = false;
            cvLines.Enabled = false;

            btnSave.Visible = false;
            lnkCancel.InnerText = "Back to Returns";

            BindViewLines(ReturnId, orderId);

            return true;
        }


        // ============================================================
        // SAVE
        //
        // One return is written in a single transaction:
        // header + details + stock movement + ledger entries.
        // ============================================================

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                ShowMessage(
                    ValidationMessage(),
                    false);

                return;
            }

            int orderId = SelectedOrderId();

            if (orderId <= 0)
            {
                ShowMessage(
                    "Select a dispatched order.",
                    false);

                return;
            }

            DateTime returnDate;

            if (!TryDate(txtReturnDate.Text, out returnDate))
            {
                ShowMessage(
                    "Return date must be a valid date (YYYY-MM-DD).",
                    false);

                return;
            }

            string reason = txtReason.Text.Trim();

            if (reason.Length == 0)
            {
                ShowMessage(
                    "Reason is required.",
                    false);

                return;
            }

            string lineError;

            List<ReturnLine> lines = ReadLines(out lineError);

            if (lines == null)
            {
                ShowMessage(lineError, false);

                return;
            }


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
                            // (a) The order must belong to this
                            // dealer and be Dispatched. Only
                            // dispatched goods can come back.
                            //
                            // Orders.Status is deliberately left
                            // untouched: a return is an additive
                            // record against the order, the order
                            // itself stays Dispatched.
                            // ------------------------------------

                            string orderNumber;
                            int shopId;
                            Dictionary<int, int> dispatched;

                            ReadDispatchedOrder(
                                con,
                                tx,
                                orderId,
                                out orderNumber,
                                out shopId,
                                out dispatched);

                            // ------------------------------------
                            // (b) Every quantity must be >= 0 and
                            // <= what the order dispatched, and at
                            // least one line must carry goods.
                            // ------------------------------------

                            ValidateLines(lines, dispatched);

                            // ------------------------------------
                            // (c) Header + detail rows
                            // ------------------------------------

                            string returnNumber =
                                NextReturnNumber(con, tx);

                            int salesReturnId =
                                InsertReturn(
                                    con,
                                    tx,
                                    returnNumber,
                                    orderId,
                                    shopId,
                                    returnDate,
                                    reason);

                            int unitId = PacketUnitId(con, tx);

                            if (unitId <= 0)
                            {
                                throw new InvalidOperationException(
                                    "The packet selling unit (PKT) is missing.");
                            }

                            foreach (ReturnLine line in lines)
                            {
                                if (line.Quantity == 0)
                                {
                                    // A line with zero packets is
                                    // not part of the return.
                                    continue;
                                }

                                InsertDetail(
                                    con,
                                    tx,
                                    salesReturnId,
                                    unitId,
                                    line);
                            }

                            // ------------------------------------
                            // (d) + (e) Stock movement and ledger
                            // ------------------------------------

                            ApplyStockAndLedger(
                                con,
                                tx,
                                returnNumber,
                                orderNumber,
                                unitId,
                                lines);

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
                    "~/Dealer/Returns.aspx?saved=1");
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error saving return: " + ex.Message,
                    false);
            }
        }


        // ============================================================
        // READING THE EDITOR
        // ============================================================

        // The line editor inside a Repeater is rebuilt on every postback, so a
        // freshly created TextBox can still be showing its bind-time
        // default when the handler runs. Request.Form always holds
        // exactly what the user submitted, so it wins when present.
        private string PostedValue(TextBox box)
        {
            if (box == null)
            {
                return "";
            }

            if (!string.IsNullOrEmpty(box.UniqueID))
            {
                string[] posted =
                    Request.Form.GetValues(box.UniqueID);

                if (posted != null && posted.Length > 0)
                {
                    return posted[0];
                }
            }

            return box.Text;
        }

        private string PostedValue(DropDownList list)
        {
            if (list == null)
            {
                return "";
            }

            if (!string.IsNullOrEmpty(list.UniqueID))
            {
                string[] posted =
                    Request.Form.GetValues(list.UniqueID);

                if (posted != null && posted.Length > 0)
                {
                    return posted[0];
                }
            }

            return list.SelectedValue;
        }

        private string PostedValue(HiddenField hidden)
        {
            if (hidden == null)
            {
                return "";
            }

            if (!string.IsNullOrEmpty(hidden.UniqueID))
            {
                string[] posted =
                    Request.Form.GetValues(hidden.UniqueID);

                if (posted != null && posted.Length > 0)
                {
                    return posted[0];
                }
            }

            return hidden.Value;
        }

        private List<ReturnLine> ReadLines(out string error)
        {
            error = null;

            List<ReturnLine> lines = new List<ReturnLine>();

            foreach (RepeaterItem item in rpLines.Items)
            {
                if (item.ItemType != ListItemType.Item &&
                    item.ItemType != ListItemType.AlternatingItem)
                {
                    continue;
                }

                // Row identity comes from the hidden fields posted with the form.
                // RepeaterItem.DataItem is always null in the save
                // handler because ASP.NET rebuilds the Repeater from
                // ViewState after Page_Load has re-bound it.
                HiddenField hidId =
                    (HiddenField)item.FindControl(
                        "hidVariantId");

                HiddenField hidLabel =
                    (HiddenField)item.FindControl(
                        "hidVariantLabel");

                string idText = PostedValue(hidId);

                int variantId;

                if (!int.TryParse(
                    idText,
                    out variantId))
                {
                    error =
                        "A receipt line is missing its product variant. " +
                        "Reload the page and try again.";

                    return null;
                }

                string label = PostedValue(hidLabel);

                TextBox txtQty =
                    (TextBox)item.FindControl("txtQty");

                DropDownList ddlCondition =
                    (DropDownList)item.FindControl(
                        "ddlCondition");

                string text = PostedValue(txtQty).Trim();

                int quantity;

                if (text.Length == 0)
                {
                    quantity = 0;
                }
                else if (!int.TryParse(text, out quantity))
                {
                    error =
                        "Returned quantity must be a whole number of packets.";

                    return null;
                }

                if (quantity < 0)
                {
                    error =
                        "Returned quantity cannot be negative.";

                    return null;
                }

                string condition =
                    ddlCondition != null &&
                    PostedValue(ddlCondition) == "Damaged"
                        ? "Damaged"
                        : "Good";

                lines.Add(
                    new ReturnLine(
                        variantId,
                        label,
                        quantity,
                        condition));
            }

            return lines;
        }


        // ============================================================
        // SERVER-SIDE LINE VALIDATION
        //
        // Wired to cvLines via OnServerValidate. The quantities live
        // inside a Repeater, so they cannot be validated by a normal
        // validation control - this reuses the exact parsing the save
        // path uses, so the user sees the same message either way.
        // ============================================================

        protected void cvLines_ServerValidate(
            object source,
            ServerValidateEventArgs args)
        {
            string error;

            List<ReturnLine> lines = ReadLines(out error);

            if (lines == null)
            {
                cvLines.ErrorMessage = error;
                args.IsValid = false;

                return;
            }

            bool any = false;

            foreach (ReturnLine line in lines)
            {
                if (line.Quantity > 0)
                {
                    any = true;
                    break;
                }
            }

            if (!any)
            {
                cvLines.ErrorMessage =
                    "Enter the returned quantity for at least one line.";

                args.IsValid = false;

                return;
            }

            args.IsValid = true;
        }


        // Turns a bare Page.IsValid failure into something actionable.
        // The validation controls render their text with display:none
        // until they actually fail, so returning quietly here would leave
        // the user looking at an unchanged form.
        private string ValidationMessage()
        {
            System.Text.StringBuilder sb =
                new System.Text.StringBuilder();

            foreach (BaseValidator v in Page.Validators)
            {
                if (v.IsValid)
                {
                    continue;
                }

                string text = v.ErrorMessage;

                if (string.IsNullOrWhiteSpace(text))
                {
                    text = v.GetType().Name +
                           " rejected a value on " +
                           v.ControlToValidate;
                }

                if (sb.Length > 0)
                {
                    sb.Append(" ");
                }

                sb.Append(text);
            }

            if (sb.Length == 0)
            {
                return "Please correct the highlighted fields and save again.";
            }

            return sb.ToString();
        }


        private static bool TryDate(
            string text,
            out DateTime value)
        {
            return DateTime.TryParse(
                text.Trim(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out value);
        }


        // ============================================================
        // TRANSACTION WORK
        // ============================================================

        private void ReadDispatchedOrder(
            SqlConnection con,
            SqlTransaction tx,
            int orderId,
            out string orderNumber,
            out int shopId,
            out Dictionary<int, int> dispatched)
        {
            orderNumber = null;
            shopId = 0;
            dispatched = new Dictionary<int, int>();

            string header = @"
                SELECT OrderNumber, ShopId, Status
                FROM Orders
                WHERE OrderId = @OrderId
                  AND DealerId = @DealerId";

            using (SqlCommand cmd =
                new SqlCommand(header, con, tx))
            {
                cmd.Parameters.Add(
                    "@OrderId",
                    SqlDbType.Int).Value = orderId;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                using (SqlDataReader reader =
                    cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        // Not this dealer's order (or no such
                        // order) - same rejection as a pending one.
                        throw new InvalidOperationException(
                            "Only dispatched orders can have returns recorded.");
                    }

                    orderNumber =
                        reader["OrderNumber"].ToString();

                    shopId = reader["ShopId"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(reader["ShopId"]);

                    string status =
                        reader["Status"].ToString();

                    if (!status.Equals(
                        "Dispatched",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            "Only dispatched orders can have returns recorded.");
                    }
                }
            }

            string details = @"
                SELECT
                    ProductVariantId,
                    ISNULL(SUM(QuantityPackets), 0)
                FROM OrderDetails
                WHERE OrderId = @OrderId
                GROUP BY ProductVariantId";

            using (SqlCommand cmd =
                new SqlCommand(details, con, tx))
            {
                cmd.Parameters.Add(
                    "@OrderId",
                    SqlDbType.Int).Value = orderId;

                using (SqlDataReader reader =
                    cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dispatched[
                            Convert.ToInt32(reader[0])] =
                            Convert.ToInt32(reader[1]);
                    }
                }
            }
        }


        private void ValidateLines(
            List<ReturnLine> lines,
            Dictionary<int, int> dispatched)
        {
            bool any = false;

            foreach (ReturnLine line in lines)
            {
                if (line.Quantity < 0)
                {
                    throw new InvalidOperationException(
                        "Returned quantity cannot be negative.");
                }

                if (line.Quantity == 0)
                {
                    continue;
                }

                any = true;

                int left;

                if (!dispatched.TryGetValue(
                    line.VariantId,
                    out left))
                {
                    throw new InvalidOperationException(
                        "The order has no line for " +
                        line.Label + ".");
                }

                if (line.Quantity > left)
                {
                    throw new InvalidOperationException(
                        "Returned quantity for " +
                        line.Label +
                        " cannot exceed the dispatched quantity (" +
                        left + " packets).");
                }
            }

            if (!any)
            {
                throw new InvalidOperationException(
                    "Enter the returned quantity for at least one line.");
            }
        }


        private int InsertReturn(
            SqlConnection con,
            SqlTransaction tx,
            string returnNumber,
            int orderId,
            int shopId,
            DateTime returnDate,
            string reason)
        {
            string query = @"
                INSERT INTO SalesReturns
                (
                    DealerId,
                    ReturnNumber,
                    OrderId,
                    ShopId,
                    ReturnDate,
                    Reason,
                    Status,
                    CreatedBy,
                    CreatedAt
                )
                VALUES
                (
                    @DealerId,
                    @ReturnNumber,
                    @OrderId,
                    @ShopId,
                    @ReturnDate,
                    @Reason,
                    @Status,
                    @CreatedBy,
                    GETDATE()
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@ReturnNumber",
                    SqlDbType.NVarChar,
                    50).Value = returnNumber;

                cmd.Parameters.Add(
                    "@OrderId",
                    SqlDbType.Int).Value = orderId;

                cmd.Parameters.Add(
                    "@ShopId",
                    SqlDbType.Int).Value =
                        shopId > 0
                            ? (object)shopId
                            : DBNull.Value;

                cmd.Parameters.Add(
                    "@ReturnDate",
                    SqlDbType.Date).Value = returnDate;

                cmd.Parameters.Add(
                    "@Reason",
                    SqlDbType.NVarChar,
                    500).Value = reason;

                cmd.Parameters.Add(
                    "@Status",
                    SqlDbType.NVarChar,
                    30).Value = "Posted";

                cmd.Parameters.Add(
                    "@CreatedBy",
                    SqlDbType.Int).Value = DealerId;

                return Convert.ToInt32(
                    cmd.ExecuteScalar());
            }
        }


        private void InsertDetail(
            SqlConnection con,
            SqlTransaction tx,
            int salesReturnId,
            int unitId,
            ReturnLine line)
        {
            string query = @"
                INSERT INTO SalesReturnDetails
                (
                    SalesReturnId,
                    ProductVariantId,
                    UnitId,
                    Quantity,
                    QuantityPackets,
                    Condition
                )
                VALUES
                (
                    @SalesReturnId,
                    @ProductVariantId,
                    @UnitId,
                    @Quantity,
                    @QuantityPackets,
                    @Condition
                )";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@SalesReturnId",
                    SqlDbType.Int).Value = salesReturnId;

                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value =
                        line.VariantId;

                cmd.Parameters.Add(
                    "@UnitId",
                    SqlDbType.Int).Value = unitId;

                // Stock is counted in base packets, so the line is
                // stored in packets against the PKT unit.
                cmd.Parameters.Add(
                    "@Quantity",
                    SqlDbType.Int).Value = line.Quantity;

                cmd.Parameters.Add(
                    "@QuantityPackets",
                    SqlDbType.Int).Value =
                        line.Quantity;

                cmd.Parameters.Add(
                    "@Condition",
                    SqlDbType.NVarChar,
                    30).Value = line.Condition;

                cmd.ExecuteNonQuery();
            }
        }


        // ============================================================
        // STOCK IMPACT
        //
        // Good packets go back into the godown (Inventory grows).
        // Damaged packets are not sellable stock, so Inventory is
        // left alone - the loss is recorded in the ledger only.
        // ============================================================

        private void ApplyStockAndLedger(
            SqlConnection con,
            SqlTransaction tx,
            string returnNumber,
            string orderNumber,
            int unitId,
            List<ReturnLine> lines)
        {
            List<ReturnLine> good =
                new List<ReturnLine>();

            List<ReturnLine> damaged =
                new List<ReturnLine>();

            foreach (ReturnLine line in lines)
            {
                if (line.Quantity == 0)
                {
                    continue;
                }

                if (line.Condition == "Damaged")
                {
                    damaged.Add(line);
                }
                else
                {
                    good.Add(line);

                    AddGoodPackets(
                        con,
                        tx,
                        line.VariantId,
                        line.Quantity);
                }
            }

            if (good.Count > 0)
            {
                WriteLedger(
                    con,
                    tx,
                    "Stock In",
                    returnNumber,
                    orderNumber,
                    good,
                    unitId,
                    1,
                    "Sales return " + returnNumber +
                    " against order " + orderNumber +
                    " - goods received back in good condition");
            }

            if (damaged.Count > 0)
            {
                WriteLedger(
                    con,
                    tx,
                    "Damage",
                    returnNumber,
                    orderNumber,
                    damaged,
                    unitId,
                    -1,
                    "Damaged goods in sales return " +
                    returnNumber +
                    " against order " + orderNumber +
                    " - returned packets in damaged condition, not sellable stock");
            }
        }


        private void AddGoodPackets(
            SqlConnection con,
            SqlTransaction tx,
            int variantId,
            int packets)
        {
            // Lock the row first so two returns recorded at the
            // same moment cannot both read the old level.
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
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                cmd.ExecuteScalar();
            }

            if (RunAddPackets(
                con,
                tx,
                variantId,
                packets))
            {
                return;
            }

            // No Inventory row for this variant yet - the unique
            // key is (DealerId, ProductVariantId), so insert it.
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

            try
            {
                using (SqlCommand cmd =
                    new SqlCommand(insert, con, tx))
                {
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    cmd.Parameters.Add(
                        "@ProductVariantId",
                        SqlDbType.Int).Value = variantId;

                    cmd.Parameters.Add(
                        "@QuantityPackets",
                        SqlDbType.Int).Value = packets;

                    cmd.Parameters.Add(
                        "@UpdatedBy",
                        SqlDbType.Int).Value = DealerId;

                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number != 2627 && ex.Number != 2601)
                {
                    throw;
                }

                // Someone created the row between our lock read
                // and this insert - retry the update once.
                RunAddPackets(
                    con,
                    tx,
                    variantId,
                    packets);
            }
        }


        private bool RunAddPackets(
            SqlConnection con,
            SqlTransaction tx,
            int variantId,
            int packets)
        {
            string update = @"
                UPDATE Inventory
                SET QuantityPackets =
                        QuantityPackets + @QuantityPackets,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE DealerId = @DealerId
                  AND ProductVariantId = @ProductVariantId";

            using (SqlCommand cmd =
                new SqlCommand(update, con, tx))
            {
                cmd.Parameters.Add(
                    "@QuantityPackets",
                    SqlDbType.Int).Value = packets;

                cmd.Parameters.Add(
                    "@UpdatedBy",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                return cmd.ExecuteNonQuery() > 0;
            }
        }


        // The ledger stores the signed stock movement: Stock In
        // rows are positive (packets entering the godown) and the
        // damage rows are negative (a loss), exactly like the
        // existing "Damaged packets removed" entry. Damaged
        // quantities are passed with sign -1 by the caller.
        private void WriteLedger(
            SqlConnection con,
            SqlTransaction tx,
            string transactionType,
            string returnNumber,
            string orderNumber,
            List<ReturnLine> lines,
            int unitId,
            int sign,
            string remarks)
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
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@TransactionType",
                    SqlDbType.NVarChar,
                    80).Value = transactionType;

                cmd.Parameters.Add(
                    "@ReferenceNo",
                    SqlDbType.NVarChar,
                    200).Value = returnNumber;

                cmd.Parameters.Add(
                    "@Remarks",
                    SqlDbType.NVarChar,
                    1000).Value = remarks;

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

            foreach (ReturnLine line in lines)
            {
                using (SqlCommand cmd =
                    new SqlCommand(detail, con, tx))
                {
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

                    // Signed movement, never an absolute level.
                    cmd.Parameters.Add(
                        "@Quantity",
                        SqlDbType.Int).Value =
                            sign * line.Quantity;

                    cmd.Parameters.Add(
                        "@QuantityPackets",
                        SqlDbType.Int).Value =
                            sign * line.Quantity;

                    cmd.ExecuteNonQuery();
                }
            }
        }


        // ============================================================
        // HELPERS
        // ============================================================

        // RET-yyyymmdd-HHmmss, unique per dealer.
        private string NextReturnNumber(
            SqlConnection con,
            SqlTransaction tx)
        {
            string prefix =
                "RET-" +
                DateTime.Now.ToString("yyyyMMdd-HHmmss");

            string candidate = prefix;
            int suffix = 2;

            while (ReturnNumberExists(con, tx, candidate))
            {
                candidate = prefix + "-" + suffix++;
            }

            return candidate;
        }


        private bool ReturnNumberExists(
            SqlConnection con,
            SqlTransaction tx,
            string number)
        {
            string query = @"
                SELECT COUNT(*)
                FROM SalesReturns
                WHERE DealerId = @DealerId
                  AND ReturnNumber = @ReturnNumber";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@ReturnNumber",
                    SqlDbType.NVarChar,
                    50).Value = number;

                return Convert.ToInt32(
                    cmd.ExecuteScalar()) > 0;
            }
        }


        // Stock is counted in base packets, so every detail row of
        // this module is tagged with the Packet selling unit.
        private int PacketUnitId(
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
                    ? 0
                    : Convert.ToInt32(result);
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


        // ============================================================
        // MODEL
        // ============================================================

        private sealed class ReturnLine
        {
            public ReturnLine(
                int variantId,
                string label,
                int quantity,
                string condition)
            {
                VariantId = variantId;
                Label = label;
                Quantity = quantity;
                Condition = condition;
            }

            public int VariantId { get; private set; }
            public string Label { get; private set; }
            public int Quantity { get; private set; }
            public string Condition { get; private set; }
        }
    }
}
