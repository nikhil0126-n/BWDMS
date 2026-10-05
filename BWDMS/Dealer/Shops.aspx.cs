using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class Shops : System.Web.UI.Page
    {
        // Dealer's own id lives in UserId (there is no Session["DealerId"]).
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
                LoadVillageFilter();
                LoadShops();

                if (Request.QueryString["saved"] == "1")
                {
                    ShowMessage(
                        "Shop saved successfully.",
                        true);
                }
            }
        }


        private void LoadVillageFilter()
        {
            string query = @"
                SELECT VillageId, VillageName
                FROM Villages
                WHERE IsActive = 1
                ORDER BY VillageName";

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

                        ddlVillageFilter.DataSource = dt;
                        ddlVillageFilter.DataTextField = "VillageName";
                        ddlVillageFilter.DataValueField = "VillageId";
                        ddlVillageFilter.DataBind();

                        ddlVillageFilter.Items.Insert(
                            0,
                            new ListItem("All Villages", ""));
                    }
                }
            }
        }


        private void LoadShops()
        {
            string search = txtSearch.Text.Trim();
            string villageId = ddlVillageFilter.SelectedValue;
            string status = ddlStatusFilter.SelectedValue;

            string query = @"
                SELECT
                    s.ShopId,
                    s.ShopCode,
                    s.ShopName,
                    s.OwnerName,
                    s.Phone,
                    s.IsActive,
                    s.OpeningBalance,
                    VillageName = ISNULL(v.VillageName, '-'),
                    RouteName = ISNULL(
                        CASE
                            WHEN r.RouteCode IS NULL
                                 OR r.RouteCode = '' THEN r.RouteName
                            ELSE r.RouteCode + ' - ' + r.RouteName
                        END, '-'),
                    OpeningBalanceText =
                        CASE
                            WHEN s.OpeningBalance IS NULL THEN '-'
                            ELSE CONVERT(NVARCHAR(20), s.OpeningBalance)
                        END,
                    OrderCount =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM Orders o
                             WHERE o.ShopId = s.ShopId
                               AND o.DealerId = s.DealerId),
                            0)
                FROM Shops s
                LEFT JOIN Villages v
                    ON v.VillageId = s.VillageId
                LEFT JOIN Routes r
                    ON r.RouteId = s.RouteId
                WHERE s.DealerId = @DealerId
                  AND
                  (
                    s.ShopName LIKE @Search
                    OR ISNULL(s.ShopCode, '') LIKE @Search
                    OR ISNULL(s.OwnerName, '') LIKE @Search
                    OR ISNULL(s.Phone, '') LIKE @Search
                  )
                  AND (@VillageId = ''
                       OR CONVERT(NVARCHAR(20), s.VillageId) = @VillageId)
                  AND (@Status = ''
                       OR CONVERT(NVARCHAR(5), s.IsActive) = @Status)
                ORDER BY s.ShopName";

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
                        "@VillageId",
                        villageId);

                    cmd.Parameters.AddWithValue(
                        "@Status",
                        status);

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


        protected void txtSearch_TextChanged(
            object sender,
            EventArgs e)
        {
            LoadShops();
        }


        protected void FilterChanged(
            object sender,
            EventArgs e)
        {
            LoadShops();
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
