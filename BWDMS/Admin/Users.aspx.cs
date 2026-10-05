using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Admin
{
    public partial class Users : System.Web.UI.Page
    {
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
                LoadUsers();

                ShowFlashMessage();
            }
        }


        // ==============================================
        // FLASH MESSAGE
        //
        // The message is stored in Session, shown once and
        // immediately cleared, so refreshing the page
        // never shows the same message again.
        // ==============================================

        private void ShowFlashMessage()
        {
            object text = Session["UsersFlashText"];

            if (text == null)
            {
                return;
            }

            bool success =
                Session["UsersFlashOk"] != null &&
                Session["UsersFlashOk"].ToString() == "1";

            Session.Remove("UsersFlashText");
            Session.Remove("UsersFlashOk");

            ShowMessage(text.ToString(), success);
        }


        private void SetFlashMessage(
            string message,
            bool success)
        {
            Session["UsersFlashText"] = message;

            Session["UsersFlashOk"] = success ? "1" : "0";
        }


        private void LoadUsers()
        {
            string search = txtSearch.Text.Trim();
            string role = ddlRole.SelectedValue;

            // PasswordHash is deliberately never selected.
            string query = @"
                SELECT
                    u.UserId,
                    u.FullName,
                    ISNULL(u.Email, '') AS Email,
                    ISNULL(u.Phone, '') AS Phone,
                    u.Role,
                    u.IsActive,
                    DealerName = ISNULL(d.FullName, '-'),
                    CreatedAtText =
                        CONVERT(NVARCHAR(10), u.CreatedAt, 120)
                FROM Users u
                LEFT JOIN Users d
                    ON d.UserId = u.DealerId
                WHERE
                (
                    ISNULL(u.FullName, '') LIKE @Search
                    OR ISNULL(u.Email, '') LIKE @Search
                    OR ISNULL(u.Phone, '') LIKE @Search
                )
                  AND (@Role = '' OR u.Role = @Role)
                ORDER BY u.Role, u.FullName";

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue(
                        "@Search",
                        "%" + search + "%");

                    cmd.Parameters.AddWithValue(
                        "@Role",
                        role);

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            gvUsers.DataSource = dt;
            gvUsers.DataBind();
        }


        protected void txtSearch_TextChanged(
            object sender,
            EventArgs e)
        {
            LoadUsers();
        }


        protected void FilterChanged(
            object sender,
            EventArgs e)
        {
            LoadUsers();
        }


        // ==============================================
        // GRID PAGING
        // ==============================================

        protected void gvUsers_PageIndexChanging(
            object sender,
            GridViewPageEventArgs e)
        {
            gvUsers.PageIndex = e.NewPageIndex;

            LoadUsers();
        }


        // ==============================================
        // GRID COMMANDS
        // ==============================================

        protected void gvUsers_RowCommand(
            object sender,
            GridViewCommandEventArgs e)
        {
            if (e.CommandName != "DeleteUser")
            {
                return;
            }

            int userId;

            if (!int.TryParse(
                e.CommandArgument.ToString(),
                out userId))
            {
                return;
            }

            // When DeleteUser redirected, the page is
            // already being replaced - do not rebind.
            if (DeleteUser(userId))
            {
                return;
            }

            LoadUsers();
        }


        // ==============================================
        // DELETE USER
        //
        // Returns true when the page redirected, so the
        // delete always ends in a Post / Redirect / Get.
        // Refreshing afterwards never repeats the action
        // and never shows the message again.
        // ==============================================

        private bool DeleteUser(int userId)
        {
            // ------------------------------------------
            // NEVER LET AN ADMIN DELETE HIMSELF
            // ------------------------------------------

            int currentUserId;

            if (!int.TryParse(
                Session["UserId"] == null
                    ? ""
                    : Session["UserId"].ToString(),
                out currentUserId))
            {
                currentUserId = 0;
            }

            if (userId == currentUserId)
            {
                SetFlashMessage(
                    "You cannot delete your own account.",
                    false);

                GoToUsers();

                return true;
            }


            string fullName = "";
            string role = "";

            try
            {
                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    con.Open();


                    // --------------------------------------
                    // LOAD THE TARGET USER
                    // --------------------------------------

                    string loadQuery = @"
                        SELECT
                            FullName,
                            Role
                        FROM Users
                        WHERE UserId = @UserId";

                    using (SqlCommand cmd =
                        new SqlCommand(loadQuery, con))
                    {
                        cmd.Parameters.AddWithValue(
                            "@UserId",
                            userId);

                        using (SqlDataReader reader =
                            cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                SetFlashMessage(
                                    "This user was not found. " +
                                    "It may already have been deleted.",
                                    false);

                                GoToUsers();

                                return true;
                            }

                            fullName =
                                reader["FullName"].ToString();

                            role =
                                reader["Role"].ToString();
                        }
                    }


                    // --------------------------------------
                    // THE LAST ADMIN ACCOUNT CAN NEVER BE
                    // REMOVED, OTHERWISE NOBODY CAN LOG IN
                    // TO THE ADMIN PANEL AGAIN.
                    // --------------------------------------

                    if (role.Equals(
                        "Admin",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        string countQuery = @"
                            SELECT COUNT(*)
                            FROM Users
                            WHERE Role = 'Admin'";

                        using (SqlCommand cmd =
                            new SqlCommand(countQuery, con))
                        {
                            int adminCount =
                                Convert.ToInt32(
                                    cmd.ExecuteScalar());

                            if (adminCount <= 1)
                            {
                                SetFlashMessage(
                                    "\"" + fullName + "\" " +
                                    "is the only admin account and cannot be deleted.",
                                    false);

                                GoToUsers();

                                return true;
                            }
                        }
                    }


                    // --------------------------------------
                    // DELETE
                    // --------------------------------------

                    string deleteQuery = @"
                        DELETE FROM Users
                        WHERE UserId = @UserId";

                    using (SqlCommand cmd =
                        new SqlCommand(deleteQuery, con))
                    {
                        cmd.Parameters.AddWithValue(
                            "@UserId",
                            userId);

                        cmd.ExecuteNonQuery();
                    }
                }

                SetFlashMessage(
                    "\"" + fullName + "\" (" + role + ") " +
                    "deleted successfully.",
                    true);
            }
            catch (SqlException ex)
                when (ex.Number == 547)
            {
                // Foreign key: the user owns orders,
                // receipts, stock or other records.
                SetFlashMessage(
                    "\"" + fullName + "\" (" + role + ") " +
                    "is linked to existing records and cannot be deleted. " +
                    "Set the account to Inactive instead.",
                    false);
            }
            catch (Exception ex)
            {
                SetFlashMessage(
                    "Error deleting \"" + fullName + "\": " +
                    ex.Message,
                    false);
            }

            GoToUsers();

            return true;
        }


        // ==============================================
        // REDIRECT WITH THE MESSAGE IN SESSION
        // ==============================================

        private void GoToUsers()
        {
            Response.Redirect(
                "~/Admin/Users.aspx",
                false);

            Context.ApplicationInstance.CompleteRequest();
        }


        // ==============================================
        // MESSAGE
        // ==============================================

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
