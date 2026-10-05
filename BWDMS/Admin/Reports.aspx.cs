using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Admin
{
    public partial class Reports : System.Web.UI.Page
    {
        // The DataTable the grid is currently bound to. The CSV
        // export writes exactly this table, so the download can
        // never drift away from what is on screen.
        private DataTable _currentTable;


        // True while the CSV body is being streamed; the page's
        // normal HTML render is skipped in that case.
        private bool _exporting;


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
                LoadKpis();
                RunReport();
            }
        }


        // ------------------------------------------------------------
        // KPI TILES
        // ------------------------------------------------------------

        private void LoadKpis()
        {
            string query = @"
                SELECT
                    Users = (SELECT COUNT(*) FROM Users),
                    Dealers =
                        (SELECT COUNT(*) FROM Users WHERE Role = 'Dealer'),
                    Shops =
                        (SELECT COUNT(*) FROM Shops),
                    Sales =
                        ISNULL(
                            (SELECT SUM(GrandTotal) FROM Orders),
                            0)";

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

                        if (dt.Rows.Count > 0)
                        {
                            DataRow row = dt.Rows[0];

                            lblKpiUsers.Text =
                                row["Users"].ToString();

                            lblKpiDealers.Text =
                                row["Dealers"].ToString();

                            lblKpiShops.Text =
                                row["Shops"].ToString();

                            lblKpiSales.Text =
                                Convert.ToDecimal(row["Sales"])
                                    .ToString("N2");
                        }
                    }
                }
            }
        }


        // ------------------------------------------------------------
        // REPORT
        // ------------------------------------------------------------

        protected void ReportChanged(
            object sender,
            EventArgs e)
        {
            RunReport();
        }


        private void RunReport()
        {
            string kind = ddlReport.SelectedValue;

            string title;
            string query;

            switch (kind)
            {
                case "orders":
                    title = "Orders by Status";
                    query = @"
                        SELECT
                            Label = Status,
                            Count = COUNT(*),
                            Amount =
                                CAST(SUM(GrandTotal) AS NVARCHAR(30))
                        FROM Orders
                        GROUP BY Status
                        ORDER BY COUNT(*) DESC, Status";
                    break;

                case "products":
                    title = "Products by Category";
                    query = @"

                        SELECT
                            Label = ISNULL(c.CategoryName, '(none)'),
                            Count = COUNT(DISTINCT p.ProductId),
                            Amount = CAST(
                                COUNT(v.ProductVariantId)
                                AS NVARCHAR(30))
                        FROM ProductCategories c
                        LEFT JOIN Products p
                            ON p.CategoryId = c.CategoryId
                        LEFT JOIN ProductVariants v
                            ON v.ProductId = p.ProductId
                        GROUP BY c.CategoryName, c.CategoryId
                        ORDER BY COUNT(DISTINCT p.ProductId) DESC,
                                 c.CategoryName";
                    break;

                case "shops":
                    title = "Shops by Dealer";
                    query = @"
                        SELECT
                            Label = ISNULL(d.FullName, '(unassigned)'),
                            Count = COUNT(*),
                            Amount =
                                CAST(
                                    SUM(ISNULL(s.OpeningBalance, 0))
                                    AS NVARCHAR(30))
                        FROM Shops s
                        LEFT JOIN Users d
                            ON d.UserId = s.DealerId
                        GROUP BY d.FullName, d.UserId
                        ORDER BY COUNT(*) DESC, d.FullName";
                    break;

                case "stock":
                    title = "Stock Value by Dealer";
                    query = @"
                        SELECT
                            Label = ISNULL(d.FullName, '(unassigned)'),
                            Count = CAST(
                                SUM(ISNULL(i.QuantityPackets, 0))
                                AS NVARCHAR(30)),
                            Amount = CAST(
                                SUM(
                                    ISNULL(i.QuantityPackets, 0)
                                    * ISNULL(pk.PurchasePrice, 0))
                                AS NVARCHAR(30))
                        FROM Inventory i
                        LEFT JOIN Users d
                            ON d.UserId = i.DealerId
                        OUTER APPLY
                        (
                            SELECT TOP (1)
                                PurchasePrice = pp.DealerPurchasePrice
                            FROM ProductUnitOptions uo
                            LEFT JOIN ProductPrices pp
                                ON pp.ProductOptionId =
                                   uo.ProductOptionId
                            WHERE uo.ProductVariantId =
                                  i.ProductVariantId
                              AND uo.UnitId =
                                  (
                                      SELECT TOP (1) su.UnitId
                                      FROM SellingUnits su
                                      WHERE su.UnitCode = 'PKT'
                                        AND su.IsActive = 1
                                      ORDER BY su.UnitId
                                  )
                        ) pk
                        GROUP BY d.FullName, d.UserId
                        ORDER BY SUM(ISNULL(i.QuantityPackets, 0)) DESC";

                    break;

                case "ordersource":
                    title = "Orders by Source";
                    query = @"
                        SELECT
                            Dealer =
                                ISNULL(
                                    d.FullName,
                                    '(unassigned)'),
                            Source =
                                ISNULL(
                                    NULLIF(
                                        o.OrderSource,
                                        ''),
                                    '(none)'),
                            o.Status,
                            Orders = COUNT(*),
                            GrandTotal =
                                ISNULL(
                                    SUM(o.GrandTotal),
                                    0)
                        FROM Orders o
                        LEFT JOIN Users d
                            ON d.UserId = o.DealerId
                        GROUP BY
                            d.FullName,
                            d.UserId,
                            o.OrderSource,
                            o.Status
                        ORDER BY d.FullName, d.UserId,
                                 o.OrderSource, o.Status";

                    break;

                case "returns":
                    title = "Returns";
                    query = @"
                        SELECT
                            Dealer =
                                ISNULL(
                                    d.FullName,
                                    '(unassigned)'),
                            ReturnNumber =
                                ISNULL(
                                    sr.ReturnNumber,
                                    '-'),
                            ReturnDate = sr.ReturnDate,
                            Shop =
                                ISNULL(s.ShopName, '-'),
                            sr.Status,
                            Condition =
                                ISNULL(
                                    srd.Condition,
                                    '-'),
                            Quantity =
                                ISNULL(srd.Quantity, 0),
                            Packets =
                                ISNULL(
                                    srd.QuantityPackets,
                                    0)
                        FROM SalesReturns sr
                        LEFT JOIN SalesReturnDetails srd
                            ON srd.SalesReturnId =
                               sr.SalesReturnId
                        LEFT JOIN Shops s
                            ON s.ShopId = sr.ShopId
                        LEFT JOIN Users d
                            ON d.UserId = sr.DealerId
                        ORDER BY sr.ReturnDate DESC,
                                 sr.SalesReturnId DESC,
                                 srd.SalesReturnDetailId";

                    break;

                case "receipts":
                    title = "Company Receipts";
                    query = @"
                        SELECT
                            Dealer =
                                ISNULL(
                                    d.FullName,
                                    '(unassigned)'),
                            ReceiptNumber =
                                ISNULL(
                                    cr.ReceiptNumber,
                                    '-'),
                            ReceiptDate = cr.ReceiptDate,
                            CompanyInvoiceNo =
                                ISNULL(
                                    cr.CompanyInvoiceNo,
                                    '-'),
                            cr.Status,
                            Lines =
                                ISNULL(x.LineCount, 0),
                            TotalCost =
                                ISNULL(cr.TotalCost, 0)
                        FROM CompanyStockReceipts cr
                        LEFT JOIN
                        (
                            SELECT
                                ReceiptId,
                                LineCount = COUNT(*)
                            FROM CompanyStockReceiptDetails
                            GROUP BY ReceiptId
                        ) x
                            ON x.ReceiptId = cr.ReceiptId
                        LEFT JOIN Users d
                            ON d.UserId = cr.DealerId
                        ORDER BY cr.ReceiptDate DESC,
                                 cr.ReceiptId DESC";

                    break;

                default:
                    title = "Users by Role";
                    query = @"
                        SELECT
                            Label = Role,
                            Count = COUNT(*),
                            Amount = CAST(
                                SUM(
                                    CASE
                                        WHEN IsActive = 1 THEN 1
                                        ELSE 0
                                    END) AS NVARCHAR(30))
                        FROM Users
                        GROUP BY Role
                        ORDER BY COUNT(*) DESC, Role";
                    break;
            }

            lblReportTitle.Text = title;

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            // Reports that carry their own money column keep it
            // untouched; only the classic Label/Count/Amount shape
            // gets the "-" placeholder and the N2 formatting.
            if (dt.Columns.Contains("Amount"))
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row["Amount"] == null ||
                        row["Amount"] == DBNull.Value ||
                        row["Amount"].ToString() == "")
                    {
                        row["Amount"] = "-";
                    }
                    else
                    {
                        decimal value;

                        if (decimal.TryParse(
                            row["Amount"].ToString(),
                            out value))
                        {
                            row["Amount"] = value.ToString("N2");
                        }
                    }
                }
            }

            lblRecordCount.Text =
                "records: " + dt.Rows.Count;

            _currentTable = dt;

            gvReport.DataSource = dt;
            gvReport.DataBind();
        }


        // ------------------------------------------------------------
        // CSV EXPORT
        // ------------------------------------------------------------

        protected void btnExport_Click(object sender, EventArgs e)
        {
            // Re-run the exact same report (same key, same SQL) so
            // the file always holds what the grid shows.
            RunReport();

            if (_currentTable == null ||
                _currentTable.Columns.Count == 0)
            {
                return;
            }

            _exporting = true;

            CsvExport.Write(
                Response,
                "admin-reports-" + ddlReport.SelectedValue,
                _currentTable);
        }


        // While the CSV is being written the page must not render
        // its HTML into the download.
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            if (_exporting)
            {
                return;
            }

            base.Render(writer);
        }
    }
}
