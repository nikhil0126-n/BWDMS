using System;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Salesman
{
    public partial class Profile : System.Web.UI.Page
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
                Response.Redirect("~/Account/Login.aspx", false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }


            string role = Session["UserRole"] == null
                ? string.Empty
                : Session["UserRole"].ToString();

            if (!role.Equals(
                "Salesman",
                StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect(
                    AppAuth.HomeUrl(Session["UserRole"]),
                    false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }


            if (!IsPostBack)
            {
                LoadProfile();
            }
        }


        // ======================================================
        // LOAD PROFILE
        // ======================================================

        private void LoadProfile()
        {
            string fullName = Session["FullName"] == null
                ? "Salesman"
                : Session["FullName"].ToString();

            string email = Session["UserEmail"] == null
                ? string.Empty
                : Session["UserEmail"].ToString();

            string userRole = Session["UserRole"] == null
                ? "Salesman"
                : Session["UserRole"].ToString();

            string phone = string.Empty;

            string joined = string.Empty;


            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT Phone, CreatedAt
                        FROM Users
                        WHERE UserId = @UserId",
                    con))
                {
                    cmd.Parameters.AddWithValue("@UserId", UserId);

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader["Phone"] != DBNull.Value)
                            {
                                phone = reader["Phone"].ToString();
                            }

                            if (reader["CreatedAt"] != DBNull.Value)
                            {
                                joined = Convert
                                    .ToDateTime(reader["CreatedAt"])
                                    .ToString("dd MMM yyyy");
                            }
                        }
                    }
                }
            }


            lblFullName.Text = fullName;

            lblRole.Text = userRole;

            lblEmail.Text = string.IsNullOrWhiteSpace(email)
                ? "-"
                : email;

            lblEmailForm.Text = string.IsNullOrWhiteSpace(email)
                ? "-"
                : email;

            lblPhone.Text = string.IsNullOrWhiteSpace(phone)
                ? "Not provided"
                : phone;

            lblJoined.Text = string.IsNullOrWhiteSpace(joined)
                ? "-"
                : joined;

            lblInitial.Text = string.IsNullOrWhiteSpace(fullName)
                ? "S"
                : fullName.Substring(0, 1).ToUpper();

            txtFullName.Text = fullName;

            txtPhone.Text = phone;
        }


        // ======================================================
        // SAVE PROFILE
        // ======================================================

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string fullName = txtFullName.Text.Trim();

            string phone = txtPhone.Text.Trim();


            if (string.IsNullOrWhiteSpace(fullName))
            {
                ShowMessage("Full name is required.", false);

                return;
            }


            if (fullName.Length > 200)
            {
                ShowMessage(
                    "Full name cannot be longer than 200 characters.",
                    false);

                return;
            }


            if (phone.Length > 40)
            {
                ShowMessage(
                    "Phone number cannot be longer than 40 characters.",
                    false);

                return;
            }


            try
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand cmd = new SqlCommand(
                        @"UPDATE Users
                             SET FullName = @FullName,
                                 Phone = @Phone
                           WHERE UserId = @UserId",
                        con))
                    {
                        cmd.Parameters.AddWithValue("@FullName", fullName);

                        cmd.Parameters.AddWithValue(
                            "@Phone",
                            string.IsNullOrWhiteSpace(phone)
                                ? (object)DBNull.Value
                                : phone);

                        cmd.Parameters.AddWithValue("@UserId", UserId);

                        con.Open();

                        cmd.ExecuteNonQuery();
                    }
                }


                // Keep the shell in sync on this postback too; the master
                // only loads user details on its first (non-postback) load.
                Session["FullName"] = fullName;

                Label shellName =
                    Master.FindControl("lblUserName") as Label;

                Label shellInitial =
                    Master.FindControl("lblUserInitial") as Label;

                if (shellName != null)
                {
                    shellName.Text = fullName;
                }

                if (shellInitial != null)
                {
                    shellInitial.Text =
                        fullName.Substring(0, 1).ToUpper();
                }


                LoadProfile();

                ShowMessage("Your profile has been updated.", true);
            }
            catch (SqlException)
            {
                ShowMessage(
                    "Could not save your profile. Please try again.",
                    false);
            }
        }


        private void ShowMessage(string message, bool success)
        {
            lblMessage.Text = message;

            lblMessage.CssClass = success
                ? "pf-message pf-ok"
                : "pf-message pf-error";
        }
    }
}
