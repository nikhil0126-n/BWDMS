using System;
using System.Data;
using System.Data.SqlClient;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class Vehicles : System.Web.UI.Page
    {
        // ============================================================
        // PAGE LOAD
        // ============================================================

        protected void Page_Load(object sender, EventArgs e)
        {
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
                Response.Redirect(
                    BWDMS.Data.AppAuth.HomeUrl(Session["UserRole"]));
                return;
            }


            if (!IsPostBack)
            {
                LoadVehicles();

                ShowQueryMessage();
            }
        }


        // ============================================================
        // ?saved=1 / ?deleted=1 FEEDBACK AFTER REDIRECT
        // ============================================================

        private void ShowQueryMessage()
        {
            string saved = Request.QueryString["saved"];

            if (saved == "1")
            {
                ShowMessage("Vehicle saved successfully.", true);
            }
        }


        // ============================================================
        // LOAD VEHICLES
        //
        // A dealer sees his OWN vehicles plus the shared company
        // vehicles (DealerId IS NULL). Only his own rows are
        // editable; shared rows are shown read-only.
        // ============================================================

        private void LoadVehicles()
        {
            try
            {
                int dealerId =
                    Convert.ToInt32(Session["UserId"]);


                string query = @"
                    SELECT
                        v.VehicleId,
                        v.VehicleNumber,
                        v.VehicleName,
                        v.VehicleType,
                        v.IsActive,
                        v.DealerId,
                        OwnerText =
                            CASE
                                WHEN v.DealerId IS NULL
                                    THEN 'Company'
                                WHEN v.DealerId = @DealerId
                                    THEN 'Your Dealership'
                                ELSE 'Other Dealer'
                            END,
                        UsedByText =
                        (
                            SELECT COUNT(*)
                            FROM RouteSchedules rs
                            WHERE rs.VehicleId = v.VehicleId
                        )
                        FROM Vehicles v
                        WHERE v.DealerId = @DealerId
                           OR v.DealerId IS NULL
                    ORDER BY
                        CASE WHEN v.DealerId = @DealerId
                             THEN 0 ELSE 1 END,
                        v.VehicleNumber";


                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    con.Open();

                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@DealerId",
                            SqlDbType.Int).Value =
                            dealerId;

                        using (SqlDataAdapter da =
                            new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();

                            da.Fill(dt);

                            gvVehicles.DataSource = dt;
                            gvVehicles.DataBind();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error loading vehicles: " +
                    ex.Message,
                    false);
            }
        }


        // ============================================================
        // ROW COMMAND - DELETE VEHICLE (soft delete)
        // ============================================================

        protected void gvVehicles_RowCommand(
            object sender,
            System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (e.CommandName != "DeleteVehicle")
            {
                return;
            }

            try
            {
                int vehicleId =
                    Convert.ToInt32(e.CommandArgument);

                int dealerId =
                    Convert.ToInt32(Session["UserId"]);

                string query = @"
                    UPDATE Vehicles
                    SET IsActive = 0
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
                            SqlDbType.Int).Value = vehicleId;

                        cmd.Parameters.Add(
                            "@DealerId",
                            SqlDbType.Int).Value = dealerId;

                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage("Vehicle deleted.", true);

                LoadVehicles();
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error deleting vehicle: " +
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

            lblMessage.Text = message;

            pnlMessage.CssClass = success
                ? "alert alert-success mb-4"
                : "alert alert-danger mb-4";
        }

        // ------------------------------------------------------------
        // PAGING
        //
        // The new index has to be applied before the grid is rebound,
        // otherwise the postback lands back on the page it came from.
        // ------------------------------------------------------------

        protected void gvVehicles_PageIndexChanging(
            object sender,
            System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            gvVehicles.PageIndex = e.NewPageIndex;

            LoadVehicles();
        }

    }
}
