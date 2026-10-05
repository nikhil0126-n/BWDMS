using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Salesman
{
    public partial class VehicleStock : System.Web.UI.Page
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
                LoadStock();
            }
        }


        private void LoadStock()
        {
            string search = txtSearch.Text.Trim();
            string reconciled = ddlReconciled.SelectedValue;

            // Bad / unparsable dates are simply ignored rather
            // than turning the page into an error.
            DateTime fromDate;
            DateTime toDate;

            bool hasFrom = DateTime.TryParse(
                txtFromDate.Text.Trim(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out fromDate);

            bool hasTo = DateTime.TryParse(
                txtToDate.Text.Trim(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out toDate);

            string query = @"
                SELECT
                    StockDateText =
                        CONVERT(NVARCHAR(10), vs.StockDate, 120),
                    VehicleNumber = ISNULL(v.VehicleNumber, '-'),
                    ProductName = ISNULL(p.ProductName, '-'),
                    VariantName = ISNULL(vr.VariantName, '-'),
                    vs.LoadedPackets,
                    vs.SoldPackets,
                    vs.ReturnedPackets,
                    vs.DamagedPackets,
                    vs.ActualClosingPackets,
                    vs.IsReconciled
                FROM VehicleStock vs
                LEFT JOIN Vehicles v
                    ON v.VehicleId = vs.VehicleId
                LEFT JOIN ProductVariants vr
                    ON vr.ProductVariantId = vs.ProductVariantId
                LEFT JOIN Products p
                    ON p.ProductId = vr.ProductId
                WHERE vs.SalesmanId = @SalesmanId
                  AND
                  (
                    ISNULL(v.VehicleNumber, '') LIKE @Search
                    OR ISNULL(p.ProductName, '') LIKE @Search
                    OR ISNULL(vr.VariantName, '') LIKE @Search
                  )
                  AND (@HasFrom = 0 OR vs.StockDate >= @From)
                  AND (@HasTo = 0 OR vs.StockDate <= @To)
                  AND
                  (
                    @Reconciled = ''
                    OR CONVERT(NVARCHAR(10), vs.IsReconciled) =
                       @Reconciled
                  )
                ORDER BY vs.StockDate DESC, vs.VehicleStockId DESC";

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

                    cmd.Parameters.Add(
                        "@HasFrom",
                        SqlDbType.Int).Value =
                        hasFrom ? 1 : 0;

                    cmd.Parameters.Add(
                        "@From",
                        SqlDbType.Date).Value =
                        hasFrom ? (object)fromDate : DBNull.Value;

                    cmd.Parameters.Add(
                        "@HasTo",
                        SqlDbType.Int).Value =
                        hasTo ? 1 : 0;

                    cmd.Parameters.Add(
                        "@To",
                        SqlDbType.Date).Value =
                        hasTo ? (object)toDate : DBNull.Value;

                    cmd.Parameters.AddWithValue(
                        "@Reconciled",
                        reconciled);

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            dt.Columns.Add("VarianceText", typeof(string));

            foreach (DataRow row in dt.Rows)
            {
                row["VarianceText"] = VarianceOf(row);
            }

            lblRecordCount.Text =
                "records: " + dt.Rows.Count;

            gvStock.DataSource = dt;
            gvStock.DataBind();
        }


        // Variance only means something once a closing count
        // has been entered, so '-' is shown until then.
        private static string VarianceOf(DataRow row)
        {
            if (row["ActualClosingPackets"] == null ||
                row["ActualClosingPackets"] == DBNull.Value)
            {
                return "-";
            }

            int loaded =
                Convert.ToInt32(row["LoadedPackets"]);

            int sold =
                Convert.ToInt32(row["SoldPackets"]);

            int returned =
                Convert.ToInt32(row["ReturnedPackets"]);

            int damaged =
                Convert.ToInt32(row["DamagedPackets"]);

            int actual =
                Convert.ToInt32(row["ActualClosingPackets"]);

            int variance =
                actual - (loaded - sold - returned - damaged);

            if (variance == 0)
            {
                return "<span class=\"badge bg-success\">0 OK</span>";
            }

            if (variance < 0)
            {
                return "<span class=\"badge bg-danger\">" +
                       variance +
                       " short</span>";
            }

            return "<span class=\"badge bg-warning text-dark\">+" +
                   variance +
                   " extra</span>";
        }


        protected void FilterChanged(
            object sender,
            EventArgs e)
        {
            LoadStock();
        }
    }
}
