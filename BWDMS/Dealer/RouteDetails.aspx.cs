using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    // ============================================================
    // ROUTE DETAILS
    //
    // ~/Dealer/RouteDetails.aspx?id=N shows one route of the
    // logged-in dealer together with the shops assigned to it. The
    // route is always re-checked against the logged-in dealer, so a
    // forged ?id= can never expose another dealership's route.
    // ============================================================

    public partial class RouteDetails : System.Web.UI.Page
    {
        // Dealer's own id lives in UserId (there is no Session["DealerId"]).
        private int DealerId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }

        // ?id=<RouteId>; 0 when the value is missing or malformed.
        private int RouteId
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

        // True only after the route has been proven to belong to
        // this dealer. No other query runs while it is false.
        private bool routeLoaded;


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
                if (RouteId <= 0)
                {
                    Response.Redirect("~/Dealer/Routes.aspx");

                    return;
                }

                routeLoaded = LoadRouteHeader();

                if (!routeLoaded)
                {
                    Response.Redirect("~/Dealer/Routes.aspx");

                    return;
                }

                LoadShops();
            }
        }


        // ============================================================
        // OWNERSHIP CHECK - runs before anything else is queried
        // ============================================================

        private bool LoadRouteHeader()
        {
            string query = @"
                SELECT
                    r.RouteName,
                    r.RouteCode,
                    r.RouteType,
                    r.IsActive,
                    r.CreatedAt,
                    Days =
                        CASE
                            WHEN NULLIF(r.OrderDispatchDays,
                                        '') IS NULL
                                THEN ISNULL(
                                         NULLIF(r.DayOfWeek, ''),
                                         '-')
                            ELSE r.OrderDispatchDays
                        END,
                    SalesmanName = ISNULL(s.FullName, '-'),
                    VehicleNumber = ISNULL(v.VehicleNumber, '-'),
                    VillageCount =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM RouteVillages rv
                             WHERE rv.RouteId = r.RouteId
                               AND rv.IsActive = 1),
                            0)
                FROM Routes r
                LEFT JOIN Users s
                    ON s.UserId = r.PreferredSalesmanId
                LEFT JOIN Vehicles v
                    ON v.VehicleId = r.DefaultVehicleId
                WHERE r.RouteId = @RouteId
                  AND r.DealerId = @DealerId";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@RouteId",
                        SqlDbType.Int).Value = RouteId;

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

                        lblRouteName.Text =
                            reader["RouteName"].ToString();

                        bool isActive =
                            Convert.ToBoolean(reader["IsActive"]);

                        lblStatus.Text =
                            isActive ? "Active" : "Inactive";

                        lblStatus.CssClass = isActive
                            ? "badge bg-success"
                            : "badge bg-secondary";

                        lblRouteCode.Text =
                            TextOrDash(reader["RouteCode"]);

                        lblRouteType.Text =
                            TextOrDash(reader["RouteType"]);

                        lblDays.Text =
                            reader["Days"].ToString();

                        lblSalesman.Text =
                            reader["SalesmanName"].ToString();

                        lblVehicle.Text =
                            reader["VehicleNumber"].ToString();

                        lblVillageCount.Text =
                            Convert.ToInt32(
                                reader["VillageCount"])
                                .ToString();

                        lblCreatedAt.Text =
                            Convert.ToDateTime(
                                reader["CreatedAt"])
                                .ToString("dd-MM-yyyy");

                        return true;
                    }
                }
            }
        }


        // ============================================================
        // SHOPS IN THIS ROUTE
        //
        // A shop carries the route it belongs to directly in its own
        // RouteId column (Shops.RouteId -> Routes.RouteId), the same
        // link the shop list page uses.
        // ============================================================

        private void LoadShops()
        {
            if (!routeLoaded)
            {
                return;
            }

            string query = @"
                SELECT
                    s.ShopCode,
                    s.ShopName,
                    VillageName = ISNULL(v.VillageName, '-')
                FROM Shops s
                LEFT JOIN Villages v
                    ON v.VillageId = s.VillageId
                WHERE s.RouteId = @RouteId
                  AND s.DealerId = @DealerId
                ORDER BY s.ShopName";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@RouteId",
                        SqlDbType.Int).Value = RouteId;

                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        gvShops.DataSource = dt;
                        gvShops.DataBind();
                    }
                }
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

        protected void gvShops_PageIndexChanging(
            object sender,
            System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            gvShops.PageIndex = e.NewPageIndex;

            LoadShops();
        }

    }
}
