using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using BWDMS.Data;

namespace BWDMS.Salesman
{
    public partial class VehicleStock : System.Web.UI.Page
    {
        private int SalesmanId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            Response.Cache.SetExpires(
                DateTime.UtcNow.AddYears(-1));

            Response.Cache.SetRevalidation(
                System.Web.HttpCacheRevalidation.AllCaches);


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
                    "Salesman",
                    StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect(
                    BWDMS.Data.AppAuth.HomeUrl(Session["UserRole"]),
                    false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }


            if (!IsPostBack)
            {
                LoadVehicle();

                LoadStock();
            }
        }


        // ============================================================
        // VEHICLE + BEAT — top card
        // ============================================================

        private void LoadVehicle()
        {
            string query = @"
                SELECT TOP 1
                    VehicleNumber = ISNULL(v.VehicleNumber, '-'),
                    BeatName      = ISNULL(r.RouteName, '')
                FROM RouteSchedules rs
                INNER JOIN Routes r
                    ON r.RouteId = rs.RouteId
                LEFT JOIN Vehicles v
                    ON v.VehicleId = rs.VehicleId
                WHERE rs.SalesmanId = @SalesmanId
                ORDER BY rs.RouteScheduleId";

            DataTable dt = RunQuery(query);

            // No schedule yet - fall back to the stock rows
            // the dealer already loaded for this salesman.

            if (dt.Rows.Count == 0 ||
                string.IsNullOrWhiteSpace(
                    Convert.ToString(dt.Rows[0]["BeatName"])))
            {
                string fallback = @"
                    SELECT TOP 1
                        VehicleNumber =
                            ISNULL(v.VehicleNumber, '-'),
                        BeatName = ''
                    FROM VehicleStock vs
                    LEFT JOIN Vehicles v
                        ON v.VehicleId = vs.VehicleId
                    WHERE vs.SalesmanId = @SalesmanId
                    ORDER BY vs.StockDate DESC,
                             vs.VehicleStockId DESC";

                DataTable fb = RunQuery(fallback);

                if (fb.Rows.Count == 0)
                {
                    lblVehicleNumber.Text = "-";
                    lblBeat.Text = "No beat assigned";

                    return;
                }

                dt = fb;
            }

            DataRow row = dt.Rows[0];

            string number =
                Convert.ToString(row["VehicleNumber"]);

            string beat =
                Convert.ToString(row["BeatName"]);

            lblVehicleNumber.Text =
                string.IsNullOrWhiteSpace(number)
                    ? "-"
                    : number;

            lblBeat.Text =
                string.IsNullOrWhiteSpace(beat)
                    ? "No beat assigned"
                    : beat;
        }


        // ============================================================
        // PRODUCT WISE STOCK — summary + table
        // ============================================================

        // The product catalogue is always listed, so products
        // show up even before the dealer has loaded the vehicle.
        // Whatever the salesman's latest load contains is simply
        // overlaid on top of the catalogue.

        private void LoadStock()
        {
            string query = @"
                ;WITH Latest AS
                (
                    SELECT MAX(StockDate) AS StockDate
                    FROM VehicleStock
                    WHERE SalesmanId = @SalesmanId
                ),
                Stock AS
                (
                    SELECT
                        vs.ProductVariantId,
                        LoadedPackets =
                            ISNULL(SUM(vs.LoadedPackets), 0),
                        SoldPackets =
                            ISNULL(SUM(vs.SoldPackets), 0),
                        RemainingPackets =
                            ISNULL
                            (
                                SUM
                                (
                                    vs.LoadedPackets
                                    - vs.SoldPackets
                                    - vs.ReturnedPackets
                                    - vs.DamagedPackets
                                ),
                                0
                            )
                    FROM VehicleStock vs
                    INNER JOIN Latest l
                        ON l.StockDate = vs.StockDate
                    WHERE vs.SalesmanId = @SalesmanId
                    GROUP BY vs.ProductVariantId
                )
                SELECT
                    ProductName     = ISNULL(p.ProductName, '-'),
                    vr.PacketWeight,
                    vr.WeightUnit,
                    UnitName        = ISNULL(su.UnitName, ''),
                    PacketsPerUnit  = ISNULL(u.PacketsPerUnit, 0),
                    LoadedPackets   = ISNULL(s.LoadedPackets, 0),
                    SoldPackets     = ISNULL(s.SoldPackets, 0),
                    RemainingPackets =
                        ISNULL(s.RemainingPackets, 0)
                FROM Products p
                LEFT JOIN ProductVariants vr
                    ON vr.ProductId = p.ProductId
                   AND vr.IsActive = 1
                LEFT JOIN Stock s
                    ON s.ProductVariantId =
                       vr.ProductVariantId
                OUTER APPLY
                (
                    SELECT TOP 1
                        pou.PacketsPerUnit,
                        pou.UnitId
                    FROM ProductUnitOptions pou
                    WHERE pou.ProductVariantId =
                          vr.ProductVariantId
                      AND pou.IsActive = 1
                    ORDER BY pou.PacketsPerUnit DESC
                ) u
                LEFT JOIN SellingUnits su
                    ON su.UnitId = u.UnitId
                WHERE p.IsActive = 1
                ORDER BY p.ProductName, vr.VariantName";

            DataTable dt = RunQuery(query);


            dt.Columns.Add("PackingText", typeof(string));
            dt.Columns.Add("OpeningText", typeof(string));
            dt.Columns.Add("LoadedText", typeof(string));
            dt.Columns.Add("SoldText", typeof(string));
            dt.Columns.Add("RemainingText", typeof(string));

            int totalLoaded = 0;
            int totalSold = 0;
            int totalRemaining = 0;

            foreach (DataRow row in dt.Rows)
            {
                string unit = UnitOf(row);

                int loaded = ToInt(row["LoadedPackets"]);
                int sold = ToInt(row["SoldPackets"]);

                int remaining = ToInt(row["RemainingPackets"]);

                if (remaining < 0)
                {
                    remaining = 0;
                }

                totalLoaded += loaded;
                totalSold += sold;
                totalRemaining += remaining;

                row["PackingText"] = PackingOf(row);
                row["OpeningText"] = "0";
                row["LoadedText"] = loaded + " " + unit;
                row["SoldText"] = sold + " " + unit;
                row["RemainingText"] = remaining + " " + unit;
            }

            lblLoaded.Text = totalLoaded + " Box";
            lblSold.Text = totalSold + " Box";
            lblRemaining.Text = totalRemaining + " Box";

            gvStock.DataSource = dt;
            gvStock.DataBind();
        }


        // "Box (40 Packet)" / "Patti (6 Packet)" / "Packet (20g)"
        private static string PackingOf(DataRow row)
        {
            string unit = UnitOf(row);

            int perUnit = ToInt(row["PacketsPerUnit"]);

            if (perUnit > 1)
            {
                return unit + " (" + perUnit + " Packet)";
            }

            if (row["PacketWeight"] != DBNull.Value)
            {
                decimal weight =
                    Convert.ToDecimal(row["PacketWeight"]);

                string weightText = weight.ToString(
                    "0.###",
                    CultureInfo.InvariantCulture);

                string weightUnit =
                    Convert.ToString(row["WeightUnit"]);

                return unit +
                       " (" +
                       weightText +
                       weightUnit +
                       ")";
            }

            return unit;
        }


        private static string UnitOf(DataRow row)
        {
            string unit =
                Convert.ToString(row["UnitName"]);

            return string.IsNullOrWhiteSpace(unit)
                ? "Packet"
                : unit;
        }


        private static int ToInt(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToInt32(value);
        }


        private DataTable RunQuery(string query)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@SalesmanId",
                        SqlDbType.Int).Value = SalesmanId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            return dt;
        }

    }
}
