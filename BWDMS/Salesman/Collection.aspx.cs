using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Salesman
{
    public partial class Collection : System.Web.UI.Page
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
                LoadVillages();
                LoadShops();
            }
        }


        private void LoadVillages()
        {
            string query = @"
                SELECT DISTINCT
                    v.VillageId,
                    v.VillageName
                FROM Villages v
                INNER JOIN Shops s
                    ON s.VillageId = v.VillageId
                WHERE v.IsActive = 1
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
                  )
                ORDER BY v.VillageName";

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

                        ddlVillage.DataSource = dt;
                        ddlVillage.DataTextField = "VillageName";
                        ddlVillage.DataValueField = "VillageId";
                        ddlVillage.DataBind();
                    }
                }
            }

            ddlVillage.Items.Insert(
                0,
                new ListItem("All Villages", ""));
        }


        private void LoadShops()
        {
            string search = txtSearch.Text.Trim();
            string villageId = ddlVillage.SelectedValue;

            // Same predicate as Salesman/Shops.aspx.cs: a shop is
            // visible when either its route or its schedule is ours.
            string query = @"
                SELECT
                    s.ShopCode,
                    s.ShopName,
                    ISNULL(s.OwnerName, '') AS OwnerName,
                    ISNULL(s.Phone, '') AS Phone,
                    VillageName = ISNULL(v.VillageName, '-'),
                    VisitText = ISNULL(
                        CASE
                            WHEN rs.DayOfWeek IS NOT NULL
                                THEN rs.DayOfWeek
                            ELSE r.RouteName
                        END, '-'),
                    s.OpeningBalance,
                    s.CreditLimit
                FROM Shops s
                LEFT JOIN Villages v
                    ON v.VillageId = s.VillageId
                LEFT JOIN Routes r
                    ON r.RouteId = s.RouteId
                LEFT JOIN RouteSchedules rs
                    ON rs.RouteScheduleId = s.RouteScheduleId
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
                  AND
                  (
                    s.ShopName LIKE @Search
                    OR ISNULL(s.ShopCode, '') LIKE @Search
                    OR ISNULL(s.OwnerName, '') LIKE @Search
                  )
                  AND (@VillageId = '' OR s.VillageId = @VillageId)
                ORDER BY VillageName, s.ShopName";

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

                    cmd.Parameters.AddWithValue(
                        "@VillageId",
                        villageId);

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            dt.Columns.Add(
                "OpeningBalanceText",
                typeof(string));

            dt.Columns.Add(
                "CreditLimitText",
                typeof(string));

            decimal balanceTotal = 0m;
            decimal creditTotal = 0m;

            foreach (DataRow row in dt.Rows)
            {
                decimal opening =
                    row["OpeningBalance"] == DBNull.Value
                        ? 0m
                        : Convert.ToDecimal(row["OpeningBalance"]);

                decimal credit =
                    row["CreditLimit"] == DBNull.Value
                        ? 0m
                        : Convert.ToDecimal(row["CreditLimit"]);

                row["OpeningBalanceText"] =
                    opening.ToString("N2");

                row["CreditLimitText"] =
                    credit.ToString("N2");

                balanceTotal += opening;
                creditTotal += credit;
            }

            lblShopCount.Text = dt.Rows.Count.ToString();
            lblBalanceTotal.Text = balanceTotal.ToString("N2");
            lblCreditTotal.Text = creditTotal.ToString("N2");

            gvShops.DataSource = dt;
            gvShops.DataBind();
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
    }
}
