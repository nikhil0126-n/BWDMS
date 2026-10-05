using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    // ============================================================
    // SHOP ORDER HISTORY
    //
    // ~/Dealer/ShopOrders.aspx?shopId=N lists every order taken
    // for one shop. The shop is always re-checked against the
    // logged-in dealer, so a forged ?shopId= can never expose
    // another dealership's shop or orders.
    // ============================================================

    public partial class ShopOrders : System.Web.UI.Page
    {
        private int DealerId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }

        private int ShopId
        {
            get
            {
                int id;

                if (int.TryParse(
                    Request.QueryString["shopId"],
                    out id))
                {
                    return id;
                }

                return 0;
            }
        }

        // True only after the shop has been proven to belong to
        // this dealer. No other query runs while it is false.
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
                    "Dealer",
                    StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect(
                    BWDMS.Data.AppAuth.HomeUrl(Session["UserRole"]),
                    false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }


            shopLoaded = LoadShopHeader();

            if (!shopLoaded)
            {
                pnlContent.Visible = false;

                return;
            }


            if (!IsPostBack)
            {
                LoadOrders();
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
                    s.DealerId,
                    VillageName = ISNULL(v.VillageName, '-'),
                    RouteName = ISNULL(
                        CASE
                            WHEN r.RouteCode IS NULL
                                 OR r.RouteCode = '' THEN r.RouteName
                            ELSE r.RouteCode + ' - ' + r.RouteName
                        END, '-')
                FROM Shops s
                LEFT JOIN Villages v
                    ON v.VillageId = s.VillageId
                LEFT JOIN Routes r
                    ON r.RouteId = s.RouteId
                WHERE s.ShopId = @ShopId
                  AND s.DealerId = @DealerId";

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
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            ShowMessage(
                                "Shop not found.",
                                false);

                            return false;
                        }

                        lblShopName.Text =
                            reader["ShopName"].ToString();

                        lblShopMeta.Text =
                            "Village: " +
                            reader["VillageName"] +
                            "  |  Route: " +
                            reader["RouteName"];

                        return true;
                    }
                }
            }
        }


        // ============================================================
        // ORDER LIST
        // ============================================================

        private void LoadOrders()
        {
            if (!shopLoaded)
            {
                return;
            }

            string search = txtSearch.Text.Trim();
            string status = ddlStatus.SelectedValue;

            string query = @"
                SELECT
                    o.OrderId,
                    o.OrderNumber,
                    o.OrderSource,
                    o.Status,
                    OrderDateText =
                        CONVERT(NVARCHAR(10), o.OrderDate, 120),
                    LineCount =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM OrderDetails d
                             WHERE d.OrderId = o.OrderId),
                            0),
                    o.GrandTotal
                FROM Orders o
                WHERE o.DealerId = @DealerId
                  AND o.ShopId = @ShopId
                  AND o.OrderNumber LIKE @Search
                  AND (@Status = '' OR o.Status = @Status)
                ORDER BY o.OrderDate DESC, o.OrderId DESC";

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    cmd.Parameters.Add(
                        "@ShopId",
                        SqlDbType.Int).Value = ShopId;

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
                dt.Columns.Add("GrandTotalText", typeof(string));
            }

            foreach (DataRow row in dt.Rows)
            {
                row["StatusCss"] = Orders.StatusCss(
                    row["Status"].ToString());

                row["GrandTotalText"] =
                    Convert.ToDecimal(row["GrandTotal"])
                        .ToString("N2");
            }

            gvOrders.DataSource = dt;
            gvOrders.DataBind();
        }


        protected void txtSearch_TextChanged(
            object sender,
            EventArgs e)
        {
            LoadOrders();
        }


        protected void FilterChanged(
            object sender,
            EventArgs e)
        {
            LoadOrders();
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
