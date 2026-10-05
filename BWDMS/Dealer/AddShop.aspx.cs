using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class AddShop : System.Web.UI.Page
    {
        // ============================================================
        // IDS
        // ============================================================

        private int ShopId
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
                LoadVillages();
                LoadRoutes();

                if (ShopId > 0)
                {
                    ShopRow row = LoadShop();

                    if (row == null)
                    {
                        Response.Redirect(
                            "~/Dealer/Shops.aspx",
                            false);

                        Context.ApplicationInstance.CompleteRequest();

                        return;
                    }

                    SetEditMode();

                    LoadSchedules(row.RouteId ?? 0);

                    SelectValue(
                        ddlRoute,
                        row.RouteId.HasValue
                            ? row.RouteId.Value.ToString()
                            : "");

                    SelectValue(
                        ddlSchedule,
                        row.RouteScheduleId.HasValue
                            ? row.RouteScheduleId.Value.ToString()
                            : "");
                }
                else
                {
                    SetAddMode();

                    LoadSchedules(0);
                }
            }
        }


        private void SetAddMode()
        {
            lblPageTitle.Text = "Add Shop";
            lblHeading.Text = "Add Shop";
            lblSubHeading.Text =
                "Register a retail shop served by your dealership";
            btnSave.Text = "Create Shop";
        }


        private void SetEditMode()
        {
            lblPageTitle.Text = "Edit Shop";
            lblHeading.Text = "Edit Shop";
            lblSubHeading.Text =
                "Update shop details for this dealership";
            btnSave.Text = "Update Shop";
        }


        private static void SelectValue(
            DropDownList dropdown,
            string value)
        {
            if (dropdown.Items.FindByValue(value) != null)
            {
                dropdown.SelectedValue = value;
            }
            else if (dropdown.Items.Count > 0)
            {
                dropdown.SelectedIndex = 0;
            }
        }


        // ============================================================
        // DROPDOWNS
        // ============================================================

        private void LoadVillages()
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

                        ddlVillage.DataSource = dt;
                        ddlVillage.DataTextField = "VillageName";
                        ddlVillage.DataValueField = "VillageId";
                        ddlVillage.DataBind();

                        ddlVillage.Items.Insert(
                            0,
                            new ListItem("Select Village", ""));
                    }
                }
            }
        }


        private void LoadRoutes()
        {
            string query = @"
                SELECT
                    RouteId,
                    RouteLabel =
                        CASE
                            WHEN RouteCode IS NULL
                                 OR RouteCode = '' THEN RouteName
                            ELSE RouteCode + ' - ' + RouteName
                        END
                FROM Routes
                WHERE DealerId = @DealerId
                  AND IsActive = 1
                ORDER BY RouteCode, RouteName";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        ddlRoute.DataSource = dt;
                        ddlRoute.DataTextField = "RouteLabel";
                        ddlRoute.DataValueField = "RouteId";
                        ddlRoute.DataBind();

                        ddlRoute.Items.Insert(
                            0,
                            new ListItem("No Route", ""));
                    }
                }
            }
        }


        // Schedules only appear once a route is chosen, so a shop
        // can never carry a schedule belonging to another route.
        private void LoadSchedules(int routeId)
        {
            ddlSchedule.Items.Clear();

            if (routeId <= 0)
            {
                ddlSchedule.Items.Add(
                    new ListItem("No Schedule", ""));

                return;
            }

            string query = @"
                SELECT
                    rs.RouteScheduleId,
                    ScheduleLabel =
                        rs.DayOfWeek
                        + ' ( #' + CONVERT(NVARCHAR(10), rs.RouteScheduleId) + ' )'
                FROM RouteSchedules rs
                INNER JOIN Routes r
                    ON r.RouteId = rs.RouteId
                WHERE rs.RouteId = @RouteId
                  AND r.DealerId = @DealerId
                  AND rs.IsActive = 1
                ORDER BY rs.RouteScheduleId";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@RouteId",
                        SqlDbType.Int).Value = routeId;

                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        ddlSchedule.DataSource = dt;
                        ddlSchedule.DataTextField = "ScheduleLabel";
                        ddlSchedule.DataValueField = "RouteScheduleId";
                        ddlSchedule.DataBind();

                        ddlSchedule.Items.Insert(
                            0,
                            new ListItem("No Schedule", ""));
                    }
                }
            }
        }


        protected void ddlRoute_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            int routeId = 0;

            int.TryParse(ddlRoute.SelectedValue, out routeId);

            LoadSchedules(routeId);
        }


        // ============================================================
        // LOAD (EDIT MODE)
        // ============================================================

        private sealed class ShopRow
        {
            public int? RouteId;
            public int? RouteScheduleId;
        }


        private ShopRow LoadShop()
        {
            string query = @"
                SELECT
                    ShopName,
                    ShopCode,
                    OwnerName,
                    Phone,
                    Email,
                    Address,
                    VillageId,
                    RouteId,
                    RouteScheduleId,
                    OpeningBalance,
                    CreditLimit,
                    IsActive
                FROM Shops
                WHERE ShopId = @ShopId
                  AND DealerId = @DealerId";

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
                            // Not found, or owned by another dealer.
                            return null;
                        }

                        txtShopName.Text =
                            reader["ShopName"].ToString();

                        txtShopCode.Text =
                            TextOrNull(reader["ShopCode"]);

                        txtOwnerName.Text =
                            TextOrNull(reader["OwnerName"]);

                        txtPhone.Text =
                            TextOrNull(reader["Phone"]);

                        txtEmail.Text =
                            TextOrNull(reader["Email"]);

                        txtAddress.Text =
                            TextOrNull(reader["Address"]);

                        SelectValue(
                            ddlVillage,
                            reader["VillageId"] == DBNull.Value
                                ? ""
                                : reader["VillageId"].ToString());

                        txtOpeningBalance.Text =
                            reader["OpeningBalance"] == DBNull.Value
                                ? ""
                                : Convert.ToDecimal(
                                    reader["OpeningBalance"],
                                    CultureInfo.InvariantCulture)
                                  .ToString(
                                      "0.##",
                                      CultureInfo.InvariantCulture);

                        txtCreditLimit.Text =
                            reader["CreditLimit"] == DBNull.Value
                                ? ""
                                : Convert.ToDecimal(
                                    reader["CreditLimit"],
                                    CultureInfo.InvariantCulture)
                                  .ToString(
                                      "0.##",
                                      CultureInfo.InvariantCulture);

                        ddlStatus.SelectedValue =
                            Convert.ToBoolean(reader["IsActive"])
                                ? "1"
                                : "0";

                        return new ShopRow
                        {
                            RouteId =
                                reader["RouteId"] == DBNull.Value
                                    ? (int?)null
                                    : Convert.ToInt32(
                                        reader["RouteId"]),

                            RouteScheduleId =
                                reader["RouteScheduleId"] == DBNull.Value
                                    ? (int?)null
                                    : Convert.ToInt32(
                                        reader["RouteScheduleId"])
                        };
                    }
                }
            }
        }


        private static string TextOrNull(object value)
        {
            return value == null || value == DBNull.Value
                ? ""
                : value.ToString();
        }


        // ============================================================
        // SAVE
        // ============================================================

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            string shopName = txtShopName.Text.Trim();
            string shopCode = txtShopCode.Text.Trim();
            string ownerName = txtOwnerName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string email = txtEmail.Text.Trim();
            string address = txtAddress.Text.Trim();

            int villageId = 0;

            if (!string.IsNullOrWhiteSpace(ddlVillage.SelectedValue))
            {
                int.TryParse(ddlVillage.SelectedValue, out villageId);
            }

            int routeId = 0;

            int.TryParse(ddlRoute.SelectedValue, out routeId);

            int scheduleId = 0;

            int.TryParse(ddlSchedule.SelectedValue, out scheduleId);

            // A schedule may only be stored when it belongs to the
            // chosen route.
            if (routeId <= 0)
            {
                scheduleId = 0;
            }

            decimal? openingBalance = ParseMoney(
                txtOpeningBalance.Text);

            if (txtOpeningBalance.Text.Trim().Length > 0 &&
                openingBalance.HasValue &&
                openingBalance.Value < 0)
            {
                ShowMessage(
                    "Opening balance cannot be negative.",
                    false);

                return;
            }

            decimal? creditLimit = ParseMoney(
                txtCreditLimit.Text);

            bool isActive = ddlStatus.SelectedValue == "1";

            bool isEdit = ShopId > 0;


            try
            {
                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    con.Open();

                    using (SqlTransaction tx =
                        con.BeginTransaction())
                    {
                        try
                        {
                            // ------------------------------------
                            // Soft duplicate check on shop code.
                            // Shops has no unique index, so this
                            // is application level only.
                            // ------------------------------------

                            if (!string.IsNullOrWhiteSpace(shopCode))
                            {
                                string check = @"
                                    SELECT COUNT(*)
                                    FROM Shops
                                    WHERE DealerId = @DealerId
                                      AND ShopCode = @ShopCode";

                                if (isEdit)
                                {
                                    check += " AND ShopId <> @ShopId";
                                }

                                using (SqlCommand cmd =
                                    new SqlCommand(
                                        check,
                                        con,
                                        tx))
                                {
                                    cmd.Parameters.Add(
                                        "@DealerId",
                                        SqlDbType.Int).Value =
                                        DealerId;

                                    cmd.Parameters.Add(
                                        "@ShopCode",
                                        SqlDbType.NVarChar,
                                        200).Value = shopCode;

                                    if (isEdit)
                                    {
                                        cmd.Parameters.Add(
                                            "@ShopId",
                                            SqlDbType.Int).Value =
                                            ShopId;
                                    }

                                    int count =
                                        Convert.ToInt32(
                                            cmd.ExecuteScalar());

                                    if (count > 0)
                                    {
                                        tx.Rollback();

                                        ShowMessage(
                                            "A shop with this code already exists.",
                                            false);

                                        return;
                                    }
                                }
                            }


                            if (isEdit)
                            {
                                UpdateShop(
                                    con,
                                    tx,
                                    shopName,
                                    shopCode,
                                    ownerName,
                                    phone,
                                    email,
                                    address,
                                    villageId,
                                    routeId,
                                    scheduleId,
                                    openingBalance,
                                    creditLimit,
                                    isActive);
                            }
                            else
                            {
                                InsertShop(
                                    con,
                                    tx,
                                    shopName,
                                    shopCode,
                                    ownerName,
                                    phone,
                                    email,
                                    address,
                                    villageId,
                                    routeId,
                                    scheduleId,
                                    openingBalance,
                                    creditLimit,
                                    isActive);
                            }

                            tx.Commit();
                        }
                        catch
                        {
                            tx.Rollback();
                            throw;
                        }
                    }
                }

                Response.Redirect(
                    "~/Dealer/Shops.aspx?saved=1");
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error saving shop: " + ex.Message,
                    false);
            }
        }


        private static decimal? ParseMoney(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            decimal value;

            if (decimal.TryParse(
                text,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out value))
            {
                return value;
            }

            return null;
        }


        private void InsertShop(
            SqlConnection con,
            SqlTransaction tx,
            string shopName,
            string shopCode,
            string ownerName,
            string phone,
            string email,
            string address,
            int villageId,
            int routeId,
            int scheduleId,
            decimal? openingBalance,
            decimal? creditLimit,
            bool isActive)
        {
            string query = @"
                INSERT INTO Shops
                (
                    DealerId,
                    ShopCode,
                    ShopName,
                    OwnerName,
                    Phone,
                    Email,
                    Address,
                    VillageId,
                    RouteId,
                    RouteScheduleId,
                    OpeningBalance,
                    CreditLimit,
                    IsActive,
                    CreatedBy,
                    CreatedAt
                )
                VALUES
                (
                    @DealerId,
                    @ShopCode,
                    @ShopName,
                    @OwnerName,
                    @Phone,
                    @Email,
                    @Address,
                    @VillageId,
                    @RouteId,
                    @RouteScheduleId,
                    @OpeningBalance,
                    @CreditLimit,
                    @IsActive,
                    @CreatedBy,
                    GETDATE()
                )";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                AddParameters(
                    cmd,
                    shopName,
                    shopCode,
                    ownerName,
                    phone,
                    email,
                    address,
                    villageId,
                    routeId,
                    scheduleId,
                    openingBalance,
                    creditLimit,
                    isActive);

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@CreatedBy",
                    SqlDbType.Int).Value = DealerId;

                cmd.ExecuteNonQuery();
            }
        }


        private void UpdateShop(
            SqlConnection con,
            SqlTransaction tx,
            string shopName,
            string shopCode,
            string ownerName,
            string phone,
            string email,
            string address,
            int villageId,
            int routeId,
            int scheduleId,
            decimal? openingBalance,
            decimal? creditLimit,
            bool isActive)
        {
            string query = @"
                UPDATE Shops
                SET ShopCode = @ShopCode,
                    ShopName = @ShopName,
                    OwnerName = @OwnerName,
                    Phone = @Phone,
                    Email = @Email,
                    Address = @Address,
                    VillageId = @VillageId,
                    RouteId = @RouteId,
                    RouteScheduleId = @RouteScheduleId,
                    OpeningBalance = @OpeningBalance,
                    CreditLimit = @CreditLimit,
                    IsActive = @IsActive,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE ShopId = @ShopId
                  AND DealerId = @DealerId";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                AddParameters(
                    cmd,
                    shopName,
                    shopCode,
                    ownerName,
                    phone,
                    email,
                    address,
                    villageId,
                    routeId,
                    scheduleId,
                    openingBalance,
                    creditLimit,
                    isActive);

                cmd.Parameters.Add(
                    "@UpdatedBy",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@ShopId",
                    SqlDbType.Int).Value = ShopId;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                int rows = cmd.ExecuteNonQuery();

                if (rows == 0)
                {
                    throw new InvalidOperationException(
                        "The shop could not be updated.");
                }
            }
        }


        private static void AddParameters(
            SqlCommand cmd,
            string shopName,
            string shopCode,
            string ownerName,
            string phone,
            string email,
            string address,
            int villageId,
            int routeId,
            int scheduleId,
            decimal? openingBalance,
            decimal? creditLimit,
            bool isActive)
        {
            cmd.Parameters.Add(
                "@ShopName",
                SqlDbType.NVarChar,
                500).Value = shopName;

            cmd.Parameters.Add(
                "@ShopCode",
                SqlDbType.NVarChar,
                200).Value =
                string.IsNullOrWhiteSpace(shopCode)
                    ? (object)DBNull.Value
                    : shopCode;

            cmd.Parameters.Add(
                "@OwnerName",
                SqlDbType.NVarChar,
                400).Value =
                string.IsNullOrWhiteSpace(ownerName)
                    ? (object)DBNull.Value
                    : ownerName;

            cmd.Parameters.Add(
                "@Phone",
                SqlDbType.NVarChar,
                80).Value =
                string.IsNullOrWhiteSpace(phone)
                    ? (object)DBNull.Value
                    : phone;

            cmd.Parameters.Add(
                "@Email",
                SqlDbType.NVarChar,
                600).Value =
                string.IsNullOrWhiteSpace(email)
                    ? (object)DBNull.Value
                    : email;

            cmd.Parameters.Add(
                "@Address",
                SqlDbType.NVarChar,
                1000).Value =
                string.IsNullOrWhiteSpace(address)
                    ? (object)DBNull.Value
                    : address;

            cmd.Parameters.Add(
                "@VillageId",
                SqlDbType.Int).Value =
                villageId > 0 ? (object)villageId : DBNull.Value;

            cmd.Parameters.Add(
                "@RouteId",
                SqlDbType.Int).Value =
                routeId > 0 ? (object)routeId : DBNull.Value;

            cmd.Parameters.Add(
                "@RouteScheduleId",
                SqlDbType.Int).Value =
                scheduleId > 0 ? (object)scheduleId : DBNull.Value;

            cmd.Parameters.Add(
                "@OpeningBalance",
                SqlDbType.Decimal).Value =
                openingBalance.HasValue
                    ? (object)openingBalance.Value
                    : DBNull.Value;

            cmd.Parameters.Add(
                "@CreditLimit",
                SqlDbType.Decimal).Value =
                creditLimit.HasValue
                    ? (object)creditLimit.Value
                    : DBNull.Value;

            cmd.Parameters.Add(
                "@IsActive",
                SqlDbType.Bit).Value = isActive;
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
