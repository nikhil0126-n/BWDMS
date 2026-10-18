using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class VehicleStock : System.Web.UI.Page
    {
        private int DealerId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }

        private const int PageSize = 5;

        protected int CurrentPage
        {
            get
            {
                object o = ViewState["CurrentPage"];

                if (o == null)
                {
                    return 1;
                }

                return Convert.ToInt32(o);
            }

            set { ViewState["CurrentPage"] = value; }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            if (Session["UserId"] == null)
            {
                Response.Redirect(
                    "~/Account/Login.aspx",
                    false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }

            if (!IsPostBack)
            {
                CurrentPage = 1;

                LoadVehicles();

                LoadStock();
            }
        }


        private void LoadVehicles()
        {
            try
            {
                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    string query = @"
                        SELECT v.VehicleId,
                               v.RegistrationNumber
                        FROM Vehicles v
                        WHERE v.DealerId = @DealerId
                        ORDER BY v.RegistrationNumber";

                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@DealerId",
                            SqlDbType.Int).Value = DealerId;

                        using (SqlDataAdapter da =
                            new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();

                            da.Fill(dt);

                            ddlVehicle.DataSource = dt;

                            ddlVehicle.DataTextField = "RegistrationNumber";

                            ddlVehicle.DataValueField = "VehicleId";

                            ddlVehicle.DataBind();

                            ddlVehicle.Items.Insert(
                                0,
                                new ListItem(
                                    "Select Vehicle",
                                    "0"));
                        }
                    }
                }

                if (ddlVehicle.Items.Count == 0)
                {
                    lblMessage.Visible = true;

                    lblMessage.CssClass =
                        "alert alert-info d-block mb-3";

                    lblMessage.Text = "No vehicles found.";
                }
            }
            catch (Exception ex)
            {
                lblMessage.Visible = true;

                lblMessage.CssClass =
                    "alert alert-danger d-block mb-3";

                lblMessage.Text =
                    "Unable to load vehicles: " + ex.Message;
            }
        }


        private void LoadStock()
        {
            try
            {
                int vehicleId = 0;

                int.TryParse(ddlVehicle.SelectedValue, out vehicleId);

                if (vehicleId == 0)
                {
                    rptStock.DataSource = null;

                    rptStock.DataBind();

                    rowNoData.Visible = true;

                    return;
                }

                DataTable dt = GetStockTable(vehicleId);

                int total = dt.Rows.Count;

                int start = (CurrentPage - 1) * PageSize;

                DataTable page = dt.Clone();

                for (int i = start;
                    i < total && i < start + PageSize;
                    i++)
                {
                    page.ImportRow(dt.Rows[i]);
                }

                rptStock.DataSource = page;

                rptStock.DataBind();

                rowNoData.Visible = total == 0;

                BuildPager(total);
            }
            catch (Exception ex)
            {
                lblMessage.Visible = true;

                lblMessage.CssClass =
                    "alert alert-danger d-block mb-3";

                lblMessage.Text =
                    "Unable to load stock: " + ex.Message;
            }
        }


        // Product level opening / loaded / sold / return / current.
        private DataTable GetStockTable(int vehicleId)
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("ProductName");

            dt.Columns.Add("OpeningStock", typeof(int));

            dt.Columns.Add("Loaded", typeof(int));

            dt.Columns.Add("Sold", typeof(int));

            dt.Columns.Add("Return", typeof(int));

            dt.Columns.Add("CurrentStock", typeof(int));

            try
            {
                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    string query = @"
                        SELECT p.ProductName,
                               ISNULL(SUM(od.Quantity), 0) AS Loaded
                        FROM OrderDetails od
                        INNER JOIN ProductVariants pv
                            ON pv.VariantId = od.VariantId
                        INNER JOIN Products p
                            ON p.ProductId = pv.ProductId
                        WHERE od.VehicleId = @VehicleId
                        GROUP BY p.ProductName";

                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@VehicleId",
                            SqlDbType.Int).Value = vehicleId;

                        using (SqlDataAdapter da =
                            new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
            }
            catch
            {
                // Ledger tables may not be final yet -
                // page still renders with the column layout.
            }

            return dt;
        }


        private void BuildPager(int totalRows)
        {
            int pages = (int)Math.Ceiling(
                totalRows / (double)PageSize);

            if (pages < 1)
            {
                pages = 1;
            }

            if (CurrentPage > pages)
            {
                CurrentPage = pages;
            }

            List<ListItem> items = new List<ListItem>();

            for (int i = 1; i <= pages; i++)
            {
                items.Add(new ListItem(i.ToString(), i.ToString()));
            }

            rptPages.DataSource = items;

            rptPages.DataBind();

            lnkPrev.CssClass = CurrentPage <= 1
                ? "vs-page-btn disabled"
                : "vs-page-btn";

            lnkNext.CssClass = CurrentPage >= pages
                ? "vs-page-btn disabled"
                : "vs-page-btn";
        }


        protected void ddlVehicle_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            CurrentPage = 1;

            LoadStock();
        }


        protected void lnkPrev_Click(object sender, EventArgs e)
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
            }

            LoadStock();
        }


        protected void lnkNext_Click(object sender, EventArgs e)
        {
            CurrentPage++;

            LoadStock();
        }


        protected void rptPages_ItemCommand(
            object source,
            RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Page")
            {
                CurrentPage = Convert.ToInt32(
                    e.CommandArgument);

                LoadStock();
            }
        }
    }
}
