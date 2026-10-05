using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Account
{
    public partial class ChangePassword : System.Web.UI.Page
    {
        private int UserId
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
                    "Admin",
                    StringComparison.OrdinalIgnoreCase) &&
                !Session["UserRole"].ToString().Equals(
                    "Dealer",
                    StringComparison.OrdinalIgnoreCase) &&
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
                LoadAccount();
            }
        }


        private void LoadAccount()
        {
            string query = @"
                SELECT
                    FullName,
                    ISNULL(Email, '') AS Email,
                    ISNULL(Phone, '') AS Phone,
                    Role,
                    IsActive
                FROM Users
                WHERE UserId = @UserId";

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@UserId",
                        SqlDbType.Int).Value = UserId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            if (dt.Rows.Count == 0)
            {
                Session.Abandon();
                Response.Redirect(
                    "~/Account/Login.aspx",
                    false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }

            DataRow row = dt.Rows[0];

            lblFullName.Text = row["FullName"].ToString();
            lblEmail.Text = row["Email"].ToString();
            lblRole.Text = row["Role"].ToString();
            lblPhone.Text =
                string.IsNullOrEmpty(row["Phone"].ToString())
                    ? "-"
                    : row["Phone"].ToString();

            bool active =
                Convert.ToBoolean(row["IsActive"]);

            lblStatus.Text = active ? "Active" : "Inactive";
            lblStatus.CssClass =
                active ? "text-success" : "text-danger";
        }


        // Current password is checked against the stored hash the
        // same way Login.aspx does, so a wrong password cannot
        // overwrite the account.
        protected void btnSave_Click(object sender, EventArgs e)
        {
            Page.Validate();

            if (!Page.IsValid)
            {
                ShowMessage(
                    "Please correct the highlighted fields.",
                    false);

                return;
            }

            string currentHash =
                PasswordHelper.HashPassword(txtCurrent.Text);

            string newHash =
                PasswordHelper.HashPassword(txtNew.Text);

            string query = @"
                UPDATE Users
                SET PasswordHash = @NewHash,
                    UpdatedAt = GETDATE()
                WHERE UserId = @UserId
                  AND PasswordHash = @CurrentHash";

            int affected;

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@NewHash",
                        SqlDbType.NVarChar,
                        512).Value = newHash;

                    cmd.Parameters.Add(
                        "@UserId",
                        SqlDbType.Int).Value = UserId;

                    cmd.Parameters.Add(
                        "@CurrentHash",
                        SqlDbType.NVarChar,
                        512).Value = currentHash;

                    affected = cmd.ExecuteNonQuery();
                }
            }

            if (affected == 0)
            {
                ShowMessage(
                    "The current password is not correct.",
                    false);

                return;
            }

            txtCurrent.Text = "";
            txtNew.Text = "";
            txtConfirm.Text = "";

            ShowMessage(
                "Password updated successfully.",
                true);
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
