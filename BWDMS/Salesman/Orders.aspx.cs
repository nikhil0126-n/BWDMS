using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Salesman
{
    public partial class Orders : System.Web.UI.Page
    {
        private int SalesmanId
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
                    "Salesman",
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
                bool orderPlaced =
                    Request.QueryString["saved"] == "1" &&
                    ShowOrderSuccess();

                if (!orderPlaced)
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
        }


        // Renders the "Placed Order Successful!" card for the
        // order that was just saved. Returns false when the
        // order cannot be found, so the normal list shows.
        private bool ShowOrderSuccess()
        {
            int orderId;

            if (!int.TryParse(
                Request.QueryString["orderId"],
                out orderId))
            {
                return false;
            }

            string query = @"
                SELECT
                    o.OrderNumber,
                    OrderDateText =
                        CONVERT(NVARCHAR(10), o.OrderDate, 106),
                    o.GrandTotal,
                    s.ShopName
                FROM Orders o
                INNER JOIN Shops s
                    ON s.ShopId = o.ShopId
                WHERE o.OrderId = @OrderId
                  AND o.SalesmanId = @SalesmanId";

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@OrderId",
                        SqlDbType.Int).Value = orderId;

                    cmd.Parameters.Add(
                        "@SalesmanId",
                        SqlDbType.Int).Value = SalesmanId;

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

            lblShopName.Text =
                row["ShopName"].ToString();

            lblPlacedOn.Text =
                row["OrderDateText"].ToString();

            lblTotalAmount.Text =
                Convert.ToDecimal(
                    row["GrandTotal"]).ToString("N2");

            lblOrderNumber.Text =
                row["OrderNumber"].ToString();

            lblPaymentMethod.Text = "Cash";

            lblCollatedBy.Text =
                Session["FullName"] == null ||
                string.IsNullOrWhiteSpace(
                    Session["FullName"].ToString())
                    ? "Salesman"
                    : Session["FullName"].ToString();

            lnkViewOrder.HRef =
                ResolveUrl(
                    "~/Salesman/Bill.aspx?orderId=" + orderId);

            pnlSuccess.Visible = true;
            pnlList.Visible = false;

            return true;
        }


        private void LoadOrders()
        {
            string search = txtSearch.Text.Trim();
            string status = ddlStatus.SelectedValue;

            string query = @"
                SELECT
                    o.OrderId,
                    o.OrderNumber,
                    OrderDateText =
                        CONVERT(NVARCHAR(10), o.OrderDate, 106),
                    DeliveryDateText =
                        CASE
                            WHEN o.DeliveryDate IS NULL THEN '-'
                            ELSE CONVERT(NVARCHAR(10), o.DeliveryDate, 120)
                        END,
                    s.ShopName,
                    o.Status,
                    ItemCount =
                        ISNULL(
                            (
                                SELECT COUNT(*)
                                FROM OrderDetails d
                                WHERE d.OrderId = o.OrderId
                            ),
                            0),
                    o.GrandTotal
                FROM Orders o
                INNER JOIN Shops s
                    ON s.ShopId = o.ShopId
                WHERE o.SalesmanId = @SalesmanId
                  AND
                  (
                    o.OrderNumber LIKE @Search
                    OR s.ShopName LIKE @Search
                  )
                  AND (@Status = '' OR o.Status = @Status)
                ORDER BY o.OrderDate DESC, o.OrderId DESC";

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@SalesmanId",
                        SqlDbType.Int).Value = SalesmanId;

                    cmd.Parameters.AddWithValue(
                        "@Search",
                        "%" + search + "%");

                    cmd.Parameters.AddWithValue(
                        "@Status",
                        status);

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
                dt.Columns.Add("AmountText", typeof(string));
                dt.Columns.Add("PaymentText", typeof(string));
            }

            foreach (DataRow row in dt.Rows)
            {
                row["StatusCss"] = StatusCss(
                    row["Status"].ToString());

                row["GrandTotalText"] =
                    Convert.ToDecimal(row["GrandTotal"])
                        .ToString("N2");

                row["AmountText"] =
                    AmountOf(row["GrandTotal"]);

                // Orders are settled in cash at delivery.
                row["PaymentText"] = "Cash";
            }

            lblRecordCount.Text =
                "records: " + dt.Rows.Count;

            gvOrders.DataSource = dt;
            gvOrders.DataBind();
        }


        // Plain coloured text (no pill) so the table matches
        // the mock-up.
        private static string StatusCss(string status)
        {
            switch (status.Trim().ToUpperInvariant())
            {
                case "DELIVERED":
                case "COMPLETE":
                case "COMPLETED":
                case "DISPATCHED":
                    return "or-status ok";

                case "CONFIRMED":
                case "APPROVED":
                case "PREPARED":
                    return "or-status info";

                case "CANCELLED":
                case "RETURNED":
                    return "or-status bad";

                default:
                    return "or-status warn";
            }
        }


        // 270 -> 270, 270.50 -> 270.50
        private static string AmountOf(object value)
        {
            decimal amount =
                value == null || value == DBNull.Value
                    ? 0
                    : Convert.ToDecimal(value);

            string text = amount % 1 == 0
                ? amount.ToString("0")
                : amount.ToString("N2");

            return "\u20B9" + text;
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

        // ------------------------------------------------------------
        // PAGING
        //
        // The new index has to be applied before the grid is rebound,
        // otherwise the postback lands back on the page it came from.
        // ------------------------------------------------------------

        protected void gvOrders_PageIndexChanging(
            object sender,
            System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            gvOrders.PageIndex = e.NewPageIndex;

            LoadOrders();
        }

    }
}
