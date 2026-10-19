using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Admin
{
    public partial class Orders : System.Web.UI.Page
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
                LoadOrders();
            }
        }


        private void LoadOrders()
        {
            string search = txtSearch.Text.Trim();
            string status = ddlStatus.SelectedValue;

            string query = @"
                SELECT
                    o.OrderNumber,
                    DealerName = ISNULL(d.FullName, '-'),
                    s.ShopName,
                    OrderDateText =
                        CONVERT(NVARCHAR(10), o.OrderDate, 120),
                    o.Status,
                    ItemCount =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM OrderDetails od
                             WHERE od.OrderId = o.OrderId),
                            0),
                    o.GrandTotal
                FROM Orders o
                INNER JOIN Shops s
                    ON s.ShopId = o.ShopId
                LEFT JOIN Users d
                    ON d.UserId = o.DealerId
                WHERE
                (
                    ISNULL(o.OrderNumber, '') LIKE @Search
                    OR ISNULL(s.ShopName, '') LIKE @Search
                    OR ISNULL(d.FullName, '') LIKE @Search
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
            }

            foreach (DataRow row in dt.Rows)
            {
                row["StatusCss"] = StatusCss(
                    row["Status"].ToString());

                row["GrandTotalText"] =
                    Convert.ToDecimal(row["GrandTotal"])
                        .ToString("N2");
            }

            gvOrders.DataSource = dt;
            gvOrders.DataBind();

            UpdateCount(dt.Rows.Count);

            litPager.Text = GridPager.Html(gvOrders);
        }


        // "Showing 5-8 of 8" while the pager itself only knows the
        // page numbers.
        private void UpdateCount(int total)
        {
            int size = gvOrders.PageSize;

            if (size < 1)
            {
                size = 10;
            }

            int pages = total == 0
                ? 1
                : (int)Math.Ceiling((double)total / size);

            int pageIndex = gvOrders.PageIndex;

            if (pageIndex > pages - 1)
            {
                pageIndex = pages - 1;
            }

            if (pageIndex < 0)
            {
                pageIndex = 0;
            }

            gvOrders.PageIndex = pageIndex;

            if (total == 0)
            {
                lblCount.Text = "Newest first · No orders";

                return;
            }

            int first = pageIndex * size + 1;
            int last = Math.Min((pageIndex + 1) * size, total);

            lblCount.Text = string.Format(
                "Newest first · Showing {0}-{1} of {2}",
                first,
                last,
                total);
        }


        private static string StatusCss(string status)
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
            // A new search starts at the top of the results.
            gvOrders.PageIndex = 0;

            LoadOrders();
        }


        protected void FilterChanged(
            object sender,
            EventArgs e)
        {
            gvOrders.PageIndex = 0;

            LoadOrders();
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


        // The pager bar under the grid links back to the grid with a
        // "Page$N" argument. Event validation only accepts arguments that
        // were registered while rendering, which the grid's own (hidden)
        // pager would normally do, so register them here.
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            GridPager.RegisterForEventValidation(gvOrders);

            base.Render(writer);
        }

    }
}
