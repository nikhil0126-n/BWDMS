using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class DispatchDetails : System.Web.UI.Page
    {
        private int DealerId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }

        private int DispatchId
        {
            get
            {
                int id;

                if (int.TryParse(
                    Request.QueryString["id"],
                    out id))
                {
                    return id;
                }

                return 0;
            }
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
                LoadDispatch();
            }
        }


        // ============================================================
        // DISPATCH
        // ============================================================

        private void LoadDispatch()
        {
            if (DispatchId <= 0 || !LoadHeader())
            {
                // DealerId on the header is what stops a crafted
                // ?id= from reading another dealership's dispatch.
                pnlDispatch.Visible = false;

                ShowMessage(
                    "Dispatch not found.",
                    false);

                return;
            }

            LoadLines();
            LoadOrders();
        }


        private bool LoadHeader()
        {
            string query = @"
                SELECT
                    d.DispatchNumber,
                    DispatchDateText =
                        CONVERT(NVARCHAR(10), d.DispatchDate, 120),
                    CreatedAtText =
                        CONVERT(NVARCHAR(16), d.CreatedAt, 120),
                    d.Status,
                    Remarks =
                        ISNULL(d.Remarks, ''),
                    VehicleNumber =
                        ISNULL(v.VehicleNumber, '-'),
                    SalesmanName =
                        ISNULL(u.FullName, '-'),
                    RouteLabel =
                        ISNULL(
                            r.RouteName + ' - ' + rs.DayOfWeek,
                            '-')
                FROM Dispatches d
                LEFT JOIN Vehicles v
                    ON v.VehicleId = d.VehicleId
                LEFT JOIN Users u
                    ON u.UserId = d.SalesmanId
                LEFT JOIN RouteSchedules rs
                    ON rs.RouteScheduleId = d.RouteScheduleId
                LEFT JOIN Routes r
                    ON r.RouteId = rs.RouteId
                WHERE d.DispatchId = @DispatchId
                  AND d.DealerId = @DealerId";

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@DispatchId",
                        SqlDbType.Int).Value = DispatchId;

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

            if (dt.Rows.Count == 0)
            {
                return false;
            }

            DataRow row = dt.Rows[0];

            lblDispatchNumber.Text =
                row["DispatchNumber"].ToString();

            lblDispatchDate.Text =
                row["DispatchDateText"].ToString();

            lblVehicle.Text = row["VehicleNumber"].ToString();

            lblSalesman.Text = row["SalesmanName"].ToString();

            lblRouteSchedule.Text = row["RouteLabel"].ToString();

            lblCreatedAt.Text = row["CreatedAtText"].ToString();

            lblRemarks.Text =
                string.IsNullOrEmpty(row["Remarks"].ToString())
                    ? "-"
                    : row["Remarks"].ToString();

            string status = row["Status"].ToString();

            lblStatus.Text = status;
            lblStatus.CssClass = Dispatches.StatusCss(status);

            return true;
        }


        private void LoadLines()
        {
            string query = @"
                SELECT
                    p.ProductName,
                    v.VariantName,
                    u.UnitCode,
                    dd.Quantity,
                    dd.QuantityPackets
                FROM DispatchDetails dd
                INNER JOIN ProductVariants v
                    ON v.ProductVariantId = dd.ProductVariantId
                INNER JOIN Products p
                    ON p.ProductId = v.ProductId
                INNER JOIN SellingUnits u
                    ON u.UnitId = dd.UnitId
                WHERE dd.DispatchId = @DispatchId
                ORDER BY dd.DispatchDetailId";

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@DispatchId",
                        SqlDbType.Int).Value = DispatchId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            int totalQty = 0;
            int totalPackets = 0;

            foreach (DataRow row in dt.Rows)
            {
                totalQty +=
                    Convert.ToInt32(row["Quantity"]);

                totalPackets +=
                    Convert.ToInt32(row["QuantityPackets"]);
            }

            lblTotalQty.Text = totalQty.ToString();
            lblTotalPackets.Text = totalPackets.ToString();

            gvLines.DataSource = dt;
            gvLines.DataBind();
        }


        private void LoadOrders()
        {
            string query = @"
                SELECT DISTINCT
                    o.OrderNumber,
                    s.ShopName,
                    o.Status,
                    o.GrandTotal
                FROM DispatchDetails dd
                INNER JOIN Orders o
                    ON o.OrderId = dd.OrderId
                INNER JOIN Shops s
                    ON s.ShopId = o.ShopId
                WHERE dd.DispatchId = @DispatchId
                ORDER BY o.OrderNumber";

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@DispatchId",
                        SqlDbType.Int).Value = DispatchId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            if (!dt.Columns.Contains("GrandTotalText"))
            {
                dt.Columns.Add(
                    "GrandTotalText",
                    typeof(string));
            }

            foreach (DataRow row in dt.Rows)
            {
                row["GrandTotalText"] =
                    Convert.ToDecimal(row["GrandTotal"])
                        .ToString("N2");
            }

            gvOrders.DataSource = dt;
            gvOrders.DataBind();
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
