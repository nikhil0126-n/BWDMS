using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Salesman
{
    public partial class Reports : System.Web.UI.Page
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
                LoadKpis();
                RunReport();
            }
        }


        private void LoadKpis()
        {
            string query = @"
                SELECT
                    Orders =
                        (
                            SELECT COUNT(*)
                            FROM Orders
                            WHERE SalesmanId = @SalesmanId
                        ),
                    Value =
                        ISNULL(
                            (
                                SELECT SUM(GrandTotal)
                                FROM Orders
                                WHERE SalesmanId = @SalesmanId
                            ),
                            0),
                    Shops =
                        (
                            SELECT COUNT(*)
                            FROM Shops s
                            WHERE s.IsActive = 1
                              AND
                              (
                                s.RouteScheduleId IN
                                (
                                    SELECT rs2.RouteScheduleId
                                    FROM RouteSchedules rs2
                                    WHERE rs2.SalesmanId = @SalesmanId
                                )
                                OR s.RouteId IN
                                (
                                    SELECT rs3.RouteId
                                    FROM RouteSchedules rs3
                                    WHERE rs3.SalesmanId = @SalesmanId
                                )
                              )
                        ),
                    Routes =
                        (
                            SELECT COUNT(*)
                            FROM RouteSchedules
                            WHERE SalesmanId = @SalesmanId
                        )";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@SalesmanId",
                        SqlDbType.Int).Value = SalesmanId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            DataRow row = dt.Rows[0];

                            lblKpiOrders.Text =
                                row["Orders"].ToString();

                            lblKpiValue.Text =
                                Convert.ToDecimal(row["Value"])
                                    .ToString("N2");

                            lblKpiShops.Text =
                                row["Shops"].ToString();

                            lblKpiRoutes.Text =
                                row["Routes"].ToString();
                        }
                    }
                }
            }
        }


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
                case "orderday":
                    title = "Orders by Day";
                    query = @"
                        SELECT
                            Label =
                                CONVERT(NVARCHAR(10), OrderDate, 120),
                            Count = COUNT(*),
                            Amount =
                                CAST(SUM(GrandTotal) AS NVARCHAR(30))
                        FROM Orders
                        WHERE SalesmanId = @SalesmanId
                        GROUP BY CONVERT(NVARCHAR(10), OrderDate, 120)
                        ORDER BY CONVERT(NVARCHAR(10), OrderDate, 120) DESC";
                    break;

                case "shops":
                    title = "Shops by Village";
                    query = @"
                        SELECT
                            Label = ISNULL(v.VillageName, '(none)'),
                            Count = COUNT(*),
                            Amount =
                                CAST(
                                    SUM(ISNULL(s.OpeningBalance, 0))
                                    AS NVARCHAR(30))
                        FROM Shops s
                        LEFT JOIN Villages v
                            ON v.VillageId = s.VillageId
                        WHERE s.IsActive = 1
                          AND
                          (
                            s.RouteScheduleId IN
                            (
                                SELECT rs2.RouteScheduleId
                                FROM RouteSchedules rs2
                                WHERE rs2.SalesmanId = @SalesmanId
                            )
                            OR s.RouteId IN
                            (
                                SELECT rs3.RouteId
                                FROM RouteSchedules rs3
                                WHERE rs3.SalesmanId = @SalesmanId
                            )
                          )
                        GROUP BY v.VillageName, v.VillageId
                        ORDER BY COUNT(*) DESC, v.VillageName";
                    break;

                case "stock":
                    title = "Stock Loaded";
                    query = @"
                        SELECT
                            Label = ISNULL(p.ProductName, '(unknown)')
                                    + ' - '
                                    + ISNULL(vr.VariantName, ''),
                            Count = SUM(vs.LoadedPackets),
                            Amount =
                                CAST(
                                    SUM(vs.LoadedPackets)
                                    - SUM(vs.SoldPackets)
                                    - SUM(vs.ReturnedPackets)
                                    - SUM(vs.DamagedPackets)
                                    AS NVARCHAR(30))
                        FROM VehicleStock vs
                        LEFT JOIN ProductVariants vr
                            ON vr.ProductVariantId =
                               vs.ProductVariantId
                        LEFT JOIN Products p
                            ON p.ProductId = vr.ProductId
                        WHERE vs.SalesmanId = @SalesmanId
                        GROUP BY p.ProductName, vr.VariantName
                        ORDER BY SUM(vs.LoadedPackets) DESC";
                    break;

                case "ordersource":
                    title = "My Orders by Source";
                    query = @"
                        SELECT
                            Label =
                                ISNULL(
                                    NULLIF(OrderSource, ''),
                                    '(none)')
                                + ' / '
                                + ISNULL(Status, '-'),
                            Count = COUNT(*),
                            Amount =
                                CAST(
                                    SUM(GrandTotal)
                                    AS NVARCHAR(30))
                        FROM Orders
                        WHERE SalesmanId = @SalesmanId
                        GROUP BY OrderSource, Status
                        ORDER BY OrderSource, Status";
                    break;

                default:
                    title = "Orders by Status";
                    query = @"
                        SELECT
                            Label = o.Status,
                            Count = COUNT(*),
                            Amount =
                                CAST(SUM(o.GrandTotal) AS NVARCHAR(30))
                        FROM Orders o
                        WHERE o.SalesmanId = @SalesmanId
                        GROUP BY o.Status
                        ORDER BY COUNT(*) DESC, o.Status";
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

            if (!dt.Columns.Contains("Amount"))
            {
                dt.Columns.Add("Amount", typeof(string));
            }

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

            lblRecordCount.Text =
                "records: " + dt.Rows.Count;

            gvReport.DataSource = dt;
            gvReport.DataBind();
        }
    }
}
