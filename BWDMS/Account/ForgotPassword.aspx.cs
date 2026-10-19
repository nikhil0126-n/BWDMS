using System;
using System.Data;
using System.Data.SqlClient;
using BWDMS.Data;

namespace BWDMS.Account
{
    public partial class ForgotPassword : System.Web.UI.Page
    {
        // Keeps the email that has already been verified in step 1
        // so step 2 can only be reached after a successful lookup.
        private string ResetEmail
        {
            get { return Convert.ToString(ViewState["ResetEmail"]); }
            set { ViewState["ResetEmail"] = value; }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            Response.Cache.SetExpires(
                DateTime.UtcNow.AddYears(-1));

            if (!IsPostBack)
            {
                ShowStep(ResetStep.Request);
            }
        }


        // ============================================================
        // STEP 1 - verify the email belongs to an active account
        // ============================================================
        protected void btnSend_Click(object sender, EventArgs e)
        {
            Page.Validate("ForgotGroup");

            if (!Page.IsValid)
            {
                return;
            }

            string email = txtEmail.Text.Trim();

            if (!EmailExists(email))
            {
                lblRequestMessage.Visible = true;
                lblRequestMessage.Text =
                    "No active account was found for that email address.";

                return;
            }

            ResetEmail = email;

            lblResetEmail.Text = email;

            txtNew.Text = "";
            txtConfirm.Text = "";

            ShowStep(ResetStep.Reset);
        }


        // ============================================================
        // STEP 2 - save the new password for the verified account
        // ============================================================
        protected void btnReset_Click(object sender, EventArgs e)
        {
            // Guard: the reset step must follow a verified email.
            if (string.IsNullOrEmpty(ResetEmail))
            {
                ShowStep(ResetStep.Request);

                return;
            }

            Page.Validate("ResetGroup");

            if (!Page.IsValid)
            {
                return;
            }

            string newHash =
                PasswordHelper.HashPassword(txtNew.Text);

            string query = @"
                UPDATE Users
                SET PasswordHash = @PasswordHash,
                    UpdatedAt = GETDATE()
                WHERE Email = @Email
                  AND IsActive = 1";

            int affected;

            using (SqlConnection connection =
                   DatabaseHelper.GetConnection())
            {
                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    command.Parameters.Add(
                        "@PasswordHash",
                        SqlDbType.NVarChar,
                        512).Value = newHash;

                    command.Parameters.Add(
                        "@Email",
                        SqlDbType.NVarChar,
                        300).Value = ResetEmail;

                    connection.Open();

                    affected = command.ExecuteNonQuery();
                }
            }

            if (affected == 0)
            {
                ResetEmail = null;

                txtEmail.Text = "";
                txtNew.Text = "";
                txtConfirm.Text = "";

                ShowStep(ResetStep.Request);

                lblRequestMessage.Visible = true;
                lblRequestMessage.Text =
                    "We could not reset the password. " +
                    "Please enter your email again.";

                return;
            }

            ResetEmail = null;

            txtNew.Text = "";
            txtConfirm.Text = "";

            ShowStep(ResetStep.Success);
        }


        // ============================================================
        // Returns to step 1 so the user can enter another email
        // ============================================================
        protected void btnBack_Click(object sender, EventArgs e)
        {
            ResetEmail = null;

            txtEmail.Text = "";

            lblRequestMessage.Visible = false;

            ShowStep(ResetStep.Request);
        }


        private bool EmailExists(string email)
        {
            string query = @"
                SELECT COUNT(*)
                FROM Users
                WHERE Email = @Email
                  AND IsActive = 1";

            using (SqlConnection connection =
                   DatabaseHelper.GetConnection())
            {
                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    command.Parameters.Add(
                        "@Email",
                        SqlDbType.NVarChar,
                        300).Value = email;

                    connection.Open();

                    int count = Convert.ToInt32(
                        command.ExecuteScalar());

                    return count > 0;
                }
            }
        }


        private enum ResetStep
        {
            Request = 1,
            Reset = 2,
            Success = 3
        }


        private void ShowStep(ResetStep step)
        {
            pnlRequest.Visible = step == ResetStep.Request;

            pnlReset.Visible = step == ResetStep.Reset;

            pnlSuccess.Visible = step == ResetStep.Success;
        }
    }
}
