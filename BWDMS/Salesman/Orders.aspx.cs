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
                LoadOrders();

                if (Request.QueryString["saved"] == "1")
                {
                    ShowMessage(
                        "Order saved successfully.",
                        true);
                }
            }
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
                        CONVERT(NVARCHAR(10), o.OrderDate, 120),
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
            }

            foreach (DataRow row in dt.Rows)
            {
                row["StatusCss"] = StatusCss(
                    row["Status"].ToString());

                row["GrandTotalText"] =
                    Convert.ToDecimal(row["GrandTotal"])
                        .ToString("N2");
            }

            lblRecordCount.Text =
                "records: " + dt.Rows.Count;

            gvOrders.DataSource = dt;
            gvOrders.DataBind();
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
    }
}
