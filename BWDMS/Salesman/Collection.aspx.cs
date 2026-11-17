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

        private int DealerId
        {
            get { return Convert.ToInt32(Session["DealerId"]); }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddYears(-1));
            Response.Cache.SetRevalidation(System.Web.HttpCacheRevalidation.AllCaches);


            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Account/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (Session["UserRole"] == null ||
                !Session["UserRole"].ToString().Equals("Salesman", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect(BWDMS.Data.AppAuth.HomeUrl(Session["UserRole"]), false);
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
                INNER JOIN Shops s ON s.VillageId = v.VillageId
                WHERE v.IsActive = 1
                  AND s.IsActive = 1
                  AND (
                    s.RouteScheduleId IN (
                        SELECT rs2.RouteScheduleId FROM RouteSchedules rs2 WHERE rs2.SalesmanId = @SalesmanId
                    )
                    OR s.RouteId IN (
                        SELECT rs3.RouteId FROM RouteSchedules rs3 WHERE rs3.SalesmanId = @SalesmanId
                    )
                  )
                ORDER BY v.VillageName";

            using (SqlConnection con = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.Add("@SalesmanId", SqlDbType.Int).Value = SalesmanId;

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    ddlVillage.DataSource = dt;
                    ddlVillage.DataTextField = "VillageName";
                    ddlVillage.DataValueField = "VillageId";
                    ddlVillage.DataBind();
                }
            }

            ddlVillage.Items.Insert(0, new ListItem("All Villages", ""));
        }


        private void LoadShops()
        {
            string search = txtSearch.Text.Trim();
            string villageId = ddlVillage.SelectedValue;
            string filter = ddlFilter.SelectedValue;

            string query = @"
                SELECT
                    s.ShopId,
                    s.ShopCode,
                    s.ShopName,
                    ISNULL(s.OwnerName, '') AS OwnerName,
                    ISNULL(s.Phone, '') AS Phone,
                    VillageName = ISNULL(v.VillageName, '-'),
                    VisitText = ISNULL(
                        CASE
                            WHEN rs.DayOfWeek IS NOT NULL THEN rs.DayOfWeek
                            ELSE r.RouteName
                        END, '-'),
                    s.OpeningBalance,
                    s.CreditLimit,
                    -- Total sales (excluding cancelled/returned)
                    ISNULL(Sales.TotalSales, 0) AS TotalSales,
                    -- Total collections
                    ISNULL(Coll.TotalCollections, 0) AS TotalCollections,
                    -- Today's collections
                    ISNULL(TodayColl.TodayAmount, 0) AS TodayCollection,
                    -- Outstanding = OpeningBalance + TotalSales - TotalCollections
                    (s.OpeningBalance + ISNULL(Sales.TotalSales, 0) - ISNULL(Coll.TotalCollections, 0)) AS OutstandingBalance
                FROM Shops s
                LEFT JOIN Villages v ON v.VillageId = s.VillageId
                LEFT JOIN Routes r ON r.RouteId = s.RouteId
                LEFT JOIN RouteSchedules rs ON rs.RouteScheduleId = s.RouteScheduleId
                OUTER APPLY (
                    SELECT SUM(o.GrandTotal) AS TotalSales
                    FROM Orders o
                    WHERE o.ShopId = s.ShopId
                      AND o.Status NOT IN ('Cancelled', 'Returned')
                ) Sales
                OUTER APPLY (
                    SELECT SUM(c.Amount) AS TotalCollections
                    FROM Collections c
                    WHERE c.ShopId = s.ShopId
                ) Coll
                OUTER APPLY (
                    SELECT SUM(c.Amount) AS TodayAmount
                    FROM Collections c
                    WHERE c.ShopId = s.ShopId
                      AND c.CollectionDate = CAST(GETDATE() AS DATE)
                ) TodayColl
                WHERE s.IsActive = 1
                  AND (
                    s.RouteScheduleId IN (
                        SELECT rs2.RouteScheduleId FROM RouteSchedules rs2 WHERE rs2.SalesmanId = @SalesmanId
                    )
                    OR s.RouteId IN (
                        SELECT rs3.RouteId FROM RouteSchedules rs3 WHERE rs3.SalesmanId = @SalesmanId
                    )
                  )
                  AND (
                    s.ShopName LIKE @Search
                    OR ISNULL(s.ShopCode, '') LIKE @Search
                    OR ISNULL(s.OwnerName, '') LIKE @Search
                  )
                  AND (@VillageId = '' OR s.VillageId = @VillageId)";

            // Apply filter
            switch (filter)
            {
                case "outstanding":
                    query += " AND (s.OpeningBalance + ISNULL(Sales.TotalSales, 0) - ISNULL(Coll.TotalCollections, 0)) > 0";
                    break;
                case "overlimit":
                    query += " AND (s.OpeningBalance + ISNULL(Sales.TotalSales, 0) - ISNULL(Coll.TotalCollections, 0)) > ISNULL(s.CreditLimit, 999999999)";
                    break;
                case "collectedtoday":
                    query += " AND ISNULL(TodayColl.TodayAmount, 0) > 0";
                    break;
            }

            query += " ORDER BY OutstandingBalance DESC, VillageName, s.ShopName";

            DataTable dt = new DataTable();

            using (SqlConnection con = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.Add("@SalesmanId", SqlDbType.Int).Value = SalesmanId;
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
                cmd.Parameters.AddWithValue("@VillageId", villageId);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            // Compute summary
            int shopCount = dt.Rows.Count;
            decimal totalOutstanding = 0m;
            decimal todayCollections = 0m;

            foreach (DataRow row in dt.Rows)
            {
                totalOutstanding += row["OutstandingBalance"] == DBNull.Value
                    ? 0m : Convert.ToDecimal(row["OutstandingBalance"]);

                todayCollections += row["TodayCollection"] == DBNull.Value
                    ? 0m : Convert.ToDecimal(row["TodayCollection"]);
            }

            lblShopCount.Text = shopCount.ToString();
            lblTotalOutstanding.Text = totalOutstanding.ToString("N2");
            lblTodayCollections.Text = todayCollections.ToString("N2");

            gvShops.DataSource = dt;
            gvShops.DataBind();
        }


        protected void txtSearch_TextChanged(object sender, EventArgs e)
        {
            gvShops.PageIndex = 0;
            LoadShops();
        }


        protected void FilterChanged(object sender, EventArgs e)
        {
            gvShops.PageIndex = 0;
            LoadShops();
        }


        protected void gvShops_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvShops.PageIndex = e.NewPageIndex;
            LoadShops();
        }


        protected void gvShops_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "RecordCollection")
            {
                int shopId = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = (GridViewRow)((Control)e.CommandSource).NamingContainer;

                TextBox txtAmount = (TextBox)row.FindControl("txtCollectionAmount");
                DropDownList ddlMode = (DropDownList)row.FindControl("ddlPaymentMode");
                TextBox txtRef = (TextBox)row.FindControl("txtReference");

                if (txtAmount == null || string.IsNullOrWhiteSpace(txtAmount.Text))
                {
                    ShowMessage("Please enter a collection amount.", false);
                    return;
                }

                decimal amount;
                if (!decimal.TryParse(txtAmount.Text, out amount) || amount <= 0)
                {
                    ShowMessage("Please enter a valid positive amount.", false);
                    return;
                }

                string paymentMode = ddlMode.SelectedValue;
                string reference = txtRef?.Text?.Trim() ?? "";

                RecordCollection(shopId, amount, paymentMode, reference);

                // Clear inputs
                txtAmount.Text = "";
                txtRef.Text = "";

                LoadShops(); // Refresh grid
            }
        }


        private void RecordCollection(int shopId, decimal amount, string paymentMode, string reference)
        {
            // Verify shop belongs to this salesman
            string verifyQuery = @"
                SELECT COUNT(*) FROM Shops s
                WHERE s.ShopId = @ShopId
                  AND s.IsActive = 1
                  AND (
                    s.RouteScheduleId IN (SELECT RouteScheduleId FROM RouteSchedules WHERE SalesmanId = @SalesmanId)
                    OR s.RouteId IN (SELECT RouteId FROM RouteSchedules WHERE SalesmanId = @SalesmanId)
                  )";

            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                con.Open();

                // Verify ownership
                using (SqlCommand verifyCmd = new SqlCommand(verifyQuery, con))
                {
                    verifyCmd.Parameters.Add("@ShopId", SqlDbType.Int).Value = shopId;
                    verifyCmd.Parameters.Add("@SalesmanId", SqlDbType.Int).Value = SalesmanId;

                    int count = (int)verifyCmd.ExecuteScalar();
                    if (count == 0)
                    {
                        ShowMessage("You are not authorized to collect from this shop.", false);
                        return;
                    }
                }

                // Insert collection record
                string insertQuery = @"
                    INSERT INTO Collections (ShopId, SalesmanId, DealerId, CollectionDate, CollectionTime, Amount, PaymentMode, ReferenceNumber, Notes, CreatedAt)
                    VALUES (@ShopId, @SalesmanId, @DealerId, @Date, @Time, @Amount, @PaymentMode, @Reference, @Notes, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                {
                    cmd.Parameters.Add("@ShopId", SqlDbType.Int).Value = shopId;
                    cmd.Parameters.Add("@SalesmanId", SqlDbType.Int).Value = SalesmanId;
                    cmd.Parameters.Add("@DealerId", SqlDbType.Int).Value = DealerId;
                    cmd.Parameters.Add("@Date", SqlDbType.Date).Value = DateTime.Today;
                    cmd.Parameters.Add("@Time", SqlDbType.Time).Value = DateTime.Now.TimeOfDay;
                    cmd.Parameters.Add("@Amount", SqlDbType.Decimal).Value = amount;
                    cmd.Parameters.Add("@PaymentMode", SqlDbType.NVarChar, 20).Value = paymentMode;
                    cmd.Parameters.Add("@Reference", SqlDbType.NVarChar, 100).Value = (object)reference ?? DBNull.Value;
                    cmd.Parameters.Add("@Notes", SqlDbType.NVarChar, 500).Value = DBNull.Value; // Could add a notes field later

                    cmd.ExecuteNonQuery();
                }
            }

            ShowMessage($"Collection of ₹{amount:N2} recorded successfully!", true);
        }


        private void ShowMessage(string message, bool success)
        {
            lblMessage.Visible = true;
            lblMessage.Text = message;
            lblMessage.CssClass = success ? "alert alert-success d-block" : "alert alert-danger d-block";
        }
    }
}