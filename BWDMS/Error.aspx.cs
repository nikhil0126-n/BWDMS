using System;
using System.Web;
using System.Web.UI;
using BWDMS.Data;

namespace BWDMS
{
    // ============================================================
    // FRIENDLY ERROR PAGE
    //
    // Rendered by <customErrors> (Web.config) for 404 / 500 and for
    // any unhandled exception after Global.Application_Error logged
    // it to App_Data\AppLog.txt.
    //
    // INFORMATION-SAFE BY DESIGN:
    //   - never renders the exception, message, stack trace,
    //     connection strings or record data - in any customErrors mode
    //   - the only dynamic value is a correlation reference built
    //     here (timestamp + short GUID) and HTML-encoded on output
    //   - everything the user sees is static text
    // ============================================================

    public partial class Error : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Error pages must never be cached.
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            if (!IsPostBack)
            {
                // Correlation reference: matches the UTC timestamp the
                // application log uses, plus a short unique suffix.
                string reference =
                    DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ") +
                    "-" +
                    Guid.NewGuid().ToString("N")
                         .Substring(0, 8)
                         .ToUpperInvariant();

                lblReference.Text = Server.HtmlEncode(reference);
            }

            // Send the user to his own home page when the session is
            // still alive; otherwise AppAuth.HomeUrl falls back to the
            // login page. Session access is guarded because a rewrite
            // based error page may not have session state available.
            string homeUrl = "~/Account/Login.aspx";
            string homeText = "Back to login";

            try
            {
                object role = Session["UserRole"];

                homeUrl = AppAuth.HomeUrl(role);

                if (role != null)
                {
                    homeText = "Go to my dashboard";
                }
            }
            catch
            {
                homeUrl = "~/Account/Login.aspx";
                homeText = "Back to login";
            }

            lnkHome.NavigateUrl = ResolveUrl(homeUrl);
            lnkHome.Text = homeText;
        }
    }
}
