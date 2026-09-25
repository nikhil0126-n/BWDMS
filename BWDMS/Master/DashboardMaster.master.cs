using System;

namespace BWDMS.Master
{
    public partial class DashboardMaster : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // ==========================================
            // PREVENT BROWSER CACHE
            // ==========================================

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

                Context.ApplicationInstance
                       .CompleteRequest();

                return;
            }


            // ==========================================
            // LOAD USER INFORMATION
            // ==========================================

            if (!IsPostBack)
            {
                LoadUserInformation();
            }
        }


        // ==============================================
        // LOAD USER INFORMATION
        // ==============================================

        private void LoadUserInformation()
        {
            string fullName = "User";

            string role = "User";


            // ------------------------------------------
            // GET FULL NAME
            // ------------------------------------------

            if (Session["FullName"] != null)
            {
                fullName =
                    Session["FullName"].ToString();
            }


            // ------------------------------------------
            // GET ROLE
            // ------------------------------------------

            if (Session["UserRole"] != null)
            {
                role =
                    Session["UserRole"].ToString();
            }


            // ------------------------------------------
            // DISPLAY USER INFORMATION
            // ------------------------------------------

            lblUserName.Text =
                fullName;

            lblUserRole.Text =
                role;


            // ------------------------------------------
            // USER INITIAL
            // ------------------------------------------

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                lblUserInitial.Text =
                    fullName.Substring(0, 1).ToUpper();
            }


            // ==========================================
            // HIDE ALL ROLE MENUS FIRST
            // ==========================================

            pnlAdminMenu.Visible = false;

            pnlDealerMenu.Visible = false;

            pnlSalesmanMenu.Visible = false;


            // ==========================================
            // SHOW MENU ACCORDING TO ROLE
            // ==========================================

            if (role.Equals(
                "Admin",
                StringComparison.OrdinalIgnoreCase))
            {
                pnlAdminMenu.Visible = true;

                lnkDashboard.NavigateUrl =
                    "~/Admin/Dashboard.aspx";
            }


            else if (role.Equals(
                "Dealer",
                StringComparison.OrdinalIgnoreCase))
            {
                pnlDealerMenu.Visible = true;

                lnkDashboard.NavigateUrl =
                    "~/Dealer/Dashboard.aspx";
            }


            else if (role.Equals(
                "Salesman",
                StringComparison.OrdinalIgnoreCase))
            {
                pnlSalesmanMenu.Visible = true;

                lnkDashboard.NavigateUrl =
                    "~/Salesman/Dashboard.aspx";
            }


            else
            {
                // Unknown role

                Response.Redirect(
                    "~/Account/Login.aspx",
                    false);

                Context.ApplicationInstance
                       .CompleteRequest();

                return;
            }
        }


        // ==============================================
        // LOGOUT
        // ==============================================

        protected void btnLogout_Click(
            object sender,
            EventArgs e)
        {
            // ------------------------------------------
            // CLEAR SESSION
            // ------------------------------------------

            Session.Clear();

            Session.RemoveAll();

            Session.Abandon();


            // ------------------------------------------
            // PREVENT CACHE
            // ------------------------------------------

            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            Response.Cache.SetExpires(
                DateTime.UtcNow.AddYears(-1));

            Response.Cache.SetRevalidation(
                System.Web.HttpCacheRevalidation.AllCaches);


            // ------------------------------------------
            // REDIRECT TO LOGIN
            // ------------------------------------------

            Response.Redirect(
                "~/Account/Login.aspx",
                false);

            Context.ApplicationInstance
                   .CompleteRequest();
        }
    }
}