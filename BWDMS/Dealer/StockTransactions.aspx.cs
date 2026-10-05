using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class StockTransactions : System.Web.UI.Page
    {
        private int DealerId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }


        // Header rows keyed so the nested line repeater can find
        // its own transaction id.
        private DataTable _headers;


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
                LoadTransactions();

                if (Request.QueryString["saved"] == "1")
                {
                    ShowMessage(
                        "Stock saved successfully.",
                        true);
                }
            }
        }


        private void LoadTransactions()
        {
            string search = txtSearch.Text.Trim();
            string type = ddlType.SelectedValue;

            string query = @"
                SELECT
                    st.StockTransactionId,
                    st.TransactionType,
                    st.ReferenceNo,
                    st.Remarks,
                    TransactionDateText =
                        CONVERT(NVARCHAR(16), st.TransactionDate, 120)
                FROM StockTransactions st
                WHERE st.DealerId = @DealerId
                  AND
                  (
                    ISNULL(st.ReferenceNo, '') LIKE @Search
                    OR ISNULL(st.Remarks, '') LIKE @Search
                  )
                  AND (@Type = '' OR st.TransactionType = @Type)
                ORDER BY st.TransactionDate DESC,
                         st.StockTransactionId DESC";

            _headers = new DataTable();

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
                        "@Type",
                        type);

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(_headers);
                    }
                }
            }

            if (_headers.Rows.Count == 0)
            {
                pnlEmpty.Visible = true;
                repTransactions.Visible = false;
                return;
            }

            pnlEmpty.Visible = false;
            repTransactions.Visible = true;

            // Type badge colour
            if (!_headers.Columns.Contains("TypeCss"))
            {
                _headers.Columns.Add(
                    "TypeCss",
                    typeof(string));
            }

            foreach (DataRow row in _headers.Rows)
            {
                row["TypeCss"] =
                    row["TransactionType"].ToString()
                        .Equals(
                            "Stock Out",
                            StringComparison.OrdinalIgnoreCase)
                        ? "badge bg-danger"
                        : "badge bg-success";
            }

            repTransactions.DataSource = _headers;
            repTransactions.DataBind();
        }


        protected void repTransactions_ItemDataBound(
            object sender,
            RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item &&
                e.Item.ItemType != ListItemType.AlternatingItem)
            {
                return;
            }

            if (_headers == null)
            {
                return;
            }

            DataRowView view =
                e.Item.DataItem as DataRowView;

            if (view == null)
            {
                return;
            }

            int transactionId =
                Convert.ToInt32(
                    view["StockTransactionId"]);

            var lines =
                (Repeater)e.Item.FindControl("repLines");

            if (lines == null)
            {
                return;
            }

            string query = @"
                SELECT
                    d.QuantityPackets,
                    p.ProductName,
                    v.VariantName
                FROM StockTransactionDetails d
                INNER JOIN ProductVariants v
                    ON v.ProductVariantId = d.ProductVariantId
                INNER JOIN Products p
                    ON p.ProductId = v.ProductId
                WHERE d.StockTransactionId = @StockTransactionId
                ORDER BY d.StockTransactionDetailId";

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@StockTransactionId",
                        SqlDbType.Int).Value = transactionId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            lines.DataSource = dt;
            lines.DataBind();
        }


        protected void txtSearch_TextChanged(
            object sender,
            EventArgs e)
        {
            LoadTransactions();
        }


        protected void FilterChanged(
            object sender,
            EventArgs e)
        {
            LoadTransactions();
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
