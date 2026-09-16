using System;
using System.Data.SqlClient;
using BWDMS.Data;

namespace BWDMS.Account
{
    public partial class CreateAdmin : System.Web.UI.Page
    {
        protected void btnCreate_Click(object sender, EventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            string passwordHash =
                PasswordHelper.HashPassword(password);

            using (SqlConnection connection =
                   DatabaseHelper.GetConnection())
            {
                string query = @"
                    INSERT INTO Users
                    (
                        FullName,
                        Email,
                        PasswordHash,
                        Role,
                        IsActive
                    )
                    VALUES
                    (
                        @FullName,
                        @Email,
                        @PasswordHash,
                        'Admin',
                        1
                    )";

                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue(
                        "@FullName", fullName);

                    command.Parameters.AddWithValue(
                        "@Email", email);

                    command.Parameters.AddWithValue(
                        "@PasswordHash", passwordHash);

                    try
                    {
                        connection.Open();

                        command.ExecuteNonQuery();

                        lblMessage.Text =
                            "Admin created successfully!";

                        lblMessage.CssClass =
                            "alert alert-success d-block mt-3";
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 2627)
                        {
                            lblMessage.Text =
                                "This email already exists.";
                        }
                        else
                        {
                            lblMessage.Text =
                                "Database error: " + ex.Message;
                        }

                        lblMessage.CssClass =
                            "alert alert-danger d-block mt-3";
                    }
                }
            }
        }
    }
}