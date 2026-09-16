using System;

namespace BWDMS.Master
{
    public partial class DashboardMaster : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Prevent browser from caching dashboard pages
            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            Response.Cache.SetExpires(DateTime.UtcNow.AddYears(-1));

            Response.Cache.SetRevalidation(
                System.Web.HttpCacheRevalidation.AllCaches);


            // Check whether user is logged in
            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Account/Login.aspx", false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }


            // Load user information
            if (!IsPostBack)
            {
                LoadUserInformation();
            }
        }


        private void LoadUserInformation()
        {
            string fullName = "User";
            string role = "User";


            if (Session["FullName"] != null)
            {
                fullName =
                    Session["FullName"].ToString();
            }


            if (Session["UserRole"] != null)
            {
                role =
                    Session["UserRole"].ToString();
            }


            lblUserName.Text = fullName;

            lblUserRole.Text = role;


            if (!string.IsNullOrWhiteSpace(fullName))
            {
                lblUserInitial.Text =
                    fullName.Substring(0, 1).ToUpper();
            }
        }


        protected void btnLogout_Click(object sender, EventArgs e)
        {
            // Remove all session data
            Session.Clear();

            Session.Abandon();


            // Prevent browser from caching previous page
            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            Response.Cache.SetExpires(
                DateTime.UtcNow.AddYears(-1));

            Response.Cache.SetRevalidation(
                System.Web.HttpCacheRevalidation.AllCaches);


            // Go back to login
            Response.Redirect(
                "~/Account/Login.aspx",
                false);

            Context.ApplicationInstance.CompleteRequest();
        }
    }
}