using System;
using System.Data.SqlClient;
using BWDMS.Data;

namespace BWDMS.Account
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            string passwordHash =
                PasswordHelper.HashPassword(password);

            using (SqlConnection connection =
                   DatabaseHelper.GetConnection())
            {
                string query = @"
                    SELECT
                        UserId,
                        FullName,
                        Email,
                        Role
                    FROM Users
                    WHERE Email = @Email
                    AND PasswordHash = @PasswordHash
                    AND IsActive = 1";

                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue(
                        "@Email", email);

                    command.Parameters.AddWithValue(
                        "@PasswordHash", passwordHash);

                    connection.Open();

                    using (SqlDataReader reader =
                           command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string role =
                                reader["Role"].ToString();

                            Session["UserId"] =
                                reader["UserId"].ToString();

                            Session["FullName"] =
                                reader["FullName"].ToString();

                            Session["UserEmail"] =
                                reader["Email"].ToString();

                            Session["UserRole"] =
                                role;


                            // ADMIN
                            if (role == "Admin")
                            {
                                Response.Redirect(
                                    "~/Admin/Dashboard.aspx");

                                return;
                            }


                            // DEALER
                            if (role == "Dealer")
                            {
                                Response.Redirect(
                                    "~/Dealer/Dashboard.aspx");

                                return;
                            }


                            // SALESMAN
                            if (role == "Salesman")
                            {
                                Response.Redirect(
                                    "~/Salesman/Dashboard.aspx");

                                return;
                            }


                            // Invalid role

                            lblMessage.Text =
                                "Invalid user role.";

                            lblMessage.Visible = true;
                        }
                    }
                }
            }


            // Login failed

            lblMessage.Text =
                "Invalid email address or password.";

            lblMessage.Visible = true;
        }
    }
}