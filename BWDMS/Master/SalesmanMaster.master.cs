using System;

namespace BWDMS.Master
{
    public partial class SalesmanMaster : System.Web.UI.MasterPage
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
            // ONLY SALESMEN MAY USE THIS SHELL
            // ==========================================

            string role = Session["UserRole"] == null
                ? string.Empty
                : Session["UserRole"].ToString();

            if (!role.Equals(
                "Salesman",
                StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect(
                    BWDMS.Data.AppAuth.HomeUrl(
                        Session["UserRole"]),
                    false);

                Context.ApplicationInstance
                       .CompleteRequest();

                return;
            }


            if (!IsPostBack)
            {
                LoadUserInformation();

                HighlightActiveMenu();
            }
        }


        // ==============================================
        // LOAD USER INFORMATION
        // ==============================================

        private void LoadUserInformation()
        {
            string fullName = "Salesman";

            string role = "Salesman";


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


        // ==============================================
        // HIGHLIGHT ACTIVE MENU ITEM
        // ==============================================

        private void HighlightActiveMenu()
        {
            string path =
                (Request.AppRelativeCurrentExecutionFilePath ?? "")
                .ToLowerInvariant();


            ApplyActive(lnkDashboard, path, "/salesman/dashboard.aspx");

            ApplyActive(lnkRoutes, path, "/salesman/routes.aspx");

            ApplyActive(lnkShops, path, "/salesman/shops.aspx");

            ApplyActive(lnkCreateOrder, path, "/salesman/createorder.aspx");

            ApplyActive(lnkOrders, path, "/salesman/orders.aspx");

            ApplyActive(lnkStock, path, "/salesman/vehiclestock.aspx");

            ApplyActive(lnkClosing, path, "/salesman/collection.aspx");

            ApplyActive(lnkReports, path, "/salesman/reports.aspx");
        }


        private void ApplyActive(
            System.Web.UI.WebControls.HyperLink link,
            string path,
            string match)
        {
            if (path.EndsWith(match))
            {
                link.CssClass = "sw-menu-item active";
            }
        }


        // ==============================================
        // LOGOUT
        // ==============================================

        protected void btnLogout_Click(
            object sender,
            EventArgs e)
        {
            Session.Clear();

            Session.RemoveAll();

            Session.Abandon();


            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            Response.Cache.SetExpires(
                DateTime.UtcNow.AddYears(-1));

            Response.Cache.SetRevalidation(
                System.Web.HttpCacheRevalidation.AllCaches);


            Response.Redirect(
                "~/Account/Login.aspx",
                false);

            Context.ApplicationInstance
                   .CompleteRequest();
        }
    }
}
