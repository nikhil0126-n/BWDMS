using System;
using System.Data;
using System.Data.SqlClient;
using BWDMS.Data;

namespace BWDMS.Salesman
{
    // ============================================================
    // SHOP DETAILS
    //
    // ~/Salesman/ShopDetails.aspx?id=N shows one shop that sits on
    // this salesman's routes together with this month's summary.
    // The shop is always re-checked against the salesman's routes,
    // so a forged ?id= can never expose another route's shop.
    // ============================================================

    public partial class ShopDetails : System.Web.UI.Page
    {
        // Salesman's own id lives in UserId.
        private int SalesmanId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }

        // ?id=<ShopId>; 0 when the value is missing or malformed.
        private int ShopId
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

        // True only after the shop has been proven to sit on this
        // salesman's routes. No other query runs while it is false.
        private bool shopLoaded;


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


            if (ShopId <= 0)
            {
                Response.Redirect("~/Salesman/Shops.aspx");

                return;
            }

            shopLoaded = LoadShopHeader();

            if (!shopLoaded)
            {
                Response.Redirect("~/Salesman/Shops.aspx?denied=1");

                return;
            }


            if (!IsPostBack)
            {
                LoadSummary();
                LoadCollectionHistory();
            }
        }


        // ============================================================
        // OWNERSHIP CHECK - runs before anything else is queried
        // ============================================================

        private bool LoadShopHeader()
        {
            string query = @"
                SELECT
                    s.ShopName,
                    s.ShopCode,
                    s.OwnerName,
                    s.Phone,
                    s.Address,
                    s.OpeningBalance,
                    s.CreditLimit,
                    VillageName = ISNULL(v.VillageName, '-'),
                    RouteName = ISNULL(
                        CASE
                            WHEN rs.DayOfWeek IS NOT NULL
                                THEN rs.DayOfWeek
                            ELSE r.RouteName
                        END, '-')
                FROM Shops s
                LEFT JOIN Villages v
                    ON v.VillageId = s.VillageId
                LEFT JOIN Routes r
                    ON r.RouteId = s.RouteId
                LEFT JOIN RouteSchedules rs
                    ON rs.RouteScheduleId = s.RouteScheduleId
                WHERE s.ShopId = @ShopId
                  AND s.IsActive = 1
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
                  )";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@ShopId",
                        SqlDbType.Int).Value = ShopId;

                    cmd.Parameters.Add(
                        "@SalesmanId",
                        SqlDbType.Int).Value = SalesmanId;

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return false;
                        }

                        lblShopName.Text =
                            TextOrDash(reader["ShopName"]);

                        // Small line under the name - shop code,
                        // falling back to the route / visit day.
                        lblCode.Text =
                            reader["ShopCode"] == DBNull.Value ||
                            string.IsNullOrWhiteSpace(
                                reader["ShopCode"].ToString())
                                ? reader["RouteName"].ToString()
                                : reader["ShopCode"].ToString();

                        lblOwnerName.Text =
                            TextOrDash(reader["OwnerName"]);

                        lblPhone.Text =
                            TextOrDash(reader["Phone"]);

                        lblAddress.Text =
                            TextOrDash(reader["Address"]);

                        lblVillage.Text =
                            reader["VillageName"].ToString();

                        lblOpeningBalance.Text =
                            "₹" + Convert.ToDecimal(
                                reader["OpeningBalance"])
                                .ToString("N0");

                        lblCreditLimit.Text =
                            reader["CreditLimit"] == DBNull.Value
                                ? "-"
                                : "₹" + Convert.ToDecimal(
                                      reader["CreditLimit"])
                                      .ToString("N0");

                        return true;
                    }
                }
            }
        }


        // ============================================================
        // THIS MONTH SUMMARY + VISIT STATE + LAST ORDER
        // ============================================================

        private void LoadSummary()
        {
            if (!shopLoaded)
            {
                return;
            }

            DateTime monthStart =
                new DateTime(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1);

            DateTime monthEnd = monthStart.AddMonths(1);

            DateTime visitCutOff = DateTime.Today.AddDays(-7);

            string query = @"
                SELECT
                    MonthOrders =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM Orders o
                             WHERE o.ShopId = @ShopId
                               AND o.OrderDate >= @From
                               AND o.OrderDate < @To),
                            0),
                    MonthSales =
                        ISNULL(
                            (SELECT SUM(o.GrandTotal)
                             FROM Orders o
                             WHERE o.ShopId = @ShopId
                               AND o.OrderDate >= @From
                               AND o.OrderDate < @To
                               AND o.Status NOT IN
                                   ('Cancelled', 'Returned')),
                            0),
                    AllSales =
                        ISNULL(
                            (SELECT SUM(o.GrandTotal)
                             FROM Orders o
                             WHERE o.ShopId = @ShopId
                               AND o.Status NOT IN
                                   ('Cancelled', 'Returned')),
                            0),
                    TotalCollections =
                        ISNULL(
                            (SELECT SUM(c.Amount)
                             FROM Collections c
                             WHERE c.ShopId = @ShopId),
                            0),
                    MonthCollections =
                        ISNULL(
                            (SELECT SUM(c.Amount)
                             FROM Collections c
                             WHERE c.ShopId = @ShopId
                               AND c.CollectionDate >= @From
                               AND c.CollectionDate < @To),
                            0),
                    OpenOrders =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM Orders o
                             WHERE o.ShopId = @ShopId
                               AND o.Status NOT IN
                                   ('Delivered',
                                    'Cancelled',
                                    'Returned')),
                            0),
                    LastOrder =
                        (SELECT MAX(o.OrderDate)
                         FROM Orders o
                         WHERE o.ShopId = @ShopId),
                    Visits =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM Orders o
                             WHERE o.ShopId = @ShopId
                               AND o.OrderDate >= @VisitCutOff),
                            0),
                    OpeningBalance = s.OpeningBalance,
                    CreditLimit = s.CreditLimit
                FROM Shops s
                WHERE s.ShopId = @ShopId";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@ShopId",
                        SqlDbType.Int).Value = ShopId;

                    cmd.Parameters.Add(
                        "@From",
                        SqlDbType.Date).Value = monthStart;

                    cmd.Parameters.Add(
                        "@To",
                        SqlDbType.Date).Value = monthEnd;

                    cmd.Parameters.Add(
                        "@VisitCutOff",
                        SqlDbType.Date).Value = visitCutOff;

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
                            "₹" + Convert.ToDecimal(
                                reader["MonthSales"])
                                .ToString("N0");

                        // Calculate outstanding: OpeningBalance + AllSales - TotalCollections
                        decimal openingBalance = Convert.ToDecimal(reader["OpeningBalance"]);
                        decimal allSales = Convert.ToDecimal(reader["AllSales"]);
                        decimal totalCollections = Convert.ToDecimal(reader["TotalCollections"]);
                        decimal outstanding = openingBalance + allSales - totalCollections;

                        lblOutstanding.Text =
                            "₹" + outstanding.ToString("N0");

                        // Show credit limit status
                        decimal creditLimit = reader["CreditLimit"] == DBNull.Value
                            ? decimal.MaxValue
                            : Convert.ToDecimal(reader["CreditLimit"]);

                        if (outstanding > creditLimit)
                        {
                            lblOutstanding.CssClass = "text-danger fw-bold";
                            lblCreditStatus.Text = "⚠ OVER CREDIT LIMIT";
                            lblCreditStatus.CssClass = "text-danger fw-bold";
                        }
                        else
                        {
                            lblOutstanding.CssClass = "text-success fw-bold";
                            lblCreditStatus.Text = "✓ Within Credit Limit";
                            lblCreditStatus.CssClass = "text-success";
                        }

                        // Month collections
                        decimal monthCollections = Convert.ToDecimal(reader["MonthCollections"]);
                        lblMonthCollections.Text = "₹" + monthCollections.ToString("N0");

                        lblOutstandingShops.Text =
                            Convert.ToInt32(
                                reader["OpenOrders"])
                                .ToString() + " Open";

                        lblLastOrder.Text =
                            reader["LastOrder"] == DBNull.Value
                                ? "-"
                                : Convert.ToDateTime(
                                      reader["LastOrder"])
                                      .ToString("dd MMM yyyy");

                        bool visited =
                            Convert.ToInt32(
                                reader["Visits"]) > 0;

                        lblVisit.Text =
                            visited ? "Visited" : "Not Visited";

                        visitWrap.Attributes["class"] = visited
                            ? "sd-visited"
                            : "sd-visited not-visited";
                    }
                }
            }
        }


        // ============================================================
        // LOAD COLLECTION HISTORY
        // ============================================================

        private void LoadCollectionHistory()
        {
            if (!shopLoaded)
            {
                return;
            }

            string query = @"
                SELECT TOP (20)
                    c.CollectionDate,
                    c.CollectionTime,
                    c.Amount,
                    c.PaymentMode,
                    c.ReferenceNumber,
                    c.Notes,
                    u.FullName AS CollectedBy
                FROM Collections c
                LEFT JOIN Users u ON u.UserId = c.SalesmanId
                WHERE c.ShopId = @ShopId
                ORDER BY c.CollectionDate DESC, c.CollectionTime DESC";

            DataTable dt = new DataTable();

            using (SqlConnection con = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.Add("@ShopId", SqlDbType.Int).Value = ShopId;

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            if (dt.Rows.Count > 0)
            {
                // Add formatted columns
                dt.Columns.Add("DateText", typeof(string));
                dt.Columns.Add("TimeText", typeof(string));
                dt.Columns.Add("AmountText", typeof(string));

                foreach (DataRow row in dt.Rows)
                {
                    row["DateText"] = Convert.ToDateTime(row["CollectionDate"]).ToString("dd MMM yyyy");
                    row["TimeText"] = row["CollectionTime"].ToString();
                    row["AmountText"] = "₹" + Convert.ToDecimal(row["Amount"]).ToString("N2");
                }

                gvCollectionHistory.DataSource = dt;
                gvCollectionHistory.DataBind();
                gvCollectionHistory.Visible = true;
                lblNoHistory.Visible = false;
            }
            else
            {
                gvCollectionHistory.Visible = false;
                lblNoHistory.Visible = true;
            }
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
    }
}