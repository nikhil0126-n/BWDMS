using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class Returns : System.Web.UI.Page
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
                LoadReturns();

                if (Request.QueryString["saved"] == "1")
                {
                    ShowMessage(
                        "Sales return recorded successfully.",
                        true);
                }
            }
        }


        // ============================================================
        // RETURN LIST
        // ============================================================

        private void LoadReturns()
        {
            string search = txtSearch.Text.Trim();
            string status = ddlStatus.SelectedValue;

            string query = @"
                SELECT
                    r.SalesReturnId,
                    r.ReturnNumber,
                    ReturnDateText =
                        CONVERT(NVARCHAR(10), r.ReturnDate, 120),
                    o.OrderNumber,
                    s.ShopName,
                    LineCount = ISNULL(x.LineCount, 0),
                    TotalPackets = ISNULL(x.TotalPackets, 0),
                    Conditions = ISNULL(x.Conditions, '-'),
                    r.Status,
                    r.Reason
                FROM SalesReturns r
                LEFT JOIN Orders o
                    ON o.OrderId = r.OrderId
                LEFT JOIN Shops s
                    ON s.ShopId = r.ShopId
                LEFT JOIN
                (
                    SELECT
                        d.SalesReturnId,
                        LineCount = COUNT(*),
                        TotalPackets = SUM(d.QuantityPackets),
                        Conditions =
                            ISNULL(
                                (
                                    SELECT STUFF(
                                        (
                                            SELECT ', ' + c.Condition
                                            FROM SalesReturnDetails c
                                            WHERE c.SalesReturnId =
                                                  d.SalesReturnId
                                            GROUP BY c.Condition
                                            FOR XML PATH('')
                                        ),
                                        1,
                                        2,
                                        '')
                                ),
                                '-')
                    FROM SalesReturnDetails d
                    GROUP BY d.SalesReturnId
                ) x
                    ON x.SalesReturnId = r.SalesReturnId
                WHERE r.DealerId = @DealerId
                  AND
                  (
                    r.ReturnNumber LIKE @Search
                    OR ISNULL(o.OrderNumber, '') LIKE @Search
                  )
                  AND (@Status = '' OR r.Status = @Status)
                ORDER BY r.ReturnDate DESC, r.SalesReturnId DESC";

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

                    cmd.Parameters.Add(
                        "@Search",
                        SqlDbType.NVarChar,
                        200).Value = "%" + search + "%";

                    cmd.Parameters.Add(
                        "@Status",
                        SqlDbType.NVarChar,
                        30).Value = status;

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
            }

            foreach (DataRow row in dt.Rows)
            {
                row["StatusCss"] = StatusCss(
                    row["Status"].ToString());
            }

            gvReturns.DataSource = dt;
            gvReturns.DataBind();
        }


        // The module works with exactly two statuses: Posted
        // (stock already moved) and Draft.
        internal static string StatusCss(string status)
        {
            switch (status)
            {
                case "Posted":
                    return "badge bg-success";

                case "Draft":
                    return "badge bg-warning text-dark";

                default:
                    return "badge bg-secondary";
            }
        }


        protected void txtSearch_TextChanged(
            object sender,
            EventArgs e)
        {
            LoadReturns();
        }


        protected void FilterChanged(
            object sender,
            EventArgs e)
        {
            LoadReturns();
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
