using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web;

namespace BWDMS.Data
{
    /// <summary>
    /// Streams a DataTable to the browser as an RFC 4180 CSV file.
    ///
    /// The page that calls Write() must suppress its own HTML render
    /// (the Reports pages do it by overriding Render) so the download
    /// body contains the CSV and nothing else.
    /// </summary>
    public static class CsvExport
    {
        // A field is quoted when it contains one of these characters.
        private static readonly char[] QuotingChars =
            { '"', ',', '\r', '\n' };


        // UTF-8 without a built-in preamble; the BOM is written
        // explicitly as the first character of the body.
        private static readonly Encoding Utf8 =
            new UTF8Encoding(false);


        /// <summary>
        /// Writes the table to the response as a CSV attachment and
        /// then lets the rest of the request pipeline finish normally.
        /// </summary>
        public static void Write(
            HttpResponse response,
            string fileName,
            DataTable table)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            if (table == null)
            {
                throw new ArgumentNullException("table");
            }

            // Drop any body content produced so far but keep the
            // no-cache headers the page set in Page_Load.
            response.ClearContent();

            response.BufferOutput = false;

            response.ContentType = "text/csv";

            response.ContentEncoding = Utf8;

            // Classic AddHeader - fully supported on .NET 4.7.2.
            response.AddHeader(
                "Content-Disposition",
                "attachment; filename=\"" +
                SanitizeFileName(fileName) + "\"");

            // The CSV body has to reach the browser untouched.
            response.SuppressContent = false;

            // U+FEFF = UTF-8 BOM so Excel reads Indian and other
            // unicode text correctly.
            string body =
                "\uFEFF" + BuildBody(table);

            response.BinaryWrite(Utf8.GetBytes(body));

            response.Flush();

            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }


        // ------------------------------------------------------------
        // CSV BODY
        // ------------------------------------------------------------

        private static string BuildBody(DataTable table)
        {
            StringBuilder sb = new StringBuilder();

            bool first = true;

            foreach (DataColumn column in table.Columns)
            {
                if (!first)
                {
                    sb.Append(',');
                }

                first = false;

                AppendField(sb, column.ColumnName);
            }

            sb.Append("\r\n");

            foreach (DataRow row in table.Rows)
            {
                first = true;

                foreach (DataColumn column in table.Columns)
                {
                    if (!first)
                    {
                        sb.Append(',');
                    }

                    first = false;

                    AppendField(sb, FieldText(row[column]));
                }

                sb.Append("\r\n");
            }

            return sb.ToString();
        }


        // RFC 4180: quote the field when it holds a comma, a quote,
        // CR or LF, and double every embedded quote.
        private static void AppendField(StringBuilder sb, string text)
        {
            if (text.IndexOfAny(QuotingChars) >= 0)
            {
                sb.Append('"');
                sb.Append(text.Replace("\"", "\"\""));
                sb.Append('"');
            }
            else
            {
                sb.Append(text);
            }
        }


        // DBNull and null become an empty field; everything else is
        // written with invariant culture so the file never depends on
        // the server locale.
        private static string FieldText(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            if (value is DateTime)
            {
                DateTime date = (DateTime)value;

                if (date.TimeOfDay == TimeSpan.Zero)
                {
                    return date.ToString(
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture);
                }

                return date.ToString(
                    "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture);
            }

            if (value is decimal)
            {
                return ((decimal)value).ToString(
                    CultureInfo.InvariantCulture);
            }

            if (value is double)
            {
                return ((double)value).ToString(
                    CultureInfo.InvariantCulture);
            }

            if (value is float)
            {
                return ((float)value).ToString(
                    CultureInfo.InvariantCulture);
            }

            if (value is bool)
            {
                return (bool)value ? "True" : "False";
            }

            IFormattable formattable = value as IFormattable;

            if (formattable != null)
            {
                return formattable.ToString(
                    null,
                    CultureInfo.InvariantCulture);
            }

            return value.ToString();
        }


        // ------------------------------------------------------------
        // FILE NAME
        // ------------------------------------------------------------

        // Strips quotes, CR/LF, path characters, spaces and anything
        // else that could break (or be misread in) the header, and
        // guarantees a .csv extension.
        private static string SanitizeFileName(string fileName)
        {
            string raw = (fileName ?? string.Empty).Trim();

            StringBuilder sb = new StringBuilder(raw.Length + 4);

            foreach (char c in raw)
            {
                if (char.IsLetterOrDigit(c) ||
                    c == '.' ||
                    c == '-' ||
                    c == '_')
                {
                    sb.Append(c);
                }
            }

            string name = sb.ToString().TrimStart('.');

            if (name.Length == 0)
            {
                name = "report";
            }

            if (!name.EndsWith(
                    ".csv",
                    StringComparison.OrdinalIgnoreCase))
            {
                name += ".csv";
            }

            return name;
        }
    }
}
