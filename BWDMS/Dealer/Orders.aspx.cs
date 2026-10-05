using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class Orders : System.Web.UI.Page
    {
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

                if (Request.QueryString["saved"] == "1")
                {
                    ShowMessage(
                        "Order saved successfully.",
                        true);
                }
            }
        }


        // ============================================================
        // ORDER LIST
        // ============================================================

        private void LoadOrders()
        {
            string search = txtSearch.Text.Trim();
            string status = ddlStatus.SelectedValue;
            string source = ddlSource.SelectedValue;

            string query = @"
                SELECT
                    o.OrderId,
                    o.OrderNumber,
                    o.Status,
                    o.OrderSource,
                    OrderDateText =
                        CONVERT(NVARCHAR(10), o.OrderDate, 120),
                    DeliveryDateText =
                        CASE
                            WHEN o.DeliveryDate IS NULL THEN '-'
                            ELSE CONVERT(NVARCHAR(10), o.DeliveryDate, 120)
                        END,
                    s.ShopName,
                    ItemCount =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM OrderDetails d
                             WHERE d.OrderId = o.OrderId),
                            0),
                    o.GrandTotal
                FROM Orders o
                INNER JOIN Shops s
                    ON s.ShopId = o.ShopId
                WHERE o.DealerId = @DealerId
                  AND
                  (
                    o.OrderNumber LIKE @Search
                    OR s.ShopName LIKE @Search
                  )
                  AND (@Status = '' OR o.Status = @Status)
                  AND (@Source = '' OR ISNULL(o.OrderSource, '') = @Source)
                ORDER BY o.OrderDate DESC, o.OrderId DESC";

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    cmd.Parameters.AddWithValue(
                        "@Search",
                        "%" + search + "%");

                    cmd.Parameters.AddWithValue(
                        "@Status",
                        status);

                    cmd.Parameters.Add(
                        "@Source",
                        SqlDbType.NVarChar,
                        20).Value = source;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            if (!dt.Columns.Contains("StatusCss"))
            {
                dt.Columns.Add("StatusCss", typeof(string));
                dt.Columns.Add("GrandTotalText", typeof(string));
                dt.Columns.Add("SourceText", typeof(string));
            }

            foreach (DataRow row in dt.Rows)
            {
                row["StatusCss"] = StatusCss(
                    row["Status"].ToString());

                row["GrandTotalText"] =
                    Convert.ToDecimal(row["GrandTotal"])
                        .ToString("N2");

                string orderSource =
                    row["OrderSource"] == null ||
                    row["OrderSource"] == DBNull.Value
                        ? ""
                        : row["OrderSource"].ToString();

                row["SourceText"] =
                    string.IsNullOrEmpty(orderSource)
                        ? "-"
                        : orderSource;
            }

            gvOrders.DataSource = dt;
            gvOrders.DataBind();
        }


        internal static string StatusCss(string status)
        {
            switch (status)
            {
                case "Dispatched":
                    return "badge bg-success";

                case "Confirmed":
                    return "badge bg-primary";

                case "Cancelled":
                    return "badge bg-secondary";

                default:
                    return "badge bg-warning text-dark";
            }
        }


        protected void txtSearch_TextChanged(
            object sender,
            EventArgs e)
        {
            LoadOrders();
        }


        protected void FilterChanged(
            object sender,
            EventArgs e)
        {
            LoadOrders();
        }


        // ============================================================
        // STATUS TRANSITIONS
        //
        //   Pending    -> Confirmed, Cancelled
        //   Confirmed  -> Dispatched, Cancelled
        //   Dispatched -> terminal (only a Returns record may follow)
        //   Cancelled  -> terminal
        //
        // Same table as Dealer\AddOrder.aspx.cs, kept identical so
        // every status change in the order screens is judged by
        // one set of rules.
        // ============================================================

        private static bool IsTransitionAllowed(
            string from,
            string to)
        {
            string reason;

            return IsTransitionAllowed(from, to, out reason);
        }


        private static bool IsTransitionAllowed(
            string from,
            string to,
            out string reason)
        {
            reason = null;

            if (string.IsNullOrEmpty(from))
            {
                from = "Pending";
            }

            if (from == "Pending" &&
                (to == "Pending" ||
                 to == "Confirmed" ||
                 to == "Cancelled"))
            {
                return true;
            }

            if (from == "Confirmed" &&
                (to == "Confirmed" ||
                 to == "Dispatched" ||
                 to == "Cancelled"))
            {
                return true;
            }

            if (from == "Pending" && to == "Dispatched")
            {
                reason =
                    "A Pending order cannot go straight to " +
                    "Dispatched. Confirm it first.";

                return false;
            }

            if (from == "Dispatched" || from == "Cancelled")
            {
                reason =
                    "A " + from + " order is closed and cannot " +
                    "become " + to + ". " +
                    (from == "Dispatched"
                        ? "Record a return instead."
                        : "Create a new order instead.");

                return false;
            }

            reason =
                "A " + from + " order cannot become " + to + ".";

            return false;
        }


        // ============================================================
        // CANCEL
        //
        // Stock only leaves the godown when an order is Dispatched
        // (see AddOrder.ApplyStockChanges), so cancelling a
        // Pending or Confirmed order moves no stock at all.
        // ============================================================

        protected void gvOrders_RowCommand(
            object sender,
            GridViewCommandEventArgs e)
        {
            if (e.CommandName != "CancelOrder")
            {
                return;
            }

            int orderId;

            if (!int.TryParse(
                    e.CommandArgument == null
                        ? ""
                        : e.CommandArgument.ToString(),
                    out orderId) || orderId <= 0)
            {
                ShowMessage("Order not found.", false);

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
                            // UPDLOCK holds the row for the rest
                            // of the transaction, and DealerId is
                            // part of the predicate, so a forged
                            // id cannot reach another dealer's
                            // order and no dispatch can slip in
                            // between the read and the write.
                            string read = @"
                                SELECT Status
                                FROM Orders WITH (UPDLOCK, ROWLOCK)
                                WHERE OrderId = @OrderId
                                  AND DealerId = @DealerId";

                            string status = null;

                            using (SqlCommand cmd =
                                new SqlCommand(read, con, tx))
                            {
                                cmd.Parameters.Add(
                                    "@OrderId",
                                    SqlDbType.Int).Value =
                                    orderId;

                                cmd.Parameters.Add(
                                    "@DealerId",
                                    SqlDbType.Int).Value =
                                    DealerId;

                                object result =
                                    cmd.ExecuteScalar();

                                if (result != null &&
                                    result != DBNull.Value)
                                {
                                    status = result.ToString();
                                }
                            }

                            if (status == null)
                            {
                                throw new InvalidOperationException(
                                    "Order not found.");
                            }

                            if (status == "Dispatched")
                            {
                                throw new InvalidOperationException(
                                    "A dispatched order cannot be " +
                                    "cancelled. Record a return " +
                                    "instead.");
                            }

                            if (status == "Cancelled")
                            {
                                throw new InvalidOperationException(
                                    "This order is already " +
                                    "cancelled.");
                            }

                            string reason;

                            if (!IsTransitionAllowed(
                                    status,
                                    "Cancelled",
                                    out reason))
                            {
                                throw new InvalidOperationException(
                                    reason);
                            }

                            string update = @"
                                UPDATE Orders
                                SET Status = 'Cancelled',
                                    UpdatedBy = @UpdatedBy,
                                    UpdatedAt = GETDATE()
                                WHERE OrderId = @OrderId
                                  AND DealerId = @DealerId";

                            using (SqlCommand cmd =
                                new SqlCommand(update, con, tx))
                            {
                                cmd.Parameters.Add(
                                    "@OrderId",
                                    SqlDbType.Int).Value =
                                    orderId;

                                cmd.Parameters.Add(
                                    "@DealerId",
                                    SqlDbType.Int).Value =
                                    DealerId;

                                cmd.Parameters.Add(
                                    "@UpdatedBy",
                                    SqlDbType.Int).Value =
                                    DealerId;

                                cmd.ExecuteNonQuery();
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

                LoadOrders();

                ShowMessage("Order cancelled.", true);
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, false);
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
