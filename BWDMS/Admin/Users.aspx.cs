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
            }
        }


        private void LoadUsers()
        {
            string search = txtSearch.Text.Trim();
            string role = ddlRole.SelectedValue;

            // PasswordHash is deliberately never selected.
            string query = @"
                SELECT
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
    }
}
