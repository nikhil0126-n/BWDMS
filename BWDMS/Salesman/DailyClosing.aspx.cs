using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Salesman
{
    // ============================================================
    // DAILY CLOSING
    //
    // ~/Salesman/DailyClosing.aspx shows today's trip and the boxes
    // still on the salesman's vehicle. The salesman types the actual
    // counted boxes, the difference is worked out, and submitting
    // stores the reconciliation + marks the vehicle stock counted.
    // ============================================================

    public partial class DailyClosing : System.Web.UI.Page
    {
        private int SalesmanId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }

        // Trip / stock of the day, remembered across the postback.
        private int TripDealerId
        {
            get { return Convert.ToInt32(ViewState["TripDealerId"] ?? 0); }
            set { ViewState["TripDealerId"] = value; }
        }

        private int TripVehicleId
        {
            get { return Convert.ToInt32(ViewState["TripVehicleId"] ?? 0); }
            set { ViewState["TripVehicleId"] = value; }
        }

        private DateTime TripStockDate
        {
            get
            {
                object value = ViewState["TripStockDate"];

                if (value is DateTime)
                {
                    return (DateTime)value;
                }

                return DateTime.Today;
            }
            set { ViewState["TripStockDate"] = value; }
        }

        private bool TripAlreadyClosed;


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
                    "Salesman",
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
                LoadTrip();

                LoadClosing();
            }
        }


        // ============================================================
        // TRIP INFORMATION - vehicle + route of this salesman
        // ============================================================

        private void LoadTrip()
        {
            lblDate.Text =
                DateTime.Today.ToString("dd MMM yyyy");

            lblSalesman.Text =
                Session["FullName"] != null
                    ? Session["FullName"].ToString()
                    : "-";

            string query = @"
                SELECT TOP 1
                    RouteName = ISNULL(r.RouteName, '-'),
                    VehicleNumber = ISNULL(v.VehicleNumber, '-')
                FROM RouteSchedules rs
                LEFT JOIN Routes r
                    ON r.RouteId = rs.RouteId
                LEFT JOIN Vehicles v
                    ON v.VehicleId = rs.VehicleId
                WHERE rs.SalesmanId = @SalesmanId
                ORDER BY rs.RouteScheduleId";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@SalesmanId",
                        SqlDbType.Int).Value = SalesmanId;

                    using (SqlDataReader reader =
                        cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblRoute.Text =
                                reader["RouteName"].ToString();

                            lblVehicle.Text =
                                reader["VehicleNumber"].ToString();
                        }
                    }
                }
            }
        }


        // ============================================================
        // CLOSING SUMMARY - the salesman's latest loaded stock
        //
        // Expected = boxes that should still be in the vehicle
        //            (loaded minus what was sold).
        // ============================================================

        private void LoadClosing()
        {
            string query = @"
                SELECT
                    vs.VehicleStockId,
                    vs.ProductVariantId,
                    ProductName =
                        ISNULL(p.ProductName, '-') +
                        ISNULL(
                            CASE
                                WHEN ISNULL(vr.VariantName, '') = ''
                                    THEN ''
                                ELSE ' - ' + vr.VariantName
                            END, ''),
                    Expected =
                        CASE
                            WHEN vs.LoadedPackets - vs.SoldPackets < 0
                                THEN 0
                            ELSE vs.LoadedPackets - vs.SoldPackets
                        END,
                    vs.IsReconciled,
                    vs.DealerId,
                    vs.VehicleId,
                    vs.StockDate
                FROM VehicleStock vs
                LEFT JOIN ProductVariants vr
                    ON vr.ProductVariantId = vs.ProductVariantId
                LEFT JOIN Products p
                    ON p.ProductId = vr.ProductId
                WHERE vs.SalesmanId = @SalesmanId
                  AND vs.StockDate =
                  (
                      SELECT MAX(vs2.StockDate)
                      FROM VehicleStock vs2
                      WHERE vs2.SalesmanId = @SalesmanId
                  )
                ORDER BY ProductName";

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@SalesmanId",
                        SqlDbType.Int).Value = SalesmanId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            dt.Columns.Add("Actual", typeof(int));
            dt.Columns.Add("Difference", typeof(int));

            int openRows = 0;

            foreach (DataRow row in dt.Rows)
            {
                int expected =
                    Convert.ToInt32(row["Expected"]);

                bool reconciled =
                    Convert.ToBoolean(row["IsReconciled"]);

                if (!reconciled)
                {
                    openRows++;
                }

                // Start from the expected count so the salesman
                // only has to correct what does not match.
                row["Actual"] = expected;

                row["Difference"] = 0;
            }

            if (dt.Rows.Count > 0)
            {
                DataRow first = dt.Rows[0];

                TripDealerId =
                    Convert.ToInt32(first["DealerId"]);

                TripVehicleId =
                    Convert.ToInt32(first["VehicleId"]);

                TripStockDate =
                    Convert.ToDateTime(first["StockDate"]);
            }

            TripAlreadyClosed =
                dt.Rows.Count > 0 && openRows == 0;

            gvClosing.DataSource = dt;
            gvClosing.DataBind();

            btnSubmit.Enabled = !TripAlreadyClosed;

            if (TripAlreadyClosed)
            {
                ShowMessage(
                    "Closing for " +
                    TripStockDate.ToString("dd MMM yyyy") +
                    " is already submitted.",
                    false);
            }
        }


        // ============================================================
        // SUBMIT - reconciliation header + one detail row per product
        // ============================================================

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (TripAlreadyClosed)
            {
                ShowMessage(
                    "Closing is already submitted for this day.",
                    false);

                return;
            }

            if (TripVehicleId <= 0)
            {
                ShowMessage(
                    "No vehicle stock is loaded for you yet.",
                    false);

                return;
            }


            // Read the typed counts straight out of the grid.

            var lines = new List<ClosingLine>();

            foreach (GridViewRow row in gvClosing.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow)
                {
                    continue;
                }

                TextBox txt =
                    row.FindControl("txtActual") as TextBox;

                if (txt == null)
                {
                    continue;
                }

                int actual;

                if (!int.TryParse(txt.Text.Trim(), out actual) ||
                    actual < 0)
                {
                    ShowMessage(
                        "Actual boxes must be a whole number of 0 or more.",
                        false);

                    return;
                }

                var line = new ClosingLine();

                line.VehicleStockId =
                    Convert.ToInt32(
                        gvClosing.DataKeys[row.DataItemIndex]
                            ["VehicleStockId"]);

                line.ProductVariantId =
                    Convert.ToInt32(
                        gvClosing.DataKeys[row.DataItemIndex]
                            ["ProductVariantId"]);

                line.Expected = ExpectedOf(row);

                line.Actual = actual;

                line.Variance = actual - line.Expected;

                lines.Add(line);
            }

            if (lines.Count == 0)
            {
                ShowMessage(
                    "There is no stock to close today.",
                    false);

                return;
            }


            SaveClosing(lines);
        }


        // Expected value is rendered as data-expected on the
        // difference span of each row.
        private static int ExpectedOf(GridViewRow row)
        {
            foreach (System.Web.UI.Control cell in row.Cells)
            {
                var literal =
                    FindSpan(cell);

                if (literal != null)
                {
                    int expected;

                    if (int.TryParse(
                        literal.Attributes["data-expected"],
                        out expected))
                    {
                        return expected;
                    }
                }
            }

            return 0;
        }

        private static System.Web.UI.HtmlControls.HtmlGenericControl
            FindSpan(System.Web.UI.Control parent)
        {
            foreach (System.Web.UI.Control child in parent.Controls)
            {
                var span =
                    child as System.Web.UI.HtmlControls.HtmlGenericControl;

                if (span != null &&
                    span.Attributes["data-expected"] != null)
                {
                    return span;
                }

                var nested = FindSpan(child);

                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }


        private void SaveClosing(List<ClosingLine> lines)
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
                        int reconciliationId =
                            GetOrCreateHeader(con, tx);

                        if (reconciliationId <= 0)
                        {
                            tx.Rollback();

                            ShowMessage(
                                "This closing is already approved.",
                                false);

                            return;
                        }

                        // Replace the previous counts of this day.
                        using (SqlCommand cmd =
                            new SqlCommand(
                                @"DELETE FROM VehicleReconciliationDetails
                                  WHERE VehicleReconciliationId = @Id",
                                con,
                                tx))
                        {
                            cmd.Parameters.Add(
                                "@Id",
                                SqlDbType.Int).Value =
                                    reconciliationId;

                            cmd.ExecuteNonQuery();
                        }

                        foreach (ClosingLine line in lines)
                        {
                            using (SqlCommand cmd =
                                new SqlCommand(
                                    @"INSERT INTO
                                      VehicleReconciliationDetails
                                      (
                                          VehicleReconciliationId,
                                          ProductVariantId,
                                          ExpectedPackets,
                                          ActualPackets,
                                          VariancePackets
                                      )
                                      VALUES
                                      (
                                          @HeaderId,
                                          @VariantId,
                                          @Expected,
                                          @Actual,
                                          @Variance
                                      )",
                                    con,
                                    tx))
                            {
                                cmd.Parameters.Add(
                                    "@HeaderId",
                                    SqlDbType.Int).Value =
                                        reconciliationId;

                                cmd.Parameters.Add(
                                    "@VariantId",
                                    SqlDbType.Int).Value =
                                        line.ProductVariantId;

                                cmd.Parameters.Add(
                                    "@Expected",
                                    SqlDbType.Int).Value =
                                        line.Expected;

                                cmd.Parameters.Add(
                                    "@Actual",
                                    SqlDbType.Int).Value =
                                        line.Actual;

                                cmd.Parameters.Add(
                                    "@Variance",
                                    SqlDbType.Int).Value =
                                        line.Variance;

                                cmd.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd =
                                new SqlCommand(
                                    @"UPDATE VehicleStock
                                      SET ActualClosingPackets = @Actual,
                                          IsReconciled = 1,
                                          UpdatedBy = @SalesmanId,
                                          UpdatedAt = GETDATE()
                                      WHERE VehicleStockId = @Id",
                                    con,
                                    tx))
                            {
                                cmd.Parameters.Add(
                                    "@Actual",
                                    SqlDbType.Int).Value =
                                        line.Actual;

                                cmd.Parameters.Add(
                                    "@SalesmanId",
                                    SqlDbType.Int).Value =
                                        SalesmanId;

                                cmd.Parameters.Add(
                                    "@Id",
                                    SqlDbType.Int).Value =
                                        line.VehicleStockId;

                                cmd.ExecuteNonQuery();
                            }
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

            TripAlreadyClosed = true;

            btnSubmit.Enabled = false;

            ShowMessage(
                "Closing submitted successfully.",
                true);
        }


        // Finds the day's reconciliation, creates it when missing.
        // Returns 0 when the day is already approved.
        private int GetOrCreateHeader(
            SqlConnection con,
            SqlTransaction tx)
        {
            int reconciliationId = 0;

            bool approved = false;

            using (SqlCommand cmd =
                new SqlCommand(
                    @"SELECT TOP 1
                          VehicleReconciliationId,
                          IsApproved
                      FROM VehicleReconciliations
                      WHERE DealerId = @DealerId
                        AND VehicleId = @VehicleId
                        AND ReconciliationDate = @Day",
                    con,
                    tx))
            {
                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = TripDealerId;

                cmd.Parameters.Add(
                    "@VehicleId",
                    SqlDbType.Int).Value = TripVehicleId;

                cmd.Parameters.Add(
                    "@Day",
                    SqlDbType.Date).Value = TripStockDate;

                using (SqlDataReader reader =
                    cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        reconciliationId =
                            Convert.ToInt32(
                                reader["VehicleReconciliationId"]);

                        approved =
                            Convert.ToBoolean(reader["IsApproved"]);
                    }
                }
            }

            if (reconciliationId > 0)
            {
                return approved ? 0 : reconciliationId;
            }

            using (SqlCommand cmd =
                new SqlCommand(
                    @"INSERT INTO VehicleReconciliations
                      (
                          DealerId,
                          VehicleId,
                          ReconciliationDate,
                          SalesmanId,
                          CreatedBy
                      )
                      VALUES
                      (
                          @DealerId,
                          @VehicleId,
                          @Day,
                          @SalesmanId,
                          @SalesmanId
                      );
                      SELECT SCOPE_IDENTITY();",
                    con,
                    tx))
            {
                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = TripDealerId;

                cmd.Parameters.Add(
                    "@VehicleId",
                    SqlDbType.Int).Value = TripVehicleId;

                cmd.Parameters.Add(
                    "@Day",
                    SqlDbType.Date).Value = TripStockDate;

                cmd.Parameters.Add(
                    "@SalesmanId",
                    SqlDbType.Int).Value = SalesmanId;

                reconciliationId =
                    Convert.ToInt32(cmd.ExecuteScalar());
            }

            return reconciliationId;
        }


        private class ClosingLine
        {
            public int VehicleStockId;
            public int ProductVariantId;
            public int Expected;
            public int Actual;
            public int Variance;
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
