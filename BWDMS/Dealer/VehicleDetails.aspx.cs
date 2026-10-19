using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    // ============================================================
    // VEHICLE DETAILS
    //
    // ~/Dealer/VehicleDetails.aspx?id=N shows one vehicle and this
    // month's figures. A dealer sees his OWN vehicles plus the
    // shared company vehicles (DealerId IS NULL), exactly like the
    // vehicle list page does, so a forged ?id= can never expose
    // another dealership's vehicle.
    // ============================================================

    public partial class VehicleDetails : System.Web.UI.Page
    {
        // Dealer's own id lives in UserId (there is no Session["DealerId"]).
        private int DealerId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }

        // ?id=<VehicleId>; 0 when the value is missing or malformed.
        private int VehicleId
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

        // True only after the vehicle has been proven to be visible
        // to this dealer. No other query runs while it is false.
        private bool vehicleLoaded;


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


            if (VehicleId <= 0)
            {
                Response.Redirect("~/Dealer/Vehicles.aspx");

                return;
            }

            vehicleLoaded = LoadVehicleHeader();

            if (!vehicleLoaded)
            {
                Response.Redirect("~/Dealer/Vehicles.aspx");

                return;
            }


            if (!IsPostBack)
            {
                LoadMonthSummary();
            }
        }


        // ============================================================
        // OWNERSHIP CHECK - runs before anything else is queried
        //
        // Vehicles has no driver / capacity / registration / fuel
        // / insurance column, so only the columns that really
        // exist are shown.
        // ============================================================

        private bool LoadVehicleHeader()
        {
            string query = @"
                SELECT
                    v.VehicleNumber,
                    v.VehicleName,
                    v.VehicleType,
                    v.IsActive,
                    v.CreatedAt,
                    OwnerText =
                        CASE
                            WHEN v.DealerId IS NULL
                                THEN 'Company'
                            ELSE 'Your Dealership'
                        END
                FROM Vehicles v
                WHERE v.VehicleId = @VehicleId
                  AND (v.DealerId = @DealerId
                       OR v.DealerId IS NULL)";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@VehicleId",
                        SqlDbType.Int).Value = VehicleId;

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

                        lblVehicleNumber.Text =
                            reader["VehicleNumber"].ToString();

                        bool isActive =
                            Convert.ToBoolean(reader["IsActive"]);

                        lblStatus.Text =
                            isActive ? "Active" : "Inactive";

                        lblStatus.CssClass = isActive
                            ? "badge bg-success"
                            : "badge bg-secondary";

                        lblVehicleName.Text =
                            TextOrDash(reader["VehicleName"]);

                        lblVehicleType.Text =
                            TextOrDash(reader["VehicleType"]);

                        lblOwnerText.Text =
                            reader["OwnerText"].ToString();

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
        // THIS MONTH SUMMARY
        //
        // A trip is a dispatch of this vehicle. Orders reach the
        // vehicle through the route schedule the vehicle ran
        // (Orders.RouteScheduleId -> RouteSchedules.VehicleId).
        // There is no distance column anywhere in the schema, so
        // no "Total Distance" tile exists.
        // ============================================================

        private void LoadMonthSummary()
        {
            if (!vehicleLoaded)
            {
                return;
            }

            DateTime monthStart =
                new DateTime(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1);

            DateTime monthEnd = monthStart.AddMonths(1);

            string query = @"
                SELECT
                    TripCount =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM Dispatches d
                             WHERE d.VehicleId = @VehicleId
                               AND d.DealerId = @DealerId
                               AND d.Status <> 'Cancelled'
                               AND d.DispatchDate >= @From
                               AND d.DispatchDate < @To),
                            0),
                    MonthSales =
                        ISNULL(
                            (SELECT SUM(o.GrandTotal)
                             FROM Orders o
                             INNER JOIN RouteSchedules rs
                                 ON rs.RouteScheduleId =
                                    o.RouteScheduleId
                             WHERE rs.VehicleId = @VehicleId
                               AND o.DealerId = @DealerId
                               AND o.Status NOT IN
                                   ('Cancelled', 'Returned')
                               AND o.OrderDate >= @From
                               AND o.OrderDate < @To),
                            0),
                    MonthOrders =
                        ISNULL(
                            (SELECT COUNT(*)
                             FROM Orders o
                             INNER JOIN RouteSchedules rs
                                 ON rs.RouteScheduleId =
                                    o.RouteScheduleId
                             WHERE rs.VehicleId = @VehicleId
                               AND o.DealerId = @DealerId
                               AND o.Status <> 'Cancelled'
                               AND o.OrderDate >= @From
                               AND o.OrderDate < @To),
                            0)";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@VehicleId",
                        SqlDbType.Int).Value = VehicleId;

                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    cmd.Parameters.Add(
                        "@From",
                        SqlDbType.Date).Value = monthStart;

                    cmd.Parameters.Add(
                        "@To",
                        SqlDbType.Date).Value = monthEnd;

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return;
                        }

                        lblTrips.Text =
                            Convert.ToInt32(
                                reader["TripCount"])
                                .ToString();

                        lblSales.Text =
                            Convert.ToDecimal(
                                reader["MonthSales"])
                                .ToString("N2");

                        lblOrders.Text =
                            Convert.ToInt32(
                                reader["MonthOrders"])
                                .ToString();
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

    }
}
