using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace BWDMS.Data
{
    // ============================================================
    // ONE ROW OF THE WAREHOUSE SHEET
    //
    // The eight fields below are read straight off the sheet and
    // are exactly the columns the import shows:
    //
    //   Sr. | Material | Box | Patti | Packet | Total Packet |
    //   Purchase Price | Sales Price
    //
    // The sheet also carries Purchase Value and Sales Value. Both
    // are derived figures (price x packets), so they are located
    // only to be skipped over and are never stored.
    //
    // The remaining properties are worked out from the row so the
    // import knows what to write to Products, ProductVariants,
    // ProductUnitOptions, ProductPrices and Inventory.
    // ============================================================

    [Serializable]
    public class InventoryPdfRow
    {
        // ---- read from the sheet ----
        public int Sr { get; set; }
        public string Material { get; set; }
        public int BoxQty { get; set; }
        public int PattiQty { get; set; }
        public int PacketQty { get; set; }
        public int TotalPacket { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalesPrice { get; set; }

        // ---- ticked in the preview grid ----
        public bool Include { get; set; }

        // ---- derived from the row ----
        public string ProductName { get; set; }
        public bool IsActive { get; set; }
        public decimal PacketWeight { get; set; }
        public string WeightUnit { get; set; }
        public string VariantCode { get; set; }
        public int PacketsPerBox { get; set; }
        public int PacketsPerPatti { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Note { get; set; }


        public InventoryPdfRow()
        {
            Material = "";
            ProductName = "";
            WeightUnit = "";
            VariantCode = "";
            CategoryName = "";
            Note = "";

            Include = true;
            IsActive = true;
        }
    }


    // ============================================================
    // PDF TEXT READER
    //
    // Written by hand because the project targets .NET Framework
    // 4.7.2 with no package references, so no PDF library is
    // available. The scope is deliberately narrow: FlateDecode
    // content streams, WinAnsi literal strings and the text
    // operators iText writes (BT / Tf / Tm / Td / Tj / TJ).
    //
    // The content stream is decoded one byte per character
    // (Latin-1) so byte positions survive intact; individual
    // strings are decoded as Windows-1252 at the very end, which
    // is how the sheet's non-breaking spaces inside materials
    // such as "15GM*192 CRUNCHEM SIMPLY SALTED WAFERS" come out.
    //
    // Limitation: a text matrix layered on a non-identity CTM
    // (cm) is not combined. Sheets exported by the same tool keep
    // the CTM at identity for the table, which is what this reads.
    // ============================================================

    public static class InventoryPdfParser
    {
        // ========================================================
        // COLUMN BOUNDS
        //
        // Measured from the sheet's own header, so a value belongs
        // to a column when the x it starts at falls in that span.
        // The two value columns are still classified, so they land
        // in a bucket nobody reads from.
        //
        //   Total Packet  cells start at 353.9 - 375.4
        //   Purchase Price cells start at 395.9 - 403.5
        //
        // hence the split at 385. Getting this wrong is what made
        // "250.00" read as a packet count, so the numbers below
        // are load bearing rather than cosmetic.
        // ========================================================

        private const double XSrMax = 32;
        private const double XMaterialMax = 200;
        private const double XBoxMax = 267;
        private const double XPattiMax = 303;
        private const double XPacketMax = 340;
        private const double XTotalMax = 385;
        private const double XPurchasePriceMax = 435;
        private const double XPurchaseValueMax = 480;
        private const double XSalesPriceMax = 530;

        // A wrapped material sits about 6.5 points above or below
        // its own row while the next row is 13.3 points away, so
        // anything within 12 points still belongs to the row.
        private const double RowTolerance = 12;

        private static readonly Encoding WinAnsi =
            Encoding.GetEncoding(1252);


        // ========================================================
        // ENTRY POINT
        // ========================================================

        public static List<InventoryPdfRow> Parse(byte[] pdf)
        {
            if (pdf == null || pdf.Length < 32)
            {
                throw new ArgumentException(
                    "The uploaded file could not be read.");
            }

            string magic = Latin1(pdf, 0, Math.Min(pdf.Length, 16));

            if (!magic.StartsWith("%PDF", StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "The uploaded file is not a PDF.");
            }

            List<string> pages = ReadPageContents(pdf);

            if (pages.Count == 0)
            {
                throw new InvalidOperationException(
                    "No readable content was found in the PDF.");
            }

            List<TextRun> runs = new List<TextRun>();
            int sequence = 0;

            for (int page = 0; page < pages.Count; page++)
            {
                List<TextRun> pageRuns =
                    ExtractRuns(pages[page]);

                foreach (TextRun run in pageRuns)
                {
                    run.Page = page;
                    run.Seq = sequence++;
                    runs.Add(run);
                }
            }

            List<InventoryPdfRow> rows = AssembleRows(runs);

            if (rows.Count == 0)
            {
                throw new InvalidOperationException(
                    "No inventory rows were found. The sheet " +
                    "needs an 'Sr.' column of row numbers.");
            }

            Derive(rows);

            return rows;
        }


        // ========================================================
        // CATEGORY GUESS
        //
        // Returned as a category name so the page can match it
        // against ProductCategories; an empty string means the
        // fallback dropdown decides.
        // ========================================================

        public static string GuessCategoryName(string material)
        {
            string m = (material ?? "").ToUpperInvariant();

            if (m.Contains("WAFER"))
            {
                return "Wafers";
            }

            if (m.Contains("GIPPI") ||
                m.Contains("WHEELOS") ||
                m.Contains("SCOOPITOS") ||
                m.Contains("YUMSTIX") ||
                m.Contains("NACHOS") ||
                m.Contains("HOOPERS") ||
                m.Contains("TORNADO") ||
                m.Contains("POP RINGS") ||
                m.Contains("FUNNE") ||
                m.Contains("MURUKKU"))
            {
                return "Fryums";
            }

            if (m.Contains("SEV"))
            {
                return "Sev";
            }

            if (m.Contains("POWDER") ||
                m.Contains("CHILLI") ||
                m.Contains("TURMERIC") ||
                m.Contains("CORIANDER") ||
                m.Contains("PEANUT") ||
                m.Contains("CHANADAL") ||
                m.Contains("MUNGDAL") ||
                m.Contains("CHEVDO") ||
                m.Contains("BHUJIA") ||
                m.Contains("SHING") ||
                m.Contains("GATHIYA") ||
                m.Contains("BHADANG") ||
                m.Contains("PAPAD") ||
                m.Contains("KHAKHRA") ||
                m.Contains("MURMURA"))
            {
                return "Namkeen";
            }

            if (m.Contains("JUICE") ||
                m.Contains("DRINK") ||
                m.Contains("BEVERAGE") ||
                m.Contains("COFFEE") ||
                m.Contains(" TEA"))
            {
                return "Beverages";
            }

            return "";
        }


        // ========================================================
        // PAGE CONTENT
        // ========================================================

        private class PdfObject
        {
            public string Dict;
            public byte[] Stream;
        }


        private static List<string> ReadPageContents(byte[] pdf)
        {
            string text = Latin1(pdf, 0, pdf.Length);
            Dictionary<int, PdfObject> objects =
                ScanObjects(pdf, text);

            List<string> ordered = new List<string>();

            // Walk the page tree so page 1 really is page 1
            // rather than whatever order the writer flushed the
            // streams in.
            foreach (Match page in Regex.Matches(
                text,
                @"/Type\s*/Page(?![a-zA-Z])"))
            {
                int header = text.LastIndexOf(
                    "0 obj",
                    page.Index,
                    StringComparison.Ordinal);

                if (header < 3)
                {
                    continue;
                }

                // The page dictionary runs from its "N 0 obj"
                // header to its "endobj". /Contents is written
                // ahead of /Type/Page in it, so the whole dict
                // has to be read rather than the text that
                // follows the match - which would otherwise walk
                // straight into the next page's dictionary.
                int dictStart = header + 5;

                int dictEnd = text.IndexOf(
                    "endobj",
                    page.Index,
                    StringComparison.Ordinal);

                if (dictEnd < dictStart)
                {
                    dictEnd = Math.Min(
                        text.Length,
                        page.Index + 1000);
                }

                Match contents = Regex.Match(
                    text.Substring(
                        dictStart,
                        dictEnd - dictStart),
                    @"/Contents\s+(\d+)\s+0\s+R");

                if (!contents.Success)
                {
                    continue;
                }

                int contentsNumber = int.Parse(
                    contents.Groups[1].Value,
                    CultureInfo.InvariantCulture);

                PdfObject target;

                if (objects.TryGetValue(
                        contentsNumber,
                        out target) &&
                    target.Stream != null)
                {
                    string body = DecodeStream(target.Stream);

                    if (body != null)
                    {
                        ordered.Add(body);
                    }
                }
            }

            // Fall back to file order when the page tree does not
            // resolve - some writers nest Contents in an array.
            if (ordered.Count == 0)
            {
                List<int> numbers = new List<int>(
                    objects.Keys);

                numbers.Sort();

                foreach (int number in numbers)
                {
                    PdfObject obj = objects[number];

                    if (obj.Stream == null)
                    {
                        continue;
                    }

                    string body = DecodeStream(obj.Stream);

                    // A font program inflates just fine but has
                    // no text operators in it, which is how it is
                    // told apart from a page.
                    if (body == null || !body.Contains("Tj"))
                    {
                        continue;
                    }

                    ordered.Add(body);
                }
            }

            return ordered;
        }


        private static Dictionary<int, PdfObject> ScanObjects(
            byte[] pdf,
            string text)
        {
            Dictionary<int, PdfObject> map =
                new Dictionary<int, PdfObject>();

            foreach (Match head in Regex.Matches(
                text,
                @"(\d+)\s+0\s+obj"))
            {
                int number = int.Parse(
                    head.Groups[1].Value,
                    CultureInfo.InvariantCulture);

                int bodyStart = head.Index + head.Length;

                // A dictionary ends at this object's own "endobj".
                // Bounding the probe there matters: without it a
                // plain object such as a page dictionary would run
                // on into the object behind it and adopt that
                // object's stream, which is how a single page
                // ended up contributing somebody else's rows.
                int probeEnd = text.IndexOf(
                    "endobj",
                    bodyStart,
                    StringComparison.Ordinal);

                if (probeEnd < bodyStart)
                {
                    probeEnd = Math.Min(
                        text.Length,
                        bodyStart + 4000);
                }

                if (probeEnd <= bodyStart)
                {
                    continue;
                }

                string probe = text.Substring(
                    bodyStart,
                    probeEnd - bodyStart);

                int streamAt = probe.IndexOf(
                    "stream",
                    StringComparison.Ordinal);

                PdfObject obj = new PdfObject();

                if (streamAt > 0)
                {
                    string dict = probe.Substring(0, streamAt);

                    obj.Dict = dict;

                    // Only declared, uncompressed-looking
                    // dictionaries are handled, and images are
                    // skipped because they carry no text.
                    Match length = Regex.Match(
                        dict,
                        @"/Length\s+(\d+)");

                    if (dict.Contains("FlateDecode") &&
                        length.Success &&
                        !dict.Contains("/Subtype/Image"))
                    {
                        int byteLength = int.Parse(
                            length.Groups[1].Value,
                            CultureInfo.InvariantCulture);

                        int dataStart =
                            bodyStart + streamAt + 6;

                        // The keyword is always followed by an
                        // end of line before the data starts.
                        if (dataStart >= text.Length ||
                            (text[dataStart] != '\r' &&
                             text[dataStart] != '\n' &&
                             !char.IsWhiteSpace(
                                 text[dataStart])))
                        {
                            dataStart = -1;
                        }
                        else
                        {
                            if (text[dataStart] == '\r')
                            {
                                dataStart++;
                            }

                            if (dataStart < text.Length &&
                                text[dataStart] == '\n')
                            {
                                dataStart++;
                            }
                        }

                        if (dataStart >= 0 &&
                            byteLength > 0 &&
                            dataStart + byteLength <=
                                pdf.Length)
                        {
                            byte[] raw =
                                new byte[byteLength];

                            Array.Copy(
                                pdf,
                                dataStart,
                                raw,
                                0,
                                byteLength);

                            obj.Stream = Inflate(raw);
                        }
                    }
                }

                if (!map.ContainsKey(number))
                {
                    map.Add(number, obj);
                }
            }

            return map;
        }


        private static byte[] Inflate(byte[] raw)
        {
            if (raw == null || raw.Length < 3)
            {
                return null;
            }

            // /FlateDecode is a zlib wrapper. Some writers leave
            // the two byte header off, so try both offsets.
            int[] skips = new int[] { 2, 0 };

            foreach (int skip in skips)
            {
                try
                {
                    using (MemoryStream source =
                        new MemoryStream(raw))
                    {
                        if (skip > 0)
                        {
                            source.Seek(
                                skip,
                                SeekOrigin.Begin);
                        }

                        using (DeflateStream deflate =
                            new DeflateStream(
                                source,
                                CompressionMode.Decompress))
                        {
                            using (MemoryStream output =
                                new MemoryStream())
                            {
                                deflate.CopyTo(output);

                                if (output.Length > 0)
                                {
                                    return output.ToArray();
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Wrong offset - try the other one.
                }
            }

            return null;
        }


        private static string DecodeStream(byte[] raw)
        {
            try
            {
                // Latin-1 keeps every byte addressable, which the
                // tokenizer below relies on.
                return Latin1(raw, 0, raw.Length);
            }
            catch
            {
                return null;
            }
        }


        private static string Latin1(
            byte[] data,
            int offset,
            int count)
        {
            char[] chars = new char[count];

            for (int i = 0; i < count; i++)
            {
                chars[i] = (char)data[offset + i];
            }

            return new string(chars);
        }


        // ========================================================
        // CONTENT STREAM -> TEXT RUNS
        // ========================================================

        private class TextRun
        {
            public int Page;
            public int Seq;
            public double X;
            public double Y;
            public byte[] Text;
        }


        private class PdfName
        {
            public readonly string Value;

            public PdfName(string value)
            {
                Value = value;
            }
        }


        private static List<TextRun> ExtractRuns(string content)
        {
            List<TextRun> runs = new List<TextRun>();

            List<object> operands = new List<object>();
            Stack<List<object>> arrays =
                new Stack<List<object>>();

            double x = 0;
            double y = 0;
            double fontSize = 10;
            double leading = 12;

            int i = 0;
            int n = content.Length;

            while (i < n)
            {
                char c = content[i];

                if (c == ' ' || c == '\t' || c == '\r' ||
                    c == '\n' || c == '\f' || c == '\0')
                {
                    i++;
                    continue;
                }

                if (c == '%')
                {
                    while (i < n && content[i] != '\n' &&
                           content[i] != '\r')
                    {
                        i++;
                    }

                    continue;
                }

                if (c == '(')
                {
                    byte[] literal;
                    i = ReadLiteral(content, i, out literal);
                    operands.Add(literal);
                    continue;
                }

                if (c == '<')
                {
                    if (i + 1 < n && content[i + 1] == '<')
                    {
                        // Dictionary marker - skip past it.
                        i += 2;

                        int close = content.IndexOf(
                            ">>",
                            i,
                            StringComparison.Ordinal);

                        i = close < 0 ? n : close + 2;
                        continue;
                    }

                    byte[] hex;
                    i = ReadHex(content, i, out hex);
                    operands.Add(hex);
                    continue;
                }

                if (c == '[')
                {
                    arrays.Push(operands);
                    operands = new List<object>();
                    i++;
                    continue;
                }

                if (c == ']')
                {
                    List<object> completed = operands;

                    operands = arrays.Count > 0
                        ? arrays.Pop()
                        : new List<object>();

                    operands.Add(completed);
                    i++;
                    continue;
                }

                if (c == '/')
                {
                    int start = ++i;

                    while (i < n && !IsWhitespace(content[i]) &&
                           !IsDelimiter(content[i]))
                    {
                        i++;
                    }

                    operands.Add(
                        new PdfName(
                            content.Substring(start, i - start)));

                    continue;
                }

                if (c == '-' || c == '+' || c == '.' ||
                    (c >= '0' && c <= '9'))
                {
                    int start = i;

                    if (c == '-' || c == '+')
                    {
                        i++;
                    }

                    while (i < n &&
                           (content[i] >= '0' &&
                            content[i] <= '9' ||
                            content[i] == '.' ||
                            content[i] == '-'))
                    {
                        i++;
                    }

                    double value;

                    if (double.TryParse(
                        content.Substring(start, i - start),
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out value))
                    {
                        operands.Add(value);
                    }

                    continue;
                }

                // Anything else is an operator.
                int operatorStart = i;

                while (i < n && !IsWhitespace(content[i]) &&
                       !IsDelimiter(content[i]))
                {
                    i++;
                }

                string op = content.Substring(
                    operatorStart,
                    i - operatorStart);

                if (op.Length > 0)
                {
                    Execute(
                        op,
                        operands,
                        runs,
                        ref x,
                        ref y,
                        ref fontSize,
                        ref leading);
                }

                operands.Clear();
            }

            return runs;
        }


        private static void Execute(
            string op,
            List<object> operands,
            List<TextRun> runs,
            ref double x,
            ref double y,
            ref double fontSize,
            ref double leading)
        {
            switch (op)
            {
                case "BT":
                    x = 0;
                    y = 0;
                    break;

                case "Tf":
                    fontSize = NumberAt(operands, -1, 10);
                    break;

                case "TL":
                    leading = NumberAt(operands, -1, 12);
                    break;

                case "Tm":
                    if (operands.Count >= 6)
                    {
                        x = NumberAt(operands, 4, x);
                        y = NumberAt(operands, 5, y);
                    }
                    break;

                case "Td":
                case "TD":
                    if (operands.Count >= 2)
                    {
                        x += NumberAt(operands, 0, 0);
                        y += NumberAt(operands, 1, 0);
                    }
                    break;

                case "T*":
                    y -= leading;
                    break;

                case "Tj":
                case "'":
                case "\"":
                    if (op == "'" || op == "\"")
                    {
                        y -= leading;
                    }

                    foreach (object operand in operands)
                    {
                        if (operand is byte[])
                        {
                            Emit(
                                runs,
                                (byte[])operand,
                                ref x,
                                y,
                                fontSize);
                        }
                    }
                    break;

                case "TJ":
                    foreach (object operand in operands)
                    {
                        if (operand is byte[])
                        {
                            Emit(
                                runs,
                                (byte[])operand,
                                ref x,
                                y,
                                fontSize);
                        }
                        else if (operand is List<object>)
                        {
                            foreach (object inner
                                in (List<object>)operand)
                            {
                                if (inner is byte[])
                                {
                                    Emit(
                                        runs,
                                        (byte[])inner,
                                        ref x,
                                        y,
                                        fontSize);
                                }
                                else if (inner is double)
                                {
                                    // Kerning, in thousandths of
                                    // an em.
                                    x -= ((double)inner) /
                                         1000.0 * fontSize;
                                }
                            }
                        }
                    }
                    break;
            }
        }


        private static void Emit(
            List<TextRun> runs,
            byte[] raw,
            ref double x,
            double y,
            double fontSize)
        {
            string text = WinAnsi.GetString(raw);

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            TextRun run = new TextRun();
            run.X = x;
            run.Y = y;
            run.Text = raw;

            runs.Add(run);

            // Approximate advance so several strings drawn in one
            // text block do not all report the same x.
            x += text.Length * fontSize * 0.5;
        }


        private static double NumberAt(
            List<object> operands,
            int index,
            double fallback)
        {
            int actual = index;

            if (actual < 0)
            {
                actual = operands.Count + index;
            }

            if (actual < 0 || actual >= operands.Count)
            {
                return fallback;
            }

            return operands[actual] is double
                ? (double)operands[actual]
                : fallback;
        }


        private static bool IsWhitespace(char c)
        {
            return c == ' ' || c == '\t' || c == '\r' ||
                   c == '\n' || c == '\f' || c == '\0';
        }


        private static bool IsDelimiter(char c)
        {
            return c == '(' || c == ')' || c == '<' ||
                   c == '>' || c == '[' || c == ']' ||
                   c == '{' || c == '}' || c == '/' ||
                   c == '%';
        }


        private static int ReadLiteral(
            string content,
            int start,
            out byte[] bytes)
        {
            int i = start + 1;
            int depth = 1;
            List<byte> buffer = new List<byte>();

            while (i < content.Length)
            {
                char c = content[i];

                if (c == '\\')
                {
                    i++;

                    if (i >= content.Length)
                    {
                        break;
                    }

                    char escaped = content[i];

                    switch (escaped)
                    {
                        case 'n':
                            buffer.Add(10);
                            i++;
                            break;
                        case 'r':
                            buffer.Add(13);
                            i++;
                            break;
                        case 't':
                            buffer.Add(9);
                            i++;
                            break;
                        case 'b':
                            buffer.Add(8);
                            i++;
                            break;
                        case 'f':
                            buffer.Add(12);
                            i++;
                            break;
                        case '(':
                            buffer.Add(40);
                            i++;
                            break;
                        case ')':
                            buffer.Add(41);
                            i++;
                            break;
                        case '\\':
                            buffer.Add(92);
                            i++;
                            break;
                        case '\r':
                            i++;

                            if (i < content.Length &&
                                content[i] == '\n')
                            {
                                i++;
                            }
                            break;
                        case '\n':
                            i++;
                            break;
                        default:
                            if (escaped >= '0' &&
                                escaped <= '7')
                            {
                                int value = 0;
                                int taken = 0;

                                while (taken < 3 &&
                                       i < content.Length &&
                                       content[i] >= '0' &&
                                       content[i] <= '7')
                                {
                                    value = value * 8 +
                                            (content[i] - '0');
                                    i++;
                                    taken++;
                                }

                                buffer.Add((byte)value);
                            }
                            else
                            {
                                buffer.Add((byte)escaped);
                                i++;
                            }
                            break;
                    }

                    continue;
                }

                if (c == '(')
                {
                    depth++;
                    buffer.Add(40);
                    i++;
                    continue;
                }

                if (c == ')')
                {
                    depth--;

                    if (depth == 0)
                    {
                        i++;
                        break;
                    }

                    buffer.Add(41);
                    i++;
                    continue;
                }

                // The content stream is Latin-1, so the character
                // is the byte.
                buffer.Add((byte)c);
                i++;
            }

            bytes = buffer.ToArray();
            return i;
        }


        private static int ReadHex(
            string content,
            int start,
            out byte[] bytes)
        {
            int i = start + 1;
            List<byte> buffer = new List<byte>();
            int pending = -1;

            while (i < content.Length && content[i] != '>')
            {
                char c = content[i];
                int digit;

                if (c >= '0' && c <= '9')
                {
                    digit = c - '0';
                }
                else if (c >= 'a' && c <= 'f')
                {
                    digit = c - 'a' + 10;
                }
                else if (c >= 'A' && c <= 'F')
                {
                    digit = c - 'A' + 10;
                }
                else
                {
                    i++;
                    continue;
                }

                if (pending < 0)
                {
                    pending = digit;
                }
                else
                {
                    buffer.Add((byte)((pending << 4) | digit));
                    pending = -1;
                }

                i++;
            }

            if (pending >= 0)
            {
                buffer.Add((byte)(pending << 4));
            }

            if (i < content.Length && content[i] == '>')
            {
                i++;
            }

            bytes = buffer.ToArray();
            return i;
        }


        // ========================================================
        // TEXT RUNS -> ROWS
        // ========================================================

        private enum Column
        {
            Sr,
            Material,
            Box,
            Patti,
            Packet,
            Total,
            PurchasePrice,
            PurchaseValue,
            SalesPrice,
            SalesValue
        }


        private static Column ColumnOf(double x)
        {
            if (x < XSrMax)
            {
                return Column.Sr;
            }

            if (x < XMaterialMax)
            {
                return Column.Material;
            }

            if (x < XBoxMax)
            {
                return Column.Box;
            }

            if (x < XPattiMax)
            {
                return Column.Patti;
            }

            if (x < XPacketMax)
            {
                return Column.Packet;
            }

            if (x < XTotalMax)
            {
                return Column.Total;
            }

            if (x < XPurchasePriceMax)
            {
                return Column.PurchasePrice;
            }

            if (x < XPurchaseValueMax)
            {
                return Column.PurchaseValue;
            }

            if (x < XSalesPriceMax)
            {
                return Column.SalesPrice;
            }

            return Column.SalesValue;
        }


        private static readonly string[] HeaderLabels =
            new string[]
            {
                "Sr.", "Material", "Box Qty", "Patti Qty",
                "Packet", "Qty", "Total Packet", "Price",
                "Value", "Purchase", "Purchase Stock",
                "Sales", "Sales Stock"
            };


        private static List<InventoryPdfRow> AssembleRows(
            List<TextRun> runs)
        {
            List<InventoryPdfRow> rows =
                new List<InventoryPdfRow>();

            int lastPage = -1;

            foreach (TextRun run in runs)
            {
                if (run.Page > lastPage)
                {
                    lastPage = run.Page;
                }
            }

            for (int page = 0; page <= lastPage; page++)
            {
                List<TextRun> pageRuns =
                    new List<TextRun>();

                foreach (TextRun run in runs)
                {
                    if (run.Page == page)
                    {
                        pageRuns.Add(run);
                    }
                }

                if (pageRuns.Count == 0)
                {
                    continue;
                }

                // Top of the page first, and left to right when
                // two runs share a line, so a wrapped material is
                // rebuilt in reading order.
                pageRuns.Sort(CompareByPosition);

                // The row number in the Sr. column is the only
                // thing that reliably starts a row, so it anchors
                // everything else on the same line.
                List<TextRun> anchors = new List<TextRun>();

                foreach (TextRun run in pageRuns)
                {
                    if (ColumnOf(run.X) == Column.Sr &&
                        IsRowNumber(Clean(run.Text)))
                    {
                        anchors.Add(run);
                    }
                }

                if (anchors.Count == 0)
                {
                    continue;
                }

                double highest = double.MinValue;
                double lowest = double.MaxValue;

                foreach (TextRun anchor in anchors)
                {
                    if (anchor.Y > highest)
                    {
                        highest = anchor.Y;
                    }

                    if (anchor.Y < lowest)
                    {
                        lowest = anchor.Y;
                    }
                }

                // Everything above the first row is the sheet
                // header and everything below the last row is the
                // page footer, so a 12 point margin on either side
                // keeps both out of the table.
                double topLimit = highest + RowTolerance;
                double bottomLimit = lowest - RowTolerance;

                foreach (TextRun anchor in anchors)
                {
                    InventoryPdfRow row = new InventoryPdfRow();

                    int sr;

                    if (!TryParseCount(
                            Clean(anchor.Text),
                            out sr))
                    {
                        sr = 0;
                    }

                    row.Sr = sr;

                    foreach (TextRun run in pageRuns)
                    {
                        if (run.Y > topLimit ||
                            run.Y < bottomLimit)
                        {
                            continue;
                        }

                        if (Math.Abs(run.Y - anchor.Y) >=
                            RowTolerance)
                        {
                            continue;
                        }

                        Column column = ColumnOf(run.X);

                        if (column == Column.Sr)
                        {
                            continue;
                        }

                        string value = Clean(run.Text);

                        if (value.Length == 0)
                        {
                            continue;
                        }

                        if (column == Column.Material)
                        {
                            if (Array.IndexOf(
                                    HeaderLabels,
                                    value) >= 0)
                            {
                                continue;
                            }

                            row.Material =
                                (row.Material + " " + value)
                                .Trim();

                            continue;
                        }

                        if (column == Column.PurchasePrice)
                        {
                            decimal price;

                            if (TryParsePrice(
                                    value,
                                    out price))
                            {
                                row.PurchasePrice = price;
                            }

                            continue;
                        }

                        if (column == Column.SalesPrice)
                        {
                            decimal price;

                            if (TryParsePrice(
                                    value,
                                    out price))
                            {
                                row.SalesPrice = price;
                            }

                            continue;
                        }

                        if (column != Column.Box &&
                            column != Column.Patti &&
                            column != Column.Packet &&
                            column != Column.Total)
                        {
                            // The two value columns are read
                            // only to be dropped.
                            continue;
                        }

                        int count;

                        if (!TryParseCount(value, out count))
                        {
                            // Header text leaking into the table
                            // band never parses as a count, so it
                            // stops here instead of overwriting a
                            // real figure.
                            continue;
                        }

                        switch (column)
                        {
                            case Column.Box:
                                row.BoxQty = count;
                                break;
                            case Column.Patti:
                                row.PattiQty = count;
                                break;
                            case Column.Packet:
                                row.PacketQty = count;
                                break;
                            case Column.Total:
                                row.TotalPacket = count;
                                break;
                        }
                    }

                    rows.Add(row);
                }
            }

            return rows;
        }


        private static int CompareByPosition(
            TextRun left,
            TextRun right)
        {
            int byY = right.Y.CompareTo(left.Y);

            return byY != 0 ? byY : left.Seq.CompareTo(right.Seq);
        }


        // ========================================================
        // DERIVED VALUES
        // ========================================================

        private static void Derive(
            List<InventoryPdfRow> rows)
        {
            // 1. Strip the sheet's own status tags and read the
            //    "15GM*192" style spec out of the material.
            foreach (InventoryPdfRow row in rows)
            {
                row.ProductName = ReadMaterial(row);
            }

            // 2. Packets per box. When the row carries no patti
            //    the row is a complete equation on its own, so it
            //    beats the spec written in the material - that is
            //    how the jar rows (220N*10JAR) reconcile.
            foreach (InventoryPdfRow row in rows)
            {
                row.PacketsPerBox = BoxPackets(row);
            }

            // 3. Packets per patti, which can only be measured on
            //    rows that actually hold patti stock.
            Dictionary<int, Dictionary<int, int>> observed =
                new Dictionary<int, Dictionary<int, int>>();

            foreach (InventoryPdfRow row in rows)
            {
                int patti = PattiPackets(row);

                row.PacketsPerPatti = patti;

                if (row.PattiQty <= 0 || patti <= 0)
                {
                    continue;
                }

                if (!observed.ContainsKey(row.PacketsPerBox))
                {
                    observed.Add(
                        row.PacketsPerBox,
                        new Dictionary<int, int>());
                }

                Dictionary<int, int> counts =
                    observed[row.PacketsPerBox];

                counts[patti] = counts.ContainsKey(patti)
                    ? counts[patti] + 1
                    : 1;
            }

            // 4. Rows without patti stock inherit the packing
            //    only when every row sharing that box size agrees
            //    - a shared box size with two answers means the
            //    packing is genuinely per product, and guessing
            //    would silently misprice a patti sale.
            foreach (InventoryPdfRow row in rows)
            {
                if (row.PattiQty > 0 ||
                    row.PacketsPerBox <= 0)
                {
                    continue;
                }

                Dictionary<int, int> counts;

                if (!observed.TryGetValue(
                        row.PacketsPerBox,
                        out counts))
                {
                    continue;
                }

                if (counts.Count == 1)
                {
                    foreach (int value in counts.Keys)
                    {
                        row.PacketsPerPatti = value;
                    }
                }
            }

            // 5. Anything that does not add up, or has no
            //    material to name it after, is called out so it
            //    can be unticked in the preview.
            foreach (InventoryPdfRow row in rows)
            {
                if (row.ProductName.Length == 0)
                {
                    row.Include = false;
                    row.Note = "No material name";

                    if (row.Material.Trim().Length == 0)
                    {
                        row.Material = "(blank)";
                    }

                    continue;
                }

                int total =
                    row.BoxQty * row.PacketsPerBox +
                    row.PattiQty * row.PacketsPerPatti +
                    row.PacketQty;

                if (row.TotalPacket > 0 && total != row.TotalPacket)
                {
                    row.Note =
                        "Packing adds up to " + total +
                        ", sheet says " + row.TotalPacket;
                }
            }
        }


        private static string ReadMaterial(
            InventoryPdfRow row)
        {
            string material = (row.Material ?? "").Trim();

            if (material.Length == 0)
            {
                return "";
            }

            // The sheet marks status with a flagged tag that is
            // a flag rather than part of the name. It writes the
            // same tag in both kinds of bracket - "[INACT.] 32GM"
            // and "(INACT.) ADVT. MATERIAL" - so both count.
            bool inactive = Regex.IsMatch(
                material,
                @"[\[(]\s*INACT\.?\s*[\])]",
                RegexOptions.IgnoreCase);

            bool isNew = Regex.IsMatch(
                material,
                @"[\[(]\s*NEW\s*[\])]",
                RegexOptions.IgnoreCase);

            row.IsActive = !inactive;

            string name = material;

            if (inactive || isNew)
            {
                name = Regex.Replace(
                    name,
                    @"[\[(]\s*INACT\.?\s*[\])]",
                    " ",
                    RegexOptions.IgnoreCase);

                name = Regex.Replace(
                    name,
                    @"[\[(]\s*NEW\s*[\])]",
                    " ",
                    RegexOptions.IgnoreCase);
            }

            name = Regex.Replace(name, @"\s+", " ").Trim();

            // Keep the spec for the variant, but only when the
            // material is not purely a flag.
            Match spec = Regex.Match(
                name,
                @"^(\d+(?:\.\d+)?)\s*GM\s*\*\s*(\d+)");

            if (spec.Success)
            {
                decimal weight;

                if (decimal.TryParse(
                        spec.Groups[1].Value,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out weight))
                {
                    row.PacketWeight = weight;
                    row.WeightUnit = "g";
                    row.VariantCode =
                        spec.Groups[1].Value +
                        "GM*" +
                        spec.Groups[2].Value;
                }
            }

            if (row.VariantCode.Length == 0)
            {
                // A variant is mandatory on ProductVariants, so
                // rows with no spec still need a label.
                row.VariantCode = name.Length <= 200
                    ? name
                    : name.Substring(0, 200);
            }

            return name;
        }


        private static int BoxPackets(InventoryPdfRow row)
        {
            // No patti on the row means the totals line is a
            // single equation: boxQty * packetsPerBox plus any
            // loose packets equals the total.
            if (row.PattiQty == 0 && row.BoxQty > 0)
            {
                int rest = row.TotalPacket - row.PacketQty;

                if (rest > 0 && rest % row.BoxQty == 0)
                {
                    int perBox = rest / row.BoxQty;

                    if (perBox > 0)
                    {
                        return perBox;
                    }
                }
            }

            // Otherwise take the count written in the material,
            // which is what "15GM*192" means.
            int fromSpec = SpecBoxPackets(row.Material);

            if (fromSpec > 0)
            {
                return fromSpec;
            }

            if (row.BoxQty > 0)
            {
                int rest = row.TotalPacket - row.PacketQty;

                if (rest > 0 && rest % row.BoxQty == 0)
                {
                    return rest / row.BoxQty;
                }
            }

            return 0;
        }


        private static int SpecBoxPackets(string material)
        {
            Match match = Regex.Match(
                material ?? "",
                @"GM\s*\*\s*(\d+)");

            if (!match.Success)
            {
                return 0;
            }

            int value;

            if (!int.TryParse(
                    match.Groups[1].Value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out value))
            {
                return 0;
            }

            return value;
        }


        private static int PattiPackets(InventoryPdfRow row)
        {
            if (row.PattiQty <= 0)
            {
                return 0;
            }

            int rest =
                row.TotalPacket -
                row.PacketQty -
                row.BoxQty * row.PacketsPerBox;

            if (rest < 0 || rest % row.PattiQty != 0)
            {
                return 0;
            }

            return rest / row.PattiQty;
        }


        // ============================================================
        // CELL CLEANING
        //
        // Everything outside printable ASCII becomes a space, which
        // collapses the non-breaking spaces the sheet uses to pad
        // materials and prices.
        // ============================================================

        private static string Clean(byte[] raw)
        {
            if (raw == null || raw.Length == 0)
            {
                return "";
            }

            string text = WinAnsi.GetString(raw);

            StringBuilder builder =
                new StringBuilder(text.Length);

            bool pendingSpace = false;

            foreach (char c in text)
            {
                if (c >= '\x20' && c <= '\x7E')
                {
                    if (c == ' ')
                    {
                        pendingSpace = builder.Length > 0;
                        continue;
                    }

                    if (pendingSpace)
                    {
                        builder.Append(' ');
                        pendingSpace = false;
                    }

                    builder.Append(c);
                }
                else
                {
                    pendingSpace = builder.Length > 0;
                }
            }

            return builder.ToString().Trim();
        }


        private static bool IsRowNumber(string value)
        {
            return Regex.IsMatch(value ?? "", @"^\d{1,3}$");
        }


        private static bool TryParseCount(
            string value,
            out int count)
        {
            count = 0;

            string digits = (value ?? "").Replace(",", "");

            if (!Regex.IsMatch(digits, @"^\d{1,9}$"))
            {
                return false;
            }

            return int.TryParse(
                digits,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out count);
        }


        private static bool TryParsePrice(
            string value,
            out decimal price)
        {
            price = 0;

            string digits = (value ?? "").Replace(",", "");

            if (!Regex.IsMatch(digits, @"^\d+(\.\d{1,4})?$"))
            {
                return false;
            }

            return decimal.TryParse(
                digits,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out price);
        }
    }
}
