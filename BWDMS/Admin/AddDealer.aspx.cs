using System;
using System.Data;
using System.Data.SqlClient;
using BWDMS.Data;

namespace BWDMS.Admin
{
    // ================================================================
    // Admin > Add / Edit Dealer
    //
    // Without ?id= this creates a new dealer account.
    // With    ?id= it edits that dealer; the password fields become
    // optional and a blank password keeps the current hash.
    // ================================================================

    public partial class AddDealer : System.Web.UI.Page
    {
        // 0 = add mode, > 0 = edit mode.
        private int EditId { get; set; }


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
                    BWDMS.Data.AppAuth.HomeUrl(
                        Session["UserRole"]),
                    false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }


            int editId;

            int.TryParse(
                Request.QueryString["id"],
                out editId);

            EditId = editId;


            if (!IsPostBack && EditId > 0)
            {
                ApplyEditMode();

                if (!LoadDealer())
                {
                    return;
                }
            }
        }


        // ============================================================
        // EDIT MODE
        // ============================================================

        private void ApplyEditMode()
        {
            lblHeading.Text = "Edit Dealer";

            lblSubHeading.Text =
                "Update the dealer account details";

            // Password is optional when editing.
            rfvPassword.Enabled = false;

            revPassword.Enabled = false;

            rfvConfirmPassword.Enabled = false;

            lblPasswordHint.InnerText =
                "leave blank to keep the current password";

            btnCreateDealer.Text = "Update Dealer";

            txtPassword.Attributes.Add(
                "placeholder",
                "Leave blank to keep current password");

            txtConfirmPassword.Attributes.Add(
                "placeholder",
                "Re-enter only if changing the password");
        }


        private bool LoadDealer()
        {
            string query = @"
                SELECT
                    FullName,
                    Email,
                    Phone,
                    IsActive
                FROM Users
                WHERE UserId = @UserId
                  AND Role = 'Dealer'";

            using (SqlConnection connection =
                   DatabaseHelper.GetConnection())
            {
                connection.Open();

                using (SqlCommand command =
                       new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue(
                        "@UserId",
                        EditId);

                    using (SqlDataReader reader =
                           command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            ShowMessage(
                                "Dealer not found.",
                                "alert alert-danger d-block");

                            return false;
                        }

                        txtFullName.Text =
                            reader["FullName"].ToString();

                        txtEmail.Text =
                            reader["Email"].ToString();

                        txtPhone.Text =
                            reader["Phone"] == DBNull.Value
                                ? ""
                                : reader["Phone"].ToString();

                        ddlStatus.SelectedValue =
                            Convert.ToBoolean(
                                reader["IsActive"])
                                ? "1"
                                : "0";
                    }
                }
            }

            return true;
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

            bool isActive =
                ddlStatus.SelectedValue == "1";

            string password =
                txtPassword.Text;


            using (SqlConnection connection =
                   DatabaseHelper.GetConnection())
            {
                connection.Open();


                // ======================================
                // CHECK EMAIL (ignores the row being edited)
                // ======================================

                string checkQuery = @"
                    SELECT COUNT(*)
                    FROM Users
                    WHERE Email = @Email
                      AND UserId <> @UserId";


                using (SqlCommand checkCommand =
                       new SqlCommand(
                           checkQuery,
                           connection))
                {
                    checkCommand.Parameters.AddWithValue(
                        "@Email",
                        email);

                    checkCommand.Parameters.AddWithValue(
                        "@UserId",
                        EditId);


                    int count =
                        Convert.ToInt32(
                            checkCommand.ExecuteScalar());


                    if (count > 0)
                    {
                        ShowMessage(
                            "A user with this email already exists.",
                            "alert alert-danger d-block");

                        return;
                    }
                }


                if (EditId > 0)
                {
                    if (!UpdateDealer(
                            connection,
                            fullName,
                            email,
                            phone,
                            password,
                            isActive))
                    {
                        // Message already shown - stay on the page.
                        return;
                    }
                }
                else
                {
                    CreateDealer(
                        connection,
                        fullName,
                        email,
                        phone,
                        password,
                        isActive);
                }
            }


            Response.Redirect(
                "~/Admin/Dealers.aspx",
                false);

            Context.ApplicationInstance.CompleteRequest();
        }


        // ============================================================
        // INSERT
        // ============================================================

        private void CreateDealer(
            SqlConnection connection,
            string fullName,
            string email,
            string phone,
            string password,
            bool isActive)
        {
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
                    PasswordHelper.HashPassword(
                        password));

                command.Parameters.AddWithValue(
                    "@IsActive",
                    isActive);


                command.ExecuteNonQuery();
            }
        }


        // ============================================================
        // UPDATE
        //
        // A blank password keeps the stored hash, so an admin can
        // fix a phone number without resetting the dealer's login.
        // ============================================================

        private bool UpdateDealer(
            SqlConnection connection,
            string fullName,
            string email,
            string phone,
            string password,
            bool isActive)
        {
            bool changePassword =
                !string.IsNullOrWhiteSpace(password);

            string updateQuery = changePassword
                ? @"
                UPDATE Users
                SET FullName = @FullName,
                    Email = @Email,
                    Phone = @Phone,
                    PasswordHash = @PasswordHash,
                    IsActive = @IsActive,
                    UpdatedAt = GETDATE()
                WHERE UserId = @UserId
                  AND Role = 'Dealer'"
                : @"
                UPDATE Users
                SET FullName = @FullName,
                    Email = @Email,
                    Phone = @Phone,
                    IsActive = @IsActive,
                    UpdatedAt = GETDATE()
                WHERE UserId = @UserId
                  AND Role = 'Dealer'";


            using (SqlCommand command =
                   new SqlCommand(
                       updateQuery,
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
                    "@IsActive",
                    isActive);

                command.Parameters.AddWithValue(
                    "@UserId",
                    EditId);

                if (changePassword)
                {
                    command.Parameters.AddWithValue(
                        "@PasswordHash",
                        PasswordHelper.HashPassword(
                            password));
                }

                int affected =
                    command.ExecuteNonQuery();

                if (affected == 0)
                {
                    ShowMessage(
                        "Dealer not found.",
                        "alert alert-danger d-block");

                    return false;
                }
            }

            return true;
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
