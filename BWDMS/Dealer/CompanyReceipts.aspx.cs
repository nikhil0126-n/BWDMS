using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class CompanyReceipts : System.Web.UI.Page
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
                LoadReceipts();

                if (Request.QueryString["posted"] == "1")
                {
                    ShowMessage(
                        "Receipt posted. Godown stock has been increased.",
                        true);
                }
                else if (Request.QueryString["saved"] == "1")
                {
                    ShowMessage(
                        "Receipt saved successfully.",
                        true);
                }
            }
        }


        // ============================================================
        // RECEIPT LIST
        // ============================================================

        private void LoadReceipts()
        {
            string search = txtSearch.Text.Trim();
            string status = ddlStatus.SelectedValue;

            string query = @"
                SELECT
                    r.ReceiptId,
                    r.ReceiptNumber,
                    ReceiptDateText =
                        CONVERT(NVARCHAR(10), r.ReceiptDate, 120),
                    InvoiceText =
                        CASE
                            WHEN ISNULL(r.CompanyInvoiceNo, '') = ''
                                THEN '-'
                            ELSE r.CompanyInvoiceNo
                        END,
                    NotesText =
                        CASE
                            WHEN r.Notes IS NULL THEN ''
                            WHEN LEN(r.Notes) > 60
                                THEN LEFT(r.Notes, 60) + '...'
                            ELSE r.Notes
                        END,
                    r.Status,
                    r.TotalCost,
                    LineCount =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM CompanyStockReceiptDetails d
                             WHERE d.ReceiptId = r.ReceiptId),
                            0)
                FROM CompanyStockReceipts r
                WHERE r.DealerId = @DealerId
                  AND
                  (
                    r.ReceiptNumber LIKE @Search
                    OR ISNULL(r.CompanyInvoiceNo, '') LIKE @Search
                  )
                  AND (@Status = '' OR r.Status = @Status)
                ORDER BY r.ReceiptDate DESC, r.ReceiptId DESC";

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
                dt.Columns.Add("TotalCostText", typeof(string));
                dt.Columns.Add("CanPost", typeof(bool));
            }

            foreach (DataRow row in dt.Rows)
            {
                string rowStatus = row["Status"].ToString();

                row["StatusCss"] = StatusCss(rowStatus);

                row["TotalCostText"] =
                    Convert.ToDecimal(row["TotalCost"])
                        .ToString("N2");

                // Only a Draft may be posted.
                row["CanPost"] =
                    rowStatus.Equals(
                        "Draft",
                        StringComparison.OrdinalIgnoreCase);
            }

            gvReceipts.DataSource = dt;
            gvReceipts.DataBind();
        }


        internal static string StatusCss(string status)
        {
            switch (status)
            {
                case "Posted":
                    return "badge bg-success";

                default:
                    return "badge bg-warning text-dark";
            }
        }


        protected void btnSearch_Click(
            object sender,
            EventArgs e)
        {
            LoadReceipts();
        }


        protected void FilterChanged(
            object sender,
            EventArgs e)
        {
            LoadReceipts();
        }


        // ============================================================
        // POST
        //
        // The work itself lives in AddCompanyReceipt.PostReceipt so
        // the "Save & Post" button and this link can never drift
        // apart - the routine runs inside ONE transaction and is
        // race-safe: the Draft -> Posted flip must affect exactly
        // one row.
        // ============================================================

        protected void lnkPost_Click(
            object sender,
            EventArgs e)
        {
            LinkButton button = sender as LinkButton;

            int receiptId = 0;

            if (button == null ||
                !int.TryParse(
                    button.CommandArgument,
                    out receiptId) ||
                receiptId <= 0)
            {
                ShowMessage(
                    "Receipt not found.",
                    false);

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
                            AddCompanyReceipt.PostReceipt(
                                con,
                                tx,
                                DealerId,
                                receiptId);

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
                    "~/Dealer/CompanyReceipts.aspx?posted=1");
            }
            catch (Exception ex)
            {
                LoadReceipts();

                ShowMessage(
                    ex.Message,
                    false);
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
