
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace BWDMS.Dealer
{
    public partial class AddRouteSchedule : System.Web.UI.Page
    {
        private readonly string connectionString =
            ConfigurationManager
                .ConnectionStrings["BWDMSConnection"]
                .ConnectionString;

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

        private void LoadRoutes()
        {
            string query = @"
                SELECT
                    RouteId,
                    RouteName
                FROM Routes
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
            string query = @"
                SELECT
                    UserId,
                    FullName
                FROM Users
                WHERE Role = 'Driver'
                  AND IsActive = 1
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
                new SqlConnection(connectionString))
            {
                using (SqlCommand command =
                    new SqlCommand(query, connection))
                {
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
            string query = @"
                SELECT
                    RouteId,
                    DayOfWeek,
                    VehicleId,
                    SalesmanId,
                    DriverId,
                    IsActive
                FROM RouteSchedules
                WHERE RouteScheduleId = @RouteScheduleId;
            ";

            using (SqlConnection connection =
                new SqlConnection(connectionString))
            {
                using (SqlCommand command =
                    new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue(
                        "@RouteScheduleId",
                        ScheduleId);

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
                UPDATE RouteSchedules
                SET
                    RouteId = @RouteId,
                    DayOfWeek = @DayOfWeek,
                    VehicleId = @VehicleId,
                    SalesmanId = @SalesmanId,
                    DriverId = @DriverId,
                    IsActive = @IsActive,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE RouteScheduleId = @RouteScheduleId;
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
                new SqlConnection(connectionString))
            {
                using (SqlCommand command =
                    new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue(
                        "@RouteId",
                        routeId);

                    command.Parameters.AddWithValue(
                        "@DayOfWeek",
                        dayOfWeek);

                    command.Parameters.AddWithValue(
                        "@VehicleId",
                        vehicleId.HasValue
                            ? (object)vehicleId.Value
                            : DBNull.Value);

                    command.Parameters.AddWithValue(
                        "@SalesmanId",
                        salesmanId.HasValue
                            ? (object)salesmanId.Value
                            : DBNull.Value);

                    command.Parameters.AddWithValue(
                        "@DriverId",
                        driverId.HasValue
                            ? (object)driverId.Value
                            : DBNull.Value);

                    command.Parameters.AddWithValue(
                        "@IsActive",
                        isActive);

                    if (scheduleId.HasValue)
                    {
                        command.Parameters.AddWithValue(
                            "@RouteScheduleId",
                            scheduleId.Value);

                        command.Parameters.AddWithValue(
                            "@UpdatedBy",
                            currentUserId);
                    }
                    else
                    {
                        command.Parameters.AddWithValue(
                            "@CreatedBy",
                            currentUserId);
                    }

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}