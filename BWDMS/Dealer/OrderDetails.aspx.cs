using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class OrderDetails : System.Web.UI.Page
    {
        private int DealerId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }

        private int OrderId
        {
            get
            {
                int id;

                if (int.TryParse(Request.QueryString["id"], out id))
                {
                    return id;
                }

                return 0;
            }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Account/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadOrder();
            }
        }


        private void LoadOrder()
        {
            if (OrderId == 0)
            {
                ShowError("Order not found.");
                return;
            }

            try
            {
                // ------------------------------------------------
                // Header
                // ------------------------------------------------
                string headerSql = @"
                    SELECT
                        o.OrderNumber,
                        o.Status,
                        OrderDateText =
                            CONVERT(NVARCHAR(10), o.OrderDate, 103),
                        o.OrderType,
                        o.GrandTotal,
                        s.ShopName,
                        SalesmanName = ISNULL(u.FullName, '-')
                    FROM Orders o
                    LEFT JOIN Shops s
                        ON s.ShopId = o.ShopId
                    LEFT JOIN Users u
                        ON u.UserId = o.SalesmanId
                    WHERE o.OrderId = @OrderId
                      AND o.DealerId = @DealerId";

                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand cmd = new SqlCommand(headerSql, con))
                    {
                        cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = OrderId;
                        cmd.Parameters.Add("@DealerId", SqlDbType.Int).Value = DealerId;

                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                lblOrderNumber.Text = rd["OrderNumber"].ToString();
                                lblStatus.Text = rd["Status"].ToString();
                                lblShopName.Text = rd["ShopName"].ToString();
                                lblSalesman.Text = rd["SalesmanName"].ToString();
                                lblDate.Text = rd["OrderDateText"].ToString();
                                lblPayment.Text = rd["OrderType"].ToString();
                                lblCollection.Text =
                                    Convert.ToDecimal(rd["GrandTotal"]).ToString("N0");
                            }
                            else
                            {
                                ShowError("Order not found.");
                                return;
                            }
                        }
                    }
                }

                LoadLines();
            }
            catch (Exception ex)
            {
                ShowError("Unable to load order: " + ex.Message);
            }
        }


        private void LoadLines()
        {
            string lineSql = @"
                SELECT
                    ProductName = p.ProductName,
                    Unit        = ISNULL(su.UnitName, 'Unit'),
                    Qty         = SUM(od.Quantity),
                    Rate        = MAX(od.UnitPrice),
                    Amount      = SUM(od.LineTotal)
                FROM OrderDetails od
                INNER JOIN ProductVariants pv
                    ON pv.ProductVariantId = od.ProductVariantId
                INNER JOIN Products p
                    ON p.ProductId = pv.ProductId
                LEFT JOIN SellingUnits su
                    ON su.UnitId = od.UnitId
                WHERE od.OrderId = @OrderId
                GROUP BY p.ProductName, su.UnitName
                ORDER BY p.ProductName";

            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(lineSql, con))
                {
                    cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = OrderId;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        if (dt.Rows.Count == 0)
                        {
                            rptLines.DataSource = null;
                            rptLines.DataBind();
                            lblNoLines.Visible = true;
                        }
                        else
                        {
                            lblNoLines.Visible = false;
                            rptLines.DataSource = dt;
                            rptLines.DataBind();
                        }
                    }
                }
            }
        }


        private void ShowError(string message)
        {
            lblMessage.Visible = true;
            lblMessage.CssClass = "alert alert-danger d-block";
            lblMessage.Text = message;
        }
    }
}
