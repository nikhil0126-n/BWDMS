
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class AddRouteSchedule : System.Web.UI.Page
    {
        private int ScheduleId
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

            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Account/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }


            // ========================================================
            // CHECK DEALER ROLE
            //
            // This page previously accepted any logged-in user, and
            // every dropdown returned ALL routes, vehicles,
            // salesmen and drivers in the system.
            // ========================================================

            if (Session["UserRole"] == null ||
                Session["UserRole"].ToString() != "Dealer")
            {
                Response.Redirect(BWDMS.Data.AppAuth.HomeUrl(Session["UserRole"]), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadRoutes();
                LoadVehicles();
                LoadSalesmen();
                LoadDrivers();

                if (ScheduleId > 0)
                {
                    litPageTitle.Text = "Edit Route Schedule";
                    btnSave.Text = "Update Schedule";

                    LoadScheduleDetails();
                }
                else
                {
                    litPageTitle.Text = "Add Route Schedule";
                    btnSave.Text = "Save Schedule";
                }
            }
        }


        private int DealerId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }


        private void LoadRoutes()
        {
            string query = @"
                SELECT
                    RouteId,
                    RouteName
                FROM Routes
                WHERE DealerId = @DealerId
                ORDER BY RouteName;
            ";

            LoadDropDown(
                ddlRoute,
                query,
                "RouteName",
                "RouteId",
                "Select Route");
        }

        private void LoadVehicles()
        {
            string query = @"
                SELECT
                    VehicleId,
                    VehicleNumber
                FROM Vehicles
                WHERE IsActive = 1
                AND (DealerId = @DealerId
                     OR DealerId IS NULL)
                ORDER BY VehicleNumber;
            ";

            LoadDropDown(
                ddlVehicle,
                query,
                "VehicleNumber",
                "VehicleId",
                "Not Assigned");
        }

        private void LoadSalesmen()
        {
            string query = @"
                SELECT
                    UserId,
                    FullName
                FROM Users
                WHERE Role = 'Salesman'
                  AND IsActive = 1
                  AND (DealerId = @DealerId
                       OR DealerId IS NULL)
                ORDER BY FullName;
            ";

            LoadDropDown(
                ddlSalesman,
                query,
                "FullName",
                "UserId",
                "Not Assigned");
        }

        private void LoadDrivers()
        {
            // ----------------------------------------------------
            // This database has no 'Driver' role yet, so the list
            // falls back to the dealer's active salesmen. Adding a
            // 'Driver' role later needs no code change here.
            // ----------------------------------------------------

            string query = @"
                SELECT
                    UserId,
                    FullName
                FROM Users
                WHERE Role IN ('Driver', 'Salesman')
                  AND IsActive = 1
                  AND (DealerId = @DealerId
                       OR DealerId IS NULL)
                ORDER BY FullName;
            ";

            LoadDropDown(
                ddlDriver,
                query,
                "FullName",
                "UserId",
                "Not Assigned");
        }

        private void LoadDropDown(
            DropDownList dropdown,
            string query,
            string textField,
            string valueField,
            string firstItemText)
        {
            DataTable dt = new DataTable();

            using (SqlConnection connection =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand command =
                    new SqlCommand(query, connection))
                {
                    command.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value =
                        DealerId;

                    using (SqlDataAdapter adapter =
                        new SqlDataAdapter(command))
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

        private void LoadScheduleDetails()
        {
            // DealerId on Routes guards against editing another
            // dealer's schedule by guessing its id.
            string query = @"
                SELECT
                    rs.RouteId,
                    rs.DayOfWeek,
                    rs.VehicleId,
                    rs.SalesmanId,
                    rs.DriverId,
                    rs.IsActive
                FROM RouteSchedules rs
                INNER JOIN Routes r
                    ON rs.RouteId = r.RouteId
                    AND r.DealerId = @DealerId
                WHERE rs.RouteScheduleId = @RouteScheduleId;
            ";

            using (SqlConnection connection =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand command =
                    new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue(
                        "@RouteScheduleId",
                        ScheduleId);

                    command.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value =
                        DealerId;

                    connection.Open();

                    using (SqlDataReader reader =
                        command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            SetSelectedValue(
                                ddlRoute,
                                reader["RouteId"].ToString());

                            SetSelectedValue(
                                ddlDayOfWeek,
                                reader["DayOfWeek"].ToString());

                            SetSelectedValue(
                                ddlVehicle,
                                reader["VehicleId"] == DBNull.Value
                                    ? ""
                                    : reader["VehicleId"].ToString());

                            SetSelectedValue(
                                ddlSalesman,
                                reader["SalesmanId"] == DBNull.Value
                                    ? ""
                                    : reader["SalesmanId"].ToString());

                            SetSelectedValue(
                                ddlDriver,
                                reader["DriverId"] == DBNull.Value
                                    ? ""
                                    : reader["DriverId"].ToString());

                            SetSelectedValue(
                                ddlStatus,
                                Convert.ToBoolean(reader["IsActive"])
                                    ? "1"
                                    : "0");
                        }
                        else
                        {
                            Response.Redirect(
                                "RouteSchedules.aspx",
                                false);

                            Context.ApplicationInstance
                                .CompleteRequest();
                        }
                    }
                }
            }
        }

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

        protected void btnSave_Click(
            object sender,
            EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            int routeId = Convert.ToInt32(
                ddlRoute.SelectedValue);

            string dayOfWeek = ddlDayOfWeek.SelectedValue;

            int? vehicleId = GetNullableInt(
                ddlVehicle.SelectedValue);

            int? salesmanId = GetNullableInt(
                ddlSalesman.SelectedValue);

            int? driverId = GetNullableInt(
                ddlDriver.SelectedValue);

            bool isActive =
                ddlStatus.SelectedValue == "1";

            int currentUserId =
                Convert.ToInt32(Session["UserId"]);


            // ========================================================
            // THE ROUTE MUST BELONG TO THIS DEALER
            //
            // The dropdown is already dealer scoped, but a crafted
            // post could submit any RouteId, so it is re-checked
            // against the database here.
            // ========================================================

            if (!RouteBelongsToDealer(routeId))
            {
                lblMessage.Text =
                    "The selected route does not belong to your dealership.";

                lblMessage.CssClass =
                    "d-block mt-3 text-danger";

                lblMessage.Visible = true;

                return;
            }

            // "One vehicle to one route IF FREE". A vehicle physically cannot
            // run two routes on the same weekday.
            if (isActive &&
                vehicleId.HasValue &&
                VehicleAlreadyBooked(
                    vehicleId.Value,
                    dayOfWeek))
            {
                lblMessage.Text =
                    "That vehicle is already assigned to another route on " +
                    dayOfWeek + ". Choose a free vehicle, or a different day.";

                lblMessage.CssClass =
                    "d-block mt-3 text-danger";

                lblMessage.Visible = true;

                return;
            }

            if (ScheduleId > 0)
            {
                UpdateSchedule(
                    routeId,
                    dayOfWeek,
                    vehicleId,
                    salesmanId,
                    driverId,
                    isActive,
                    currentUserId);
            }
            else
            {
                InsertSchedule(
                    routeId,
                    dayOfWeek,
                    vehicleId,
                    salesmanId,
                    driverId,
                    isActive,
                    currentUserId);
            }
        }

        private int? GetNullableInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return Convert.ToInt32(value);
        }


        // ============================================================
        // ROUTE OWNERSHIP CHECK
        // ============================================================

        private bool RouteBelongsToDealer(int routeId)
        {
            string query = @"
                SELECT COUNT(*)
                FROM Routes
                WHERE RouteId = @RouteId
                AND DealerId = @DealerId";

            using (SqlConnection connection =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand command =
                    new SqlCommand(query, connection))
                {
                    command.Parameters.Add(
                        "@RouteId",
                        SqlDbType.Int).Value =
                        routeId;

                    command.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value =
                        DealerId;

                    connection.Open();

                    return Convert.ToInt32(
                        command.ExecuteScalar()) > 0;
                }
            }
        }

        // A vehicle can only run ONE route on a given weekday. The filtered unique
        // index UQ_RouteSchedules_Vehicle_Weekday enforces this in the
        // database; this check exists so the operator gets an explanation
        // instead of a duplicate-key error.
        private bool VehicleAlreadyBooked(
            int vehicleId,
            string dayOfWeek)
        {
            if (vehicleId <= 0 ||
                string.IsNullOrWhiteSpace(dayOfWeek))
            {
                return false;
            }

            string query = @"
                SELECT COUNT(*)
                FROM RouteSchedules rs
                INNER JOIN Routes r
                    ON r.RouteId = rs.RouteId
                WHERE rs.VehicleId = @VehicleId
                  AND rs.DayOfWeek = @DayOfWeek
                  AND rs.IsActive = 1
                  AND r.DealerId = @DealerId
                  AND (
                        @SelfId = 0
                        OR rs.RouteScheduleId <> @SelfId
                      )";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@VehicleId",
                        SqlDbType.Int).Value = vehicleId;

                    cmd.Parameters.Add(
                        "@DayOfWeek",
                        SqlDbType.NVarChar,
                        20).Value = dayOfWeek;

                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    cmd.Parameters.Add(
                        "@SelfId",
                        SqlDbType.Int).Value = ScheduleId;

                    con.Open();

                    return Convert.ToInt32(
                        cmd.ExecuteScalar()) > 0;
                }
            }
        }


        private void InsertSchedule(
            int routeId,
            string dayOfWeek,
            int? vehicleId,
            int? salesmanId,
            int? driverId,
            bool isActive,
            int currentUserId)
        {
            string query = @"
                INSERT INTO RouteSchedules
                (
                    RouteId,
                    DayOfWeek,
                    VehicleId,
                    SalesmanId,
                    DriverId,
                    IsActive,
                    CreatedBy,
                    CreatedAt
                )
                VALUES
                (
                    @RouteId,
                    @DayOfWeek,
                    @VehicleId,
                    @SalesmanId,
                    @DriverId,
                    @IsActive,
                    @CreatedBy,
                    GETDATE()
                );
            ";

            ExecuteSaveQuery(
                query,
                routeId,
                dayOfWeek,
                vehicleId,
                salesmanId,
                driverId,
                isActive,
                currentUserId,
                null);

            Response.Redirect(
                "RouteSchedules.aspx",
                false);

            Context.ApplicationInstance
                .CompleteRequest();
        }

        private void UpdateSchedule(
            int routeId,
            string dayOfWeek,
            int? vehicleId,
            int? salesmanId,
            int? driverId,
            bool isActive,
            int currentUserId)
        {
            string query = @"
                UPDATE rs
                SET
                    RouteId = @RouteId,
                    DayOfWeek = @DayOfWeek,
                    VehicleId = @VehicleId,
                    SalesmanId = @SalesmanId,
                    DriverId = @DriverId,
                    IsActive = @IsActive,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                FROM RouteSchedules rs
                INNER JOIN Routes r
                    ON rs.RouteId = r.RouteId
                WHERE rs.RouteScheduleId = @RouteScheduleId
                AND r.DealerId = @DealerId;
            ";

            ExecuteSaveQuery(
                query,
                routeId,
                dayOfWeek,
                vehicleId,
                salesmanId,
                driverId,
                isActive,
                currentUserId,
                ScheduleId);

            Response.Redirect(
                "RouteSchedules.aspx",
                false);

            Context.ApplicationInstance
                .CompleteRequest();
        }

        private void ExecuteSaveQuery(
            string query,
            int routeId,
            string dayOfWeek,
            int? vehicleId,
            int? salesmanId,
            int? driverId,
            bool isActive,
            int currentUserId,
            int? scheduleId)
        {
            using (SqlConnection connection =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand command =
                    new SqlCommand(query, connection))
                {
                    // @DealerId is referenced by the UPDATE only
                    // (the INSERT is guarded by RouteBelongsToDealer).
                    if (scheduleId.HasValue)
                    {
                        command.Parameters.Add(
                            "@DealerId",
                            SqlDbType.Int).Value =
                            DealerId;
                    }

                    command.Parameters.Add(
                        "@RouteId",
                        SqlDbType.Int).Value =
                        routeId;

                    command.Parameters.Add(
                        "@DayOfWeek",
                        SqlDbType.NVarChar,
                        40).Value =
                        dayOfWeek;

                    command.Parameters.Add(
                        "@VehicleId",
                        SqlDbType.Int).Value =
                        vehicleId.HasValue
                            ? (object)vehicleId.Value
                            : DBNull.Value;

                    command.Parameters.Add(
                        "@SalesmanId",
                        SqlDbType.Int).Value =
                        salesmanId.HasValue
                            ? (object)salesmanId.Value
                            : DBNull.Value;

                    command.Parameters.Add(
                        "@DriverId",
                        SqlDbType.Int).Value =
                        driverId.HasValue
                            ? (object)driverId.Value
                            : DBNull.Value;

                    command.Parameters.Add(
                        "@IsActive",
                        SqlDbType.Bit).Value =
                        isActive;

                    if (scheduleId.HasValue)
                    {
                        command.Parameters.Add(
                            "@RouteScheduleId",
                            SqlDbType.Int).Value =
                            scheduleId.Value;

                        command.Parameters.Add(
                            "@UpdatedBy",
                            SqlDbType.Int).Value =
                            currentUserId;
                    }
                    else
                    {
                        command.Parameters.Add(
                            "@CreatedBy",
                            SqlDbType.Int).Value =
                            currentUserId;
                    }

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}