using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace BWDMS
{
    // ============================================================
    // GLOBAL APPLICATION CLASS
    //
    // Keeps one single responsibility: log unhandled exceptions to
    // App_Data\AppLog.txt so an administrator can trace an incident
    // after the user has already been shown the friendly page.
    //
    // The exception is NEVER swallowed here - we only observe it and
    // then let ASP.NET continue so <customErrors> can render
    // Error.aspx (which never shows exception details).
    //
    // No BeginRequest / EndRequest / Session_End handlers are added
    // on purpose - session lifetime comes from <sessionState> in
    // Web.config and request-level caching is handled per page.
    // ============================================================

    public class Global : HttpApplication
    {
        // Matches "password=...;"/"pwd=..." style fragments so a
        // connection-string-shaped exception message can never leak
        // a secret into the application log.
        private static readonly Regex SecretPattern = new Regex(
            @"(password|pwd|user\s+id)\s*=\s*[^;""'\s]+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        protected void Application_Error(object sender, EventArgs e)
        {
            // Logging must NEVER throw - swallow only logging failures.
            try
            {
                Exception error = Server.GetLastError();

                if (error == null)
                {
                    return;
                }

                // Unwrap to the innermost exception: that is the real cause.
                Exception inner = error;

                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                }

                HttpContext context = HttpContext.Current;

                string url = "(unknown)";
                string user = "anonymous";

                try
                {
                    if (context != null && context.Request != null)
                    {
                        string raw = context.Request.RawUrl;

                        if (!string.IsNullOrEmpty(raw))
                        {
                            url = raw;
                        }
                    }
                }
                catch
                {
                    // Request may already be torn down; keep the default.
                }

                try
                {
                    if (context != null && context.Session != null)
                    {
                        object userId = context.Session["UserId"];

                        if (userId != null)
                        {
                            user = userId.ToString();
                        }
                    }
                }
                catch
                {
                    // Session may be unavailable during the error; keep "anonymous".
                }

                string timestamp =
                    DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

                string entry =
                    "=== " + timestamp + Environment.NewLine +
                    "URL: " + Sanitize(url) + Environment.NewLine +
                    "User: " + Sanitize(user) + Environment.NewLine +
                    "Type: " + inner.GetType().FullName + Environment.NewLine +
                    "Message: " + Sanitize(inner.Message) + Environment.NewLine +
                    "Stack: " + Sanitize(inner.ToString()) + Environment.NewLine +
                    "===" + Environment.NewLine + Environment.NewLine;

                string logPath =
                    context.Server.MapPath("~/App_Data/AppLog.txt");

                string directory = Path.GetDirectoryName(logPath);

                if (!string.IsNullOrEmpty(directory) &&
                    !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.AppendAllText(logPath, entry);
            }
            catch
            {
                // Never let the logger break the request pipeline.
            }

            // Deliberately NO Server.ClearError() here: ASP.NET must keep
            // handling the exception so <customErrors> shows Error.aspx.
        }

        // --------------------------------------------------------
        // Strips anything that looks like a credential value before
        // a line is written to the log.
        // --------------------------------------------------------
        private static string Sanitize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return SecretPattern.Replace(value, "$1=***");
        }
    }
}
