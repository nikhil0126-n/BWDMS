using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class VehicleLoading : System.Web.UI.Page
    {
        private int DealerId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }


        // The line list survives postbacks so the builder, the
        // grid and the load all work from one source of truth.
        private DataTable Lines
        {
            get
            {
                DataTable dt =
                    ViewState["Lines"] as DataTable;

                if (dt == null)
                {
                    dt = NewLineTable();
                    ViewState["Lines"] = dt;
                }

                return dt;
            }
            set { ViewState["Lines"] = value; }
        }

        private static DataTable NewLineTable()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("Index", typeof(int));
            dt.Columns.Add("ProductVariantId", typeof(int));
            dt.Columns.Add("VariantLabel", typeof(string));
            dt.Columns.Add("Quantity", typeof(int));

            return dt;
        }


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
                LoadSchedules();
                LoadVehicles();
                LoadVariants();

                txtStockDate.Text =
                    DateTime.Today.ToString("yyyy-MM-dd");

                BindLines();
                LoadTodaySummary();
                LoadHistory();
            }
        }


        // ============================================================
        // DROPDOWNS
        // ============================================================

        // A route day that has been counted and approved is finished: nothing
        // more may be loaded onto it. StockDate is set on the schedule the
        // first time a day is closed, so an untimed recurring schedule is
        // only blocked once a specific day was closed.
        private bool RouteDayClosed(int scheduleId, DateTime date)
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

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
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
        }


        private void LoadSchedules()
        {
            // Only schedules that actually have a vehicle can receive a load:
            // the vehicle is what carries the stock on the route.
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
                  AND rs.VehicleId IS NOT NULL
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
                                "Select Route / Schedule",
                                ""));
                    }
                }
            }
        }


        // The route owns the vehicle: choosing a route fills the vehicle in.
        // A forged combination (vehicle from another route) is rejected again
        // on save, so this is convenience, not enforcement.
        protected void ddlRouteSchedule_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            int scheduleId = SelectedScheduleId();

            if (scheduleId > 0)
            {
                int vehicleId = VehicleForSchedule(scheduleId);

                if (vehicleId > 0 &&
                    ddlVehicle.Items.FindByValue(
                        vehicleId.ToString()) != null)
                {
                    ddlVehicle.SelectedValue =
                        vehicleId.ToString();
                }
            }

            LoadTodaySummary();
            LoadHistory();
        }


        private int VehicleForSchedule(int scheduleId)
        {
            string query = @"
                SELECT ISNULL(rs.VehicleId, 0)
                FROM RouteSchedules rs
                INNER JOIN Routes r
                    ON r.RouteId = rs.RouteId
                WHERE rs.RouteScheduleId = @RouteScheduleId
                  AND r.DealerId = @DealerId
                  AND rs.IsActive = 1";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                // ExecuteScalar needs an open connection; the DataAdapter
                // calls above open theirs implicitly, this one does not.
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


        private int SelectedScheduleId()
        {
            int id = 0;

            int.TryParse(
                ddlRouteSchedule.SelectedValue,
                out id);

            return id;
        }


        private void LoadVehicles()
        {
            string query = @"
                SELECT
                    VehicleId,
                    Label = VehicleNumber
                            + ISNULL(
                                ' - ' + VehicleName,
                                '')
                FROM Vehicles
                WHERE DealerId = @DealerId
                  AND IsActive = 1
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
                        ddlVehicle.DataTextField = "Label";
                        ddlVehicle.DataValueField = "VehicleId";
                        ddlVehicle.DataBind();

                        ddlVehicle.Items.Insert(
                            0,
                            new ListItem("Select Vehicle", ""));
                    }
                }
            }
        }


        private void LoadVariants()
        {
            string query = @"
                SELECT
                    v.ProductVariantId,
                    VariantLabel =
                        p.ProductName
                        + ' - '
                        + v.VariantName
                        + ISNULL(
                            ' ('
                            + CONVERT(NVARCHAR(20), v.PacketWeight)
                            + ' ' + v.WeightUnit + ')',
                            '')
                FROM ProductVariants v
                INNER JOIN Products p
                    ON p.ProductId = v.ProductId
                WHERE v.IsActive = 1
                  AND p.IsActive = 1
                ORDER BY p.ProductName, v.SortOrder, v.VariantName";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        ddlVariant.DataSource = dt;
                        ddlVariant.DataTextField = "VariantLabel";
                        ddlVariant.DataValueField = "ProductVariantId";
                        ddlVariant.DataBind();

                        ddlVariant.Items.Insert(
                            0,
                            new ListItem("Select Variant", ""));
                    }
                }
            }
        }


        // ============================================================
        // LIVE GODOWN-STOCK BANNER
        // ============================================================

        private void ShowCurrentStock()
        {
            int variantId = SelectedVariantId();

            if (variantId <= 0)
            {
                lblCurrent.Visible = false;
                return;
            }

            string query = @"
                SELECT
                    ISNULL(QuantityPackets, 0),
                    ISNULL(ReorderLevel, 0)
                FROM Inventory
                WHERE DealerId = @DealerId
                  AND ProductVariantId = @ProductVariantId";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    cmd.Parameters.Add(
                        "@ProductVariantId",
                        SqlDbType.Int).Value = variantId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        if (dt.Rows.Count == 0)
                        {
                            lblCurrent.Text =
                                "No stock recorded for this variant in the godown.";

                            lblCurrent.Visible = true;

                            return;
                        }

                        int qty = Convert.ToInt32(
                            dt.Rows[0][0]);

                        int reorder = Convert.ToInt32(
                            dt.Rows[0][1]);

                        lblCurrent.Text =
                            "Godown stock: <strong>" + qty +
                            "</strong> packets &nbsp;|&nbsp; reorder level: <strong>" +
                            reorder + "</strong>";

                        lblCurrent.Visible = true;
                    }
                }
            }
        }


        protected void ddlVariant_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            ShowCurrentStock();
        }


        protected void ddlVehicle_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            LoadTodaySummary();
        }


        // ============================================================
        // ALREADY LOADED FOR THIS VEHICLE
        // ============================================================

        private void LoadTodaySummary()
        {
            DateTime stockDate;

            if (!TryDate(
                    txtStockDate.Text.Trim(),
                    out stockDate))
            {
                stockDate = DateTime.Today;
            }

            int vehicleId = SelectedVehicleId();

            lblSummaryHeading.Text =
                vehicleId > 0
                    ? "Loaded on " +
                      stockDate.ToString("yyyy-MM-dd") +
                      " for this vehicle"
                    : "Select a vehicle to see what has already been loaded.";

            DataTable dt = new DataTable();

            if (vehicleId > 0)
            {
                string query = @"
                    SELECT
                        VariantLabel =
                            p.ProductName + ' - ' + v.VariantName,
                        vs.LoadedPackets
                    FROM VehicleStock vs
                    INNER JOIN ProductVariants v
                        ON v.ProductVariantId =
                           vs.ProductVariantId
                    INNER JOIN Products p
                        ON p.ProductId = v.ProductId
                    WHERE vs.DealerId = @DealerId
                      AND vs.VehicleId = @VehicleId
                      AND vs.StockDate = @StockDate
                    ORDER BY p.ProductName, v.VariantName";

                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@DealerId",
                            SqlDbType.Int).Value = DealerId;

                        cmd.Parameters.Add(
                            "@VehicleId",
                            SqlDbType.Int).Value = vehicleId;

                        cmd.Parameters.Add(
                            "@StockDate",
                            SqlDbType.Date).Value = stockDate;

                        using (SqlDataAdapter da =
                            new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
            }

            gvTodayLoaded.DataSource = dt;
            gvTodayLoaded.DataBind();
        }


        // ============================================================
        // LOAD HISTORY - the LOAD- entries in the stock ledger
        // ============================================================

        private void LoadHistory()
        {
            string query = @"
                ;WITH latest AS
                (
                    SELECT TOP (20)
                        st.StockTransactionId,
                        st.TransactionDate,
                        st.ReferenceNo
                    FROM StockTransactions st
                    WHERE st.DealerId = @DealerId
                      AND st.ReferenceNo LIKE 'LOAD-%'
                    ORDER BY
                        st.TransactionDate DESC,
                        st.StockTransactionId DESC
                )
                SELECT
                    l.ReferenceNo,
                    LoadDateText =
                        CONVERT(
                            NVARCHAR(16),
                            l.TransactionDate,
                            120),
                    VariantLabel =
                        p.ProductName + ' - ' + v.VariantName,
                    PacketsText =
                        CONVERT(
                            NVARCHAR(20),
                            ABS(d.QuantityPackets))
                FROM latest l
                INNER JOIN StockTransactionDetails d
                    ON d.StockTransactionId =
                       l.StockTransactionId
                INNER JOIN ProductVariants v
                    ON v.ProductVariantId = d.ProductVariantId
                INNER JOIN Products p
                    ON p.ProductId = v.ProductId
                ORDER BY
                    l.TransactionDate DESC,
                    l.StockTransactionId DESC,
                    d.StockTransactionDetailId";

            DataTable dt = new DataTable();

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
                        da.Fill(dt);
                    }
                }
            }

            gvLoadHistory.DataSource = dt;
            gvLoadHistory.DataBind();
        }


        // ============================================================
        // LINES
        // ============================================================

        protected void btnAddLine_Click(
            object sender,
            EventArgs e)
        {
            Page.Validate("LineAdd");

            if (!Page.IsValid)
            {
                ShowMessage(
                    "Please correct the highlighted line fields.",
                    false);

                return;
            }

            int variantId = SelectedVariantId();

            if (variantId <= 0)
            {
                ShowMessage(
                    "Select a product variant.",
                    false);

                return;
            }

            int quantity;

            if (!int.TryParse(
                    txtQuantity.Text.Trim(),
                    out quantity) || quantity <= 0)
            {
                ShowMessage(
                    "Packets must be a whole number above zero.",
                    false);

                return;
            }

            DataTable dt = Lines;

            // One line per variant - repeated selections grow the
            // line that is already there.
            DataRow existing = null;

            foreach (DataRow row in dt.Rows)
            {
                if (Convert.ToInt32(
                        row["ProductVariantId"]) == variantId)
                {
                    existing = row;
                    break;
                }
            }

            if (existing != null)
            {
                existing["Quantity"] =
                    Convert.ToInt32(existing["Quantity"]) +
                    quantity;

                ShowMessage(
                    "That variant is already on the load - its packets were increased.",
                    true);
            }
            else
            {
                DataRow row = dt.NewRow();

                row["Index"] = dt.Rows.Count;
                row["ProductVariantId"] = variantId;
                row["VariantLabel"] = VariantLabel(variantId);
                row["Quantity"] = quantity;

                dt.Rows.Add(row);

                ShowMessage("Line added.", true);
            }

            Lines = dt;

            BindLines();
        }


        protected void gvLines_RowCommand(
            object sender,
            GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Remove")
            {
                return;
            }

            int index;

            if (!int.TryParse(
                e.CommandArgument.ToString(),
                out index))
            {
                return;
            }

            DataTable dt = Lines;

            if (index < 0 || index >= dt.Rows.Count)
            {
                return;
            }

            dt.Rows.RemoveAt(index);
            Lines = dt;

            BindLines();
        }


        private void BindLines()
        {
            DataTable dt = Lines;

            int i = 0;
            int total = 0;

            foreach (DataRow row in dt.Rows)
            {
                row["Index"] = i++;

                total += Convert.ToInt32(row["Quantity"]);
            }

            gvLines.DataSource = dt;
            gvLines.DataBind();

            lblTotalPackets.Text = total.ToString();
        }


        // ============================================================
        // LOAD VEHICLE
        //
        // ONE transaction: godown down, vehicle up, ledger written.
        // Any problem rolls the whole thing back.
        // ============================================================

        protected void btnLoad_Click(
            object sender,
            EventArgs e)
        {
            Page.Validate("Load");

            if (!Page.IsValid)
            {
                ShowMessage(
                    "Please correct the highlighted fields.",
                    false);

                return;
            }

            int scheduleId = SelectedScheduleId();

            if (scheduleId <= 0)
            {
                ShowMessage(
                    "Select the route this load is for.",
                    false);

                return;
            }

            int vehicleId = SelectedVehicleId();

            if (vehicleId <= 0)
            {
                ShowMessage(
                    "Select a vehicle.",
                    false);

                return;
            }

            // The dropdown is dealer scoped, but a crafted combination must
            // not load one route's stock onto another route's vehicle.
            int scheduleVehicle = VehicleForSchedule(scheduleId);

            if (scheduleVehicle <= 0 ||
                scheduleVehicle != vehicleId)
            {
                ShowMessage(
                    "The selected vehicle is not the one assigned to that route. " +
                    "Pick the route first, then the vehicle.",
                    false);

                return;
            }

            DateTime stockDate;

            if (!TryDate(
                    txtStockDate.Text.Trim(),
                    out stockDate))
            {
                ShowMessage(
                    "Stock date is required and must be a valid date (YYYY-MM-DD).",
                    false);

                return;
            }

            // The route day has to still be open.
            if (RouteDayClosed(scheduleId, stockDate))
            {
                ShowMessage(
                    "This route's day for " +
                    stockDate.ToString("yyyy-MM-dd") +
                    " has already been counted and closed. " +
                    "Un-approve the reconciliation on Dealer > Stock " +
                    "Reconciliation if the count needs to be reopened.",
                    false);

                return;
            }

            DataTable lines = Lines;

            if (lines.Rows.Count == 0)
            {
                ShowMessage(
                    "Add at least one line to load.",
                    false);

                return;
            }

            // Reads the vehicle back from the master so a forged
            // postback cannot load onto another dealer's vehicle.
            string vehicleNumber = VehicleNumber(vehicleId);

            if (vehicleNumber.Length == 0)
            {
                ShowMessage(
                    "Vehicle not found.",
                    false);

                return;
            }

            Dictionary<int, int> needed =
                new Dictionary<int, int>();

            int totalPackets = 0;

            foreach (DataRow row in lines.Rows)
            {
                int variantId =
                    Convert.ToInt32(row["ProductVariantId"]);

                int quantity =
                    Convert.ToInt32(row["Quantity"]);

                totalPackets += quantity;

                if (needed.ContainsKey(variantId))
                {
                    needed[variantId] += quantity;
                }
                else
                {
                    needed[variantId] = quantity;
                }
            }

            string reference =
                "LOAD-" + vehicleNumber + "-" +
                stockDate.ToString(
                    "yyyyMMdd",
                    CultureInfo.InvariantCulture);

            string remarks =
                "Loaded " + totalPackets +
                " packet(s) onto " + vehicleNumber + ".";

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
                            // ----------------------------
                            // 1. out of the godown
                            // ----------------------------

                            foreach (KeyValuePair<int, int> pair
                                in needed)
                            {
                                DecreaseGodown(
                                    con,
                                    tx,
                                    pair.Key,
                                    pair.Value);
                            }

                            // ----------------------------
                            // 2. onto the vehicle
                            // ----------------------------

                            foreach (KeyValuePair<int, int> pair
                                in needed)
                            {
                                AddToVehicle(
                                    con,
                                    tx,
                                    vehicleId,
                                    scheduleId,
                                    stockDate,
                                    pair.Key,
                                    pair.Value);
                            }

                            // ----------------------------
                            // 3. the ledger
                            // ----------------------------

                            WriteLoadLedger(
                                con,
                                tx,
                                reference,
                                remarks,
                                needed);

                            tx.Commit();
                        }
                        catch
                        {
                            tx.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, false);

                return;
            }

            Lines = NewLineTable();
            BindLines();

            LoadTodaySummary();
            LoadHistory();

            ShowCurrentStock();

            ShowMessage(
                "Loaded " + totalPackets +
                " packet(s) onto " + vehicleNumber +
                " for " + stockDate.ToString("yyyy-MM-dd") +
                ".",
                true);
        }


        // ------------------------------------------------------------
        // Godown: read the row under a lock, refuse to go below
        // zero, then take the packets out.
        // ------------------------------------------------------------

        private void DecreaseGodown(
            SqlConnection con,
            SqlTransaction tx,
            int variantId,
            int quantity)
        {
            string label =
                VariantLabel(con, tx, variantId);

            int current = 0;

            string read = @"
                SELECT QuantityPackets
                FROM Inventory WITH (UPDLOCK, ROWLOCK)
                WHERE DealerId = @DealerId
                  AND ProductVariantId = @ProductVariantId";

            using (SqlCommand cmd =
                new SqlCommand(read, con, tx))
            {
                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                object result = cmd.ExecuteScalar();

                if (result != null &&
                    result != DBNull.Value)
                {
                    current = Convert.ToInt32(result);
                }
            }

            if (current < quantity)
            {
                throw new InvalidOperationException(
                    "Not enough godown stock for " + label +
                    ": only " + current +
                    " packet(s) available, the load needs " +
                    quantity + ".");
            }

            string update = @"
                UPDATE Inventory
                SET QuantityPackets =
                        QuantityPackets - @QuantityPackets,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE DealerId = @DealerId
                  AND ProductVariantId = @ProductVariantId
                  AND QuantityPackets >= @QuantityPackets";

            using (SqlCommand cmd =
                new SqlCommand(update, con, tx))
            {
                cmd.Parameters.Add(
                    "@QuantityPackets",
                    SqlDbType.Int).Value = quantity;

                cmd.Parameters.Add(
                    "@UpdatedBy",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                if (cmd.ExecuteNonQuery() != 1)
                {
                    // The guard above can never be passed twice
                    // inside one load, so this only fires when
                    // another user emptied the godown first.
                    throw new InvalidOperationException(
                        "Not enough godown stock for " + label +
                        ": only " + current +
                        " packet(s) available, the load needs " +
                        quantity + ".");
                }
            }
        }


        // ------------------------------------------------------------
        // Vehicle stock: one row per dealer + vehicle + date +
        // variant (UQ_VehicleStock_Day), packets are added to it.
        // ------------------------------------------------------------

        private void AddToVehicle(
            SqlConnection con,
            SqlTransaction tx,
            int vehicleId,
            int scheduleId,
            DateTime stockDate,
            int variantId,
            int quantity)
        {
            // Scoped by ROUTE as well as vehicle: two routes running on the
            // same day must not share one bag of stock.
            string read = @"
                SELECT VehicleStockId
                FROM VehicleStock WITH (UPDLOCK, ROWLOCK)
                WHERE DealerId = @DealerId
                  AND VehicleId = @VehicleId
                  AND RouteScheduleId = @RouteScheduleId
                  AND StockDate = @StockDate
                  AND ProductVariantId = @ProductVariantId";

            bool exists;

            using (SqlCommand cmd =
                new SqlCommand(read, con, tx))
            {
                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@VehicleId",
                    SqlDbType.Int).Value = vehicleId;

                cmd.Parameters.Add(
                    "@RouteScheduleId",
                    SqlDbType.Int).Value = scheduleId;

                cmd.Parameters.Add(
                    "@StockDate",
                    SqlDbType.Date).Value = stockDate;

                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                exists = cmd.ExecuteScalar() != null;
            }

            string update = @"
                UPDATE VehicleStock
                SET LoadedPackets =
                        LoadedPackets + @LoadedPackets,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE DealerId = @DealerId
                  AND VehicleId = @VehicleId
                  AND RouteScheduleId = @RouteScheduleId
                  AND StockDate = @StockDate
                  AND ProductVariantId = @ProductVariantId";

            if (exists)
            {
                ExecuteVehicleUpdate(
                    con,
                    tx,
                    update,
                    vehicleId,
                    scheduleId,
                    stockDate,
                    variantId,
                    quantity);

                return;
            }

            string insert = @"
                INSERT INTO VehicleStock
                (
                    DealerId,
                    VehicleId,
                    RouteScheduleId,
                    StockDate,
                    ProductVariantId,
                    LoadedPackets,
                    CreatedBy
                )
                VALUES
                (
                    @DealerId,
                    @VehicleId,
                    @RouteScheduleId,
                    @StockDate,
                    @ProductVariantId,
                    @LoadedPackets,
                    @CreatedBy
                )";

            using (SqlCommand cmd =
                new SqlCommand(insert, con, tx))
            {
                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@VehicleId",
                    SqlDbType.Int).Value = vehicleId;

                cmd.Parameters.Add(
                    "@RouteScheduleId",
                    SqlDbType.Int).Value = scheduleId;

                cmd.Parameters.Add(
                    "@StockDate",
                    SqlDbType.Date).Value = stockDate;

                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                cmd.Parameters.Add(
                    "@LoadedPackets",
                    SqlDbType.Int).Value = quantity;

                cmd.Parameters.Add(
                    "@CreatedBy",
                    SqlDbType.Int).Value = DealerId;

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    // UQ_VehicleStock_Route_Day lost the race - the row
                    // exists now, so retry the increase once.
                    if (ex.Number != 2601 && ex.Number != 2627)
                    {
                        throw;
                    }

                    ExecuteVehicleUpdate(
                        con,
                        tx,
                        update,
                        vehicleId,
                        scheduleId,
                        stockDate,
                        variantId,
                        quantity);
                }
            }
        }


        private void ExecuteVehicleUpdate(
            SqlConnection con,
            SqlTransaction tx,
            string update,
            int vehicleId,
            int scheduleId,
            DateTime stockDate,
            int variantId,
            int quantity)
        {
            using (SqlCommand cmd =
                new SqlCommand(update, con, tx))
            {
                cmd.Parameters.Add(
                    "@LoadedPackets",
                    SqlDbType.Int).Value = quantity;

                cmd.Parameters.Add(
                    "@UpdatedBy",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@VehicleId",
                    SqlDbType.Int).Value = vehicleId;

                cmd.Parameters.Add(
                    "@RouteScheduleId",
                    SqlDbType.Int).Value = scheduleId;

                cmd.Parameters.Add(
                    "@StockDate",
                    SqlDbType.Date).Value = stockDate;

                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                cmd.ExecuteNonQuery();
            }
        }


        // One ledger header for the load plus one detail per line.
        private void WriteLoadLedger(
            SqlConnection con,
            SqlTransaction tx,
            string reference,
            string remarks,
            Dictionary<int, int> needed)
        {
            string header = @"
                INSERT INTO StockTransactions
                (
                    DealerId,
                    TransactionType,
                    TransactionDate,
                    ReferenceNo,
                    Remarks,
                    CreatedBy,
                    CreatedAt
                )
                VALUES
                (
                    @DealerId,
                    @TransactionType,
                    GETDATE(),
                    @ReferenceNo,
                    @Remarks,
                    @CreatedBy,
                    GETDATE()
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int transactionId;

            using (SqlCommand cmd =
                new SqlCommand(header, con, tx))
            {
                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                cmd.Parameters.Add(
                    "@TransactionType",
                    SqlDbType.NVarChar,
                    80).Value = "Stock Out";

                cmd.Parameters.Add(
                    "@ReferenceNo",
                    SqlDbType.NVarChar,
                    200).Value = reference;

                cmd.Parameters.Add(
                    "@Remarks",
                    SqlDbType.NVarChar,
                    1000).Value = remarks;

                cmd.Parameters.Add(
                    "@CreatedBy",
                    SqlDbType.Int).Value = DealerId;

                transactionId =
                    Convert.ToInt32(cmd.ExecuteScalar());
            }

            int unitId = PacketUnitId(con, tx);

            string detail = @"
                INSERT INTO StockTransactionDetails
                (
                    StockTransactionId,
                    ProductVariantId,
                    UnitId,
                    Quantity,
                    QuantityPackets
                )
                VALUES
                (
                    @StockTransactionId,
                    @ProductVariantId,
                    @UnitId,
                    @Quantity,
                    @QuantityPackets
                )";

            foreach (KeyValuePair<int, int> pair in needed)
            {
                using (SqlCommand cmd =
                    new SqlCommand(detail, con, tx))
                {
                    // Stored as the signed stock movement, so the
                    // Stock Out row carries a negative quantity -
                    // the same convention the rest of the ledger
                    // already uses.
                    int movement = -pair.Value;

                    cmd.Parameters.Add(
                        "@StockTransactionId",
                        SqlDbType.Int).Value =
                        transactionId;

                    cmd.Parameters.Add(
                        "@ProductVariantId",
                        SqlDbType.Int).Value =
                        pair.Key;

                    cmd.Parameters.Add(
                        "@UnitId",
                        SqlDbType.Int).Value = unitId;

                    cmd.Parameters.Add(
                        "@Quantity",
                        SqlDbType.Int).Value = movement;

                    cmd.Parameters.Add(
                        "@QuantityPackets",
                        SqlDbType.Int).Value = movement;

                    cmd.ExecuteNonQuery();
                }
            }
        }


        // Stock is counted in base packets, so the detail rows are
        // tagged with the Packet selling unit.
        private int PacketUnitId(
            SqlConnection con,
            SqlTransaction tx)
        {
            string query = @"
                SELECT TOP (1) UnitId
                FROM SellingUnits
                WHERE UnitCode = 'PKT'
                  AND IsActive = 1
                ORDER BY UnitId";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                object result = cmd.ExecuteScalar();

                int unitId =
                    result == null ||
                    result == DBNull.Value
                        ? 0
                        : Convert.ToInt32(result);

                if (unitId <= 0)
                {
                    throw new InvalidOperationException(
                        "The packet selling unit (PKT) is missing from the unit master.");
                }

                return unitId;
            }
        }


        private string VariantLabel(
            SqlConnection con,
            SqlTransaction tx,
            int variantId)
        {
            string query = @"
                SELECT p.ProductName + ' - ' + v.VariantName
                FROM ProductVariants v
                INNER JOIN Products p
                    ON p.ProductId = v.ProductId
                WHERE v.ProductVariantId = @ProductVariantId";

            using (SqlCommand cmd =
                new SqlCommand(query, con, tx))
            {
                cmd.Parameters.Add(
                    "@ProductVariantId",
                    SqlDbType.Int).Value = variantId;

                object result = cmd.ExecuteScalar();

                return result == null ||
                       result == DBNull.Value
                    ? "variant #" + variantId
                    : result.ToString();
            }
        }


        private string VariantLabel(int variantId)
        {
            string query = @"
                SELECT p.ProductName + ' - ' + v.VariantName
                FROM ProductVariants v
                INNER JOIN Products p
                    ON p.ProductId = v.ProductId
                WHERE v.ProductVariantId = @ProductVariantId";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@ProductVariantId",
                        SqlDbType.Int).Value = variantId;

                    object result = cmd.ExecuteScalar();

                    return result == null ||
                           result == DBNull.Value
                        ? "variant #" + variantId
                        : result.ToString();
                }
            }
        }


        private string VehicleNumber(int vehicleId)
        {
            string query = @"
                SELECT VehicleNumber
                FROM Vehicles
                WHERE VehicleId = @VehicleId
                  AND DealerId = @DealerId
                  AND IsActive = 1";

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
                        SqlDbType.Int).Value = DealerId;

                    object result = cmd.ExecuteScalar();

                    return result == null ||
                           result == DBNull.Value
                        ? ""
                        : result.ToString();
                }
            }
        }


        // ============================================================
        // HELPERS
        // ============================================================

        private int SelectedVehicleId()
        {
            int id = 0;

            int.TryParse(ddlVehicle.SelectedValue, out id);

            return id;
        }


        private int SelectedVariantId()
        {
            int id = 0;

            int.TryParse(ddlVariant.SelectedValue, out id);

            return id;
        }


        private static bool TryDate(
            string text,
            out DateTime value)
        {
            return DateTime.TryParse(
                text,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out value);
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
