namespace BWDMS.Data
{
    // ============================================================
    // ROLE-BASED HOME URL
    //
    // Used by the page-level security guards. A signed-in user who
    // opens a page belonging to ANOTHER role is sent to his own
    // dashboard instead of being dumped on the login screen (which
    // would look like a logout and lose his session).
    //
    // A missing / unknown role still goes to the login page.
    // ============================================================

    public static class AppAuth
    {
        public static string HomeUrl(object userRole)
        {
            string role =
                userRole == null
                    ? null
                    : userRole.ToString();


            switch (role)
            {
                case "Admin":
                    return "~/Admin/Dashboard.aspx";

                case "Dealer":
                    return "~/Dealer/Dashboard.aspx";

                case "Salesman":
                    return "~/Salesman/Dashboard.aspx";

                default:
                    return "~/Account/Login.aspx";
            }
        }
    }
}
