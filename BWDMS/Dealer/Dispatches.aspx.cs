using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class Dispatches : System.Web.UI.Page
    {
        private int DealerId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
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
                LoadDispatches();

                if (Request.QueryString["saved"] == "1")
                {
                    ShowMessage(
                        "Dispatch saved successfully.",
                        true);
                }
            }
        }


        // ============================================================
        // DISPATCH LIST
        // ============================================================

        private void LoadDispatches()
        {
            string search = txtSearch.Text.Trim();
            string status = ddlStatus.SelectedValue;

            DateTime? from;
            DateTime? to;

            if (!ParseFilterDate(txtDateFrom.Text, out from) ||
                !ParseFilterDate(txtDateTo.Text, out to))
            {
                ShowMessage(
                    "Dates must be entered as YYYY-MM-DD.",
                    false);

                return;
            }

            string query = @"
                SELECT
                    d.DispatchId,
                    d.DispatchNumber,
                    DispatchDateText =
                        CONVERT(NVARCHAR(10), d.DispatchDate, 120),
                    VehicleNumber =
                        ISNULL(v.VehicleNumber, '-'),
                    SalesmanName =
                        ISNULL(u.FullName, '-'),
                    OrderCount =
                        ISNULL(
                            (SELECT COUNT(DISTINCT dd.OrderId)
                             FROM DispatchDetails dd
                             WHERE dd.DispatchId = d.DispatchId),
                            0),
                    TotalPackets =
                        ISNULL(
                            (SELECT SUM(dd.QuantityPackets)
                             FROM DispatchDetails dd
                             WHERE dd.DispatchId = d.DispatchId),
                            0),
                    d.Status,
                    Remarks =
                        ISNULL(d.Remarks, '')
                FROM Dispatches d
                LEFT JOIN Vehicles v
                    ON v.VehicleId = d.VehicleId
                LEFT JOIN Users u
                    ON u.UserId = d.SalesmanId
                WHERE d.DealerId = @DealerId
                  AND ISNULL(d.DispatchNumber, '') LIKE @Search
                  AND (@Status = '' OR d.Status = @Status)
                  AND (@From IS NULL OR d.DispatchDate >= @From)
                  AND (@To IS NULL OR d.DispatchDate <= @To)
                ORDER BY d.DispatchDate DESC, d.DispatchId DESC";

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

                    cmd.Parameters.Add(
                        "@Search",
                        SqlDbType.NVarChar,
                        200).Value = "%" + search + "%";

                    cmd.Parameters.Add(
                        "@Status",
                        SqlDbType.NVarChar,
                        30).Value = status;

                    cmd.Parameters.Add(
                        "@From",
                        SqlDbType.Date).Value =
                        from.HasValue
                            ? (object)from.Value
                            : DBNull.Value;

                    cmd.Parameters.Add(
                        "@To",
                        SqlDbType.Date).Value =
                        to.HasValue
                            ? (object)to.Value
                            : DBNull.Value;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            if (!dt.Columns.Contains("StatusCss"))
            {
                dt.Columns.Add("StatusCss", typeof(string));
            }

            foreach (DataRow row in dt.Rows)
            {
                row["StatusCss"] = StatusCss(
                    row["Status"].ToString());
            }

            gvDispatches.DataSource = dt;
            gvDispatches.DataBind();
        }


        internal static string StatusCss(string status)
        {
            switch (status)
            {
                case "Dispatched":
                    return "badge bg-success";

                case "Completed":
                    return "badge bg-primary";

                case "Cancelled":
                    return "badge bg-secondary";

                default:
                    return "badge bg-warning text-dark";
            }
        }


        private static bool ParseFilterDate(
            string text,
            out DateTime? value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            DateTime parsed;

            if (DateTime.TryParseExact(
                    text.Trim(),
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out parsed))
            {
                value = parsed;
                return true;
            }

            return false;
        }


        protected void txtSearch_TextChanged(
            object sender,
            EventArgs e)
        {
            LoadDispatches();
        }


        protected void FilterChanged(
            object sender,
            EventArgs e)
        {
            LoadDispatches();
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
