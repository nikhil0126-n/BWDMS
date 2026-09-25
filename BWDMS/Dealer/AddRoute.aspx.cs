using System;
using System.Data;
using System.Data.SqlClient;
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
                Response.Redirect("~/Account/Login.aspx");
                return;
            }


            // ========================================================
            // FIRST LOAD ONLY
            // ========================================================

            if (!IsPostBack)
            {
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
                        DayOfWeek,
                        IsActive
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


                            // Route Code
                            if (reader["RouteCode"] == DBNull.Value)
                            {
                                txtRouteCode.Text = "";
                            }
                            else
                            {
                                txtRouteCode.Text =
                                    reader["RouteCode"].ToString();
                            }


                            // Day
                            string day =
                                reader["DayOfWeek"] == DBNull.Value
                                    ? ""
                                    : reader["DayOfWeek"].ToString();


                            if (ddlDayOfWeek.Items.FindByValue(day)
                                != null)
                            {
                                ddlDayOfWeek.SelectedValue =
                                    day;
                            }


                            // Status
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
                    "Error loading route: " + ex.Message,
                    false);
            }
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


            try
            {
                int dealerId =
                    Convert.ToInt32(Session["UserId"]);


                string routeName =
                    txtRouteName.Text.Trim();


                string routeCode =
                    txtRouteCode.Text.Trim();


                string dayOfWeek =
                    ddlDayOfWeek.SelectedValue;


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
                    // CHECK DUPLICATE ROUTE CODE
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
                                    "This route code already exists.",
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
                                DayOfWeek = @DayOfWeek,
                                IsActive = @IsActive,
                                UpdatedAt = GETDATE()
                            WHERE RouteId = @RouteId
                            AND DealerId = @DealerId";


                        using (SqlCommand cmd =
                            new SqlCommand(
                                updateQuery,
                                con))
                        {
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
                                "@DayOfWeek",
                                SqlDbType.NVarChar,
                                20).Value =
                                dayOfWeek;


                            cmd.Parameters.Add(
                                "@IsActive",
                                SqlDbType.Bit).Value =
                                isActive;


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
                                DayOfWeek,
                                IsActive,
                                CreatedAt
                            )
                            VALUES
                            (
                                @DealerId,
                                @RouteName,
                                @RouteCode,
                                @DayOfWeek,
                                @IsActive,
                                GETDATE()
                            )";


                        using (SqlCommand cmd =
                            new SqlCommand(
                                insertQuery,
                                con))
                        {
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
                                "@DayOfWeek",
                                SqlDbType.NVarChar,
                                20).Value =
                                dayOfWeek;


                            cmd.Parameters.Add(
                                "@IsActive",
                                SqlDbType.Bit).Value =
                                isActive;


                            cmd.ExecuteNonQuery();
                        }
                    }
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