using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    // ============================================================
    // SALESMAN DETAILS
    //
    // ~/Dealer/SalesmanDetails.aspx?id=N shows one salesman of the
    // logged-in dealer together with this month's performance and
    // his top shops. The salesman is always re-checked against the
    // logged-in dealer, so a forged ?id= can never expose another
    // dealership's salesman.
    // ============================================================

    public partial class SalesmanDetails : System.Web.UI.Page
    {
        // Dealer's own id lives in UserId (there is no Session["DealerId"]).
        private int DealerId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }

        // ?id=<UserId>; 0 when the value is missing or malformed.
        private int SalesmanId
        {
            get
            {
                int id;

                if (int.TryParse(Request.QueryString["id"], out id))
                {
                    return id;
                }

                return 0;
            }
        }

        // True only after the salesman has been proven to belong to
        // this dealer. No other query runs while it is false.
        private bool salesmanLoaded;


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


            if (SalesmanId <= 0)
            {
                Response.Redirect("~/Dealer/Salesmen.aspx");

                return;
            }

            salesmanLoaded = LoadSalesmanHeader();

            if (!salesmanLoaded)
            {
                Response.Redirect("~/Dealer/Salesmen.aspx");

                return;
            }


            if (!IsPostBack)
            {
                LoadPerformance();

                LoadTopShops();
            }
        }


        // ============================================================
        // OWNERSHIP CHECK - runs before anything else is queried
        //
        // Users has no code / address / village column, so only the
        // columns that really exist are shown. The assigned vehicle
        // lives on RouteSchedules (the salesman <-> vehicle link).
        // ============================================================

        private bool LoadSalesmanHeader()
        {
            string query = @"
                SELECT
                    u.FullName,
                    u.Email,
                    u.Phone,
                    u.IsActive,
                    u.CreatedAt,
                    AssignedVehicle =
                        ISNULL(
                            NULLIF(
                                STUFF(
                                    (SELECT ', ' + v.VehicleNumber
                                     FROM RouteSchedules rs
                                     INNER JOIN Vehicles v
                                         ON v.VehicleId =
                                            rs.VehicleId
                                     WHERE rs.SalesmanId =
                                           @SalesmanId
                                     GROUP BY v.VehicleNumber
                                     FOR XML PATH(''), TYPE)
                                        .value('.',
                                            'NVARCHAR(MAX)'),
                                    1, 2, ''),
                                ''),
                            '-')
                FROM Users u
                WHERE u.UserId = @SalesmanId
                  AND u.DealerId = @DealerId
                  AND u.Role = 'Salesman'";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@SalesmanId",
                        SqlDbType.Int).Value = SalesmanId;

                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return false;
                        }

                        lblSalesmanName.Text =
                            reader["FullName"].ToString();

                        bool isActive =
                            Convert.ToBoolean(reader["IsActive"]);

                        lblStatus.Text =
                            isActive ? "Active" : "Inactive";

                        lblStatus.CssClass = isActive
                            ? "badge bg-success"
                            : "badge bg-secondary";

                        lblStatusText.Text =
                            isActive ? "Active" : "Inactive";

                        lblPhone.Text =
                            TextOrDash(reader["Phone"]);

                        lblEmail.Text =
                            TextOrDash(reader["Email"]);

                        lblVehicle.Text =
                            reader["AssignedVehicle"].ToString();

                        lblJoiningDate.Text =
                            Convert.ToDateTime(
                                reader["CreatedAt"])
                                .ToString("dd-MM-yyyy");

                        return true;
                    }
                }
            }
        }


        // ============================================================
        // PERFORMANCE - THIS MONTH
        // No payments table exists in this schema, so "Outstanding"
        // and "Collections" have no source; the tiles shown are the
        // real aggregates that do exist.
        // ============================================================

        private void LoadPerformance()
        {
            if (!salesmanLoaded)
            {
                return;
            }

            DateTime monthStart =
                new DateTime(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1);

            DateTime monthEnd = monthStart.AddMonths(1);

            string query = @"
                SELECT
                    MonthOrders =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM Orders o
                             WHERE o.SalesmanId = @SalesmanId
                               AND o.DealerId = @DealerId
                               AND o.Status <> 'Cancelled'
                               AND o.OrderDate >= @From
                               AND o.OrderDate < @To),
                            0),
                    MonthSales =
                        ISNULL(
                            (SELECT SUM(o.GrandTotal)
                             FROM Orders o
                             WHERE o.SalesmanId = @SalesmanId
                               AND o.DealerId = @DealerId
                               AND o.Status <> 'Cancelled'
                               AND o.OrderDate >= @From
                               AND o.OrderDate < @To),
                            0),
                    MonthPackets =
                        ISNULL(
                            (SELECT SUM(vs.SoldPackets)
                             FROM VehicleStock vs
                             WHERE vs.SalesmanId = @SalesmanId
                               AND vs.DealerId = @DealerId
                               AND vs.StockDate >= @From
                               AND vs.StockDate < @To),
                            0),
                    MonthDispatches =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM Dispatches d
                             WHERE d.SalesmanId = @SalesmanId
                               AND d.DealerId = @DealerId
                               AND d.Status <> 'Cancelled'
                               AND d.DispatchDate >= @From
                               AND d.DispatchDate < @To),
                            0)";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@SalesmanId",
                        SqlDbType.Int).Value = SalesmanId;

                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    cmd.Parameters.Add(
                        "@From",
                        SqlDbType.Date).Value = monthStart;

                    cmd.Parameters.Add(
                        "@To",
                        SqlDbType.Date).Value = monthEnd;

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return;
                        }

                        lblTotalOrders.Text =
                            Convert.ToInt32(
                                reader["MonthOrders"])
                                .ToString();

                        lblTotalSales.Text =
                            Convert.ToDecimal(
                                reader["MonthSales"])
                                .ToString("N2");

                        lblPacketsSold.Text =
                            Convert.ToInt32(
                                reader["MonthPackets"])
                                .ToString();

                        lblDispatches.Text =
                            Convert.ToInt32(
                                reader["MonthDispatches"])
                                .ToString();
                    }
                }
            }
        }


        // ============================================================
        // TOP SHOPS - the salesman's shops by order value, highest
        // first. Cancelled orders never count as value.
        // ============================================================

        private void LoadTopShops()
        {
            if (!salesmanLoaded)
            {
                return;
            }

            string query = @"
                SELECT TOP 10
                    s.ShopName,
                    OrderCount = COUNT(o.OrderId),
                    TotalValue = SUM(o.GrandTotal)
                FROM Orders o
                INNER JOIN Shops s
                    ON s.ShopId = o.ShopId
                WHERE o.SalesmanId = @SalesmanId
                  AND o.DealerId = @DealerId
                  AND s.DealerId = @DealerId
                  AND o.Status <> 'Cancelled'
                GROUP BY s.ShopId, s.ShopName
                ORDER BY TotalValue DESC";

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

            if (!dt.Columns.Contains("TotalText"))
            {
                dt.Columns.Add(
                    "TotalText",
                    typeof(string));
            }

            foreach (DataRow row in dt.Rows)
            {
                row["TotalText"] =
                    Convert.ToDecimal(row["TotalValue"])
                        .ToString("N2");
            }

            gvTopShops.DataSource = dt;
            gvTopShops.DataBind();
        }


        private static string TextOrDash(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return "-";
            }

            string text = value.ToString();

            return string.IsNullOrWhiteSpace(text) ? "-" : text;
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

        // ------------------------------------------------------------
        // PAGING
        //
        // The new index has to be applied before the grid is rebound,
        // otherwise the postback lands back on the page it came from.
        // ------------------------------------------------------------

        protected void gvTopShops_PageIndexChanging(
            object sender,
            System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            gvTopShops.PageIndex = e.NewPageIndex;

            LoadTopShops();
        }

    }
}
