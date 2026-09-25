
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class AddWeeklyRoutePlan : System.Web.UI.Page
    {
        private int? PlanId
        {
            get
            {
                int id;

                if (int.TryParse(Request.QueryString["id"], out id))
                {
                    return id;
                }

                return null;
            }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            // ============================================
            // PREVENT BROWSER CACHE
            // ============================================

            Response.Cache.SetCacheability(
                HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            Response.Cache.SetExpires(
                DateTime.UtcNow.AddDays(-1));

            Response.Cache.SetRevalidation(
                HttpCacheRevalidation.AllCaches);


            // ============================================
            // CHECK LOGIN
            // ============================================

            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }


            // ============================================
            // CHECK DEALER ROLE
            // ============================================

            if (Session["UserRole"] == null ||
                Session["UserRole"].ToString() != "Dealer")
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }


            if (!IsPostBack)
            {
                LoadRoutes();
                LoadVillages();


                if (PlanId.HasValue)
                {
                    lblPageTitle.Text =
                        "Edit Weekly Route Plan";

                    btnSavePlan.Text =
                        "Update Weekly Plan";

                    LoadPlanForEdit(PlanId.Value);
                }
                else
                {
                    lblPageTitle.Text =
                        "Add Weekly Route Plan";

                    btnSavePlan.Text =
                        "Save Weekly Plan";

                    ddlStatus.SelectedValue = "1";
                }
            }
        }


        // ============================================
        // LOAD DEALER ROUTES
        // ============================================

        private void LoadRoutes()
        {
            int dealerId =
                Convert.ToInt32(Session["UserId"]);


            string query = @"
                SELECT
                    RouteId,
                    RouteCode,
                    RouteName
                FROM Routes
                WHERE DealerId = @DealerId
                AND IsActive = 1
                ORDER BY RouteName";


            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value =
                        dealerId;


                    con.Open();


                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        ddlRoute.DataSource = reader;

                        ddlRoute.DataTextField =
                            "RouteName";

                        ddlRoute.DataValueField =
                            "RouteId";

                        ddlRoute.DataBind();
                    }
                }
            }


            ddlRoute.Items.Insert(
                0,
                new ListItem(
                    "-- Select Route --",
                    "0"));
        }


        // ============================================
        // LOAD DEALER VILLAGES
        // ============================================

        private void LoadVillages()
        {
            string query = @"
                SELECT
                    VillageId,
                    VillageName,
                    Taluka,
                    District
                FROM Villages
                WHERE IsActive = 1
                ORDER BY VillageName";


            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    con.Open();


                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        cblVillages.DataSource =
                            reader;

                        cblVillages.DataTextField =
                            "VillageName";

                        cblVillages.DataValueField =
                            "VillageId";

                        cblVillages.DataBind();
                    }
                }
            }
        }


        // ============================================
        // LOAD EXISTING PLAN FOR EDIT
        // ============================================

        private void LoadPlanForEdit(int planId)
        {
            int dealerId =
                Convert.ToInt32(Session["UserId"]);


            string query = @"
                SELECT
                    RouteId,
                    WeekStartDate,
                    WeekEndDate,
                    IsActive
                FROM WeeklyRoutePlans
                WHERE WeeklyRoutePlanId = @PlanId
                AND DealerId = @DealerId";


            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@PlanId",
                        SqlDbType.Int).Value =
                        planId;

                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value =
                        dealerId;


                    con.Open();


                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            Response.Redirect(
                                "~/Dealer/WeeklyRoutePlans.aspx");

                            return;
                        }


                        ddlRoute.SelectedValue =
                            reader["RouteId"].ToString();


                        txtWeekStartDate.Text =
                            Convert.ToDateTime(
                                reader["WeekStartDate"])
                            .ToString("yyyy-MM-dd");


                        txtWeekEndDate.Text =
                            Convert.ToDateTime(
                                reader["WeekEndDate"])
                            .ToString("yyyy-MM-dd");


                        ddlStatus.SelectedValue =
                            Convert.ToBoolean(
                                reader["IsActive"])
                            ? "1"
                            : "0";
                    }
                }
            }


            LoadSelectedVillages(planId);
        }


        // ============================================
        // LOAD SELECTED VILLAGES
        // ============================================

        private void LoadSelectedVillages(int planId)
        {
            string query = @"
                SELECT VillageId
                FROM WeeklyRoutePlanVillages
                WHERE WeeklyRoutePlanId = @PlanId
                ORDER BY VisitSequence";


            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@PlanId",
                        SqlDbType.Int).Value =
                        planId;


                    con.Open();


                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string villageId =
                                reader["VillageId"].ToString();


                            ListItem item =
                                cblVillages.Items.FindByValue(
                                    villageId);


                            if (item != null)
                            {
                                item.Selected = true;
                            }
                        }
                    }
                }
            }
        }


        // ============================================
        // VILLAGE VALIDATION
        // ============================================

        protected void cvVillages_ServerValidate(
            object source,
            ServerValidateEventArgs args)
        {
            args.IsValid = false;


            foreach (ListItem item in cblVillages.Items)
            {
                if (item.Selected)
                {
                    args.IsValid = true;
                    break;
                }
            }
        }


        // ============================================
        // SAVE OR UPDATE PLAN
        // ============================================

        protected void btnSavePlan_Click(
            object sender,
            EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }


            DateTime startDate;
            DateTime endDate;


            if (!DateTime.TryParse(
                txtWeekStartDate.Text,
                out startDate) ||
                !DateTime.TryParse(
                txtWeekEndDate.Text,
                out endDate))
            {
                ShowMessage(
                    "Please enter valid dates.",
                    "text-danger");

                return;
            }


            if (endDate < startDate)
            {
                ShowMessage(
                    "Week end date cannot be before week start date.",
                    "text-danger");

                return;
            }


            int dealerId =
                Convert.ToInt32(Session["UserId"]);

            int routeId =
                Convert.ToInt32(ddlRoute.SelectedValue);

            bool isActive =
                ddlStatus.SelectedValue == "1";


            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();


                SqlTransaction transaction =
                    con.BeginTransaction();


                try
                {
                    int savedPlanId;


                    if (PlanId.HasValue)
                    {
                        savedPlanId =
                            UpdatePlan(
                                con,
                                transaction,
                                dealerId,
                                routeId,
                                startDate,
                                endDate,
                                isActive,
                                PlanId.Value);
                    }
                    else
                    {
                        savedPlanId =
                            InsertPlan(
                                con,
                                transaction,
                                dealerId,
                                routeId,
                                startDate,
                                endDate,
                                isActive);
                    }


                    DeletePlanVillages(
                        con,
                        transaction,
                        savedPlanId);


                    InsertPlanVillages(
                        con,
                        transaction,
                        savedPlanId);


                    transaction.Commit();

                    Response.Redirect(
                        "~/Dealer/WeeklyRoutePlans.aspx",
                        false);

                    Context.ApplicationInstance.CompleteRequest();

                    return;
                }
                catch (Exception ex)
                {
                    // Rollback only if the transaction is still active
                    try
                    {
                        if (transaction != null &&
                            transaction.Connection != null)
                        {
                            transaction.Rollback();
                        }
                    }
                    catch
                    {
                        // Ignore rollback errors
                    }

                    ShowMessage(
                        "Error saving plan: " + ex.Message,
                        "text-danger");
                }
            }
        }


        // ============================================
        // INSERT PLAN
        // ============================================

        private int InsertPlan(
            SqlConnection con,
            SqlTransaction transaction,
            int dealerId,
            int routeId,
            DateTime startDate,
            DateTime endDate,
            bool isActive)
        {
            string query = @"
                INSERT INTO WeeklyRoutePlans
                (
                    DealerId,
                    RouteId,
                    WeekStartDate,
                    WeekEndDate,
                    IsActive,
                    CreatedAt
                )
                OUTPUT INSERTED.WeeklyRoutePlanId
                VALUES
                (
                    @DealerId,
                    @RouteId,
                    @WeekStartDate,
                    @WeekEndDate,
                    @IsActive,
                    GETDATE()
                )";


            using (SqlCommand cmd =
                new SqlCommand(
                    query,
                    con,
                    transaction))
            {
                AddPlanParameters(
                    cmd,
                    dealerId,
                    routeId,
                    startDate,
                    endDate,
                    isActive);


                return Convert.ToInt32(
                    cmd.ExecuteScalar());
            }
        }


        // ============================================
        // UPDATE PLAN
        // ============================================

        private int UpdatePlan(
            SqlConnection con,
            SqlTransaction transaction,
            int dealerId,
            int routeId,
            DateTime startDate,
            DateTime endDate,
            bool isActive,
            int planId)
        {
            string query = @"
                UPDATE WeeklyRoutePlans
                SET
                    RouteId = @RouteId,
                    WeekStartDate = @WeekStartDate,
                    WeekEndDate = @WeekEndDate,
                    IsActive = @IsActive,
                    UpdatedAt = GETDATE()
                WHERE WeeklyRoutePlanId = @PlanId
                AND DealerId = @DealerId";


            using (SqlCommand cmd =
                new SqlCommand(
                    query,
                    con,
                    transaction))
            {
                AddPlanParameters(
                    cmd,
                    dealerId,
                    routeId,
                    startDate,
                    endDate,
                    isActive);


                cmd.Parameters.Add(
                    "@PlanId",
                    SqlDbType.Int).Value =
                    planId;


                int affectedRows =
                    cmd.ExecuteNonQuery();


                if (affectedRows == 0)
                {
                    throw new Exception(
                        "Weekly route plan was not found.");
                }


                return planId;
            }
        }


        // ============================================
        // ADD COMMON PARAMETERS
        // ============================================

        private void AddPlanParameters(
            SqlCommand cmd,
            int dealerId,
            int routeId,
            DateTime startDate,
            DateTime endDate,
            bool isActive)
        {
            cmd.Parameters.Add(
                "@DealerId",
                SqlDbType.Int).Value =
                dealerId;

            cmd.Parameters.Add(
                "@RouteId",
                SqlDbType.Int).Value =
                routeId;

            cmd.Parameters.Add(
                "@WeekStartDate",
                SqlDbType.Date).Value =
                startDate.Date;

            cmd.Parameters.Add(
                "@WeekEndDate",
                SqlDbType.Date).Value =
                endDate.Date;

            cmd.Parameters.Add(
                "@IsActive",
                SqlDbType.Bit).Value =
                isActive;
        }


        // ============================================
        // DELETE EXISTING VILLAGES
        // ============================================

        private void DeletePlanVillages(
            SqlConnection con,
            SqlTransaction transaction,
            int planId)
        {
            string query = @"
                DELETE FROM WeeklyRoutePlanVillages
                WHERE WeeklyRoutePlanId = @PlanId";


            using (SqlCommand cmd =
                new SqlCommand(
                    query,
                    con,
                    transaction))
            {
                cmd.Parameters.Add(
                    "@PlanId",
                    SqlDbType.Int).Value =
                    planId;


                cmd.ExecuteNonQuery();
            }
        }


        // ============================================
        // INSERT PLAN VILLAGES
        // ============================================

        private void InsertPlanVillages(
            SqlConnection con,
            SqlTransaction transaction,
            int planId)
        {
            int sequence = 1;


            foreach (ListItem item in cblVillages.Items)
            {
                if (!item.Selected)
                {
                    continue;
                }


                string query = @"
                    INSERT INTO WeeklyRoutePlanVillages
                    (
                        WeeklyRoutePlanId,
                        VillageId,
                        VisitSequence,
                        CreatedAt
                    )
                    VALUES
                    (
                        @PlanId,
                        @VillageId,
                        @VisitSequence,
                        GETDATE()
                    )";


                using (SqlCommand cmd =
                    new SqlCommand(
                        query,
                        con,
                        transaction))
                {
                    cmd.Parameters.Add(
                        "@PlanId",
                        SqlDbType.Int).Value =
                        planId;

                    cmd.Parameters.Add(
                        "@VillageId",
                        SqlDbType.Int).Value =
                        Convert.ToInt32(item.Value);

                    cmd.Parameters.Add(
                        "@VisitSequence",
                        SqlDbType.Int).Value =
                        sequence;


                    cmd.ExecuteNonQuery();
                }


                sequence++;
            }
        }


        // ============================================
        // SHOW MESSAGE
        // ============================================

        private void ShowMessage(
            string message,
            string cssClass)
        {
            lblMessage.Text =
                HttpUtility.HtmlEncode(message);

            lblMessage.CssClass =
                "d-block mt-3 " + cssClass;

            lblMessage.Visible = true;
        }
    }
}