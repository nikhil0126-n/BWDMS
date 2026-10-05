using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    // ============================================================
    // ROUTE VILLAGE ASSIGNMENT  (Routes / Villages module - Phase 3)
    //
    // Permanently assigns villages to ONE route.
    //
    // Business rules implemented on this page:
    //
    //   * A village lives on exactly ONE route (RouteVillages).
    //   * The tick order inside the list becomes VisitSequence 1..N.
    //   * Everything is scoped to the logged-in dealer (DealerId).
    //   * Several tables are written inside ONE SqlTransaction.
    //
    // NOTE:
    // RouteVillageSchedules.RouteVillageId is a foreign key to
    // RouteVillages.RouteVillageId, so every schedule visit that
    // points at a row we delete must be removed in the same
    // transaction, otherwise the delete would fail.
    // ============================================================

    public partial class RouteVillageAssignment : System.Web.UI.Page
    {
        // ============================================================
        // PAGE LOAD
        // ============================================================

        protected void Page_Load(object sender, EventArgs e)
        {
            // --------------------------------------------------------
            // Prevent browser cache
            // --------------------------------------------------------

            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            Response.Cache.SetExpires(
                DateTime.UtcNow.AddDays(-1));

            Response.Cache.SetRevalidation(
                HttpCacheRevalidation.AllCaches);


            // --------------------------------------------------------
            // Check login
            // --------------------------------------------------------

            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Account/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }


            // --------------------------------------------------------
            // Wrong role -> send the user to THEIR dashboard.
            // Only a missing session goes to the login page, so a
            // salesman clicking a dealer link keeps his session.
            // --------------------------------------------------------

            if (Session["UserRole"] == null ||
                Session["UserRole"].ToString() != "Dealer")
            {
                Response.Redirect(
                    BWDMS.Data.AppAuth.HomeUrl(
                        Session["UserRole"]),
                    false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }


            // --------------------------------------------------------
            // First load only
            //
            // Routes + villages are bound once; the list items are
            // kept in ViewState from then on.
            // --------------------------------------------------------

            if (!IsPostBack)
            {
                LoadRoutes();


                BindVillageList();


                // ----------------------------------------------------
                // No route selected yet -> pick the first one so the
                // page shows the current assignment straight away.
                // ----------------------------------------------------

                if (ddlRoute.Items.Count > 1)
                {
                    ddlRoute.SelectedIndex = 1;
                }


                // ----------------------------------------------------
                // ?route=<id>  -> open on the route that was just saved
                // ----------------------------------------------------

                int routeFromUrl;

                if (int.TryParse(
                        Request.QueryString["route"],
                        out routeFromUrl) &&
                    routeFromUrl > 0 &&
                    ddlRoute.Items.FindByValue(
                        routeFromUrl.ToString()) != null)
                {
                    ddlRoute.SelectedValue =
                        routeFromUrl.ToString();
                }


                LoadAssignedVillages();


                // Friendly hint when the dealer has no active route
                pnlNoRoutes.Visible = (ddlRoute.Items.Count <= 1);


                // ----------------------------------------------------
                // ?saved=1 -> confirm the save that just happened
                // ----------------------------------------------------

                if (Request.QueryString["saved"] == "1")
                {
                    ShowMessage(
                        "Village assignment saved.",
                        true);
                }
            }
        }


        // ============================================================
        // SESSION GUARD FOR EVENT HANDLERS
        //
        // Response.Redirect(url, false) does not stop the page
        // life cycle, so every event handler checks the session
        // again before touching the database.
        // ============================================================

        private bool IsDealerSessionValid()
        {
            return Session["UserId"] != null &&
                   Session["UserRole"] != null &&
                   Session["UserRole"].ToString() == "Dealer";
        }


        // ============================================================
        // LOAD DEALER ROUTES  (active routes only)
        // ============================================================

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


        // ============================================================
        // BIND THE VILLAGE CHECKBOX LIST
        //
        // Villages are global (no DealerId on the table), only the
        // assignment rows are dealer scoped.
        //
        // Label format:  VillageName - Taluka, District
        // ============================================================

        private void BindVillageList()
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


            DataTable dt = new DataTable();


            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    con.Open();


                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }


            cblVillages.Items.Clear();


            foreach (DataRow row in dt.Rows)
            {
                int villageId =
                    Convert.ToInt32(row["VillageId"]);


                string villageName =
                    row["VillageName"].ToString();


                string taluka =
                    row["Taluka"] == DBNull.Value
                        ? ""
                        : row["Taluka"].ToString().Trim();


                string district =
                    row["District"] == DBNull.Value
                        ? ""
                        : row["District"].ToString().Trim();


                cblVillages.Items.Add(
                    new ListItem(
                        BuildVillageLabel(
                            villageName,
                            taluka,
                            district),
                        villageId.ToString()));
            }
        }


        // ============================================================
        // BUILD "VillageName - Taluka, District"
        // ============================================================

        private string BuildVillageLabel(
            string villageName,
            string taluka,
            string district)
        {
            string location = "";


            if (taluka.Length > 0 && district.Length > 0)
            {
                location = taluka + ", " + district;
            }
            else if (taluka.Length > 0)
            {
                location = taluka;
            }
            else if (district.Length > 0)
            {
                location = district;
            }


            if (location.Length == 0)
            {
                return villageName;
            }


            // "VillageName \u2014 Taluka, District"
            return villageName + " \u2014 " + location;
        }


        // ============================================================
        // LOAD THE VILLAGES ALREADY ASSIGNED TO THE SELECTED ROUTE
        // ============================================================

        private void LoadAssignedVillages()
        {
            int routeId;

            if (!int.TryParse(
                ddlRoute.SelectedValue,
                out routeId) ||
                routeId <= 0)
            {
                return;
            }


            int dealerId =
                Convert.ToInt32(Session["UserId"]);


            string query = @"
                SELECT rv.VillageId
                FROM RouteVillages rv
                INNER JOIN Routes r
                    ON rv.RouteId = r.RouteId
                    AND r.DealerId = @DealerId
                WHERE rv.RouteId = @RouteId
                AND rv.IsActive = 1
                ORDER BY rv.VisitSequence";


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

                    cmd.Parameters.Add(
                        "@RouteId",
                        SqlDbType.Int).Value =
                        routeId;


                    con.Open();


                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TickVillage(
                                reader["VillageId"].ToString());
                        }
                    }
                }
            }
        }


        // ============================================================
        // TICK / UNTICK HELPERS
        // ============================================================

        private void TickVillage(string villageId)
        {
            ListItem item =
                cblVillages.Items.FindByValue(villageId);


            if (item != null)
            {
                item.Selected = true;
            }
        }


        private void ClearVillageSelections()
        {
            foreach (ListItem item in cblVillages.Items)
            {
                item.Selected = false;
            }
        }


        private List<int> GetSelectedVillageIds()
        {
            List<int> selected = new List<int>();


            foreach (ListItem item in cblVillages.Items)
            {
                if (item.Selected)
                {
                    selected.Add(
                        Convert.ToInt32(item.Value));
                }
            }


            return selected;
        }


        private void ApplySelectedVillageIds(
            List<int> villageIds)
        {
            foreach (int villageId in villageIds)
            {
                TickVillage(villageId.ToString());
            }
        }


        // ============================================================
        // ROUTE CHANGED -> RE-TICK THE VILLAGES OF THAT ROUTE
        // ============================================================

        protected void ddlRoute_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            if (!IsDealerSessionValid())
            {
                return;
            }


            ClearVillageSelections();

            LoadAssignedVillages();
        }


        // ============================================================
        // VILLAGE VALIDATION (at least one tick)
        // ============================================================

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


        // ============================================================
        // QUICK ADD VILLAGE
        //
        // Creates the village, then refreshes the checkbox list
        // keeping the route assignment AND the current ticks.
        // ============================================================

        protected void btnAddVillage_Click(
            object sender,
            EventArgs e)
        {
            if (!IsDealerSessionValid())
            {
                return;
            }


            string villageName =
                txtNewVillage.Text.Trim();


            if (villageName.Length == 0)
            {
                ShowMessage(
                    "Please enter a village name.",
                    false);

                return;
            }


            try
            {
                int newVillageId = 0;


                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    con.Open();


                    // ------------------------------------------------
                    // DUPLICATE NAME CHECK
                    // ------------------------------------------------

                    string checkQuery = @"
                        SELECT COUNT(*)
                        FROM Villages
                        WHERE VillageName = @VillageName";


                    using (SqlCommand cmd =
                        new SqlCommand(checkQuery, con))
                    {
                        cmd.Parameters.Add(
                            "@VillageName",
                            SqlDbType.NVarChar,
                            300).Value =
                            villageName;


                        int count =
                            Convert.ToInt32(
                                cmd.ExecuteScalar());


                        if (count > 0)
                        {
                            ShowMessage(
                                "A village with this name already exists.",
                                false);

                            return;
                        }
                    }


                    // ------------------------------------------------
                    // INSERT VILLAGE  (Taluka / District / Pincode
                    // can be filled in from the Villages page)
                    // ------------------------------------------------

                    string insertQuery = @"
                        INSERT INTO Villages
                        (
                            VillageName,
                            IsActive,
                            CreatedAt
                        )
                        OUTPUT INSERTED.VillageId
                        VALUES
                        (
                            @VillageName,
                            1,
                            GETDATE()
                        )";


                    using (SqlCommand cmd =
                        new SqlCommand(insertQuery, con))
                    {
                        cmd.Parameters.Add(
                            "@VillageName",
                            SqlDbType.NVarChar,
                            300).Value =
                            villageName;


                        newVillageId =
                            Convert.ToInt32(
                                cmd.ExecuteScalar());
                    }


                    // ------------------------------------------------
                    // REFRESH THE LIST AND KEEP THE TICKS
                    // ------------------------------------------------

                    List<int> selected =
                        GetSelectedVillageIds();


                    BindVillageList();

                    LoadAssignedVillages();

                    ApplySelectedVillageIds(selected);


                    // The brand new village is ticked for this route
                    TickVillage(
                        newVillageId.ToString());
                }


                txtNewVillage.Text = "";


                ShowMessage(
                    "Village added and ticked for this route.",
                    true);
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error adding village: " + ex.Message,
                    false);
            }
        }


        // ============================================================
        // SAVE THE ASSIGNMENT
        //
        // Inside ONE SqlTransaction:
        //   1. verify the route belongs to this dealer
        //   2. delete the schedule visits pointing at this route's
        //      RouteVillages rows (foreign key)
        //   3. delete every RouteVillages row of this route
        //   4. insert the ticked villages in displayed order with
        //      VisitSequence = 1..N
        // ============================================================

        protected void btnSave_Click(
            object sender,
            EventArgs e)
        {
            if (!IsDealerSessionValid())
            {
                return;
            }


            if (!Page.IsValid)
            {
                return;
            }


            int dealerId =
                Convert.ToInt32(Session["UserId"]);


            int routeId;

            if (!int.TryParse(
                ddlRoute.SelectedValue,
                out routeId) ||
                routeId <= 0)
            {
                ShowMessage(
                    "Please select a route.",
                    false);

                return;
            }


            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();


                SqlTransaction transaction =
                    con.BeginTransaction();


                try
                {
                    // =============================================
                    // ROUTE MUST BELONG TO THIS DEALER
                    // =============================================

                    if (!RouteBelongsToDealer(
                        con,
                        transaction,
                        dealerId,
                        routeId))
                    {
                        throw new Exception(
                            "Route was not found.");
                    }


                    // =============================================
                    // CURRENT ASSIGNMENT OF THIS ROUTE
                    //
                    // Saved as a diff instead of delete-all so that
                    // villages which stay on the route keep their
                    // RouteVillages row - and therefore keep the
                    // weekday order stored in RouteVillageSchedules.
                    // =============================================

                    Dictionary<int, int> existing =
                        GetRouteVillages(
                            con,
                            transaction,
                            routeId);


                    HashSet<int> kept = new HashSet<int>();

                    int sequence = 1;


                    // =============================================
                    // INSERT OR RESEQUENCE THE TICKED VILLAGES
                    // =============================================

                    foreach (ListItem item in cblVillages.Items)
                    {
                        if (!item.Selected)
                        {
                            continue;
                        }


                        int villageId =
                            Convert.ToInt32(item.Value);


                        // A village belongs to exactly one route
                        RemoveVillageFromOtherRoutes(
                            con,
                            transaction,
                            dealerId,
                            routeId,
                            villageId);


                        if (existing.ContainsKey(villageId))
                        {
                            // Still on this route -> only the
                            // visit order may have changed.
                            UpdateRouteVillageSequence(
                                con,
                                transaction,
                                existing[villageId],
                                sequence,
                                dealerId);
                        }
                        else
                        {
                            InsertRouteVillage(
                                con,
                                transaction,
                                routeId,
                                villageId,
                                sequence,
                                dealerId);
                        }


                        kept.Add(villageId);
                        sequence++;
                    }


                    // =============================================
                    // DROP ONLY THE VILLAGES THAT WERE UNTICKED
                    // =============================================

                    foreach (KeyValuePair<int, int> pair
                        in existing)
                    {
                        if (kept.Contains(pair.Key))
                        {
                            continue;
                        }


                        DeleteRouteScheduleVisits(
                            con,
                            transaction,
                            pair.Value);


                        DeleteRouteVillage(
                            con,
                            transaction,
                            pair.Value);
                    }


                    transaction.Commit();


                    // =============================================
                    // SUCCESS
                    // =============================================

                    Response.Redirect(
                        "~/Dealer/RouteVillageAssignment.aspx" +
                        "?route=" + routeId +
                        "&saved=1",
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
                        "Error saving route villages: " + ex.Message,
                        false);
                }
            }
        }


        // ============================================================
        // ROUTE OWNERSHIP CHECK
        // ============================================================

        private bool RouteBelongsToDealer(
            SqlConnection con,
            SqlTransaction transaction,
            int dealerId,
            int routeId)
        {
            string query = @"
                SELECT COUNT(*)
                FROM Routes
                WHERE RouteId = @RouteId
                AND DealerId = @DealerId";


            using (SqlCommand cmd =
                new SqlCommand(query, con, transaction))
            {
                cmd.Parameters.Add(
                    "@RouteId",
                    SqlDbType.Int).Value =
                    routeId;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value =
                    dealerId;


                return Convert.ToInt32(
                    cmd.ExecuteScalar()) > 0;
            }
        }


        // ============================================================
        // READ THE ROUTE'S CURRENT ASSIGNMENT
        //
        // VillageId -> RouteVillageId, so the save can be applied
        // as a diff instead of deleting and rebuilding everything.
        // ============================================================

        private Dictionary<int, int> GetRouteVillages(
            SqlConnection con,
            SqlTransaction transaction,
            int routeId)
        {
            string query = @"
                SELECT VillageId, RouteVillageId
                FROM RouteVillages
                WHERE RouteId = @RouteId";

            Dictionary<int, int> result =
                new Dictionary<int, int>();


            using (SqlCommand cmd =
                new SqlCommand(query, con, transaction))
            {
                cmd.Parameters.Add(
                    "@RouteId",
                    SqlDbType.Int).Value =
                    routeId;


                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result[reader.GetInt32(0)] =
                            reader.GetInt32(1);
                    }
                }
            }

            return result;
        }


        // ============================================================
        // DELETE THE SCHEDULE VISITS OF ONE VILLAGE
        //
        // Required because RouteVillageSchedules.RouteVillageId
        // references RouteVillages.RouteVillageId.
        // ============================================================

        private void DeleteRouteScheduleVisits(
            SqlConnection con,
            SqlTransaction transaction,
            int routeVillageId)
        {
            string query = @"
                DELETE FROM RouteVillageSchedules
                WHERE RouteVillageId = @RouteVillageId";


            using (SqlCommand cmd =
                new SqlCommand(query, con, transaction))
            {
                cmd.Parameters.Add(
                    "@RouteVillageId",
                    SqlDbType.Int).Value =
                    routeVillageId;


                cmd.ExecuteNonQuery();
            }
        }


        // ============================================================
        // DELETE ONE RouteVillages ROW
        // ============================================================

        private void DeleteRouteVillage(
            SqlConnection con,
            SqlTransaction transaction,
            int routeVillageId)
        {
            string query = @"
                DELETE FROM RouteVillages
                WHERE RouteVillageId = @RouteVillageId";


            using (SqlCommand cmd =
                new SqlCommand(query, con, transaction))
            {
                cmd.Parameters.Add(
                    "@RouteVillageId",
                    SqlDbType.Int).Value =
                    routeVillageId;


                cmd.ExecuteNonQuery();
            }
        }


        // ============================================================
        // RESEQUENCE A VILLAGE THAT STAYS ON THIS ROUTE
        //
        // The RouteVillages row (and its schedule visits) is left
        // untouched apart from the new visit order.
        // ============================================================

        private void UpdateRouteVillageSequence(
            SqlConnection con,
            SqlTransaction transaction,
            int routeVillageId,
            int visitSequence,
            int updatedBy)
        {
            string query = @"
                UPDATE RouteVillages
                SET VisitSequence = @VisitSequence,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE RouteVillageId = @RouteVillageId";


            using (SqlCommand cmd =
                new SqlCommand(query, con, transaction))
            {
                cmd.Parameters.Add(
                    "@VisitSequence",
                    SqlDbType.Int).Value =
                    visitSequence;

                cmd.Parameters.Add(
                    "@UpdatedBy",
                    SqlDbType.Int).Value =
                    updatedBy;

                cmd.Parameters.Add(
                    "@RouteVillageId",
                    SqlDbType.Int).Value =
                    routeVillageId;


                cmd.ExecuteNonQuery();
            }
        }


        // ============================================================
        // KEEP A VILLAGE ON EXACTLY ONE ROUTE
        //
        // When a village is ticked here, any older row that links
        // the same village to ANOTHER route of this dealer is
        // removed (together with its schedule visits, which would
        // otherwise violate the foreign key).
        // ============================================================

        private void RemoveVillageFromOtherRoutes(
            SqlConnection con,
            SqlTransaction transaction,
            int dealerId,
            int routeId,
            int villageId)
        {
            string deleteVisitsQuery = @"
                DELETE rvs
                FROM RouteVillageSchedules rvs
                INNER JOIN RouteVillages rv
                    ON rvs.RouteVillageId = rv.RouteVillageId
                INNER JOIN Routes r
                    ON rv.RouteId = r.RouteId
                WHERE rv.VillageId = @VillageId
                AND rv.RouteId <> @RouteId
                AND r.DealerId = @DealerId";


            using (SqlCommand cmd =
                new SqlCommand(
                    deleteVisitsQuery,
                    con,
                    transaction))
            {
                cmd.Parameters.Add(
                    "@VillageId",
                    SqlDbType.Int).Value =
                    villageId;

                cmd.Parameters.Add(
                    "@RouteId",
                    SqlDbType.Int).Value =
                    routeId;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value =
                    dealerId;


                cmd.ExecuteNonQuery();
            }


            string deleteRowQuery = @"
                DELETE rv
                FROM RouteVillages rv
                INNER JOIN Routes r
                    ON rv.RouteId = r.RouteId
                WHERE rv.VillageId = @VillageId
                AND rv.RouteId <> @RouteId
                AND r.DealerId = @DealerId";


            using (SqlCommand cmd =
                new SqlCommand(
                    deleteRowQuery,
                    con,
                    transaction))
            {
                cmd.Parameters.Add(
                    "@VillageId",
                    SqlDbType.Int).Value =
                    villageId;

                cmd.Parameters.Add(
                    "@RouteId",
                    SqlDbType.Int).Value =
                    routeId;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value =
                    dealerId;


                cmd.ExecuteNonQuery();
            }
        }


        // ============================================================
        // INSERT ONE RouteVillages ROW
        // ============================================================

        private void InsertRouteVillage(
            SqlConnection con,
            SqlTransaction transaction,
            int routeId,
            int villageId,
            int visitSequence,
            int createdBy)
        {
            string query = @"
                INSERT INTO RouteVillages
                (
                    RouteId,
                    VillageId,
                    IsActive,
                    VisitSequence,
                    CreatedBy,
                    CreatedAt
                )
                VALUES
                (
                    @RouteId,
                    @VillageId,
                    1,
                    @VisitSequence,
                    @CreatedBy,
                    GETDATE()
                )";


            using (SqlCommand cmd =
                new SqlCommand(query, con, transaction))
            {
                cmd.Parameters.Add(
                    "@RouteId",
                    SqlDbType.Int).Value =
                    routeId;

                cmd.Parameters.Add(
                    "@VillageId",
                    SqlDbType.Int).Value =
                    villageId;

                cmd.Parameters.Add(
                    "@VisitSequence",
                    SqlDbType.Int).Value =
                    visitSequence;

                cmd.Parameters.Add(
                    "@CreatedBy",
                    SqlDbType.Int).Value =
                    createdBy;


                cmd.ExecuteNonQuery();
            }
        }


        // ============================================================
        // SHOW MESSAGE
        // ============================================================

        private void ShowMessage(
            string message,
            bool success)
        {
            lblMessage.Text =
                HttpUtility.HtmlEncode(message);

            lblMessage.CssClass =
                "d-block mb-3 " +
                (success ? "text-success" : "text-danger");

            lblMessage.Visible = true;
        }
    }
}
