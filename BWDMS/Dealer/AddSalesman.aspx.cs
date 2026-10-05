using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class AddSalesman : System.Web.UI.Page
    {
        // ============================================================
        // EDIT ID (0 = Add mode)
        // ============================================================

        private int TargetUserId
        {
            get
            {
                int id;

                if (int.TryParse(
                    Request.QueryString["id"],
                    out id))
                {
                    return id;
                }

                return 0;
            }
        }


        private int DealerId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }


        // ============================================================
        // PAGE LOAD
        // ============================================================

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }

            if (Session["UserRole"] == null ||
                Session["UserRole"].ToString() != "Dealer")
            {
                Response.Redirect(
                    BWDMS.Data.AppAuth.HomeUrl(Session["UserRole"]));
                return;
            }


            if (!IsPostBack)
            {
                if (TargetUserId > 0)
                {
                    SetEditMode();

                    LoadAccount();
                }
                else
                {
                    SetAddMode();
                }
            }
        }


        private void SetAddMode()
        {
            lblPageTitle.Text = "Add Account";
            lblHeading.Text = "Add Account";
            lblSubHeading.Text =
                "Create a salesman login for your dealership";
            lblFormTitle.Text = "Account Details";
            btnSave.Text = "Create Account";

            // Password is mandatory when creating.
            rfvPassword.Enabled = true;
            revPassword.Enabled = true;
            spanPasswordMark.Visible = true;
            lblPasswordHint.Visible = false;
        }


        private void SetEditMode()
        {
            lblPageTitle.Text = "Edit Account";
            lblHeading.Text = "Edit Account";
            lblSubHeading.Text =
                "Update salesman details";
            lblFormTitle.Text = "Account Details";
            btnSave.Text = "Update Account";

            // Password becomes optional while editing.
            rfvPassword.Enabled = false;
            revPassword.Enabled = false;
            spanPasswordMark.Visible = false;
            lblPasswordHint.Visible = true;
        }


        // ============================================================
        // LOAD EXISTING ACCOUNT
        //
        // Only Salesman / Driver rows owned by this dealer can be
        // opened. Anything else (Admin, another dealer, another
        // dealer's staff) is bounced back to the list.
        // ============================================================

        private void LoadAccount()
        {
            try
            {
                string query = @"
                    SELECT
                        FullName,
                        Email,
                        Phone,
                        Role,
                        IsActive
                    FROM Users
                    WHERE UserId = @UserId
                    AND DealerId = @DealerId
                    AND Role = 'Salesman'";

                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    con.Open();

                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@UserId",
                            SqlDbType.Int).Value =
                            TargetUserId;

                        cmd.Parameters.Add(
                            "@DealerId",
                            SqlDbType.Int).Value =
                            DealerId;

                        using (SqlDataReader reader =
                            cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                Response.Redirect(
                                    "~/Dealer/Salesmen.aspx");
                                return;
                            }

                            SetEditMode();

                            txtFullName.Text =
                                reader["FullName"].ToString();

                            txtEmail.Text =
                                reader["Email"].ToString();

                            txtPhone.Text =
                                reader["Phone"] == DBNull.Value
                                    ? ""
                                    : reader["Phone"].ToString();

                            SetSelectedValue(
                                ddlRole,
                                reader["Role"].ToString());

                            ddlStatus.SelectedValue =
                                Convert.ToBoolean(reader["IsActive"])
                                    ? "1"
                                    : "0";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error loading account: " +
                    ex.Message,
                    false);
            }
        }


        private void SetSelectedValue(
            DropDownList dropdown,
            string value)
        {
            ListItem item = dropdown.Items.FindByValue(value);

            if (item != null)
            {
                dropdown.SelectedValue = value;
            }
            else
            {
                dropdown.SelectedIndex = 0;
            }
        }


        // ============================================================
        // SAVE
        // ============================================================

        protected void btnSave_Click(
            object sender,
            EventArgs e)
        {
            // --------------------------------------------------------
            // In edit mode the password field is optional and its
            // validators are disabled, so re-check the strength
            // rule here for both modes.
            // --------------------------------------------------------

            string password = txtPassword.Text;

            bool isEdit = TargetUserId > 0;

            if (!isEdit || !string.IsNullOrEmpty(password))
            {
                if (string.IsNullOrEmpty(password) ||
                    password.Length < 6)
                {
                    ShowMessage(
                        "Password must be at least 6 characters.",
                        false);

                    return;
                }
            }


            if (!Page.IsValid)
            {
                return;
            }


            // ========================================================
            // ROLE WHITELIST
            //
            // CK_Users_Role allows only Salesman / Dealer / Admin.
            // A dealer must never be able to create an Admin or a
            // second Dealer account through this form, even by
            // tampering with the posted dropdown value.
            // ========================================================

            string role = ddlRole.SelectedValue;

            if (role != "Salesman")
            {
                ShowMessage(
                    "Only Salesman accounts can be created here.",
                    false);

                return;
            }


            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();
            bool isActive = ddlStatus.SelectedValue == "1";


            try
            {
                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    con.Open();


                    // =================================================
                    // EMAIL UNIQUENESS (global unique index on Email)
                    // =================================================

                    string checkQuery = @"
                        SELECT COUNT(*)
                        FROM Users
                        WHERE Email = @Email";

                    if (isEdit)
                    {
                        checkQuery +=
                            " AND UserId <> @UserId";
                    }

                    using (SqlCommand cmd =
                        new SqlCommand(checkQuery, con))
                    {
                        cmd.Parameters.Add(
                            "@Email",
                            SqlDbType.NVarChar,
                            300).Value =
                            email;

                        if (isEdit)
                        {
                            cmd.Parameters.Add(
                                "@UserId",
                                SqlDbType.Int).Value =
                                TargetUserId;
                        }

                        int count =
                            Convert.ToInt32(
                                cmd.ExecuteScalar());

                        if (count > 0)
                        {
                            ShowMessage(
                                "This email address is already registered.",
                                false);

                            return;
                        }
                    }


                    if (isEdit)
                    {
                        UpdateAccount(
                            con,
                            fullName,
                            email,
                            phone,
                            role,
                            isActive,
                            password);
                    }
                    else
                    {
                        InsertAccount(
                            con,
                            fullName,
                            email,
                            phone,
                            role,
                            isActive,
                            password);
                    }
                }

                Response.Redirect(
                    "~/Dealer/Salesmen.aspx?saved=1");
            }
            catch (SqlException ex)
                when (ex.Number == 2601 ||
                      ex.Number == 2627)
            {
                ShowMessage(
                    "This email address is already registered.",
                    false);
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Error saving account: " +
                    ex.Message,
                    false);
            }
        }


        private void InsertAccount(
            SqlConnection con,
            string fullName,
            string email,
            string phone,
            string role,
            bool isActive,
            string password)
        {
            string query = @"
                INSERT INTO Users
                (
                    FullName,
                    Email,
                    PasswordHash,
                    Role,
                    Phone,
                    IsActive,
                    DealerId,
                    CreatedAt
                )
                VALUES
                (
                    @FullName,
                    @Email,
                    @PasswordHash,
                    @Role,
                    @Phone,
                    @IsActive,
                    @DealerId,
                    GETDATE()
                )";

            using (SqlCommand cmd =
                new SqlCommand(query, con))
            {
                cmd.Parameters.Add(
                    "@FullName",
                    SqlDbType.NVarChar,
                    200).Value = fullName;

                cmd.Parameters.Add(
                    "@Email",
                    SqlDbType.NVarChar,
                    300).Value = email;

                cmd.Parameters.Add(
                    "@PasswordHash",
                    SqlDbType.NVarChar,
                    512).Value =
                    PasswordHelper.HashPassword(password);

                cmd.Parameters.Add(
                    "@Role",
                    SqlDbType.NVarChar,
                    40).Value = role;

                cmd.Parameters.Add(
                    "@Phone",
                    SqlDbType.NVarChar,
                    40).Value =
                    string.IsNullOrWhiteSpace(phone)
                        ? (object)DBNull.Value
                        : phone;

                cmd.Parameters.Add(
                    "@IsActive",
                    SqlDbType.Bit).Value = isActive;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                cmd.ExecuteNonQuery();
            }
        }


        private void UpdateAccount(
            SqlConnection con,
            string fullName,
            string email,
            string phone,
            string role,
            bool isActive,
            string password)
        {
            // UserId + DealerId + role whitelist together stop a
            // dealer from rewriting another dealer's staff record
            // or elevating his own account.
            string query = @"
                UPDATE Users
                SET
                    FullName = @FullName,
                    Email = @Email,
                    Role = @Role,
                    Phone = @Phone,
                    IsActive = @IsActive,
                    UpdatedAt = GETDATE()";

            // Password only changes when one was typed.
            if (!string.IsNullOrEmpty(password))
            {
                query += @",
                    PasswordHash = @PasswordHash";
            }

            query += @"
                WHERE UserId = @UserId
                AND DealerId = @DealerId
                AND Role = 'Salesman'";

            using (SqlCommand cmd =
                new SqlCommand(query, con))
            {
                cmd.Parameters.Add(
                    "@FullName",
                    SqlDbType.NVarChar,
                    200).Value = fullName;

                cmd.Parameters.Add(
                    "@Email",
                    SqlDbType.NVarChar,
                    300).Value = email;

                cmd.Parameters.Add(
                    "@Role",
                    SqlDbType.NVarChar,
                    40).Value = role;

                cmd.Parameters.Add(
                    "@Phone",
                    SqlDbType.NVarChar,
                    40).Value =
                    string.IsNullOrWhiteSpace(phone)
                        ? (object)DBNull.Value
                        : phone;

                cmd.Parameters.Add(
                    "@IsActive",
                    SqlDbType.Bit).Value = isActive;

                if (!string.IsNullOrEmpty(password))
                {
                    cmd.Parameters.Add(
                        "@PasswordHash",
                        SqlDbType.NVarChar,
                        512).Value =
                        PasswordHelper.HashPassword(password);
                }

                cmd.Parameters.Add(
                    "@UserId",
                    SqlDbType.Int).Value = TargetUserId;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = DealerId;

                int rows = cmd.ExecuteNonQuery();

                if (rows == 0)
                {
                    Response.Redirect(
                        "~/Dealer/Salesmen.aspx");
                }
            }
        }


        private void ShowMessage(
            string message,
            bool success)
        {
            pnlMessage.Visible = true;

            lblMessage.Text = message;

            pnlMessage.CssClass = success
                ? "alert alert-success mb-4"
                : "alert alert-danger mb-4";
        }
    }
}
