using System;

namespace BWDMS.Dealer
{
    // ============================================================
    // DAILY CLOSING
    //
    // ~/Dealer/DailyClosing.aspx shows the closing form and the
    // closing summary. Only a logged-in Dealer can open it.
    // ============================================================

    public partial class DailyClosing : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
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
                    "Dealer",
                    StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect(
                    BWDMS.Data.AppAuth.HomeUrl(Session["UserRole"]),
                    false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }
        }


        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            // Saving logic goes here.
        }
    }
}
