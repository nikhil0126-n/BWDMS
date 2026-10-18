using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class Inventory : System.Web.UI.Page
    {
        private int DealerId
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
                    "Dealer",
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
                LoadInventory();

                if (Request.QueryString["saved"] == "1")
                {
                    ShowMessage(
                        "Stock saved successfully.",
                        true);
                }
            }
        }


        // ============================================================
        // STOCK LEVELS
        //
        // A variant with no Inventory row is shown as zero so the
        // dealer can see everything they can sell.
        // ============================================================

        private void LoadInventory()
        {
            string search = txtSearch.Text.Trim();
            string filter = ddlStockFilter.SelectedValue;

            string query = @"
                SELECT
                    InventoryId = ISNULL(i.InventoryId, 0),
                    v.ProductVariantId,
                    p.ProductName,
                    v.VariantName,
                    WeightText =
                        CASE
                            WHEN v.PacketWeight IS NULL THEN '-'
                            ELSE CONVERT(NVARCHAR(20), v.PacketWeight)
                                 + ISNULL(' ' + v.WeightUnit, '')
                        END,
                    QuantityPackets = ISNULL(i.QuantityPackets, 0),
                    ReorderLevel = ISNULL(i.ReorderLevel, 0),
                    UpdatedAtText =
                        CASE
                            WHEN i.UpdatedAt IS NULL THEN '-'
                            ELSE CONVERT(NVARCHAR(16), i.UpdatedAt, 120)
                        END,
                    StockNow = ISNULL(i.QuantityPackets, 0),
                    ReorderNow = ISNULL(i.ReorderLevel, 0)
                FROM ProductVariants v
                INNER JOIN Products p
                    ON p.ProductId = v.ProductId
                LEFT JOIN Inventory i
                    ON i.ProductVariantId = v.ProductVariantId
                   AND i.DealerId = @DealerId
                WHERE v.IsActive = 1
                  AND p.IsActive = 1
                  AND
                  (
                    p.ProductName LIKE @Search
                    OR v.VariantName LIKE @Search
                    OR ISNULL(v.VariantCode, '') LIKE @Search
                  )
                ORDER BY p.ProductName, v.SortOrder, v.VariantName";

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@DealerId",
                        SqlDbType.Int).Value = DealerId;

                    cmd.Parameters.AddWithValue(
                        "@Search",
                        "%" + search + "%");

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }


            if (!dt.Columns.Contains("StatusText"))
            {
                dt.Columns.Add("StatusText", typeof(string));
                dt.Columns.Add("StatusCss", typeof(string));
            }


            // --------------------------------------------
            // Compute status in memory so the same data
            // can be filtered without a second query.
            // --------------------------------------------

            foreach (DataRow row in dt.Rows)
            {
                int qty = Convert.ToInt32(row["StockNow"]);
                int reorder = Convert.ToInt32(row["ReorderNow"]);

                if (qty <= 0)
                {
                    row["QuantityPackets"] = 0;
                    row["StatusText"] = "Out of Stock";
                    row["StatusCss"] = "badge bg-danger";
                }
                else if (reorder > 0 && qty <= reorder)
                {
                    row["StatusText"] = "Low Stock";
                    row["StatusCss"] = "badge bg-warning text-dark";
                }
                else
                {
                    row["StatusText"] = "In Stock";
                    row["StatusCss"] = "badge bg-success";
                }
            }


            // --------------------------------------------
            // Display columns for the Warehouse Stock grid
            // (Box / Panti / Packet). Derived from the
            // packet count for display only.
            // --------------------------------------------

            if (!dt.Columns.Contains("BoxText"))
            {
                dt.Columns.Add("BoxText", typeof(string));
                dt.Columns.Add("PantiText", typeof(string));
                dt.Columns.Add("PacketText", typeof(string));
            }

            const int packetsPerPanti = 20;
            const int pantisPerBox = 10;

            foreach (DataRow row in dt.Rows)
            {
                int packets = Convert.ToInt32(row["StockNow"]);

                int pantis = packets / packetsPerPanti;
                int boxes = pantis / pantisPerBox;

                row["BoxText"] = boxes + " Box";
                row["PantiText"] = pantis.ToString();
                row["PacketText"] = packets.ToString();
            }


            if (filter == "out")
            {
                dt = dt.Select("StatusText = 'Out of Stock'")
                       .CopyToDataTableSafe(dt);
            }
            else if (filter == "low")
            {
                dt = dt.Select(
                           "(StatusText = 'Low Stock' OR StatusText = 'Out of Stock')")
                       .CopyToDataTableSafe(dt);
            }
            else if (filter == "ok")
            {
                dt = dt.Select("StatusText = 'In Stock'")
                       .CopyToDataTableSafe(dt);
            }

            gvInventory.DataSource = dt;
            gvInventory.DataBind();
        }


        protected void txtSearch_TextChanged(
            object sender,
            EventArgs e)
        {
            LoadInventory();
        }


        protected void FilterChanged(
            object sender,
            EventArgs e)
        {
            LoadInventory();
        }


        private void ShowMessage(
            string message,
            bool success)
        {
            lblMessage.Visible = true;
            lblMessage.Text = message;
            lblMessage.CssClass = success
                ? "alert alert-success d-block"
                : "alert alert-danger d-block";
        }


        // ------------------------------------------------------------
        // PAGING
        //
        // The new index has to be applied before the grid is rebound,
        // otherwise the postback lands back on the page it came from.
        // ------------------------------------------------------------

        protected void gvInventory_PageIndexChanging(
            object sender,
            System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            gvInventory.PageIndex = e.NewPageIndex;

            LoadInventory();
        }
    }


    internal static class DataTableExtensions
    {
        public static DataTable CopyToDataTableSafe(
            this DataRow[] rows,
            DataTable source)
        {
            // Keep the original columns even when nothing
            // matches, otherwise DataBind() throws
            // "field or property ... was not found".
            if (rows == null || rows.Length == 0)
            {
                return source == null
                    ? new DataTable()
                    : source.Clone();
            }

            return rows.CopyToDataTable();
        }
    }
}
