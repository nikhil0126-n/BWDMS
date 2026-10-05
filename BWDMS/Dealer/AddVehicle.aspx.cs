using System;
using System.Data;
using System.Data.SqlClient;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class AddVehicle : System.Web.UI.Page
    {
        // ============================================================
        // EDIT ID (0 = Add mode)
        // ============================================================

        private int VehicleId
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


        // ============================================================
        // PAGE LOAD
        // ============================================================

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            if (Session["UserRole"] == null ||
                Session["UserRole"].ToString() != "Dealer")
            {
                Response.Redirect(
                    BWDMS.Data.AppAuth.HomeUrl(Session["UserRole"]));
                return;
            }


            if (!IsPostBack)
            {
                if (VehicleId > 0)
                {
                    SetEditMode();

                    LoadVehicle();
                }
                else
                {
                    SetAddMode();
                }
            }
        }


        private void SetAddMode()
        {
            lblPageTitle.Text = "Add Vehicle";
            lblHeading.Text = "Add Vehicle";
            lblSubHeading.Text =
                "Register a truck or tempo for route dispatch";
            lblFormTitle.Text = "Vehicle Information";
            btnSaveVehicle.Text = "Create Vehicle";
        }

        private void SetEditMode()
        {
            lblPageTitle.Text = "Edit Vehicle";
            lblHeading.Text = "Edit Vehicle";
            lblSubHeading.Text = "Update vehicle information";
            lblFormTitle.Text = "Vehicle Information";
            btnSaveVehicle.Text = "Update Vehicle";
        }


        // ============================================================
        // LOAD EXISTING VEHICLE
        //
        // The row must belong to this dealer, otherwise the id is
        // forged or the vehicle is a shared company vehicle that
        // this dealer may only read.
        // ============================================================

        private void LoadVehicle()
        {
            try
            {
                string query = @"
                    SELECT
                        VehicleNumber,
                        VehicleName,
                        VehicleType,
                        IsActive
                    FROM Vehicles
                    WHERE VehicleId = @VehicleId
                    AND DealerId = @DealerId";

                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    con.Open();

                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@VehicleId",
                            SqlDbType.Int).Value =
                            VehicleId;

                        cmd.Parameters.Add(
                            "@DealerId",
                            SqlDbType.Int).Value =
                            DealerId;

                        using (SqlDataReader reader =
                            cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                Response.Redirect(
                                    "~/Dealer/Vehicles.aspx");
                                return;
                            }

                            SetEditMode();

                            txtVehicleNumber.Text =
                                reader["VehicleNumber"].ToString();

                            txtVehicleName.Text =
                                reader["VehicleName"] == DBNull.Value
                                    ? ""
                                    : reader["VehicleName"].ToString();

                            txtVehicleType.Text =
                                reader["VehicleType"] == DBNull.Value
                                    ? ""
                                    : reader["VehicleType"].ToString();

                            ddlStatus.SelectedValue =
                                Convert.ToBoolean(reader["IsActive"])
                                    ? "1"
                                    : "0";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error loading vehicle: " +
                    ex.Message,
                    false);
            }
        }


        // ============================================================
        // SAVE
        // ============================================================

        protected void btnSaveVehicle_Click(
            object sender,
            EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            string vehicleNumber =
                txtVehicleNumber.Text.Trim();

            string vehicleName =
                txtVehicleName.Text.Trim();

            string vehicleType =
                txtVehicleType.Text.Trim();

            bool isActive =
                ddlStatus.SelectedValue == "1";

            bool isEdit = VehicleId > 0;


            try
            {
                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    con.Open();


                    // =================================================
                    // DUPLICATE CHECK
                    //
                    // VehicleNumber has a global unique index, so
                    // the check covers every dealer's vehicles.
                    // =================================================

                    string checkQuery = @"
                        SELECT COUNT(*)
                        FROM Vehicles
                        WHERE VehicleNumber = @VehicleNumber";

                    if (isEdit)
                    {
                        checkQuery +=
                            " AND VehicleId <> @VehicleId";
                    }

                    using (SqlCommand cmd =
                        new SqlCommand(checkQuery, con))
                    {
                        cmd.Parameters.Add(
                            "@VehicleNumber",
                            SqlDbType.NVarChar,
                            100).Value =
                            vehicleNumber;

                        if (isEdit)
                        {
                            cmd.Parameters.Add(
                                "@VehicleId",
                                SqlDbType.Int).Value =
                                VehicleId;
                        }

                        int count =
                            Convert.ToInt32(
                                cmd.ExecuteScalar());

                        if (count > 0)
                        {
                            ShowMessage(
                                "A vehicle with this number already exists. " +
                                "Vehicle numbers must be unique.",
                                false);

                            return;
                        }
                    }


                    if (isEdit)
                    {
                        UpdateVehicle(
                            con,
                            vehicleNumber,
                            vehicleName,
                            vehicleType,
                            isActive);
                    }
                    else
                    {
                        InsertVehicle(
                            con,
                            vehicleNumber,
                            vehicleName,
                            vehicleType,
                            isActive);
                    }
                }

                Response.Redirect(
                    "~/Dealer/Vehicles.aspx?saved=1");
            }
            catch (SqlException ex)
                when (ex.Number == 2601 ||
                      ex.Number == 2627)
            {
                // Unique index race condition.
                ShowMessage(
                    "A vehicle with this number already exists.",
                    false);
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error saving vehicle: " +
                    ex.Message,
                    false);
            }
        }


        private void InsertVehicle(
            SqlConnection con,
            string vehicleNumber,
            string vehicleName,
            string vehicleType,
            bool isActive)
        {
            string query = @"
                INSERT INTO Vehicles
                (
                    VehicleNumber,
                    VehicleName,
                    VehicleType,
                    IsActive,
                    DealerId,
                    CreatedBy,
                    CreatedAt
                )
                VALUES
                (
                    @VehicleNumber,
                    @VehicleName,
                    @VehicleType,
                    @IsActive,
                    @DealerId,
                    @CreatedBy,
                    GETDATE()
                )";

            using (SqlCommand cmd =
                new SqlCommand(query, con))
            {
                AddVehicleParameters(
                    cmd,
                    vehicleNumber,
                    vehicleName,
                    vehicleType,
                    isActive);

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value =
                    DealerId;

                cmd.Parameters.Add(
                    "@CreatedBy",
                    SqlDbType.Int).Value =
                    DealerId;

                cmd.ExecuteNonQuery();
            }
        }


        private void UpdateVehicle(
            SqlConnection con,
            string vehicleNumber,
            string vehicleName,
            string vehicleType,
            bool isActive)
        {
            // DealerId in WHERE blocks editing another dealer's
            // vehicle by guessing the id.
            string query = @"
                UPDATE Vehicles
                SET
                    VehicleNumber = @VehicleNumber,
                    VehicleName = @VehicleName,
                    VehicleType = @VehicleType,
                    IsActive = @IsActive,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE VehicleId = @VehicleId
                AND DealerId = @DealerId";

            using (SqlCommand cmd =
                new SqlCommand(query, con))
            {
                AddVehicleParameters(
                    cmd,
                    vehicleNumber,
                    vehicleName,
                    vehicleType,
                    isActive);

                cmd.Parameters.Add(
                    "@VehicleId",
                    SqlDbType.Int).Value =
                    VehicleId;

                cmd.Parameters.Add(
                    "@UpdatedBy",
                    SqlDbType.Int).Value =
                    DealerId;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value =
                    DealerId;

                int rows = cmd.ExecuteNonQuery();

                if (rows == 0)
                {
                    Response.Redirect(
                        "~/Dealer/Vehicles.aspx");
                }
            }
        }


        private void AddVehicleParameters(
            SqlCommand cmd,
            string vehicleNumber,
            string vehicleName,
            string vehicleType,
            bool isActive)
        {
            cmd.Parameters.Add(
                "@VehicleNumber",
                SqlDbType.NVarChar,
                100).Value =
                vehicleNumber;

            cmd.Parameters.Add(
                "@VehicleName",
                SqlDbType.NVarChar,
                200).Value =
                string.IsNullOrWhiteSpace(vehicleName)
                    ? (object)DBNull.Value
                    : vehicleName;

            cmd.Parameters.Add(
                "@VehicleType",
                SqlDbType.NVarChar,
                100).Value =
                string.IsNullOrWhiteSpace(vehicleType)
                    ? (object)DBNull.Value
                    : vehicleType;

            cmd.Parameters.Add(
                "@IsActive",
                SqlDbType.Bit).Value =
                isActive;
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

            pnlMessage.CssClass = success
                ? "alert alert-success mb-4"
                : "alert alert-danger mb-4";
        }
    }
}
