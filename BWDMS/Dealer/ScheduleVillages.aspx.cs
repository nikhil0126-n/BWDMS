using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    // ============================================================
    // SCHEDULE VILLAGES  (Routes / Villages module - Phase 3)
    //
    // Chooses which villages ONE schedule (RouteSchedules row)
    // visits, and in which order.
    //
    // Business rules implemented on this page:
    //
    //   * RouteVillageSchedules NEVER stores its own village, it
    //     always points at an existing RouteVillages row.
    //   * Only villages permanently assigned to the schedule's
    //     route may be written - the posted values are re-validated
    //     against the database on every save.
    //   * Every schedule keeps its own VisitSequence 1..N.
    //   * Everything is scoped to the logged-in dealer (DealerId)
    //     and written inside ONE SqlTransaction.
    //
    // POSTBACK NOTE:
    // Repeater items are NOT kept in view state, so the list has to
    // be rebuilt on every postback. Binding happens in Page_Init,
    // BEFORE LoadViewState / LoadPostData, so the visit order the
    // dealer typed is still applied afterwards by the framework
    // and is never overwritten by the database values.
    // ============================================================

    public partial class ScheduleVillages : System.Web.UI.Page
    {
        // ============================================================
        // REQUIRED QUERY STRING  ?id=<RouteScheduleId>
        // ============================================================

        private int? ScheduleId
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

                return null;
            }
        }


        // Filled in Page_Init, so it is always fresh for the
        // current request (no view state involved).

        private bool scheduleFound = false;

        private int scheduleRouteId = 0;

        private bool routeHasVillages = false;

        private bool villagesBound = false;


        // ============================================================
        // PAGE INIT - BIND THE REPEATER
        // ============================================================

        protected void Page_Init(object sender, EventArgs e)
        {
            // --------------------------------------------------------
            // The real guards (and the redirect) live in Page_Load.
            // Here we only avoid touching the database when the
            // session or the id is invalid.
            // --------------------------------------------------------

            if (Session["UserId"] == null)
            {
                return;
            }


            if (Session["UserRole"] == null ||
                Session["UserRole"].ToString() != "Dealer")
            {
                return;
            }


            if (!ScheduleId.HasValue)
            {
                return;
            }


            // --------------------------------------------------------
            // Schedule summary (sets scheduleRouteId)
            // --------------------------------------------------------

            scheduleFound = LoadSchedule();


            if (!scheduleFound)
            {
                return;
            }


            // --------------------------------------------------------
            // Villages permanently assigned to this route
            // --------------------------------------------------------

            LoadVillageRows();

            villagesBound = true;
        }


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
                Response.Redirect(
                    "~/Account/Login.aspx",
                    false);

                Context.ApplicationInstance.CompleteRequest();
                return;
            }


            // --------------------------------------------------------
            // Check Dealer role
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
            // The id is required - redirect when it is missing or
            // when it does not belong to this dealer.
            // --------------------------------------------------------

            if (!ScheduleId.HasValue)
            {
                Response.Redirect(
                    "~/Dealer/RouteSchedules.aspx",
                    false);

                Context.ApplicationInstance.CompleteRequest();
                return;
            }


            // --------------------------------------------------------
            // Safety net: normally the list is built in Page_Init.
            // If that did not happen, build it now instead of
            // rendering an empty page.
            // --------------------------------------------------------

            if (!villagesBound)
            {
                scheduleFound = LoadSchedule();


                if (scheduleFound)
                {
                    LoadVillageRows();

                    villagesBound = true;
                }
            }


            if (!scheduleFound)
            {
                Response.Redirect(
                    "~/Dealer/RouteSchedules.aspx",
                    false);

                Context.ApplicationInstance.CompleteRequest();
                return;
            }


            // --------------------------------------------------------
            // Make sure the ticks and the typed visit order posted
            // by the dealer win over the database values.
            // --------------------------------------------------------

            RestorePostedValues();


            // --------------------------------------------------------
            // A route without villages cannot plan a schedule day
            // --------------------------------------------------------

            if (!routeHasVillages)
            {
                pnlNoVillages.Visible = true;

                pnlVillagePicker.Visible = false;

                btnSave.Enabled = false;
            }
        }


        // ============================================================
        // SESSION GUARD FOR EVENT HANDLERS
        //
        // Response.Redirect(url, false) does not stop the page
        // life cycle, so every event handler checks again before
        // touching the database.
        // ============================================================

        private bool IsDealerSessionValid()
        {
            return Session["UserId"] != null &&
                   Session["UserRole"] != null &&
                   Session["UserRole"].ToString() == "Dealer";
        }


        // ============================================================
        // RE-APPLY THE POSTED TICKS / VISIT ORDER
        //
        // Page_Init binds the list BEFORE LoadPostData, so the
        // framework normally does this for us. Doing it again here
        // is harmless (the values are identical) and keeps the page
        // correct even when the list had to be rebuilt later.
        // ============================================================

        private void RestorePostedValues()
        {
            if (!IsPostBack)
            {
                return;
            }


            foreach (RepeaterItem item in repVillages.Items)
            {
                if (item.ItemType != ListItemType.Item &&
                    item.ItemType != ListItemType.AlternatingItem)
                {
                    continue;
                }


                CheckBox chk =
                    item.FindControl("chkSelect")
                        as CheckBox;

                TextBox txt =
                    item.FindControl("txtSequence")
                        as TextBox;


                if (chk == null || txt == null)
                {
                    continue;
                }


                // A ticked checkbox is posted, an unticked one is not
                chk.Checked =
                    (Request.Form[chk.UniqueID] != null);


                string posted =
                    Request.Form[txt.UniqueID];


                if (posted != null)
                {
                    txt.Text = posted;
                }
            }
        }


        // ============================================================
        // LOAD THE SCHEDULE SUMMARY (dealer scoped)
        // ============================================================

        private bool LoadSchedule()
        {
            int dealerId =
                Convert.ToInt32(Session["UserId"]);

            int scheduleId = ScheduleId.Value;


            string query = @"
                SELECT
                    rs.RouteId,
                    ISNULL(rs.DayOfWeek, 'Not Set')
                        AS DayOfWeek,

                    r.RouteName,
                    r.RouteCode,

                    ISNULL(v.VehicleNumber, 'Not Assigned')
                        AS VehicleNumber,

                    ISNULL(s.FullName, 'Not Assigned')
                        AS SalesmanName,

                    ISNULL(d.FullName, 'Not Assigned')
                        AS DriverName

                FROM RouteSchedules rs

                INNER JOIN Routes r
                    ON rs.RouteId = r.RouteId
                    AND r.DealerId = @DealerId

                LEFT JOIN Vehicles v
                    ON rs.VehicleId = v.VehicleId

                LEFT JOIN Users s
                    ON rs.SalesmanId = s.UserId

                LEFT JOIN Users d
                    ON rs.DriverId = d.UserId

                WHERE rs.RouteScheduleId = @ScheduleId";


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
                        "@ScheduleId",
                        SqlDbType.Int).Value =
                        scheduleId;


                    con.Open();


                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return false;
                        }


                        scheduleRouteId =
                            Convert.ToInt32(
                                reader["RouteId"]);


                        string routeName =
                            reader["RouteName"].ToString();

                        string routeCode =
                            reader["RouteCode"] == DBNull.Value
                                ? ""
                                : reader["RouteCode"].ToString();


                        lblRouteName.Text = routeCode.Length == 0
                            ? routeName
                            : routeName + " (" + routeCode + ")";


                        lblDayOfWeek.Text =
                            reader["DayOfWeek"].ToString();

                        lblVehicleNumber.Text =
                            reader["VehicleNumber"].ToString();

                        lblSalesmanName.Text =
                            reader["SalesmanName"].ToString();

                        lblDriverName.Text =
                            reader["DriverName"].ToString();
                    }
                }
            }


            return true;
        }


        // ============================================================
        // LOAD THE ROUTE'S VILLAGES + THE CURRENT SCHEDULE VISITS
        // ============================================================

        private void LoadVillageRows()
        {
            int dealerId =
                Convert.ToInt32(Session["UserId"]);

            int scheduleId = ScheduleId.Value;

            int routeId = scheduleRouteId;


            // --------------------------------------------------------
            // Permanent villages of the route (ordered by their
            // permanent sequence) + the visit sequence this very
            // schedule already uses, when there is one.
            // --------------------------------------------------------

            string query = @"
                SELECT
                    rv.RouteVillageId,
                    rv.VisitSequence,

                    v.VillageName,
                    v.Taluka,
                    v.District,

                    rvs.VisitSequence AS ScheduleSequence

                FROM RouteVillages rv

                INNER JOIN Routes r
                    ON rv.RouteId = r.RouteId
                    AND r.DealerId = @DealerId

                INNER JOIN Villages v
                    ON rv.VillageId = v.VillageId

                LEFT JOIN RouteVillageSchedules rvs
                    ON rvs.RouteVillageId = rv.RouteVillageId
                    AND rvs.RouteScheduleId = @ScheduleId
                    AND rvs.IsActive = 1

                WHERE rv.RouteId = @RouteId
                AND rv.IsActive = 1

                ORDER BY rv.VisitSequence";


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
                        dealerId;

                    cmd.Parameters.Add(
                        "@ScheduleId",
                        SqlDbType.Int).Value =
                        scheduleId;

                    cmd.Parameters.Add(
                        "@RouteId",
                        SqlDbType.Int).Value =
                        routeId;


                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }


            // --------------------------------------------------------
            // Display columns used inside the item template
            // --------------------------------------------------------

            dt.Columns.Add(
                "DisplayText",
                typeof(string));

            dt.Columns.Add(
                "SequenceValue",
                typeof(string));

            dt.Columns.Add(
                "IsScheduled",
                typeof(bool));


            foreach (DataRow row in dt.Rows)
            {
                int permanentSequence =
                    Convert.ToInt32(row["VisitSequence"]);


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


                // "3. VillageName - Taluka, District"
                row["DisplayText"] =
                    permanentSequence.ToString() + ". " +
                    BuildVillageLabel(
                        villageName,
                        taluka,
                        district);


                // Pre-fill: the schedule sequence when this village
                // is already scheduled, otherwise its permanent one.
                bool isScheduled =
                    row["ScheduleSequence"] != DBNull.Value;


                row["IsScheduled"] = isScheduled;

                row["SequenceValue"] = isScheduled
                    ? Convert.ToInt32(
                        row["ScheduleSequence"]).ToString()
                    : permanentSequence.ToString();
            }


            repVillages.DataSource = dt;

            repVillages.DataBind();


            routeHasVillages = (dt.Rows.Count > 0);
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
        // ONE POSTED VILLAGE ROW
        // ============================================================

        private class ScheduleVillageRow
        {
            public int RouteVillageId;

            public int Sequence;
        }


        // ============================================================
        // SAVE THE SCHEDULE VILLAGES
        //
        // Inside ONE SqlTransaction:
        //   1. verify the schedule still belongs to this dealer
        //   2. re-validate every posted RouteVillageId against the
        //      route's permanent RouteVillages rows
        //   3. delete the schedule's current visits
        //   4. insert one row per ticked village, ordered by the
        //      typed sequence
        // ============================================================

        protected void btnSave_Click(
            object sender,
            EventArgs e)
        {
            if (!IsDealerSessionValid())
            {
                return;
            }


            if (!ScheduleId.HasValue ||
                !scheduleFound ||
                !routeHasVillages)
            {
                return;
            }


            if (!Page.IsValid)
            {
                return;
            }


            int dealerId =
                Convert.ToInt32(Session["UserId"]);

            int scheduleId = ScheduleId.Value;

            int routeId = scheduleRouteId;


            // --------------------------------------------------------
            // READ THE POSTED ROWS
            // --------------------------------------------------------

            List<ScheduleVillageRow> rows =
                new List<ScheduleVillageRow>();


            int position = 0;


            foreach (RepeaterItem item in repVillages.Items)
            {
                if (item.ItemType != ListItemType.Item &&
                    item.ItemType != ListItemType.AlternatingItem)
                {
                    continue;
                }


                position++;


                CheckBox chk =
                    item.FindControl("chkSelect")
                        as CheckBox;

                HiddenField hf =
                    item.FindControl("hfRouteVillageId")
                        as HiddenField;

                TextBox txt =
                    item.FindControl("txtSequence")
                        as TextBox;


                if (chk == null ||
                    hf == null ||
                    txt == null)
                {
                    continue;
                }


                if (!chk.Checked)
                {
                    continue;
                }


                int routeVillageId;

                if (!int.TryParse(
                    hf.Value,
                    out routeVillageId) ||
                    routeVillageId <= 0)
                {
                    // Never trust a posted value blindly
                    continue;
                }


                int sequence;

                string typed =
                    txt.Text.Trim();


                if (typed.Length == 0)
                {
                    // Blank -> fall back to the displayed position
                    sequence = position;
                }
                else if (!int.TryParse(typed, out sequence) ||
                         sequence < 1)
                {
                    ShowMessage(
                        "Visit order must be a whole number of 1 or more.",
                        false);

                    return;
                }


                rows.Add(
                    new ScheduleVillageRow
                    {
                        RouteVillageId = routeVillageId,
                        Sequence = sequence
                    });
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
                    // SCHEDULE MUST BELONG TO THIS DEALER
                    // =============================================

                    if (!ScheduleBelongsToDealer(
                        con,
                        transaction,
                        dealerId,
                        scheduleId))
                    {
                        throw new Exception(
                            "Route schedule was not found.");
                    }


                    // =============================================
                    // SERVER SIDE RE-VALIDATION
                    //
                    // Only villages permanently assigned to this
                    // schedule's route may be written.
                    // =============================================

                    HashSet<int> allowed =
                        LoadAllowedRouteVillageIds(
                            con,
                            transaction,
                            dealerId,
                            routeId);


                    List<ScheduleVillageRow> validRows =
                        rows
                            .Where(r => allowed.Contains(
                                r.RouteVillageId))
                            .OrderBy(r => r.Sequence)
                            .ToList();


                    // =============================================
                    // DELETE THE SCHEDULE'S CURRENT VISITS
                    // =============================================

                    DeleteScheduleVillages(
                        con,
                        transaction,
                        scheduleId);


                    // =============================================
                    // INSERT THE TICKED VILLAGES
                    // =============================================

                    foreach (ScheduleVillageRow row in validRows)
                    {
                        InsertScheduleVillage(
                            con,
                            transaction,
                            scheduleId,
                            row.RouteVillageId,
                            row.Sequence,
                            dealerId);
                    }


                    transaction.Commit();


                    // =============================================
                    // SUCCESS
                    // =============================================

                    Response.Redirect(
                        "~/Dealer/RouteSchedules.aspx",
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
                        "Error saving schedule villages: " + ex.Message,
                        false);
                }
            }
        }


        // ============================================================
        // SCHEDULE OWNERSHIP CHECK
        // ============================================================

        private bool ScheduleBelongsToDealer(
            SqlConnection con,
            SqlTransaction transaction,
            int dealerId,
            int scheduleId)
        {
            string query = @"
                SELECT COUNT(*)
                FROM RouteSchedules rs
                INNER JOIN Routes r
                    ON rs.RouteId = r.RouteId
                WHERE rs.RouteScheduleId = @ScheduleId
                AND r.DealerId = @DealerId";


            using (SqlCommand cmd =
                new SqlCommand(query, con, transaction))
            {
                cmd.Parameters.Add(
                    "@ScheduleId",
                    SqlDbType.Int).Value =
                    scheduleId;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value =
                    dealerId;


                return Convert.ToInt32(
                    cmd.ExecuteScalar()) > 0;
            }
        }


        // ============================================================
        // THE ROUTE'S PERMANENT RouteVillages ROWS
        // ============================================================

        private HashSet<int> LoadAllowedRouteVillageIds(
            SqlConnection con,
            SqlTransaction transaction,
            int dealerId,
            int routeId)
        {
            string query = @"
                SELECT rv.RouteVillageId
                FROM RouteVillages rv
                INNER JOIN Routes r
                    ON rv.RouteId = r.RouteId
                    AND r.DealerId = @DealerId
                WHERE rv.RouteId = @RouteId
                AND rv.IsActive = 1";


            HashSet<int> allowed =
                new HashSet<int>();


            using (SqlCommand cmd =
                new SqlCommand(query, con, transaction))
            {
                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value =
                    dealerId;

                cmd.Parameters.Add(
                    "@RouteId",
                    SqlDbType.Int).Value =
                    routeId;


                using (SqlDataReader reader =
                    cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        allowed.Add(
                            Convert.ToInt32(
                                reader["RouteVillageId"]));
                    }
                }
            }


            return allowed;
        }


        // ============================================================
        // DELETE THE SCHEDULE'S CURRENT VISITS
        // ============================================================

        private void DeleteScheduleVillages(
            SqlConnection con,
            SqlTransaction transaction,
            int scheduleId)
        {
            string query = @"
                DELETE FROM RouteVillageSchedules
                WHERE RouteScheduleId = @ScheduleId";


            using (SqlCommand cmd =
                new SqlCommand(query, con, transaction))
            {
                cmd.Parameters.Add(
                    "@ScheduleId",
                    SqlDbType.Int).Value =
                    scheduleId;


                cmd.ExecuteNonQuery();
            }
        }


        // ============================================================
        // INSERT ONE RouteVillageSchedules ROW
        // ============================================================

        private void InsertScheduleVillage(
            SqlConnection con,
            SqlTransaction transaction,
            int scheduleId,
            int routeVillageId,
            int visitSequence,
            int createdBy)
        {
            string query = @"
                INSERT INTO RouteVillageSchedules
                (
                    RouteScheduleId,
                    RouteVillageId,
                    VisitSequence,
                    IsActive,
                    CreatedBy,
                    CreatedAt
                )
                VALUES
                (
                    @ScheduleId,
                    @RouteVillageId,
                    @VisitSequence,
                    1,
                    @CreatedBy,
                    GETDATE()
                )";


            using (SqlCommand cmd =
                new SqlCommand(query, con, transaction))
            {
                cmd.Parameters.Add(
                    "@ScheduleId",
                    SqlDbType.Int).Value =
                    scheduleId;

                cmd.Parameters.Add(
                    "@RouteVillageId",
                    SqlDbType.Int).Value =
                    routeVillageId;

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
            pnlMessage.Visible = true;

            lblMessage.Text =
                HttpUtility.HtmlEncode(message);

            pnlMessage.CssClass =
                "alert mb-4 " +
                (success
                    ? "alert-success"
                    : "alert-danger");
        }
    }
}
