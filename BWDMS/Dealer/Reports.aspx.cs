using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class Reports : System.Web.UI.Page
    {
        // Dealer's own id lives in UserId (there is no Session["DealerId"]).
        private int DealerId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }


        // Grid column index of the status column, -1 when the
        // current report has no status column.
        private int _statusColumnIndex = -1;


        // The DataTable the grid is currently bound to. The CSV
        // export writes exactly this table, so the download can
        // never drift away from what is on screen.
        private DataTable _currentTable;


        // True while the CSV body is being streamed; the page's
        // normal HTML render is skipped in that case.
        private bool _exporting;


        // Friendly column headers used by every report.
        private static readonly Dictionary<string, string> ColumnHeaders =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "Product", "Product" },
                { "Variant", "Variant" },
                { "QuantityPackets", "Quantity (packets)" },
                { "ReorderLevel", "Reorder Level" },
                { "StatusText", "Status" },
                { "Status", "Status" },
                { "OrderCount", "Orders" },
                { "SubTotal", "Sub Total" },
                { "Discount", "Discount" },
                { "GrandTotal", "Grand Total" },
                { "Shop", "Shop" },
                { "TransactionDateText", "Date" },
                { "TransactionType", "Type" },
                { "ReferenceNo", "Reference" },
                { "Remarks", "Remarks" },
                { "TotalPackets", "Packets Moved" },
                { "OrderDateText", "Order Date" },
                { "Orders", "Orders" },
                { "Packets", "Packets" },
                { "Quantity", "Quantity" },
                { "GrossValue", "Gross Value" },
                { "Salesman", "Salesman" },
                { "Route", "Route" },
                { "Village", "Village" },
                { "Source", "Order Source" },
                { "ReceiptNumber", "Receipt No." },
                { "ReceiptDate", "Receipt Date" },
                { "CompanyInvoiceNo", "Invoice No." },
                { "Lines", "Lines" },
                { "TotalCost", "Total Cost" },
                { "ReconciliationDate", "Date" },
                { "Vehicle", "Vehicle" },
                { "Approval", "Approval" },
                { "ExpectedPackets", "Expected Packets" },
                { "ActualPackets", "Actual Packets" },
                { "VariancePackets", "Variance Packets" },
                { "ReturnNumber", "Return No." },
                { "ReturnDate", "Return Date" },
                { "Condition", "Condition" }
            };


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
                LoadDashboard();
            }
        }


        // ============================================================
        // PAGE LOAD
        //
        // One stock query feeds the KPI tile and the two stock
        // reports, so the numbers can never drift apart.
        // ============================================================

        private void LoadDashboard()
        {
            HideMessage();

            DataTable stock = LoadStockLevels();

            LoadKpis(stock);
            LoadReport(stock);
        }


        private void LoadKpis(DataTable stock)
        {
            int lowStockCount = 0;

            foreach (DataRow row in stock.Rows)
            {
                string status =
                    Convert.ToString(row["StatusText"]);

                if (!status.Equals(
                    "In Stock",
                    StringComparison.OrdinalIgnoreCase))
                {
                    lowStockCount++;
                }
            }

            int totalProducts = 0;
            int orderCount = 0;
            decimal salesValue = 0m;

            string productQuery = @"
                SELECT TotalProducts = COUNT(*)
                FROM Products
                WHERE IsActive = 1";

            string orderQuery = @"
                SELECT
                    OrderCount = COUNT(*),
                    SalesValue = ISNULL(SUM(GrandTotal), 0)
                FROM Orders
                WHERE DealerId = @DealerId";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(productQuery, con))
                {
                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            totalProducts = Convert.ToInt32(
                                dt.Rows[0]["TotalProducts"]);
                        }
                    }
                }

                using (SqlCommand cmd =
                    new SqlCommand(orderQuery, con))
                {
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            orderCount = Convert.ToInt32(
                                dt.Rows[0]["OrderCount"]);

                            salesValue = Convert.ToDecimal(
                                dt.Rows[0]["SalesValue"]);
                        }
                    }
                }
            }

            lblKpiProducts.Text = totalProducts.ToString();
            lblKpiLowStock.Text = lowStockCount.ToString();
            lblKpiOrders.Text = orderCount.ToString();
            lblKpiSales.Text = salesValue.ToString("N2");
        }


        // ============================================================
        // REPORTS
        // ============================================================

        private void LoadReport(DataTable stock)
        {
            string report = ddlReport.SelectedValue;

            string title = TitleFor(report);


            // --------------------------------------------
            // 1. Stock Summary
            // --------------------------------------------

            if (report == "stock")
            {
                BindReport(stock, title);
                return;
            }


            // --------------------------------------------
            // 2. Low Stock (Low + Out of Stock)
            // --------------------------------------------

            if (report == "low")
            {
                DataTable low = stock.Clone();

                foreach (DataRow row in stock.Rows)
                {
                    string status =
                        Convert.ToString(row["StatusText"]);

                    if (status.Equals(
                            "Low Stock",
                            StringComparison.OrdinalIgnoreCase) ||
                        status.Equals(
                            "Out of Stock",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        low.ImportRow(row);
                    }
                }

                BindReport(low, title);
                return;
            }


            // --------------------------------------------
            // 3. Orders by Status
            // --------------------------------------------

            if (report == "orderstatus")
            {
                string query = @"
                    SELECT
                        o.Status,
                        OrderCount = COUNT(*),
                        SubTotal = ISNULL(SUM(o.SubTotal), 0),
                        Discount = ISNULL(SUM(o.Discount), 0),
                        GrandTotal = ISNULL(SUM(o.GrandTotal), 0)
                    FROM Orders o
                    WHERE o.DealerId = @DealerId
                    GROUP BY o.Status
                    ORDER BY o.Status";

                BindReport(
                    RunDealerQuery(query),
                    title);
                return;
            }


            // --------------------------------------------
            // 4. Sales by Shop (top 20)
            // --------------------------------------------

            if (report == "shop")
            {
                string query = @"
                    SELECT TOP 20
                        Shop = ISNULL(s.ShopName, '-'),
                        OrderCount = COUNT(*),
                        GrandTotal = ISNULL(
                            SUM(o.GrandTotal), 0)
                    FROM Orders o
                    LEFT JOIN Shops s
                        ON s.ShopId = o.ShopId
                       AND s.DealerId = @DealerId
                    WHERE o.DealerId = @DealerId
                    GROUP BY s.ShopId, s.ShopName
                    ORDER BY SUM(o.GrandTotal) DESC";

                BindReport(
                    RunDealerQuery(query),
                    title);
                return;
            }


            // --------------------------------------------
            // 5. Stock Movement (the stock ledger)
            // --------------------------------------------

            if (report == "movement")
            {
                string query = @"
                    SELECT
                        TransactionDateText =
                            CONVERT(
                                NVARCHAR(16),
                                st.TransactionDate,
                                120),
                        st.TransactionType,
                        ReferenceNo =
                            ISNULL(st.ReferenceNo, '-'),
                        Remarks = ISNULL(st.Remarks, '-'),
                        TotalPackets =
                            ISNULL(SUM(d.QuantityPackets), 0)
                    FROM StockTransactions st
                    LEFT JOIN StockTransactionDetails d
                        ON d.StockTransactionId =
                           st.StockTransactionId
                    WHERE st.DealerId = @DealerId
                    GROUP BY
                        st.StockTransactionId,
                        st.TransactionDate,
                        st.TransactionType,
                        st.ReferenceNo,
                        st.Remarks
                    ORDER BY st.TransactionDate DESC,
                             st.StockTransactionId DESC";

                BindReport(
                    RunDealerQuery(query),
                    title);
                return;
            }


            // ------------------------------------------------------
            // Everything below honours the From / To date filter:
            //
            //  6. Orders by Date
            //  7. Sales by Product
            //  8. Sales by Salesman
            //  9. Sales by Route / Village
            // 10. Orders by Source
            // 11. Company Receipts
            // 12. Vehicle Reconciliation
            // 13. Returns & Damaged
            // ------------------------------------------------------

            bool fromInvalid;
            bool toInvalid;

            DateTime? fromDate =
                ParseDate(txtFromDate.Text, out fromInvalid);

            DateTime? toDate =
                ParseDate(txtToDate.Text, out toInvalid);


            if (fromInvalid || toInvalid)
            {
                ShowMessage(
                    "Please enter valid dates " +
                    "(yyyy-MM-dd or dd/MM/yyyy).",
                    false);

                BindReport(
                    new DataTable(),
                    title);

                return;
            }

            if (fromDate.HasValue &&
                toDate.HasValue &&
                fromDate.Value > toDate.Value)
            {
                ShowMessage(
                    "From date cannot be after To date.",
                    false);

                BindReport(
                    new DataTable(),
                    title);

                return;
            }

            // --------------------------------------------
            // 6. Orders by Date (From / To range)
            // --------------------------------------------

            if (report == "orderdate")
            {
                string dateQuery = @"
                    SELECT
                        OrderDateText =
                            ISNULL(
                                CONVERT(NVARCHAR(10),
                                       o.OrderDate,
                                       120),
                               '-'),
                        OrderCount = COUNT(*),
                        GrandTotal = ISNULL(
                            SUM(o.GrandTotal), 0)
                    FROM Orders o
                    WHERE o.DealerId = @DealerId
                      AND
                      (
                        @FromDate IS NULL
                        OR o.OrderDate >= @FromDate
                      )
                      AND
                      (
                        @ToDate IS NULL
                        OR o.OrderDate < DATEADD(DAY, 1, @ToDate)
                      )
                    GROUP BY ISNULL(
                                CONVERT(NVARCHAR(10),
                                        o.OrderDate,
                                        120),
                                '-')
                    ORDER BY OrderDateText";

                BindReport(
                    RunDealerQuery(dateQuery, fromDate, toDate),
                    title);

                return;
            }


            // --------------------------------------------
            // 7. Sales by Product
            //
            // Sales rule used by every "Sales by ..." report on
            // this page: an order counts as a sale whenever it is
            // not Cancelled, so Pending, Confirmed and Dispatched
            // quantities are all included. The same rule is used
            // for "Sales by Salesman" and "Sales by Route /
            // Village" below.
            // --------------------------------------------

            if (report == "salesproduct")
            {
                string query = @"
                    SELECT TOP 20
                        Product =
                            ISNULL(p.ProductName, '(unknown)'),
                        Variant =
                            ISNULL(v.VariantName, '-'),
                        Packets =
                            ISNULL(SUM(od.QuantityPackets), 0),
                        Quantity =
                            ISNULL(SUM(od.Quantity), 0),
                        Orders = COUNT(DISTINCT o.OrderId),
                        GrossValue =
                            ISNULL(SUM(od.LineTotal), 0)
                    FROM OrderDetails od
                    INNER JOIN Orders o
                        ON o.OrderId = od.OrderId
                    LEFT JOIN ProductVariants v
                        ON v.ProductVariantId =
                           od.ProductVariantId
                    LEFT JOIN Products p
                        ON p.ProductId = v.ProductId
                    WHERE o.DealerId = @DealerId
                      AND ISNULL(o.Status, '') <> 'Cancelled'
                      AND
                      (
                        @FromDate IS NULL
                        OR o.OrderDate >= @FromDate
                      )
                      AND
                      (
                        @ToDate IS NULL
                        OR o.OrderDate < DATEADD(DAY, 1, @ToDate)
                      )
                    GROUP BY p.ProductName, v.VariantName
                    ORDER BY SUM(od.LineTotal) DESC,
                             p.ProductName,
                             v.VariantName";

                BindReport(
                    RunDealerQuery(query, fromDate, toDate),
                    title);

                return;
            }


            // --------------------------------------------
            // 8. Sales by Salesman
            // --------------------------------------------

            if (report == "salesbysalesman")
            {
                string query = @"
                    SELECT TOP 20
                        Salesman =
                            ISNULL(u.FullName, '(unassigned)'),
                        Orders = COUNT(*),
                        GrandTotal =
                            ISNULL(SUM(o.GrandTotal), 0)
                    FROM Orders o
                    LEFT JOIN Users u
                        ON u.UserId = o.SalesmanId
                    WHERE o.DealerId = @DealerId
                      AND ISNULL(o.Status, '') <> 'Cancelled'
                      AND
                      (
                        @FromDate IS NULL
                        OR o.OrderDate >= @FromDate
                      )
                      AND
                      (
                        @ToDate IS NULL
                        OR o.OrderDate < DATEADD(DAY, 1, @ToDate)
                      )
                    GROUP BY u.UserId, u.FullName
                    ORDER BY SUM(o.GrandTotal) DESC,
                             u.FullName";

                BindReport(
                    RunDealerQuery(query, fromDate, toDate),
                    title);

                return;
            }


            // --------------------------------------------
            // 9. Sales by Route / Village
            //
            // Route comes from the order's route schedule, or
            // from the shop's own route when the order has no
            // schedule; the village comes from the shop.
            // --------------------------------------------

            if (report == "salesbyroute")
            {
                string query = @"
                    SELECT TOP 20
                        Route =
                            ISNULL(r.RouteName, '(none)'),
                        Village =
                            ISNULL(v.VillageName, '(none)'),
                        Orders = COUNT(*),
                        GrandTotal =
                            ISNULL(SUM(o.GrandTotal), 0)
                    FROM Orders o
                    LEFT JOIN RouteSchedules rs
                        ON rs.RouteScheduleId =
                           o.RouteScheduleId
                    LEFT JOIN Shops s
                        ON s.ShopId = o.ShopId
                       AND s.DealerId = @DealerId
                    LEFT JOIN Routes r
                        ON r.RouteId =
                           ISNULL(rs.RouteId, s.RouteId)
                       AND r.DealerId = @DealerId
                    LEFT JOIN Villages v
                        ON v.VillageId = s.VillageId
                    WHERE o.DealerId = @DealerId
                      AND ISNULL(o.Status, '') <> 'Cancelled'
                      AND
                      (
                        @FromDate IS NULL
                        OR o.OrderDate >= @FromDate
                      )
                      AND
                      (
                        @ToDate IS NULL
                        OR o.OrderDate < DATEADD(DAY, 1, @ToDate)
                      )
                    GROUP BY
                        r.RouteId,
                        r.RouteName,
                        v.VillageId,
                        v.VillageName
                    ORDER BY SUM(o.GrandTotal) DESC,
                             r.RouteName,
                             v.VillageName";

                BindReport(
                    RunDealerQuery(query, fromDate, toDate),
                    title);

                return;
            }


            // --------------------------------------------
            // 10. Orders by Source (Beat / Telephone /
            //     Counter, crossed with the order status)
            // --------------------------------------------

            if (report == "ordersource")
            {
                string query = @"
                    SELECT
                        Source =
                            ISNULL(
                                NULLIF(o.OrderSource, ''),
                                '(none)'),
                        o.Status,
                        Orders = COUNT(*),
                        GrandTotal =
                            ISNULL(SUM(o.GrandTotal), 0)
                    FROM Orders o
                    WHERE o.DealerId = @DealerId
                      AND
                      (
                        @FromDate IS NULL
                        OR o.OrderDate >= @FromDate
                      )
                      AND
                      (
                        @ToDate IS NULL
                        OR o.OrderDate < DATEADD(DAY, 1, @ToDate)
                      )
                    GROUP BY o.OrderSource, o.Status
                    ORDER BY o.OrderSource, o.Status";

                BindReport(
                    RunDealerQuery(query, fromDate, toDate),
                    title);

                return;
            }


            // --------------------------------------------
            // 11. Company Receipts (receipt history)
            // --------------------------------------------

            if (report == "receipts")
            {
                string query = @"
                    SELECT
                        ReceiptNumber =
                            ISNULL(cr.ReceiptNumber, '-'),
                        ReceiptDate = cr.ReceiptDate,
                        CompanyInvoiceNo =
                            ISNULL(cr.CompanyInvoiceNo, '-'),
                        cr.Status,
                        Lines = ISNULL(x.LineCount, 0),
                        TotalCost = ISNULL(cr.TotalCost, 0)
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
                    WHERE cr.DealerId = @DealerId
                      AND
                      (
                        @FromDate IS NULL
                        OR cr.ReceiptDate >= @FromDate
                      )
                      AND
                      (
                        @ToDate IS NULL
                        OR cr.ReceiptDate <
                           DATEADD(DAY, 1, @ToDate)
                      )
                    ORDER BY cr.ReceiptDate DESC,
                             cr.ReceiptId DESC";

                BindReport(
                    RunDealerQuery(query, fromDate, toDate),
                    title);

                return;
            }


            // --------------------------------------------
            // 12. Vehicle Reconciliation (expected vs actual
            //     packets, with the variance)
            // --------------------------------------------

            if (report == "recon")
            {
                string query = @"
                    SELECT
                        ReconciliationDate =
                            vr.ReconciliationDate,
                        Vehicle =
                            ISNULL(v.VehicleNumber, '-'),
                        Salesman =
                            ISNULL(u.FullName, '-'),
                        Approval =
                            CASE
                                WHEN vr.IsApproved = 1
                                THEN 'Approved'
                                ELSE 'Pending'
                            END,
                        ExpectedPackets =
                            ISNULL(x.ExpectedPackets, 0),
                        ActualPackets =
                            ISNULL(x.ActualPackets, 0),
                        VariancePackets =
                            ISNULL(x.VariancePackets, 0)
                    FROM VehicleReconciliations vr
                    LEFT JOIN
                    (
                        SELECT
                            VehicleReconciliationId,
                            ExpectedPackets =
                                SUM(ExpectedPackets),
                            ActualPackets =
                                SUM(ActualPackets),
                            VariancePackets =
                                SUM(VariancePackets)
                        FROM VehicleReconciliationDetails
                        GROUP BY VehicleReconciliationId
                    ) x
                        ON x.VehicleReconciliationId =
                           vr.VehicleReconciliationId
                    LEFT JOIN Vehicles v
                        ON v.VehicleId = vr.VehicleId
                    LEFT JOIN Users u
                        ON u.UserId = vr.SalesmanId
                    WHERE vr.DealerId = @DealerId
                      AND
                      (
                        @FromDate IS NULL
                        OR vr.ReconciliationDate >= @FromDate
                      )
                      AND
                      (
                        @ToDate IS NULL
                        OR vr.ReconciliationDate <
                           DATEADD(DAY, 1, @ToDate)
                      )
                    ORDER BY vr.ReconciliationDate DESC,
                             vr.VehicleReconciliationId DESC";

                BindReport(
                    RunDealerQuery(query, fromDate, toDate),
                    title);

                return;
            }


            // --------------------------------------------
            // 13. Returns & Damaged
            // --------------------------------------------

            if (report == "returns")
            {
                string query = @"
                    SELECT
                        ReturnNumber =
                            ISNULL(sr.ReturnNumber, '-'),
                        ReturnDate = sr.ReturnDate,
                        Shop = ISNULL(s.ShopName, '-'),
                        sr.Status,
                        Condition =
                            ISNULL(srd.Condition, '-'),
                        Quantity = ISNULL(srd.Quantity, 0),
                        Packets =
                            ISNULL(srd.QuantityPackets, 0)
                    FROM SalesReturns sr
                    LEFT JOIN SalesReturnDetails srd
                        ON srd.SalesReturnId =
                           sr.SalesReturnId
                    LEFT JOIN Shops s
                        ON s.ShopId = sr.ShopId
                       AND s.DealerId = @DealerId
                    WHERE sr.DealerId = @DealerId
                      AND
                      (
                        @FromDate IS NULL
                        OR sr.ReturnDate >= @FromDate
                      )
                      AND
                      (
                        @ToDate IS NULL
                        OR sr.ReturnDate <
                           DATEADD(DAY, 1, @ToDate)
                      )
                    ORDER BY sr.ReturnDate DESC,
                             sr.SalesReturnId DESC,
                             srd.SalesReturnDetailId";

                BindReport(
                    RunDealerQuery(query, fromDate, toDate),
                    title);

                return;
            }


            // Unknown report key - show the empty state instead of
            // leaving the grid bound to the previous report.
            BindReport(new DataTable(), title);
        }


        // Runs a report query that is already scoped with
        // "@DealerId = @DealerId".
        private DataTable RunDealerQuery(string query)
        {
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

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            return dt;
        }


        // Same as above, plus the From / To date filter that the
        // date-driven reports use. Both dates may be null, which
        // means "no date limit".
        private DataTable RunDealerQuery(
            string query,
            DateTime? fromDate,
            DateTime? toDate)
        {
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
                        "@FromDate",
                        SqlDbType.DateTime).Value =
                            fromDate.HasValue
                                ? (object)fromDate.Value
                                : DBNull.Value;

                    cmd.Parameters.Add(
                        "@ToDate",
                        SqlDbType.DateTime).Value =
                            toDate.HasValue
                                ? (object)toDate.Value
                                : DBNull.Value;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            return dt;
        }


        // Report key -> the title shown above the grid (and used
        // for the empty / validation states).
        private static string TitleFor(string report)
        {
            switch (report)
            {
                case "stock":
                    return "Stock Summary";

                case "low":
                    return "Low Stock";

                case "orderstatus":
                    return "Orders by Status";

                case "shop":
                    return "Sales by Shop";

                case "movement":
                    return "Stock Movement";

                case "orderdate":
                    return "Orders by Date";

                case "salesproduct":
                    return "Sales by Product";

                case "salesbysalesman":
                    return "Sales by Salesman";

                case "salesbyroute":
                    return "Sales by Route / Village";

                case "ordersource":
                    return "Orders by Source";

                case "receipts":
                    return "Company Receipts";

                case "recon":
                    return "Vehicle Reconciliation";

                case "returns":
                    return "Returns & Damaged";

                default:
                    return "Report";
            }
        }


        // ============================================================
        // STOCK LEVELS
        //
        // A variant with no Inventory row is shown as zero, and the
        // status is computed in memory with exactly the same rule
        // as Inventory.aspx.cs.
        // ============================================================

        private DataTable LoadStockLevels()
        {
            string query = @"
                SELECT
                    Product = p.ProductName,
                    Variant = v.VariantName,
                    QuantityPackets =
                        ISNULL(i.QuantityPackets, 0),
                    ReorderLevel =
                        ISNULL(i.ReorderLevel, 0)
                FROM ProductVariants v
                INNER JOIN Products p
                    ON p.ProductId = v.ProductId
                LEFT JOIN Inventory i
                    ON i.ProductVariantId =
                       v.ProductVariantId
                   AND i.DealerId = @DealerId
                WHERE v.IsActive = 1
                  AND p.IsActive = 1
                ORDER BY p.ProductName,
                         v.SortOrder,
                         v.VariantName";

            DataTable dt = RunDealerQuery(query);

            if (!dt.Columns.Contains("StatusText"))
            {
                dt.Columns.Add("StatusText", typeof(string));
                dt.Columns.Add("StatusCss", typeof(string));
            }

            foreach (DataRow row in dt.Rows)
            {
                decimal qty =
                    Convert.ToDecimal(row["QuantityPackets"]);

                decimal reorder =
                    Convert.ToDecimal(row["ReorderLevel"]);

                if (qty <= 0)
                {
                    row["StatusText"] = "Out of Stock";
                    row["StatusCss"] = "badge bg-danger";
                }
                else if (reorder > 0 && qty <= reorder)
                {
                    row["StatusText"] = "Low Stock";
                    row["StatusCss"] =
                        "badge bg-warning text-dark";
                }
                else
                {
                    row["StatusText"] = "In Stock";
                    row["StatusCss"] = "badge bg-success";
                }
            }

            return dt;
        }


        // ============================================================
        // GRID BINDING
        //
        // The columns are built in code so one GridView can serve
        // every report shape.
        // ============================================================

        private void BindReport(DataTable dt, string title)
        {
            lblReportTitle.Text = title;
            lblRecordCount.Text =
                "records: " + dt.Rows.Count.ToString();

            _currentTable = dt;

            _statusColumnIndex = -1;

            gvReport.Columns.Clear();

            if (dt.Rows.Count == 0)
            {
                gvReport.Visible = false;
                gvReport.DataSource = null;
                gvReport.DataBind();

                pnlEmpty.Visible = true;

                return;
            }

            pnlEmpty.Visible = false;
            gvReport.Visible = true;

            foreach (DataColumn column in dt.Columns)
            {
                // Helper columns (badge colours) are never shown.
                if (column.ColumnName.EndsWith(
                    "Css",
                    StringComparison.Ordinal))
                {
                    continue;
                }

                BoundField field = new BoundField();

                field.DataField = column.ColumnName;
                field.HeaderText =
                    HeaderFor(column.ColumnName);

                string format = FormatFor(column);

                if (!string.IsNullOrEmpty(format))
                {
                    field.DataFormatString = format;
                }

                if (column.ColumnName == "StatusText")
                {
                    _statusColumnIndex =
                        gvReport.Columns.Count;
                }

                gvReport.Columns.Add(field);
            }

            gvReport.DataSource = dt;
            gvReport.DataBind();
        }


        private static string HeaderFor(string columnName)
        {
            string header;

            if (ColumnHeaders.TryGetValue(
                columnName,
                out header))
            {
                return header;
            }

            return columnName;
        }


        private static string FormatFor(DataColumn column)
        {
            string name = column.ColumnName;

            // Money
            if (name == "SubTotal" ||
                name == "Discount" ||
                name == "GrandTotal" ||
                name == "SalesValue")
            {
                return "{0:N2}";
            }

            // Whole numbers (packets, levels, counts)
            if (name == "QuantityPackets" ||
                name == "ReorderLevel" ||
                name == "OrderCount" ||
                name == "TotalPackets")
            {
                return "{0:N0}";
            }

            if (column.DataType == typeof(DateTime))
            {
                return "{0:dd MMM yyyy}";
            }

            if (column.DataType == typeof(decimal) ||
                column.DataType == typeof(double) ||
                column.DataType == typeof(float))
            {
                return "{0:N2}";
            }

            return null;
        }


        protected void gvReport_RowDataBound(
            object sender,
            GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
            {
                return;
            }

            if (_statusColumnIndex < 0 ||
                _statusColumnIndex >= e.Row.Cells.Count)
            {
                return;
            }

            TableCell cell =
                e.Row.Cells[_statusColumnIndex];

            string status = cell.Text.Trim();
            string css = null;

            if (status.Equals(
                "Out of Stock",
                StringComparison.OrdinalIgnoreCase))
            {
                css = "badge bg-danger";
            }
            else if (status.Equals(
                "Low Stock",
                StringComparison.OrdinalIgnoreCase))
            {
                css = "badge bg-warning text-dark";
            }
            else if (status.Equals(
                "In Stock",
                StringComparison.OrdinalIgnoreCase))
            {
                css = "badge bg-success";
            }

            if (css == null)
            {
                return;
            }

            cell.Text =
                "<span class=\"" + css + "\">" +
                Server.HtmlEncode(status) +
                "</span>";
        }


        // ============================================================
        // EVENT HANDLERS
        // ============================================================

        protected void ddlReport_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            LoadDashboard();
        }


        protected void DateFilterChanged(
            object sender,
            EventArgs e)
        {
            LoadDashboard();
        }


        // ------------------------------------------------------------
        // CSV EXPORT
        // ------------------------------------------------------------

        protected void btnExport_Click(object sender, EventArgs e)
        {
            // Re-run the exact same report pipeline (same report
            // key, same From / To filter, same SQL) so the file
            // always holds what the grid shows.
            LoadDashboard();

            if (_currentTable == null ||
                _currentTable.Columns.Count == 0)
            {
                // Nothing valid to export (for example an invalid
                // date filter) - keep the page and its message.
                return;
            }

            _exporting = true;

            CsvExport.Write(
                Response,
                "dealer-reports-" + ddlReport.SelectedValue,
                VisibleColumns(_currentTable));
        }


        // The grid hides the helper columns (the *Css badge
        // colours), so the export hides them too.
        private static DataTable VisibleColumns(DataTable source)
        {
            DataTable table = new DataTable();

            foreach (DataColumn column in source.Columns)
            {
                if (column.ColumnName.EndsWith(
                        "Css",
                        StringComparison.Ordinal))
                {
                    continue;
                }

                table.Columns.Add(
                    column.ColumnName,
                    column.DataType);
            }

            foreach (DataRow row in source.Rows)
            {
                DataRow copy = table.NewRow();

                foreach (DataColumn column in table.Columns)
                {
                    copy[column.ColumnName] =
                        row[column.ColumnName];
                }

                table.Rows.Add(copy);
            }

            return table;
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


        // ============================================================
        // HELPERS
        // ============================================================

        private DateTime? ParseDate(
            string text,
            out bool invalid)
        {
            invalid = false;

            text = (text ?? string.Empty).Trim();

            if (text.Length == 0)
            {
                return null;
            }

            DateTime value;

            if (DateTime.TryParseExact(
                    text,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out value))
            {
                return value;
            }

            if (DateTime.TryParseExact(
                    text,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out value))
            {
                return value;
            }

            if (DateTime.TryParse(
                    text,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out value))
            {
                return value;
            }

            if (DateTime.TryParse(
                    text,
                    CultureInfo.CurrentCulture,
                    DateTimeStyles.None,
                    out value))
            {
                return value;
            }

            invalid = true;

            return null;
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


        private void HideMessage()
        {
            lblMessage.Visible = false;
            lblMessage.Text = string.Empty;
        }
    }
}
