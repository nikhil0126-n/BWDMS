using System;
using System.Data;
using System.Data.SqlClient;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class StockTransactions : System.Web.UI.Page
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
                LoadTransactions();

                if (Request.QueryString["saved"] == "1")
                {
                    ShowMessage(
                        "Stock saved successfully.",
                        true);
                }
            }
        }


        // One row per movement line so the grid matches the
        // reference design: date / type / ref / product / unit / qty.
        private void LoadTransactions()
        {
            string search = txtSearch.Text.Trim();

            string query = @"
                SELECT
                    st.TransactionDate,
                    st.TransactionType,
                    st.ReferenceNo,
                    p.ProductName,
                    su.UnitName,
                    SignedQty =
                        CASE WHEN st.TransactionType = 'Stock Out'
                             THEN -d.Quantity
                             ELSE d.Quantity
                        END
                FROM StockTransactions st
                INNER JOIN StockTransactionDetails d
                    ON d.StockTransactionId = st.StockTransactionId
                INNER JOIN ProductVariants v
                    ON v.ProductVariantId = d.ProductVariantId
                INNER JOIN Products p
                    ON p.ProductId = v.ProductId
                INNER JOIN SellingUnits su
                    ON su.UnitId = d.UnitId
                WHERE st.DealerId = @DealerId
                  AND
                  (
                    ISNULL(st.ReferenceNo, '') LIKE @Search
                    OR ISNULL(st.Remarks, '') LIKE @Search
                    OR p.ProductName LIKE @Search
                  )
                ORDER BY st.TransactionDate DESC,
                         st.StockTransactionId DESC,
                         d.StockTransactionDetailId";

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

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            gvTransactions.DataSource = dt;
            gvTransactions.DataBind();
        }


        protected void txtSearch_TextChanged(
            object sender,
            EventArgs e)
        {
            LoadTransactions();
        }


        protected void gvTransactions_PageIndexChanging(
            object sender,
            System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            gvTransactions.PageIndex = e.NewPageIndex;

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
