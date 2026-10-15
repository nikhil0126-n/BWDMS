using System.Text;
using System.Web.UI.WebControls;

namespace BWDMS.Data
{
    /// <summary>
    /// Renders the pagination bar that sits under a list grid.
    ///
    /// The GridView's own pager row is not used on these pages: it is
    /// only emitted when a grid holds more than one page of rows, so a
    /// short list showed no pagination at all. This bar is built by
    /// hand instead and is always drawn, which matches the design and
    /// still behaves once the list grows.
    ///
    /// Each page is a link that posts back to the grid itself with the
    /// same "Page$N" argument its own pager would use, so the grid's
    /// existing PageIndexChanging handler does the work. Posting to the
    /// grid rather than to a hand-added button keeps ASP.NET's event
    /// validation happy - the grid registers those arguments, a button
    /// would not.
    /// </summary>
    public static class GridPager
    {
        /// <summary>
        /// Builds the markup for a grid, or an empty string when the
        /// grid has no pages.
        /// </summary>
        public static string Html(GridView grid)
        {
            if (grid == null)
            {
                return string.Empty;
            }

            int pageCount = grid.PageCount;

            if (pageCount < 1)
            {
                pageCount = 1;
            }

            int current = grid.PageIndex;

            if (current < 0)
            {
                current = 0;
            }

            if (current > pageCount - 1)
            {
                current = pageCount - 1;
            }

            StringBuilder html = new StringBuilder();

            html.Append("<div class=\"grid-pager\">");

            AppendStep(
                html,
                grid,
                current - 1,
                current > 0,
                "\u2039",
                "Previous page",
                "pager-step");

            for (int i = 0; i < pageCount; i++)
            {
                if (i == current)
                {
                    html.Append("<span class=\"pager-page is-active\" aria-current=\"page\">")
                        .Append(i + 1)
                        .Append("</span>");
                }
                else
                {
                    AppendStep(
                        html,
                        grid,
                        i,
                        true,
                        (i + 1).ToString(),
                        "Page " + (i + 1),
                        "pager-page");
                }
            }

            AppendStep(
                html,
                grid,
                current + 1,
                current < pageCount - 1,
                "\u203a",
                "Next page",
                "pager-step");

            html.Append("</div>");

            return html.ToString();
        }


        /// <summary>
        /// Tells ASP.NET's event validation that the grid may be asked to
        /// jump to each of its pages.
        ///
        /// The grid's own pager registers those "Page$N" arguments as it
        /// renders its links. Because these pages hide that pager nothing
        /// would register them and the postback would be rejected as an
        /// invalid callback argument, so the page does it instead, calling
        /// this from its Render override.
        /// </summary>
        public static void RegisterForEventValidation(GridView grid)
        {
            if (grid == null || grid.Page == null)
            {
                return;
            }

            int pageCount = grid.PageCount;

            if (pageCount < 1)
            {
                pageCount = 1;
            }

            for (int i = 1; i <= pageCount; i++)
            {
                grid.Page.ClientScript.RegisterForEventValidation(
                    grid.UniqueID,
                    "Page$" + i);
            }
        }


        private static void AppendStep(
            StringBuilder html,
            GridView grid,
            int pageIndex,
            bool enabled,
            string text,
            string title,
            string cssClass)
        {
            if (!enabled)
            {
                html.Append("<span class=\"")
                    .Append(cssClass)
                    .Append(" is-disabled\">")
                    .Append(text)
                    .Append("</span>");

                return;
            }

            html.Append("<a class=\"")
                .Append(cssClass)
                .Append("\" href=\"javascript:__doPostBack('")
                .Append(grid.UniqueID)
                .Append("','Page$")
                .Append(pageIndex + 1)
                .Append("')\" title=\"")
                .Append(title)
                .Append("\">")
                .Append(text)
                .Append("</a>");
        }
    }
}
