using System;
using System.Data;
using System.Data.SqlClient;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class AddVillage : System.Web.UI.Page
    {
        // ============================================================
        // PAGE LOAD
        // ============================================================

        protected void Page_Load(object sender, EventArgs e)
        {
            // --------------------------------------------------------
            // Prevent browser cache
            // --------------------------------------------------------

            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            Response.Cache.SetExpires(
                DateTime.UtcNow.AddDays(-1));

            Response.Cache.SetRevalidation(
                System.Web.HttpCacheRevalidation.AllCaches);


            // --------------------------------------------------------
            // Check login
            // --------------------------------------------------------

            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }


            // --------------------------------------------------------
            // Check Dealer role
            // --------------------------------------------------------

            if (Session["UserRole"] == null ||
                Session["UserRole"].ToString() != "Dealer")
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }


            // --------------------------------------------------------
            // First page load
            // --------------------------------------------------------

            if (!IsPostBack)
            {
                string id =
                    Request.QueryString["id"];


                // ====================================================
                // ADD MODE
                // ====================================================

                if (string.IsNullOrWhiteSpace(id))
                {
                    SetAddMode();
                }


                // ====================================================
                // EDIT MODE
                // ====================================================

                else
                {
                    int villageId;


                    if (int.TryParse(id, out villageId))
                    {
                        LoadVillage(villageId);
                    }
                    else
                    {
                        Response.Redirect(
                            "~/Dealer/Villages.aspx");
                    }
                }
            }
        }


        // ============================================================
        // ADD MODE
        // ============================================================

        private void SetAddMode()
        {
            lblPageTitle.Text =
                "Add Village";

            lblHeading.Text =
                "Add Village";

            lblSubHeading.Text =
                "Add a new village for route planning";

            lblFormTitle.Text =
                "Village Information";

            btnSaveVillage.Text =
                "Create Village";
        }


        // ============================================================
        // EDIT MODE
        // ============================================================

        private void SetEditMode()
        {
            lblPageTitle.Text =
                "Edit Village";

            lblHeading.Text =
                "Edit Village";

            lblSubHeading.Text =
                "Update village information";

            lblFormTitle.Text =
                "Village Information";

            btnSaveVillage.Text =
                "Update Village";
        }


        // ============================================================
        // LOAD EXISTING VILLAGE
        // ============================================================

        private void LoadVillage(int villageId)
        {
            try
            {
                string query = @"
                    SELECT
                        VillageId,
                        VillageName,
                        Taluka,
                        District,
                        Pincode,
                        IsActive
                    FROM Villages
                    WHERE VillageId = @VillageId";


                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    con.Open();


                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@VillageId",
                            SqlDbType.Int).Value =
                            villageId;


                        using (SqlDataReader reader =
                            cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                Response.Redirect(
                                    "~/Dealer/Villages.aspx");

                                return;
                            }


                            // ------------------------------------------------
                            // Edit mode
                            // ------------------------------------------------

                            SetEditMode();


                            // ------------------------------------------------
                            // Village name
                            // ------------------------------------------------

                            txtVillageName.Text =
                                reader["VillageName"].ToString();


                            // ------------------------------------------------
                            // Taluka
                            // ------------------------------------------------

                            txtTaluka.Text =
                                reader["Taluka"] == DBNull.Value
                                    ? ""
                                    : reader["Taluka"].ToString();


                            // ------------------------------------------------
                            // District
                            // ------------------------------------------------

                            txtDistrict.Text =
                                reader["District"] == DBNull.Value
                                    ? ""
                                    : reader["District"].ToString();


                            // ------------------------------------------------
                            // Pincode
                            // ------------------------------------------------

                            txtPincode.Text =
                                reader["Pincode"] == DBNull.Value
                                    ? ""
                                    : reader["Pincode"].ToString();


                            // ------------------------------------------------
                            // Status
                            // ------------------------------------------------

                            bool isActive =
                                Convert.ToBoolean(
                                    reader["IsActive"]);


                            ddlStatus.SelectedValue =
                                isActive ? "1" : "0";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error loading village: " +
                    ex.Message,
                    false);
            }
        }


        // ============================================================
        // SAVE VILLAGE
        // ============================================================

        protected void btnSaveVillage_Click(
            object sender,
            EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }


            try
            {
                string villageName =
                    txtVillageName.Text.Trim();


                string taluka =
                    txtTaluka.Text.Trim();


                string district =
                    txtDistrict.Text.Trim();


                string pincode =
                    txtPincode.Text.Trim();


                bool isActive =
                    ddlStatus.SelectedValue == "1";


                // ----------------------------------------------------
                // Check Add/Edit mode
                // ----------------------------------------------------

                string id =
                    Request.QueryString["id"];


                int villageId = 0;


                bool isEdit =
                    int.TryParse(
                        id,
                        out villageId);


                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    con.Open();


                    // =================================================
                    // CHECK DUPLICATE VILLAGE
                    // =================================================

                    string checkQuery = @"
                        SELECT COUNT(*)
                        FROM Villages
                        WHERE VillageName = @VillageName";


                    if (isEdit)
                    {
                        checkQuery +=
                            " AND VillageId <> @VillageId";
                    }


                    using (SqlCommand cmd =
                        new SqlCommand(
                            checkQuery,
                            con))
                    {
                        cmd.Parameters.Add(
                            "@VillageName",
                            SqlDbType.NVarChar,
                            150).Value =
                            villageName;


                        if (isEdit)
                        {
                            cmd.Parameters.Add(
                                "@VillageId",
                                SqlDbType.Int).Value =
                                villageId;
                        }


                        int count =
                            Convert.ToInt32(
                                cmd.ExecuteScalar());


                        if (count > 0)
                        {
                            ShowMessage(
                                "This village already exists.",
                                false);

                            return;
                        }
                    }


                    // =================================================
                    // UPDATE
                    // =================================================

                    if (isEdit)
                    {
                        string updateQuery = @"
                            UPDATE Villages
                            SET
                                VillageName = @VillageName,
                                Taluka = @Taluka,
                                District = @District,
                                Pincode = @Pincode,
                                IsActive = @IsActive,
                                UpdatedAt = GETDATE()
                            WHERE VillageId = @VillageId";


                        using (SqlCommand cmd =
                            new SqlCommand(
                                updateQuery,
                                con))
                        {
                            cmd.Parameters.Add(
                                "@VillageName",
                                SqlDbType.NVarChar,
                                150).Value =
                                villageName;


                            cmd.Parameters.Add(
                                "@Taluka",
                                SqlDbType.NVarChar,
                                100).Value =
                                string.IsNullOrWhiteSpace(taluka)
                                    ? (object)DBNull.Value
                                    : taluka;


                            cmd.Parameters.Add(
                                "@District",
                                SqlDbType.NVarChar,
                                100).Value =
                                string.IsNullOrWhiteSpace(district)
                                    ? (object)DBNull.Value
                                    : district;


                            cmd.Parameters.Add(
                                "@Pincode",
                                SqlDbType.NVarChar,
                                10).Value =
                                string.IsNullOrWhiteSpace(pincode)
                                    ? (object)DBNull.Value
                                    : pincode;


                            cmd.Parameters.Add(
                                "@IsActive",
                                SqlDbType.Bit).Value =
                                isActive;


                            cmd.Parameters.Add(
                                "@VillageId",
                                SqlDbType.Int).Value =
                                villageId;


                            int rows =
                                cmd.ExecuteNonQuery();


                            if (rows == 0)
                            {
                                ShowMessage(
                                    "Village could not be updated.",
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
                            INSERT INTO Villages
                            (
                                VillageName,
                                Taluka,
                                District,
                                Pincode,
                                IsActive,
                                CreatedAt
                            )
                            VALUES
                            (
                                @VillageName,
                                @Taluka,
                                @District,
                                @Pincode,
                                @IsActive,
                                GETDATE()
                            )";


                        using (SqlCommand cmd =
                            new SqlCommand(
                                insertQuery,
                                con))
                        {
                            cmd.Parameters.Add(
                                "@VillageName",
                                SqlDbType.NVarChar,
                                150).Value =
                                villageName;


                            cmd.Parameters.Add(
                                "@Taluka",
                                SqlDbType.NVarChar,
                                100).Value =
                                string.IsNullOrWhiteSpace(taluka)
                                    ? (object)DBNull.Value
                                    : taluka;


                            cmd.Parameters.Add(
                                "@District",
                                SqlDbType.NVarChar,
                                100).Value =
                                string.IsNullOrWhiteSpace(district)
                                    ? (object)DBNull.Value
                                    : district;


                            cmd.Parameters.Add(
                                "@Pincode",
                                SqlDbType.NVarChar,
                                10).Value =
                                string.IsNullOrWhiteSpace(pincode)
                                    ? (object)DBNull.Value
                                    : pincode;


                            cmd.Parameters.Add(
                                "@IsActive",
                                SqlDbType.Bit).Value =
                                isActive;


                            cmd.ExecuteNonQuery();
                        }
                    }
                }


                // ----------------------------------------------------
                // Return to village list
                // ----------------------------------------------------

                Response.Redirect(
                    "~/Dealer/Villages.aspx");
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error saving village: " +
                    ex.Message,
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

            lblMessage.Text =
                message;


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