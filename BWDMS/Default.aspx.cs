using System;
using System.Web;
using System.Web.UI;
using BWDMS.Data;

namespace BWDMS
{
    // ============================================================
    // SITE ENTRY POINT
    //
    // IIS offers a default document for the site root, so a request
    // for https://localhost:44314/ lands here. Without this page the
    // root answered 403.14 - "directory browsing is not enabled" -
    // because no default document existed and none of the IIS
    // defaults (Default.htm, index.html...) are part of this app.
    //
    // It is a plain redirect: a signed-in visitor goes to his own
    // dashboard, everyone else to the login page. Both destinations
    // run their own role guard, so this page does not have to.
    // ============================================================

    public partial class Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // The destination depends on the session, which can change
            // between requests, so the redirect must never be cached.
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            string homeUrl = "~/Account/Login.aspx";

            try
            {
                // AppAuth returns the dashboard for a known role and
                // falls back to the login page otherwise.
                homeUrl = AppAuth.HomeUrl(Session["UserRole"]);
            }
            catch
            {
                // Session state is not always available while a
                // request is being re-routed; the login page is the
                // safe destination.
                homeUrl = "~/Account/Login.aspx";
            }

            Response.Redirect(homeUrl, false);

            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
