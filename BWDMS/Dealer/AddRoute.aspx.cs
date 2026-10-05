using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class AddRoute : System.Web.UI.Page
    {
        // ============================================================
        // PAGE LOAD
        // ============================================================

        protected void Page_Load(object sender, EventArgs e)
        {
            // Prevent browser cache
            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            Response.Cache.SetExpires(
                DateTime.UtcNow.AddDays(-1));

            Response.Cache.SetRevalidation(
                System.Web.HttpCacheRevalidation.AllCaches);


            // ========================================================
            // CHECK LOGIN
            // ========================================================

            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }


            // ========================================================
            // CHECK DEALER ROLE
            // ========================================================

            if (Session["UserRole"] == null ||
                Session["UserRole"].ToString() != "Dealer")
            {
                Response.Redirect(BWDMS.Data.AppAuth.HomeUrl(Session["UserRole"]));
                return;
            }


            // ========================================================
            // FIRST LOAD ONLY
            // ========================================================

            if (!IsPostBack)
            {
                // ----------------------------------------------------
                // DROPDOWNS MUST BE POPULATED BEFORE LoadRoute()
                // SO SAVED VALUES CAN BE SELECTED
                // ----------------------------------------------------

                LoadDropdowns();


                string id = Request.QueryString["id"];


                // ----------------------------------------------------
                // ADD MODE
                // ----------------------------------------------------

                if (string.IsNullOrWhiteSpace(id))
                {
                    SetAddMode();
                }


                // ----------------------------------------------------
                // EDIT MODE
                // ----------------------------------------------------

                else
                {
                    int routeId;

                    if (int.TryParse(id, out routeId))
                    {
                        LoadRoute(routeId);
                    }
                    else
                    {
                        Response.Redirect("~/Dealer/Routes.aspx");
                    }
                }
            }
        }


        // ============================================================
        // LOAD DROPDOWNS (FIRST LOAD ONLY)
        // ============================================================

        private void LoadDropdowns()
        {
            // Preferred Salesperson
            LoadDropDown(
                ddlSalesman,
                @"
                    SELECT UserId, FullName
                    FROM Users
                    WHERE Role = 'Salesman'
                    AND IsActive = 1
                    AND DealerId = @DealerId
                    ORDER BY FullName",
                "FullName",
                "UserId",
                "-- None --");


            // Preferred Driver
            // (there is no 'Driver' role yet - this degrades
            //  gracefully if one is ever added)
            LoadDropDown(
                ddlDriver,
                @"
                    SELECT UserId, FullName
                    FROM Users
                    WHERE IsActive = 1
                    AND DealerId = @DealerId
                    AND Role IN ('Salesman', 'Driver')
                    ORDER BY FullName",
                "FullName",
                "UserId",
                "-- None --");


            // Default Truck
            LoadDropDown(
                ddlVehicle,
                @"
                    SELECT VehicleId, VehicleNumber
                    FROM Vehicles
                    WHERE IsActive = 1
                    AND (DealerId = @DealerId
                         OR DealerId IS NULL)
                    ORDER BY VehicleNumber",
                "VehicleNumber",
                "VehicleId",
                "-- None --");
        }


        private void LoadDropDown(
            DropDownList dropdown,
            string query,
            string textField,
            string valueField,
            string firstItemText)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value =
                        Convert.ToInt32(Session["UserId"]);

                    using (SqlDataAdapter adapter =
                        new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            dropdown.DataSource = dt;
            dropdown.DataTextField = textField;
            dropdown.DataValueField = valueField;
            dropdown.DataBind();

            dropdown.Items.Insert(
                0,
                new ListItem(firstItemText, ""));
        }


        // ============================================================
        // ADD MODE
        // ============================================================

        private void SetAddMode()
        {
            lblPageTitle.Text = "Add Route";

            lblHeading.Text = "Add Route";

            lblSubHeading.Text =
                "Create a new sales and delivery route";

            lblFormTitle.Text =
                "Route Information";

            btnSaveRoute.Text =
                "Create Route";
        }


        // ============================================================
        // EDIT MODE
        // ============================================================

        private void SetEditMode()
        {
            lblPageTitle.Text = "Edit Route";

            lblHeading.Text = "Edit Route";

            lblSubHeading.Text =
                "Update your sales and delivery route";

            lblFormTitle.Text =
                "Route Information";

            btnSaveRoute.Text =
                "Update Route";


            // Audit block is only shown in edit mode
            pnlAudit.Visible = true;
        }


        // ============================================================
        // LOAD EXISTING ROUTE
        // ============================================================

        private void LoadRoute(int routeId)
        {
            try
            {
                int dealerId =
                    Convert.ToInt32(Session["UserId"]);


                string query = @"
                    SELECT
                        RouteId,
                        RouteCode,
                        RouteName,
                        RouteType,
                        DayOfWeek,
                        OrderDispatchDays,
                        PreferredSalesmanId,
                        PreferredDriverId,
                        DefaultVehicleId,
                        IsActive,
                        CreatedBy,
                        CreatedAt,
                        UpdatedBy,
                        UpdatedAt
                    FROM Routes
                    WHERE RouteId = @RouteId
                    AND DealerId = @DealerId";


                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    con.Open();


                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@RouteId",
                            SqlDbType.Int).Value =
                            routeId;


                        cmd.Parameters.Add(
                            "@DealerId",
                            SqlDbType.Int).Value =
                            dealerId;


                        object createdBy = DBNull.Value;
                        object createdAt = DBNull.Value;
                        object updatedBy = DBNull.Value;
                        object updatedAt = DBNull.Value;


                        using (SqlDataReader reader =
                            cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                Response.Redirect(
                                    "~/Dealer/Routes.aspx");

                                return;
                            }


                            // Switch to EDIT mode
                            SetEditMode();


                            // Route Name
                            txtRouteName.Text =
                                reader["RouteName"].ToString();


                            // Route Number (RouteCode)
                            if (reader["RouteCode"] == DBNull.Value)
                            {
                                txtRouteCode.Text = "";
                            }
                            else
                            {
                                txtRouteCode.Text =
                                    reader["RouteCode"].ToString();
                            }


                            // Route Type
                            SetSelectedValue(
                                ddlRouteType,
                                reader["RouteType"] == DBNull.Value
                                    ? ""
                                    : reader["RouteType"].ToString());


                            // Order / Dispatch Days
                            // (falls back to the legacy single
                            //  DayOfWeek for older rows)
                            string days =
                                reader["OrderDispatchDays"] == DBNull.Value
                                    ? ""
                                    : reader["OrderDispatchDays"].ToString();


                            if (string.IsNullOrWhiteSpace(days))
                            {
                                days =
                                    reader["DayOfWeek"] == DBNull.Value
                                        ? ""
                                        : reader["DayOfWeek"].ToString();
                            }

                            CheckDays(days);


                            // Preferred Salesperson
                            SetSelectedValue(
                                ddlSalesman,
                                reader["PreferredSalesmanId"] == DBNull.Value
                                    ? ""
                                    : reader["PreferredSalesmanId"].ToString());


                            // Preferred Driver
                            SetSelectedValue(
                                ddlDriver,
                                reader["PreferredDriverId"] == DBNull.Value
                                    ? ""
                                    : reader["PreferredDriverId"].ToString());


                            // Default Truck
                            SetSelectedValue(
                                ddlVehicle,
                                reader["DefaultVehicleId"] == DBNull.Value
                                    ? ""
                                    : reader["DefaultVehicleId"].ToString());


                            // Status
                            bool isActive =
                                Convert.ToBoolean(
                                    reader["IsActive"]);


                            ddlStatus.SelectedValue =
                                isActive ? "1" : "0";


                            // Audit values
                            createdBy = reader["CreatedBy"];
                            createdAt = reader["CreatedAt"];
                            updatedBy = reader["UpdatedBy"];
                            updatedAt = reader["UpdatedAt"];
                        }


                        // The reader is closed here, so user names
                        // can now be resolved on the same connection.
                        ShowAudit(
                            con,
                            createdBy,
                            createdAt,
                            updatedBy,
                            updatedAt);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error loading route: " + ex.Message,
                    false);
            }
        }


        // ============================================================
        // AUDIT INFORMATION (READ ONLY)
        // ============================================================

        private void ShowAudit(
            SqlConnection con,
            object createdBy,
            object createdAt,
            object updatedBy,
            object updatedAt)
        {
            List<string> lines = new List<string>();

            lines.Add(
                "Created By: " +
                GetUserFullName(con, createdBy));

            lines.Add(
                "Created At: " +
                FormatAuditDate(createdAt));


            if (updatedAt == null ||
                updatedAt == DBNull.Value)
            {
                lines.Add("Last Modified: Never");
            }
            else
            {
                lines.Add(
                    "Last Modified By: " +
                    GetUserFullName(con, updatedBy));

                lines.Add(
                    "Last Modified At: " +
                    FormatAuditDate(updatedAt));
            }

            lblAudit.Text =
                string.Join("<br />", lines.ToArray());

            pnlAudit.Visible = true;
        }


        private string GetUserFullName(
            SqlConnection con,
            object userId)
        {
            if (userId == null || userId == DBNull.Value)
            {
                return "\u2014"; // em dash
            }

            string query = @"
                SELECT FullName
                FROM Users
                WHERE UserId = @UserId";

            using (SqlCommand cmd =
                new SqlCommand(query, con))
            {
                cmd.Parameters.Add(
                    "@UserId",
                    SqlDbType.Int).Value =
                    Convert.ToInt32(userId);

                object result = cmd.ExecuteScalar();

                if (result == null ||
                    result == DBNull.Value)
                {
                    return "\u2014"; // em dash
                }

                return result.ToString();
            }
        }


        private string FormatAuditDate(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return "\u2014"; // em dash
            }

            return Convert.ToDateTime(value)
                .ToString("dd MMM yyyy HH:mm");
        }


        // ============================================================
        // ORDER / DISPATCH DAYS
        // ============================================================

        protected void cvDays_ServerValidate(
            object source,
            ServerValidateEventArgs args)
        {
            args.IsValid =
                GetSelectedDays().Count > 0;
        }


        private List<string> GetSelectedDays()
        {
            List<string> days = new List<string>();

            foreach (ListItem item in cblDays.Items)
            {
                if (item.Selected)
                {
                    days.Add(item.Value);
                }
            }

            return days;
        }


        private void CheckDays(string days)
        {
            cblDays.ClearSelection();

            if (string.IsNullOrWhiteSpace(days))
            {
                return;
            }

            foreach (string part in days.Split(','))
            {
                string day = part.Trim();

                if (day.Length == 0)
                {
                    continue;
                }

                ListItem item =
                    cblDays.Items.FindByValue(day);

                if (item != null)
                {
                    item.Selected = true;
                }
            }
        }


        // ============================================================
        // HELPERS
        // ============================================================

        private void SetSelectedValue(
            DropDownList dropdown,
            string value)
        {
            ListItem item = dropdown.Items.FindByValue(value);

            if (item != null)
            {
                dropdown.SelectedValue = value;
            }
            else
            {
                dropdown.SelectedIndex = 0;
            }
        }


        // Empty dropdown values become DBNull - never int.Parse("")
        private object IntOrDbNull(string value)
        {
            int number;

            if (string.IsNullOrWhiteSpace(value) ||
                !int.TryParse(value, out number))
            {
                return DBNull.Value;
            }

            return number;
        }


        // ============================================================
        // SAVE / UPDATE ROUTE
        //
        // IMPORTANT:
        // This name MUST exactly match:
        //
        // OnClick="btnSaveRoute_Click"
        // ============================================================

        protected void btnSaveRoute_Click(
            object sender,
            EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }


            // ----------------------------------------------------
            // At least one order/dispatch day is required.
            //
            // Checked directly instead of relying only on the
            // CustomValidator, so the rule can never be skipped.
            // ----------------------------------------------------

            List<string> requiredDays =
                GetSelectedDays();


            if (requiredDays.Count == 0)
            {
                ShowMessage(
                    "Select at least one order/dispatch day.",
                    false);

                return;
            }


            try
            {
                int dealerId =
                    Convert.ToInt32(Session["UserId"]);


                string routeName =
                    txtRouteName.Text.Trim();


                string routeCode =
                    txtRouteCode.Text.Trim();


                string routeType =
                    ddlRouteType.SelectedValue;


                List<string> selectedDays =
                    GetSelectedDays();


                // Comma-separated list, e.g. "Tuesday,Friday"
                string orderDispatchDays =
                    string.Join(",", selectedDays.ToArray());


                // Legacy single day: first ticked day
                string dayOfWeek =
                    selectedDays.Count > 0
                        ? selectedDays[0]
                        : null;


                object salesmanId =
                    IntOrDbNull(ddlSalesman.SelectedValue);


                object driverId =
                    IntOrDbNull(ddlDriver.SelectedValue);


                object vehicleId =
                    IntOrDbNull(ddlVehicle.SelectedValue);


                bool isActive =
                    ddlStatus.SelectedValue == "1";


                // ====================================================
                // CHECK EDIT / ADD MODE
                // ====================================================

                string id =
                    Request.QueryString["id"];


                int routeId = 0;


                bool isEdit =
                    int.TryParse(id, out routeId);


                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    con.Open();


                    // All reads/writes below share ONE transaction
                    // (rolled back automatically with the
                    //  connection if we return or fault)
                    SqlTransaction transaction =
                        con.BeginTransaction();


                    // =================================================
                    // CHECK DUPLICATE ROUTE NAME
                    // =================================================

                    string checkNameQuery = @"
                        SELECT COUNT(*)
                        FROM Routes
                        WHERE DealerId = @DealerId
                        AND RouteName = @RouteName";


                    if (isEdit)
                    {
                        checkNameQuery +=
                            " AND RouteId <> @RouteId";
                    }


                    using (SqlCommand cmd =
                        new SqlCommand(
                            checkNameQuery,
                            con))
                    {
                        cmd.Transaction = transaction;

                        cmd.Parameters.Add(
                            "@DealerId",
                            SqlDbType.Int).Value =
                            dealerId;


                        cmd.Parameters.Add(
                            "@RouteName",
                            SqlDbType.NVarChar,
                            100).Value =
                            routeName;


                        if (isEdit)
                        {
                            cmd.Parameters.Add(
                                "@RouteId",
                                SqlDbType.Int).Value =
                                routeId;
                        }


                        int count =
                            Convert.ToInt32(
                                cmd.ExecuteScalar());


                        if (count > 0)
                        {
                            ShowMessage(
                                "A route with this name already exists.",
                                false);

                            return;
                        }
                    }


                    // =================================================
                    // CHECK DUPLICATE ROUTE NUMBER (RouteCode)
                    // =================================================

                    if (!string.IsNullOrWhiteSpace(routeCode))
                    {
                        string checkCodeQuery = @"
                            SELECT COUNT(*)
                            FROM Routes
                            WHERE DealerId = @DealerId
                            AND RouteCode = @RouteCode";


                        if (isEdit)
                        {
                            checkCodeQuery +=
                                " AND RouteId <> @RouteId";
                        }


                        using (SqlCommand cmd =
                            new SqlCommand(
                                checkCodeQuery,
                                con))
                        {
                            cmd.Transaction = transaction;

                            cmd.Parameters.Add(
                                "@DealerId",
                                SqlDbType.Int).Value =
                                dealerId;


                            cmd.Parameters.Add(
                                "@RouteCode",
                                SqlDbType.NVarChar,
                                50).Value =
                                routeCode;


                            if (isEdit)
                            {
                                cmd.Parameters.Add(
                                    "@RouteId",
                                    SqlDbType.Int).Value =
                                    routeId;
                            }


                            int count =
                                Convert.ToInt32(
                                    cmd.ExecuteScalar());


                            if (count > 0)
                            {
                                ShowMessage(
                                    "This route number already exists.",
                                    false);

                                return;
                            }
                        }
                    }


                    // =================================================
                    // UPDATE
                    // =================================================

                    if (isEdit)
                    {
                        string updateQuery = @"
                            UPDATE Routes
                            SET
                                RouteName = @RouteName,
                                RouteCode = @RouteCode,
                                RouteType = @RouteType,
                                DayOfWeek = @DayOfWeek,
                                OrderDispatchDays = @OrderDispatchDays,
                                PreferredSalesmanId = @PreferredSalesmanId,
                                PreferredDriverId = @PreferredDriverId,
                                DefaultVehicleId = @DefaultVehicleId,
                                IsActive = @IsActive,
                                UpdatedBy = @UpdatedBy,
                                UpdatedAt = GETDATE()
                            WHERE RouteId = @RouteId
                            AND DealerId = @DealerId";


                        using (SqlCommand cmd =
                            new SqlCommand(
                                updateQuery,
                                con))
                        {
                            cmd.Transaction = transaction;

                            cmd.Parameters.Add(
                                "@RouteName",
                                SqlDbType.NVarChar,
                                100).Value =
                                routeName;


                            cmd.Parameters.Add(
                                "@RouteCode",
                                SqlDbType.NVarChar,
                                50).Value =
                                string.IsNullOrWhiteSpace(routeCode)
                                    ? (object)DBNull.Value
                                    : routeCode;


                            cmd.Parameters.Add(
                                "@RouteType",
                                SqlDbType.NVarChar,
                                50).Value =
                                string.IsNullOrWhiteSpace(routeType)
                                    ? (object)DBNull.Value
                                    : routeType;


                            cmd.Parameters.Add(
                                "@DayOfWeek",
                                SqlDbType.NVarChar,
                                20).Value =
                                (object)dayOfWeek ??
                                DBNull.Value;


                            cmd.Parameters.Add(
                                "@OrderDispatchDays",
                                SqlDbType.NVarChar,
                                400).Value =
                                string.IsNullOrWhiteSpace(orderDispatchDays)
                                    ? (object)DBNull.Value
                                    : orderDispatchDays;


                            cmd.Parameters.Add(
                                "@PreferredSalesmanId",
                                SqlDbType.Int).Value =
                                salesmanId;


                            cmd.Parameters.Add(
                                "@PreferredDriverId",
                                SqlDbType.Int).Value =
                                driverId;


                            cmd.Parameters.Add(
                                "@DefaultVehicleId",
                                SqlDbType.Int).Value =
                                vehicleId;


                            cmd.Parameters.Add(
                                "@IsActive",
                                SqlDbType.Bit).Value =
                                isActive;


                            cmd.Parameters.Add(
                                "@UpdatedBy",
                                SqlDbType.Int).Value =
                                dealerId;


                            cmd.Parameters.Add(
                                "@RouteId",
                                SqlDbType.Int).Value =
                                routeId;


                            cmd.Parameters.Add(
                                "@DealerId",
                                SqlDbType.Int).Value =
                                dealerId;


                            int rows =
                                cmd.ExecuteNonQuery();


                            if (rows == 0)
                            {
                                ShowMessage(
                                    "Route could not be updated.",
                                    false);

                                return;
                            }
                        }
                    }


                    // =================================================
                    // INSERT
                    // =================================================

                    else
                    {
                        string insertQuery = @"
                            INSERT INTO Routes
                            (
                                DealerId,
                                RouteName,
                                RouteCode,
                                RouteType,
                                DayOfWeek,
                                OrderDispatchDays,
                                PreferredSalesmanId,
                                PreferredDriverId,
                                DefaultVehicleId,
                                IsActive,
                                CreatedBy,
                                CreatedAt
                            )
                            VALUES
                            (
                                @DealerId,
                                @RouteName,
                                @RouteCode,
                                @RouteType,
                                @DayOfWeek,
                                @OrderDispatchDays,
                                @PreferredSalesmanId,
                                @PreferredDriverId,
                                @DefaultVehicleId,
                                @IsActive,
                                @CreatedBy,
                                GETDATE()
                            )";


                        using (SqlCommand cmd =
                            new SqlCommand(
                                insertQuery,
                                con))
                        {
                            cmd.Transaction = transaction;

                            cmd.Parameters.Add(
                                "@DealerId",
                                SqlDbType.Int).Value =
                                dealerId;


                            cmd.Parameters.Add(
                                "@RouteName",
                                SqlDbType.NVarChar,
                                100).Value =
                                routeName;


                            cmd.Parameters.Add(
                                "@RouteCode",
                                SqlDbType.NVarChar,
                                50).Value =
                                string.IsNullOrWhiteSpace(routeCode)
                                    ? (object)DBNull.Value
                                    : routeCode;


                            cmd.Parameters.Add(
                                "@RouteType",
                                SqlDbType.NVarChar,
                                50).Value =
                                string.IsNullOrWhiteSpace(routeType)
                                    ? (object)DBNull.Value
                                    : routeType;


                            cmd.Parameters.Add(
                                "@DayOfWeek",
                                SqlDbType.NVarChar,
                                20).Value =
                                (object)dayOfWeek ??
                                DBNull.Value;


                            cmd.Parameters.Add(
                                "@OrderDispatchDays",
                                SqlDbType.NVarChar,
                                400).Value =
                                string.IsNullOrWhiteSpace(orderDispatchDays)
                                    ? (object)DBNull.Value
                                    : orderDispatchDays;


                            cmd.Parameters.Add(
                                "@PreferredSalesmanId",
                                SqlDbType.Int).Value =
                                salesmanId;


                            cmd.Parameters.Add(
                                "@PreferredDriverId",
                                SqlDbType.Int).Value =
                                driverId;


                            cmd.Parameters.Add(
                                "@DefaultVehicleId",
                                SqlDbType.Int).Value =
                                vehicleId;


                            cmd.Parameters.Add(
                                "@IsActive",
                                SqlDbType.Bit).Value =
                                isActive;


                            cmd.Parameters.Add(
                                "@CreatedBy",
                                SqlDbType.Int).Value =
                                dealerId;


                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }


                // ====================================================
                // SUCCESS
                // ====================================================

                Response.Redirect(
                    "~/Dealer/Routes.aspx");
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error saving route: " + ex.Message,
                    false);
            }
        }


        // ============================================================
        // SHOW MESSAGE
        // ============================================================

        private void ShowMessage(
            string message,
            bool success)
        {
            pnlMessage.Visible = true;

            lblMessage.Text = message;


            if (success)
            {
                pnlMessage.CssClass =
                    "alert alert-success mb-4";
            }
            else
            {
                pnlMessage.CssClass =
                    "alert alert-danger mb-4";
            }
        }
    }
}
