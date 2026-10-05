using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class AddReconciliation : System.Web.UI.Page
    {
        // ============================================================
        // IDS
        //
        // ?id= -> existing VehicleReconciliations row
        // ============================================================

        private int ReconciliationId
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

        // A present but broken id (?id=abc, ?id=999999) must never
        // be treated as "add new".
        private bool HasIdQuery
        {
            get { return Request.QueryString["id"] != null; }
        }

        private int DealerId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }


        // The row loaded in edit mode, so the stored detail counts
        // are only reused while the vehicle + date still match it.
        private int _origVehicleId;
        private string _origDateText = "";
        private readonly Dictionary<int, string> _prefillActuals =
            new Dictionary<int, string>();


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


            if (!IsPostBack)
            {
                LoadVehicles();
            LoadSchedules();
                LoadUsers(ddlSalesman);
                LoadUsers(ddlDriver);

                txtDate.Text = DateTime.Today.ToString(
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture);

                if (HasIdQuery)
                {
                    HeaderRow row = LoadHeader();

                    if (row == null)
                    {
                        // Missing, forged or another dealer's row.
                        Response.Redirect(
                            "~/Dealer/Reconciliations.aspx",
                            false);

                        Context.ApplicationInstance.CompleteRequest();

                        return;
                    }

                    ApplyHeader(row);

                    SetEditMode();
                }
                else
                {
                    SetAddMode();
                }

                BuildDetailRows();
            }
        }


        private void SetAddMode()
        {
            lblPageTitle.Text = "Add Reconciliation";
            lblHeading.Text = "Add Reconciliation";
            lblSubHeading.Text =
                "Count the packets loaded on a vehicle for a day";
            btnSave.Text = "Create Reconciliation";
        }


        private void SetEditMode()
        {
            lblPageTitle.Text = "Edit Reconciliation";
            lblHeading.Text = "Edit Reconciliation";
            lblSubHeading.Text =
                "Update this day's vehicle stock count";
            btnSave.Text = "Update Reconciliation";
        }


        // ============================================================
        // DROPDOWNS
        // ============================================================

        private void LoadVehicles()
        {
            // Shared company vehicles carry DealerId IS NULL and may
            // be used by any dealer.
            string query = @"
                SELECT VehicleId, VehicleNumber
                FROM Vehicles
                WHERE IsActive = 1
                  AND (DealerId = @DealerId
                       OR DealerId IS NULL)
                ORDER BY VehicleNumber";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        ddlVehicle.DataSource = dt;
                        ddlVehicle.DataTextField = "VehicleNumber";
                        ddlVehicle.DataValueField = "VehicleId";
                        ddlVehicle.DataBind();

                        ddlVehicle.Items.Insert(
                            0,
                            new ListItem("-- Select Vehicle --", ""));
                    }
                }
            }
        }


        private void LoadSchedules()
        {
            string query = @"
                SELECT
                    rs.RouteScheduleId,
                    r.RouteName,
                    rs.DayOfWeek,
                    VehicleNumber = ISNULL(v.VehicleNumber, '')
                FROM RouteSchedules rs
                INNER JOIN Routes r
                    ON r.RouteId = rs.RouteId
                LEFT JOIN Vehicles v
                    ON v.VehicleId = rs.VehicleId
                WHERE r.DealerId = @DealerId
                  AND rs.IsActive = 1
                  AND r.IsActive = 1
                ORDER BY r.RouteName, rs.DayOfWeek";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        dt.Columns.Add("Label");

                        foreach (DataRow r in dt.Rows)
                        {
                            string number = Convert.ToString(
                                r["VehicleNumber"]);

                            r["Label"] =
                                Convert.ToString(r["RouteName"]) +
                                " - " +
                                Convert.ToString(r["DayOfWeek"]) +
                                " - " +
                                (number.Length == 0
                                    ? "No vehicle"
                                    : number);
                        }

                        ddlRouteSchedule.DataSource = dt;
                        ddlRouteSchedule.DataTextField = "Label";
                        ddlRouteSchedule.DataValueField =
                            "RouteScheduleId";
                        ddlRouteSchedule.DataBind();

                        ddlRouteSchedule.Items.Insert(
                            0,
                            new ListItem(
                                "All routes for this vehicle",
                                ""));
                    }
                }
            }
        }


        protected void ddlRouteSchedule_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            int vehicleId = VehicleForSchedule(
                SelectedScheduleId());

            if (vehicleId > 0 &&
                ddlVehicle.Items.FindByValue(
                    vehicleId.ToString()) != null)
            {
                ddlVehicle.SelectedValue =
                    vehicleId.ToString();
            }

            // Setting SelectedValue in code does NOT raise the vehicle's
            // own change event, so the expected/actual rows are rebuilt
            // here or the table would keep showing the previous route.
            BuildDetailRows();
        }


        private int SelectedScheduleId()
        {
            int id = 0;

            // The change event can fire before the dropdown's posted
            // value has been applied to the control, so the request is
            // the authoritative source and the control is the fallback.
            if (!string.IsNullOrEmpty(
                ddlRouteSchedule.UniqueID))
            {
                string[] posted =
                    Request.Form.GetValues(
                        ddlRouteSchedule.UniqueID);

                if (posted != null && posted.Length > 0)
                {
                    int.TryParse(posted[0], out id);
                }
            }

            if (id == 0)
            {
                int.TryParse(
                    ddlRouteSchedule.SelectedValue,
                    out id);
            }

            return id;
        }


        private int VehicleForSchedule(int scheduleId)
        {
            if (scheduleId <= 0)
            {
                return 0;
            }

            string query = @"
                SELECT ISNULL(rs.VehicleId, 0)
                FROM RouteSchedules rs
                INNER JOIN Routes r
                    ON r.RouteId = rs.RouteId
                WHERE rs.RouteScheduleId = @RouteScheduleId
                  AND r.DealerId = @DealerId";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                // ExecuteScalar needs an open connection.
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@RouteScheduleId",
                        SqlDbType.Int).Value = scheduleId;

                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    object result = cmd.ExecuteScalar();

                    return result == null ||
                           result == DBNull.Value
                        ? 0
                        : Convert.ToInt32(result);
                }
            }
        }


        private void LoadUsers(DropDownList dropdown)
        {
            string query = @"
                SELECT UserId, FullName
                FROM Users
                WHERE IsActive = 1
                  AND DealerId = @DealerId
                  AND Role IN ('Salesman', 'Driver')
                ORDER BY FullName";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        dropdown.DataSource = dt;
                        dropdown.DataTextField = "FullName";
                        dropdown.DataValueField = "UserId";
                        dropdown.DataBind();

                        dropdown.Items.Insert(
                            0,
                            new ListItem("-- None --", ""));
                    }
                }
            }
        }


        private static void SelectValue(
            DropDownList dropdown,
            string value)
        {
            if (dropdown.Items.FindByValue(value) != null)
            {
                dropdown.SelectedValue = value;
            }
            else if (dropdown.Items.Count > 0)
            {
                dropdown.SelectedIndex = 0;
            }
        }


        // ============================================================
        // LOAD (EDIT MODE)
        // ============================================================

        private sealed class HeaderRow
        {
            public int VehicleId;
            public string DateText;
            public int? SalesmanId;
            public int? DriverId;
            public string Remarks;
            public bool IsApproved;
        }


        // Reads the row plus its stored counts. A forged ?id= or a
        // row belonging to another dealer returns null.
        private HeaderRow LoadHeader()
        {
            string query = @"
                SELECT
                    VehicleId,
                    ReconciliationDateText =
                        CONVERT(NVARCHAR(10), ReconciliationDate, 120),
                    SalesmanId,
                    DriverId,
                    Remarks,
                    IsApproved
                FROM VehicleReconciliations
                WHERE VehicleReconciliationId = @VehicleReconciliationId
                  AND DealerId = @DealerId";

            HeaderRow row = null;

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@VehicleReconciliationId",
                        SqlDbType.Int).Value = ReconciliationId;

                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }

                        row = new HeaderRow
                        {
                            VehicleId =
                                Convert.ToInt32(reader["VehicleId"]),

                            DateText =
                                reader["ReconciliationDateText"]
                                    .ToString(),

                            SalesmanId =
                                reader["SalesmanId"] == DBNull.Value
                                    ? (int?)null
                                    : Convert.ToInt32(
                                        reader["SalesmanId"]),

                            DriverId =
                                reader["DriverId"] == DBNull.Value
                                    ? (int?)null
                                    : Convert.ToInt32(
                                        reader["DriverId"]),

                            Remarks =
                                reader["Remarks"] == DBNull.Value
                                    ? ""
                                    : reader["Remarks"].ToString(),

                            IsApproved =
                                Convert.ToBoolean(reader["IsApproved"])
                        };
                    }
                }
            }

            LoadExistingDetails(row);

            return row;
        }


        private void LoadExistingDetails(HeaderRow header)
        {
            string query = @"
                SELECT ProductVariantId, ActualPackets
                FROM VehicleReconciliationDetails
                WHERE VehicleReconciliationId = @VehicleReconciliationId";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@VehicleReconciliationId",
                        SqlDbType.Int).Value = ReconciliationId;

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int variantId =
                                Convert.ToInt32(
                                    reader["ProductVariantId"]);

                            _prefillActuals[variantId] =
                                Convert.ToInt32(
                                    reader["ActualPackets"])
                                .ToString(
                                    CultureInfo.InvariantCulture);
                        }
                    }
                }
            }

            _origVehicleId = header.VehicleId;
            _origDateText = header.DateText;
        }


        private void ApplyHeader(HeaderRow row)
        {
            txtDate.Text = row.DateText;

            SelectValue(
                ddlVehicle,
                row.VehicleId.ToString());

            SelectValue(
                ddlSalesman,
                row.SalesmanId.HasValue
                    ? row.SalesmanId.Value.ToString()
                    : "");

            SelectValue(
                ddlDriver,
                row.DriverId.HasValue
                    ? row.DriverId.Value.ToString()
                    : "");

            txtRemarks.Text = row.Remarks;

            ddlApproved.SelectedValue =
                row.IsApproved ? "1" : "0";
        }


        // ============================================================
        // DETAIL GRID
        //
        // One row per active variant. ExpectedPackets comes from
        // VehicleStock for the chosen vehicle + date.
        // ============================================================

        private void BuildDetailRows()
        {
            // Values the user already typed survive the rebind
            // triggered by the vehicle / date change.
            Dictionary<int, string> typed =
                CaptureTypedActuals();

            int scheduleId = SelectedScheduleId();

            // When a route is chosen the vehicle comes from that route, so
            // the count is always about the right van. Reading it here
            // rather than from the dropdown keeps the figures correct even
            // if the dropdown itself has not been re-rendered yet.
            int vehicleId = scheduleId > 0
                ? VehicleForSchedule(scheduleId)
                : SelectedVehicleId();

            // A route with no vehicle has nothing on board to count, and
            // an empty table would look like "everything was perfect".
            if (scheduleId > 0 && vehicleId <= 0)
            {
                repDetails.Visible = false;
                pnlNoVariants.Visible = true;

                ShowMessage(
                    "That route has no vehicle assigned, so there is no " +
                    "vehicle stock to count. Assign a vehicle to the " +
                    "route schedule first.",
                    false);

                return;
            }

            DateTime? date = SelectedDate();

            bool usePrefill =
                _prefillActuals.Count > 0
                && vehicleId > 0
                && vehicleId == _origVehicleId
                && date.HasValue
                && date.Value.ToString(
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture) == _origDateText;

            Dictionary<int, int> expected =
                LoadExpected(vehicleId, date);

            DataTable dt = LoadVariants();

            if (dt.Rows.Count == 0)
            {
                repDetails.Visible = false;
                repDetails.DataSource = null;
                repDetails.DataBind();

                pnlNoVariants.Visible = true;

                return;
            }

            repDetails.Visible = true;
            pnlNoVariants.Visible = false;

            dt.Columns.Add("ExpectedPackets", typeof(int));
            dt.Columns.Add("ActualPackets", typeof(string));
            dt.Columns.Add("VariancePackets", typeof(int));
            dt.Columns.Add("VarianceCss", typeof(string));
            dt.Columns.Add("VarianceText", typeof(string));

            foreach (DataRow row in dt.Rows)
            {
                int variantId =
                    Convert.ToInt32(row["ProductVariantId"]);

                int expectedPackets = 0;

                if (expected.ContainsKey(variantId))
                {
                    expectedPackets = expected[variantId];
                }

                row["ExpectedPackets"] = expectedPackets;

                string raw;

                if (!typed.TryGetValue(variantId, out raw))
                {
                    raw = "";

                    if (usePrefill &&
                        _prefillActuals.ContainsKey(variantId))
                    {
                        raw = _prefillActuals[variantId];
                    }
                }

                row["ActualPackets"] = raw;

                // Preview only - the real value is recalculated
                // inside the save transaction.
                int actual = 0;

                if (raw.Length > 0)
                {
                    int.TryParse(raw, out actual);

                    if (actual < 0)
                    {
                        actual = 0;
                    }
                }

                int variance = actual - expectedPackets;

                row["VariancePackets"] = variance;

                row["VarianceCss"] = variance > 0
                    ? "badge bg-warning text-dark"
                    : variance < 0
                        ? "badge bg-danger"
                        : "badge bg-success";

                row["VarianceText"] = variance > 0
                    ? "+" + variance.ToString(
                        CultureInfo.InvariantCulture)
                    : variance.ToString(
                        CultureInfo.InvariantCulture);
            }

            repDetails.DataSource = dt;
            repDetails.DataBind();
        }


        private DataTable LoadVariants()
        {
            string query = @"
                SELECT
                    v.ProductVariantId,
                    p.ProductName,
                    v.VariantName,
                    WeightText =
                        CASE
                            WHEN v.PacketWeight IS NULL THEN ''
                            ELSE CONVERT(NVARCHAR(20), v.PacketWeight)
                                 + ISNULL(' ' + v.WeightUnit, '')
                        END
                FROM ProductVariants v
                INNER JOIN Products p
                    ON p.ProductId = v.ProductId
                WHERE v.IsActive = 1
                  AND p.IsActive = 1
                ORDER BY p.ProductName, v.SortOrder, v.VariantName";

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            return dt;
        }


        // Expected closing stock for this dealer + vehicle + route + date,
        // one entry per variant that was on the vehicle that day (0 when the
        // row does not exist).
        private Dictionary<int, int> LoadExpected(
            int vehicleId,
            DateTime? date)
        {
            var map = new Dictionary<int, int>();

            if (vehicleId <= 0 || !date.HasValue)
            {
                return map;
            }

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(
                        ExpectedQuery(),
                        con))
                {
                    AddExpectedParameters(
                        cmd,
                        vehicleId,
                        date.Value,
                        SelectedScheduleId());

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int variantId =
                                Convert.ToInt32(
                                    reader["ProductVariantId"]);

                            map[variantId] =
                                Convert.ToInt32(
                                    reader["ExpectedPackets"]);
                        }
                    }
                }
            }

            return map;
        }


        private Dictionary<int, int> LoadExpected(
            SqlConnection con,
            SqlTransaction tx,
            int vehicleId,
            DateTime date,
            int scheduleId)
        {
            var map = new Dictionary<int, int>();

            using (SqlCommand cmd =
                new SqlCommand(
                    ExpectedQuery(),
                    con,
                    tx))
            {
                AddExpectedParameters(
                    cmd,
                    vehicleId,
                    date,
                    scheduleId);

                using (SqlDataReader reader =
                    cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int variantId =
                            Convert.ToInt32(
                                reader["ProductVariantId"]);

                        map[variantId] =
                            Convert.ToInt32(
                                reader["ExpectedPackets"]);
                    }
                }
            }

            return map;
        }


        // Expected CLOSING stock for a variant on the vehicle:
        //     LoadedPackets          (what went on at the start of the day)
        //   - SoldPackets            (what the salesman actually billed)
        //   - DamagedPackets
        //   + ReturnedPackets
        //
        // Anything left over that the physical count cannot account for is
        // goods that left the vehicle without a bill - exactly what the
        // end-of-day count exists to catch.
        private static string ExpectedQuery()
        {
            return @"
                SELECT
                    ProductVariantId,
                    ExpectedPackets =
                        ISNULL(LoadedPackets, 0)
                        - ISNULL(SoldPackets, 0)
                        - ISNULL(DamagedPackets, 0)
                        + ISNULL(ReturnedPackets, 0)
                FROM VehicleStock
                WHERE DealerId = @DealerId
                  AND VehicleId = @VehicleId
                  AND StockDate = @StockDate
                  AND (@RouteScheduleId IS NULL
                       OR RouteScheduleId = @RouteScheduleId)";
        }


        private void AddExpectedParameters(
            SqlCommand cmd,
            int vehicleId,
            DateTime date,
            int routeScheduleId)
        {
            cmd.Parameters.Add(
                "@DealerId",
                SqlDbType.Int).Value = DealerId;

            cmd.Parameters.Add(
                "@RouteScheduleId",
                SqlDbType.Int).Value = routeScheduleId > 0
                    ? (object)routeScheduleId
                    : DBNull.Value;

            cmd.Parameters.Add(
                "@VehicleId",
                SqlDbType.Int).Value = vehicleId;

            cmd.Parameters.Add(
                "@StockDate",
                SqlDbType.Date).Value = date;
        }


        private Dictionary<int, string> CaptureTypedActuals()
        {
            var map = new Dictionary<int, string>();

            foreach (RepeaterItem item in repDetails.Items)
            {
                if (item.ItemType != ListItemType.Item &&
                    item.ItemType != ListItemType.AlternatingItem)
                {
                    continue;
                }

                HiddenField hf =
                    item.FindControl("hfVariantId")
                        as HiddenField;

                TextBox txt =
                    item.FindControl("txtActual")
                        as TextBox;

                if (hf == null || txt == null)
                {
                    continue;
                }

                int variantId;

                if (!int.TryParse(hf.Value, out variantId))
                {
                    continue;
                }

                map[variantId] = txt.Text.Trim();
            }

            return map;
        }


        // ============================================================
        // SELECTION
        // ============================================================

        private int SelectedVehicleId()
        {
            int id = 0;

            int.TryParse(ddlVehicle.SelectedValue, out id);

            return id;
        }


        private bool TryGetSelectedDate(out DateTime date)
        {
            string raw = txtDate.Text.Trim();

            if (raw.Length == 0)
            {
                date = default(DateTime);
                return false;
            }

            if (DateTime.TryParseExact(
                raw,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date))
            {
                return true;
            }

            if (DateTime.TryParse(
                raw,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date))
            {
                return true;
            }

            return DateTime.TryParse(raw, out date);
        }


        private DateTime? SelectedDate()
        {
            DateTime date;

            return TryGetSelectedDate(out date)
                ? (DateTime?)date
                : null;
        }


        protected void ddlVehicle_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            BuildDetailRows();
        }


        protected void txtDate_TextChanged(
            object sender,
            EventArgs e)
        {
            BuildDetailRows();
        }


        // ============================================================
        // SAVE
        //
        // 1. Upsert the header (UQ_VehicleReconciliations_Day)
        // 2. Replace that header's detail lines
        // Both happen inside one SqlTransaction.
        // ============================================================

        private sealed class DetailLine
        {
            public int ProductVariantId;
            public int ActualPackets;
        }


        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            DateTime date;

            if (!TryGetSelectedDate(out date))
            {
                ShowMessage(
                    "Enter a valid reconciliation date.",
                    false);

                return;
            }

            int vehicleId = SelectedVehicleId();

            int scheduleId = SelectedScheduleId();

            if (vehicleId <= 0)
            {
                ShowMessage(
                    "Select a vehicle.",
                    false);

                return;
            }

            string remarks = txtRemarks.Text.Trim();

            if (remarks.Length > 1000)
            {
                ShowMessage(
                    "Remarks cannot be longer than 1000 characters.",
                    false);

                return;
            }

            // ------------------------------------
            // Read every detail line from the form.
            // A blank count means "counted zero";
            // anything else must be a whole
            // number of 0 or more.
            // ------------------------------------

            var lines = new List<DetailLine>();

            foreach (RepeaterItem item in repDetails.Items)
            {
                if (item.ItemType != ListItemType.Item &&
                    item.ItemType != ListItemType.AlternatingItem)
                {
                    continue;
                }

                HiddenField hf =
                    item.FindControl("hfVariantId")
                        as HiddenField;

                TextBox txt =
                    item.FindControl("txtActual")
                        as TextBox;

                if (hf == null || txt == null)
                {
                    continue;
                }

                int variantId;

                if (!int.TryParse(hf.Value, out variantId))
                {
                    continue;
                }

                string raw = txt.Text.Trim();

                int actual;

                if (raw.Length == 0)
                {
                    actual = 0;
                }
                else if (!int.TryParse(raw, out actual) ||
                         actual < 0)
                {
                    ShowMessage(
                        "Line " + (lines.Count + 1) +
                        ": actual packets must be a whole number of 0 or more.",
                        false);

                    return;
                }

                lines.Add(new DetailLine
                {
                    ProductVariantId = variantId,
                    ActualPackets = actual
                });
            }

            if (lines.Count == 0)
            {
                ShowMessage(
                    "There are no active product variants to reconcile.",
                    false);

                return;
            }

            int salesmanId = 0;

            int.TryParse(
                ddlSalesman.SelectedValue,
                out salesmanId);

            int driverId = 0;

            int.TryParse(
                ddlDriver.SelectedValue,
                out driverId);

            bool isApproved =
                ddlApproved.SelectedValue == "1";


            try
            {
                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    con.Open();

                    using (SqlTransaction tx =
                        con.BeginTransaction())
                    {
                        try
                        {
                            // ------------------------------------
                            // 1. Header - one row per dealer +
                            // vehicle + day (unique index), so an
                            // existing row is updated instead of
                            // inserted twice.
                            // ------------------------------------

                            int reconciliationId = 0;
                            bool wasApproved = false;
                            object oldApprovedBy = DBNull.Value;
                            object oldApprovedAt = DBNull.Value;

                            string lookup = @"
                                SELECT
                                    VehicleReconciliationId,
                                    IsApproved,
                                    ApprovedBy,
                                    ApprovedAt
                                FROM VehicleReconciliations
                                WHERE DealerId = @DealerId
                                  AND VehicleId = @VehicleId
                                  AND ReconciliationDate =
                                      @ReconciliationDate";

                            using (SqlCommand cmd =
                                new SqlCommand(
                                    lookup,
                                    con,
                                    tx))
                            {
                                cmd.Parameters.Add(
                                    "@DealerId",
                                    SqlDbType.Int).Value =
                                    DealerId;

                                cmd.Parameters.Add(
                                    "@VehicleId",
                                    SqlDbType.Int).Value =
                                    vehicleId;

                                cmd.Parameters.Add(
                                    "@ReconciliationDate",
                                    SqlDbType.Date).Value =
                                    date;

                                using (SqlDataReader reader =
                                    cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        reconciliationId =
                                            Convert.ToInt32(
                                                reader[
                                                    "VehicleReconciliationId"]);

                                        wasApproved =
                                            Convert.ToBoolean(
                                                reader["IsApproved"]);

                                        oldApprovedBy =
                                            reader["ApprovedBy"];

                                        oldApprovedAt =
                                            reader["ApprovedAt"];
                                    }
                                }
                            }

                            // Newly approved = approved now while
                            // the stored row was not approved yet.
                            bool approveNow =
                                isApproved && !wasApproved;

                            if (reconciliationId > 0)
                            {
                                reconciliationId =
                                    UpdateHeader(
                                        con,
                                        tx,
                                        reconciliationId,
                                        salesmanId,
                                        driverId,
                                        remarks,
                                        isApproved,
                                        approveNow,
                                        oldApprovedBy,
                                        oldApprovedAt);
                            }
                            else
                            {
                                reconciliationId =
                                    InsertHeader(
                                        con,
                                        tx,
                                        vehicleId,
                                        scheduleId,
                                        date,
                                        salesmanId,
                                        driverId,
                                        remarks,
                                        isApproved);
                            }


                            // ------------------------------------
                            // APPROVING THE COUNT CLOSES THE
                            // ROUTE DAY.
                            //
                            // Once the van has been counted and the
                            // count approved, the day is finished: no
                            // more stock may be loaded onto that route
                            // and no more bills may be raised against
                            // it. Un-approving re-opens the day.
                            // ------------------------------------

                            SetRouteDayClosed(
                                con,
                                tx,
                                scheduleId,
                                date,
                                isApproved);

                            // ------------------------------------
                            // 2. Expected packets, read inside the
                            // same transaction.
                            // ------------------------------------

                            Dictionary<int, int> expected =
                                LoadExpected(
                                    con,
                                    tx,
                                    vehicleId,
                                    date,
                                    scheduleId);

                            // ------------------------------------
                            // 3. Replace this reconciliation's own
                            // detail lines only.
                            // ------------------------------------

                            DeleteDetails(
                                con,
                                tx,
                                reconciliationId);

                            foreach (DetailLine line in lines)
                            {
                                int expectedPackets = 0;

                                if (expected.ContainsKey(
                                    line.ProductVariantId))
                                {
                                    expectedPackets =
                                        expected[
                                            line.ProductVariantId];
                                }

                                InsertDetail(
                                    con,
                                    tx,
                                    reconciliationId,
                                    line.ProductVariantId,
                                    expectedPackets,
                                    line.ActualPackets);
                            }

                            tx.Commit();
                        }
                        catch
                        {
                            tx.Rollback();
                            throw;
                        }
                    }
                }

                Response.Redirect(
                    "~/Dealer/Reconciliations.aspx?saved=1");
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error saving reconciliation: " +
                    ex.Message,
                    false);
            }
        }


        private static void AddInsertParameters(
            SqlCommand cmd,
            int vehicleId,
            DateTime date,
            int salesmanId,
            int driverId,
            string remarks,
            bool isApproved)
        {
            cmd.Parameters.Add(
                "@VehicleId",
                SqlDbType.Int).Value = vehicleId;

            cmd.Parameters.Add(
                "@ReconciliationDate",
                SqlDbType.Date).Value = date;

            cmd.Parameters.Add(
                "@SalesmanId",
                SqlDbType.Int).Value =
                salesmanId > 0
                    ? (object)salesmanId
                    : DBNull.Value;

            cmd.Parameters.Add(
                "@DriverId",
                SqlDbType.Int).Value =
                driverId > 0
                    ? (object)driverId
                    : DBNull.Value;

            cmd.Parameters.Add(
                "@Remarks",
                SqlDbType.NVarChar,
                1000).Value =
                remarks.Length == 0
                    ? (object)DBNull.Value
                    : remarks;

            cmd.Parameters.Add(
                "@IsApproved",
                SqlDbType.Bit).Value = isApproved;
        }


        // Approving the count closes the route day; un-approving re-opens it.
        // Nothing happens when no route is attached, because the count is
        // then about the vehicle as a whole rather than one route.
        private void SetRouteDayClosed(
            SqlConnection con,
            SqlTransaction tx,
            int scheduleId,
            DateTime date,
            bool isApproved)
        {
            if (scheduleId <= 0)
            {
                return;
            }

            string query = @"
                UPDATE RouteSchedules
                SET IsClosed = @IsClosed,
                    ClosedAt =
                        CASE
                            WHEN @IsClosed = 1 THEN GETDATE()
                            ELSE NULL
                        END,
                    ClosedBy =
                        CASE
                            WHEN @IsClosed = 1 THEN @DealerId
                            ELSE NULL
                        END
                WHERE RouteScheduleId = @RouteScheduleId
                  AND (StockDate IS NULL OR StockDate = @StockDate)";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@RouteScheduleId",
                    SqlDbType.Int).Value = scheduleId;

                cmd.Parameters.Add(
                    "@StockDate",
                    SqlDbType.Date).Value = date;

                cmd.Parameters.Add(
                    "@IsClosed",
                    SqlDbType.Bit).Value = isApproved;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                cmd.ExecuteNonQuery();
            }
        }


        // A route day is closed when its schedule says so. StockDate is NULL
        // on a recurring schedule, which means "this schedule has never been
        // closed" - it closes as soon as one day of it is approved.
        private static bool IsRouteDayClosed(
            SqlConnection con,
            SqlTransaction tx,
            int scheduleId,
            DateTime date)
        {
            if (scheduleId <= 0)
            {
                return false;
            }

            string query = @"
                SELECT TOP (1) IsClosed
                FROM RouteSchedules
                WHERE RouteScheduleId = @RouteScheduleId
                ORDER BY
                    CASE
                        WHEN StockDate = @StockDate THEN 0
                        ELSE 1
                    END";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@RouteScheduleId",
                    SqlDbType.Int).Value = scheduleId;

                cmd.Parameters.Add(
                    "@StockDate",
                    SqlDbType.Date).Value = date;

                object result = cmd.ExecuteScalar();

                return result != null &&
                       result != DBNull.Value &&
                       Convert.ToBoolean(result);
            }
        }


        private int InsertHeader(
            SqlConnection con,
            SqlTransaction tx,
            int vehicleId,
            int scheduleId,
            DateTime date,
            int salesmanId,
            int driverId,
            string remarks,
            bool isApproved)
        {
            string query = @"
                INSERT INTO VehicleReconciliations
                (
                    DealerId,
                    VehicleId,
                    RouteScheduleId,
                    ReconciliationDate,
                    SalesmanId,
                    DriverId,
                    Remarks,
                    IsApproved,
                    ApprovedBy,
                    ApprovedAt,
                    CreatedBy,
                    CreatedAt
                )
                VALUES
                (
                    @DealerId,
                    @VehicleId,
                    @RouteScheduleId,
                    @ReconciliationDate,
                    @SalesmanId,
                    @DriverId,
                    @Remarks,
                    @IsApproved,
                    CASE
                        WHEN @IsApproved = 1 THEN @DealerId
                        ELSE NULL
                    END,
                    CASE
                        WHEN @IsApproved = 1 THEN GETDATE()
                        ELSE NULL
                    END,
                    @DealerId,
                    GETDATE()
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@RouteScheduleId",
                    SqlDbType.Int).Value = scheduleId > 0
                        ? (object)scheduleId
                        : DBNull.Value;

                AddInsertParameters(
                    cmd,
                    vehicleId,
                    date,
                    salesmanId,
                    driverId,
                    remarks,
                    isApproved);

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                object result = cmd.ExecuteScalar();

                return result == null ||
                       result == DBNull.Value
                    ? 0
                    : Convert.ToInt32(result);
            }
        }


        private int UpdateHeader(
            SqlConnection con,
            SqlTransaction tx,
            int reconciliationId,
            int salesmanId,
            int driverId,
            string remarks,
            bool isApproved,
            bool approveNow,
            object oldApprovedBy,
            object oldApprovedAt)
        {
            string query = @"
                UPDATE VehicleReconciliations
                SET SalesmanId = @SalesmanId,
                    DriverId = @DriverId,
                    Remarks = @Remarks,
                    IsApproved = @IsApproved,
                    ApprovedBy =
                        CASE
                            WHEN @ApproveNow = 1 THEN @DealerId
                            ELSE @OldApprovedBy
                        END,
                    ApprovedAt =
                        CASE
                            WHEN @ApproveNow = 1 THEN GETDATE()
                            ELSE @OldApprovedAt
                        END,
                    UpdatedBy = @DealerId,
                    UpdatedAt = GETDATE()
                WHERE VehicleReconciliationId =
                      @VehicleReconciliationId
                  AND DealerId = @DealerId";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@SalesmanId",
                    SqlDbType.Int).Value =
                    salesmanId > 0
                        ? (object)salesmanId
                        : DBNull.Value;

                cmd.Parameters.Add(
                    "@DriverId",
                    SqlDbType.Int).Value =
                    driverId > 0
                        ? (object)driverId
                        : DBNull.Value;

                cmd.Parameters.Add(
                    "@Remarks",
                    SqlDbType.NVarChar,
                    1000).Value =
                    remarks.Length == 0
                        ? (object)DBNull.Value
                        : remarks;

                cmd.Parameters.Add(
                    "@IsApproved",
                    SqlDbType.Bit).Value = isApproved;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@VehicleReconciliationId",
                    SqlDbType.Int).Value = reconciliationId;

                cmd.Parameters.Add(
                    "@ApproveNow",
                    SqlDbType.Bit).Value = approveNow;

                cmd.Parameters.Add(
                    "@OldApprovedBy",
                    SqlDbType.Int).Value =
                    oldApprovedBy == null
                        ? (object)DBNull.Value
                        : oldApprovedBy;

                cmd.Parameters.Add(
                    "@OldApprovedAt",
                    SqlDbType.DateTime).Value =
                    oldApprovedAt == null
                        ? (object)DBNull.Value
                        : oldApprovedAt;

                int rows = cmd.ExecuteNonQuery();

                if (rows == 0)
                {
                    throw new InvalidOperationException(
                        "The reconciliation could not be updated.");
                }

                return reconciliationId;
            }
        }


        private void DeleteDetails(
            SqlConnection con,
            SqlTransaction tx,
            int reconciliationId)
        {
            string query = @"
                DELETE FROM VehicleReconciliationDetails
                WHERE VehicleReconciliationId =
                      @VehicleReconciliationId";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@VehicleReconciliationId",
                    SqlDbType.Int).Value = reconciliationId;

                cmd.ExecuteNonQuery();
            }
        }


        private void InsertDetail(
            SqlConnection con,
            SqlTransaction tx,
            int reconciliationId,
            int variantId,
            int expectedPackets,
            int actualPackets)
        {
            string query = @"
                INSERT INTO VehicleReconciliationDetails
                (
                    VehicleReconciliationId,
                    ProductVariantId,
                    ExpectedPackets,
                    ActualPackets,
                    VariancePackets
                )
                VALUES
                (
                    @VehicleReconciliationId,
                    @ProductVariantId,
                    @ExpectedPackets,
                    @ActualPackets,
                    @VariancePackets
                )";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@VehicleReconciliationId",
                    SqlDbType.Int).Value = reconciliationId;

                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                cmd.Parameters.Add(
                    "@ExpectedPackets",
                    SqlDbType.Int).Value = expectedPackets;

                cmd.Parameters.Add(
                    "@ActualPackets",
                    SqlDbType.Int).Value = actualPackets;

                cmd.Parameters.Add(
                    "@VariancePackets",
                    SqlDbType.Int).Value =
                    actualPackets - expectedPackets;

                cmd.ExecuteNonQuery();
            }
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
