using System;
using System.Data.SqlClient;
using BWDMS.Data;

namespace BWDMS.Admin
{
    public partial class AddDealer : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Prevent caching
            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            Response.Cache.SetExpires(
                DateTime.UtcNow.AddYears(-1));

            Response.Cache.SetRevalidation(
                System.Web.HttpCacheRevalidation.AllCaches);


            // Check login
            if (Session["UserId"] == null)
            {
                Response.Redirect(
                    "~/Account/Login.aspx",
                    false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }


            // Only Admin
            if (Session["UserRole"] == null ||
                !Session["UserRole"].ToString()
                    .Equals("Admin",
                        StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect(
                    "~/Account/Login.aspx",
                    false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }
        }


        protected void btnCreateDealer_Click(
            object sender,
            EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }


            string fullName =
                txtFullName.Text.Trim();

            string email =
                txtEmail.Text.Trim();

            string phone =
                txtPhone.Text.Trim();

            string password =
                txtPassword.Text;

            bool isActive =
                ddlStatus.SelectedValue == "1";


            // ==========================================
            // HASH PASSWORD
            // ==========================================

            string passwordHash =
                PasswordHelper.HashPassword(password);


            using (SqlConnection connection =
                   DatabaseHelper.GetConnection())
            {
                connection.Open();


                // ======================================
                // CHECK EMAIL
                // ======================================

                string checkQuery = @"
                    SELECT COUNT(*)
                    FROM Users
                    WHERE Email = @Email";


                using (SqlCommand checkCommand =
                       new SqlCommand(
                           checkQuery,
                           connection))
                {
                    checkCommand.Parameters.AddWithValue(
                        "@Email",
                        email);


                    int count =
                        Convert.ToInt32(
                            checkCommand.ExecuteScalar());


                    if (count > 0)
                    {
                        ShowMessage(
                            "A user with this email already exists.",
                            "alert alert-danger");

                        return;
                    }
                }


                // ======================================
                // INSERT DEALER
                // ======================================

                string insertQuery = @"
                    INSERT INTO Users
                    (
                        FullName,
                        Email,
                        Phone,
                        PasswordHash,
                        Role,
                        IsActive
                    )
                    VALUES
                    (
                        @FullName,
                        @Email,
                        @Phone,
                        @PasswordHash,
                        'Dealer',
                        @IsActive
                    )";


                using (SqlCommand command =
                       new SqlCommand(
                           insertQuery,
                           connection))
                {
                    command.Parameters.AddWithValue(
                        "@FullName",
                        fullName);

                    command.Parameters.AddWithValue(
                        "@Email",
                        email);

                    command.Parameters.AddWithValue(
                        "@Phone",
                        phone);

                    command.Parameters.AddWithValue(
                        "@PasswordHash",
                        passwordHash);

                    command.Parameters.AddWithValue(
                        "@IsActive",
                        isActive);


                    command.ExecuteNonQuery();
                }
            }


            // ==========================================
            // SUCCESS
            // ==========================================

            Response.Redirect(
                "~/Admin/Dealers.aspx",
                false);

            Context.ApplicationInstance.CompleteRequest();
        }


        private void ShowMessage(
            string message,
            string cssClass)
        {
            lblMessage.Text = message;

            lblMessage.CssClass = cssClass;

            lblMessage.Visible = true;
        }
    }
}