using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class Reconciliations : System.Web.UI.Page
    {
        // Dealer's own id lives in UserId (there is no Session["DealerId"]).
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
                LoadReconciliations();

                if (Request.QueryString["saved"] == "1")
                {
                    ShowMessage(
                        "Reconciliation saved successfully.",
                        true);
                }
            }
        }


        private void LoadReconciliations()
        {
            string search = txtSearch.Text.Trim();
            string approved = ddlApprovedFilter.SelectedValue;

            string query = @"
                SELECT
                    r.VehicleReconciliationId,
                    ReconciliationDateText =
                        CONVERT(NVARCHAR(10), r.ReconciliationDate, 120),
                    v.VehicleNumber,
                    SalesmanName = ISNULL(s.FullName, '-'),
                    DriverName = ISNULL(d.FullName, '-'),
                    r.Remarks,
                    r.IsApproved,
                    VarianceSummary =
                        CASE
                            WHEN det.VehicleReconciliationId IS NULL
                                THEN 'No lines'
                            ELSE CONVERT(NVARCHAR(10),
                                     ISNULL(det.OkCount, 0)) + ' OK / '
                                 + CONVERT(NVARCHAR(10),
                                     ISNULL(det.ShortCount, 0)) + ' short'
                                 + CASE
                                       WHEN ISNULL(det.ExtraCount, 0) > 0
                                       THEN ' / '
                                            + CONVERT(NVARCHAR(10),
                                                det.ExtraCount) + ' extra'
                                       ELSE ''
                                   END
                        END
                FROM VehicleReconciliations r
                INNER JOIN Vehicles v
                    ON v.VehicleId = r.VehicleId
                LEFT JOIN Users s
                    ON s.UserId = r.SalesmanId
                LEFT JOIN Users d
                    ON d.UserId = r.DriverId
                LEFT JOIN
                (
                    SELECT
                        VehicleReconciliationId,
                        OkCount = SUM(
                            CASE WHEN VariancePackets = 0 THEN 1 ELSE 0 END),
                        ShortCount = SUM(
                            CASE WHEN VariancePackets < 0 THEN 1 ELSE 0 END),
                        ExtraCount = SUM(
                            CASE WHEN VariancePackets > 0 THEN 1 ELSE 0 END)
                    FROM VehicleReconciliationDetails
                    GROUP BY VehicleReconciliationId
                ) det
                    ON det.VehicleReconciliationId =
                       r.VehicleReconciliationId
                WHERE r.DealerId = @DealerId
                  AND
                  (
                    ISNULL(v.VehicleNumber, '') LIKE @Search
                    OR ISNULL(r.Remarks, '') LIKE @Search
                  )
                  AND (@Approved = ''
                       OR CONVERT(NVARCHAR(5), r.IsApproved) = @Approved)
                ORDER BY r.ReconciliationDate DESC,
                         r.VehicleReconciliationId DESC";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    cmd.Parameters.AddWithValue(
                        "@Search",
                        "%" + search + "%");

                    cmd.Parameters.AddWithValue(
                        "@Approved",
                        approved);

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        gvReconciliations.DataSource = dt;
                        gvReconciliations.DataBind();
                    }
                }
            }
        }


        protected void txtSearch_TextChanged(
            object sender,
            EventArgs e)
        {
            LoadReconciliations();
        }


        protected void FilterChanged(
            object sender,
            EventArgs e)
        {
            LoadReconciliations();
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
