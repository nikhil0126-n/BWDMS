using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Admin
{
    public partial class AddUser : System.Web.UI.Page
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


        // ============================================================
        // PAGE LOAD
        // ============================================================

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            Response.Cache.SetExpires(
                DateTime.UtcNow.AddYears(-1));

            Response.Cache.SetRevalidation(
                System.Web.HttpCacheRevalidation.AllCaches);


            // ==========================================
            // CHECK LOGIN
            // ==========================================

            if (Session["UserId"] == null)
            {
                Response.Redirect(
                    "~/Account/Login.aspx",
                    false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }


            // ==========================================
            // ONLY ADMIN CAN ACCESS
            // ==========================================

            if (Session["UserRole"] == null ||
                !Session["UserRole"].ToString().Equals(
                    "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect(
                    BWDMS.Data.AppAuth.HomeUrl(
                        Session["UserRole"]),
                    false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }


            if (!IsPostBack)
            {
                LoadDealers(null);

                if (TargetUserId > 0)
                {
                    SetEditMode();

                    LoadAccount();
                }
                else
                {
                    SetAddMode();

                    // ==========================================
                    // AddUser.aspx?role=Salesman opens the form
                    // ready for a salesman with the dealer box
                    // already showing.
                    // ==========================================

                    string requestedRole =
                        Request.QueryString["role"];

                    if (requestedRole == "Admin" ||
                        requestedRole == "Dealer" ||
                        requestedRole == "Salesman")
                    {
                        SetSelectedValue(
                            ddlRole,
                            requestedRole);
                    }

                    ApplyRoleRules();
                }
            }
        }


        // ============================================================
        // ADD / EDIT MODE LABELS
        // ============================================================

        private void SetAddMode()
        {
            lblPageTitle.Text = "Add User";
            lblHeading.Text = "Add User";
            lblSubHeading.Text =
                "Create a new account for the system";
            lblFormTitle.Text = "Account Details";
            btnSave.Text = "Create User";

            // Password is mandatory when creating.
            rfvPassword.Enabled = true;
            revPassword.Enabled = true;
            spanPasswordMark.Visible = true;
            lblPasswordHint.Visible = false;
        }


        private void SetEditMode()
        {
            lblPageTitle.Text = "Edit User";
            lblHeading.Text = "Edit User";
            lblSubHeading.Text =
                "Update the account details";
            lblFormTitle.Text = "Account Details";
            btnSave.Text = "Update User";

            // Password becomes optional while editing.
            rfvPassword.Enabled = false;
            revPassword.Enabled = false;
            spanPasswordMark.Visible = false;
            lblPasswordHint.Visible = true;
        }


        // ============================================================
        // DEALER DROPDOWN (only used by salesman accounts)
        // ============================================================

        private void LoadDealers(object selectedValue)
        {
            string query = @"
                SELECT
                    UserId,
                    FullName
                FROM Users
                WHERE Role = 'Dealer'
                ORDER BY FullName";

            DataTable table = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(table);
                    }
                }
            }

            ddlDealer.Items.Clear();

            ddlDealer.Items.Add(
                new ListItem(
                    "Select a dealer",
                    ""));

            foreach (DataRow row in table.Rows)
            {
                ddlDealer.Items.Add(
                    new ListItem(
                        row["FullName"].ToString(),
                        row["UserId"].ToString()));
            }

            if (selectedValue != null)
            {
                ListItem item =
                    ddlDealer.Items.FindByValue(
                        selectedValue.ToString());

                if (item != null)
                {
                    ddlDealer.SelectedValue =
                        selectedValue.ToString();
                }
            }
        }


        // ============================================================
        // ROLE CHANGED - show the dealer box for salesmen only
        // ============================================================

        protected void ddlRole_Changed(
            object sender,
            EventArgs e)
        {
            ApplyRoleRules();
        }


        private void ApplyRoleRules()
        {
            bool needsDealer =
                ddlRole.SelectedValue == "Salesman";

            divDealer.Visible = needsDealer;

            rfvDealer.Enabled = needsDealer;
        }


        // ============================================================
        // LOAD EXISTING ACCOUNT
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
                        IsActive,
                        DealerId = ISNULL(DealerId, 0)
                    FROM Users
                    WHERE UserId = @UserId";

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

                        using (SqlDataReader reader =
                            cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                Response.Redirect(
                                    "~/Admin/Users.aspx",
                                    false);

                                Context.ApplicationInstance
                                    .CompleteRequest();

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
                                Convert.ToBoolean(
                                    reader["IsActive"])
                                    ? "1"
                                    : "0";

                            int dealerId =
                                Convert.ToInt32(
                                    reader["DealerId"]);

                            if (dealerId > 0)
                            {
                                LoadDealers(dealerId);
                            }

                            ApplyRoleRules();
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
        // SAVE (CREATE OR UPDATE)
        // ============================================================

        protected void btnSave_Click(
            object sender,
            EventArgs e)
        {
            bool isEdit = TargetUserId > 0;

            string password = txtPassword.Text;

            // --------------------------------------------------------
            // In edit mode the password field is optional and its
            // validators are disabled, so re-check the strength rule
            // here for both modes.
            // --------------------------------------------------------

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
            // CK_Users_Role allows only Admin / Dealer / Salesman.
            // ========================================================

            string role = ddlRole.SelectedValue;

            if (role != "Admin" &&
                role != "Dealer" &&
                role != "Salesman")
            {
                ShowMessage(
                    "Please select a valid role.",
                    false);

                return;
            }


            // ========================================================
            // AN ADMIN CAN NEVER DEMOTE HIS OWN ACCOUNT,
            // OTHERWISE THE SYSTEM CAN BE LEFT WITHOUT AN ADMIN.
            // ========================================================

            int currentUserId =
                Convert.ToInt32(Session["UserId"]);

            if (isEdit &&
                TargetUserId == currentUserId &&
                role != "Admin")
            {
                ShowMessage(
                    "You cannot change the role of your own account.",
                    false);

                return;
            }


            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();
            bool isActive = ddlStatus.SelectedValue == "1";

            object dealerId = DBNull.Value;

            if (role == "Salesman")
            {
                if (string.IsNullOrEmpty(
                    ddlDealer.SelectedValue))
                {
                    ShowMessage(
                        "Please select a dealer for a salesman account.",
                        false);

                    return;
                }

                dealerId =
                    Convert.ToInt32(
                        ddlDealer.SelectedValue);
            }


            try
            {
                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    con.Open();


                    // =============================================
                    // EMAIL UNIQUENESS
                    // =============================================

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
                            300).Value = email;

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
                                "A user with this email already exists.",
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
                            dealerId,
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
                            dealerId,
                            password);
                    }
                }

                Session["UsersFlashText"] =
                    "\"" + fullName + "\" (" + role + ") " +
                    (isEdit
                        ? "updated successfully."
                        : "created successfully.");

                Session["UsersFlashOk"] = "1";

                Response.Redirect(
                    "~/Admin/Users.aspx",
                    false);

                Context.ApplicationInstance.CompleteRequest();
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
                    "Error saving user: " +
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
            object dealerId,
            string password)
        {
            string query = @"
                INSERT INTO Users
                (
                    FullName,
                    Email,
                    Phone,
                    PasswordHash,
                    Role,
                    IsActive,
                    DealerId,
                    CreatedAt
                )
                VALUES
                (
                    @FullName,
                    @Email,
                    @Phone,
                    @PasswordHash,
                    @Role,
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
                    "@Phone",
                    SqlDbType.NVarChar,
                    40).Value =
                    string.IsNullOrWhiteSpace(phone)
                        ? (object)DBNull.Value
                        : phone;

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
                    "@IsActive",
                    SqlDbType.Bit).Value = isActive;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = dealerId;

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
            object dealerId,
            string password)
        {
            string query = @"
                UPDATE Users
                SET
                    FullName = @FullName,
                    Email = @Email,
                    Phone = @Phone,
                    Role = @Role,
                    IsActive = @IsActive,
                    DealerId = @DealerId,
                    UpdatedAt = GETDATE()";

            // Password only changes when one was typed.
            if (!string.IsNullOrEmpty(password))
            {
                query += @",
                    PasswordHash = @PasswordHash";
            }

            query += @"
                WHERE UserId = @UserId";

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
                    "@Phone",
                    SqlDbType.NVarChar,
                    40).Value =
                    string.IsNullOrWhiteSpace(phone)
                        ? (object)DBNull.Value
                        : phone;

                cmd.Parameters.Add(
                    "@Role",
                    SqlDbType.NVarChar,
                    40).Value = role;

                cmd.Parameters.Add(
                    "@IsActive",
                    SqlDbType.Bit).Value = isActive;

                cmd.Parameters.Add(
                    "@DealerId",
                    SqlDbType.Int).Value = dealerId;

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
                    SqlDbType.Int).Value =
                    TargetUserId;

                cmd.ExecuteNonQuery();
            }
        }


        private void ShowMessage(
            string message,
            bool success)
        {
            lblMessage.Text = message;

            lblMessage.CssClass = success
                ? "alert alert-success d-block"
                : "alert alert-danger d-block";

            lblMessage.Visible = true;
        }
    }
}
