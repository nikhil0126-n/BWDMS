using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Salesman
{
    // ================================================================
    // A salesman's bill.
    //
    // The salesman bills from the vehicle as he goes, so the bill is
    // part of the salesman's own workflow and not something the dealer
    // has to open for him. It is read-only: the order is the record,
    // this is what the shop is handed.
    // ================================================================

    public partial class Bill : System.Web.UI.Page
    {
        private int SalesmanId
        {
            get { return Convert.ToInt32(Session["UserId"]); }
        }

        private int OrderId
        {
            get
            {
                int id = 0;

                int.TryParse(
                    Request.QueryString["orderId"],
                    out id);

                return id;
            }
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


            litPrintedAt.Text =
                "Printed on " +
                DateTime.Now.ToString("dd MMM yyyy HH:mm");

            litBillTitle.Text = "SALES BILL";

            if (!IsPostBack)
            {
                LoadBill();
            }
        }


        protected void btnPrint_Click(
            object sender,
            EventArgs e)
        {
            // The page has already rendered; ask the browser to print it.
            ClientScript.RegisterStartupScript(
                GetType(),
                "bwdmsPrintBill",
                "window.print();");
        }


        private void LoadBill()
        {
            if (OrderId <= 0)
            {
                ShowMessage(
                    "No order was selected.",
                    false);

                return;
            }


            // SalesmanId is part of the WHERE, so a salesman's bill link
            // can never be re-pointed at somebody else's order.
            string query = @"
                SELECT
                    o.OrderNumber,
                    o.OrderDate,
                    o.OrderSource,
                    o.Status,
                    o.SubTotal,
                    o.Discount,
                    o.GrandTotal,
                    o.Remarks,
                    s.ShopName,
                    s.Address,
                    s.Phone,
                    v.VillageName,
                    r.RouteName,
                    rs.DayOfWeek,
                    veh.VehicleNumber,
                    u.FullName AS SalesmanName,
                    d.FullName AS DealerName,
                    d.Email AS DealerEmail,
                    d.Phone AS DealerPhone
                FROM Orders o
                INNER JOIN Shops s
                    ON s.ShopId = o.ShopId
                LEFT JOIN Villages v
                    ON v.VillageId = s.VillageId
                LEFT JOIN RouteSchedules rs
                    ON rs.RouteScheduleId = o.RouteScheduleId
                LEFT JOIN Routes r
                    ON r.RouteId = rs.RouteId
                LEFT JOIN Vehicles veh
                    ON veh.VehicleId = rs.VehicleId
                INNER JOIN Users u
                    ON u.UserId = o.SalesmanId
                INNER JOIN Users d
                    ON d.UserId = o.DealerId
                WHERE o.OrderId = @OrderId
                  AND o.SalesmanId = @SalesmanId";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@OrderId",
                        SqlDbType.Int).Value = OrderId;

                    cmd.Parameters.Add(
                        "@SalesmanId",
                        SqlDbType.Int).Value = SalesmanId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        if (dt.Rows.Count == 0)
                        {
                            ShowMessage(
                                "That bill could not be found.",
                                false);

                            return;
                        }

                        DataRow row = dt.Rows[0];

                        litBillNumber.Text =
                            row["OrderNumber"].ToString();

                        litBillDate.Text = Convert.ToDateTime(
                            row["OrderDate"]).ToString(
                                "dd MMM yyyy");

                        litShop.Text =
                            row["ShopName"].ToString();

                        litRoute.Text = BuildRouteText(row);

                        litSalesman.Text =
                            "Salesman: " +
                            row["SalesmanName"] +
                            (row["VehicleNumber"] == DBNull.Value
                                ? ""
                                : " | Vehicle: " +
                                  row["VehicleNumber"]);

                        string address =
                            row["Address"] == DBNull.Value
                                ? ""
                                : row["Address"].ToString();

                        if (row["VillageName"] != DBNull.Value)
                        {
                            address = (address.Length == 0
                                ? ""
                                : address + ", ") +
                                row["VillageName"];
                        }

                        lblShopAddress.Text = address;

                        string phone =
                            row["Phone"] == DBNull.Value
                                ? ""
                                : row["Phone"].ToString();

                        if (phone.Length > 0)
                        {
                            lblShopAddress.Text +=
                                (lblShopAddress.Text.Length == 0
                                    ? ""
                                    : " | ") + phone;
                        }

                        lblDealer.Text =
                            row["DealerName"].ToString() +
                            (row["DealerEmail"] == DBNull.Value
                                ? ""
                                : " | " + row["DealerEmail"]);

                        litSubTotal.Text =
                            Convert.ToDecimal(
                                row["SubTotal"]).ToString("N2");

                        litDiscount.Text =
                            Convert.ToDecimal(
                                row["Discount"]).ToString("N2");

                        litGrandTotal.Text =
                            Convert.ToDecimal(
                                row["GrandTotal"]).ToString("N2");
                    }
                }
            }

            LoadLines();
        }


        private static string BuildRouteText(DataRow row)
        {
            if (row["RouteName"] == DBNull.Value)
            {
                return "Counter order";
            }

            string text = row["RouteName"].ToString();

            if (row["DayOfWeek"] != DBNull.Value)
            {
                text += " (" + row["DayOfWeek"] + ")";
            }

            return text;
        }


        private void LoadLines()
        {
            string query = @"
                SELECT
                    p.ProductName,
                    pv.VariantName,
                    ISNULL(su.UnitCode, 'PKT') AS UnitCode,
                    od.Quantity,
                    od.UnitPrice,
                    od.LineTotal
                FROM OrderDetails od
                INNER JOIN ProductVariants pv
                    ON pv.ProductVariantId = od.ProductVariantId
                INNER JOIN Products p
                    ON p.ProductId = pv.ProductId
                LEFT JOIN SellingUnits su
                    ON su.UnitId = od.UnitId
                WHERE od.OrderId = @OrderId
                  AND EXISTS
                      (
                          SELECT 1
                          FROM   Orders o
                          WHERE  o.OrderId = od.OrderId
                            AND  o.SalesmanId = @SalesmanId
                      )
                ORDER BY od.OrderDetailId";

            DataTable dt = new DataTable();

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@OrderId",
                        SqlDbType.Int).Value = OrderId;

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

            dt.Columns.Add("LineNo", typeof(int));
            dt.Columns.Add("UnitPriceText", typeof(string));
            dt.Columns.Add("LineTotalText", typeof(string));

            int line = 1;

            foreach (DataRow row in dt.Rows)
            {
                row["LineNo"] = line++;

                row["UnitPriceText"] =
                    Convert.ToDecimal(
                        row["UnitPrice"]).ToString("N2");

                row["LineTotalText"] =
                    Convert.ToDecimal(
                        row["LineTotal"]).ToString("N2");
            }

            gvLines.DataSource = dt;
            gvLines.DataBind();
        }


        private void ShowMessage(
            string message,
            bool success)
        {
            lblMessage.Visible = true;
            lblMessage.Text = message;
            lblMessage.CssClass = success
                ? "alert alert-success d-block no-print"
                : "alert alert-danger d-block no-print";
        }
    }
}