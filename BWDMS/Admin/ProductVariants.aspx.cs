using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BWDMS.Data;

namespace BWDMS.Admin
{
    public partial class ProductVariants : System.Web.UI.Page
    {
        private int ProductId
        {
            get
            {
                int id;

                if (int.TryParse(
                    Request.QueryString["product"],
                    out id))
                {
                    return id;
                }

                return 0;
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
                    "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect(
                    BWDMS.Data.AppAuth.HomeUrl(Session["UserRole"]),
                    false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }


            // Missing or malformed product id.
            if (ProductId <= 0)
            {
                Response.Redirect(
                    "~/Admin/Products.aspx",
                    false);

                Context.ApplicationInstance.CompleteRequest();

                return;
            }


            if (!IsPostBack)
            {
                if (!LoadProductName())
                {
                    Response.Redirect(
                        "~/Admin/Products.aspx",
                        false);

                    return;
                }

                lnkAddVariant.NavigateUrl =
                    "~/Admin/AddVariant.aspx?product=" + ProductId;

                LoadVariants();

                if (Request.QueryString["saved"] == "1")
                {
                    ShowMessage("Variant saved successfully.", true);
                }
            }
        }


        private bool LoadProductName()
        {
            string query = @"
                SELECT ProductName
                FROM Products
                WHERE ProductId = @ProductId";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                con.Open();

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@ProductId",
                        SqlDbType.Int).Value =
                        ProductId;

                    object result = cmd.ExecuteScalar();

                    if (result == null ||
                        result == DBNull.Value)
                    {
                        return false;
                    }

                    lblProductName.Text =
                        result.ToString();

                    return true;
                }
            }
        }


        // ============================================================
        // LOAD VARIANTS + AGGREGATED UNIT / PRICE SUMMARY
        // ============================================================

        private void LoadVariants()
        {
            string query = @"
                SELECT
                    v.ProductVariantId,
                    v.VariantCode,
                    v.VariantName,
                    v.PacketWeight,
                    v.WeightUnit,
                    v.IsActive,
                    WeightText =
                        CASE
                            WHEN v.PacketWeight IS NULL
                                THEN '-'
                            ELSE CONVERT(NVARCHAR(20), v.PacketWeight)
                                 + ISNULL(' ' + v.WeightUnit, '')
                        END,
                    UnitText =
                        ISNULL((
                            SELECT STUFF((
                                SELECT ', ' + su.UnitName
                                FROM ProductUnitOptions uo
                                INNER JOIN SellingUnits su
                                    ON su.UnitId = uo.UnitId
                                WHERE uo.ProductVariantId = v.ProductVariantId
                                AND uo.IsActive = 1
                                ORDER BY su.SortOrder
                                FOR XML PATH('')), 1, 2, '')
                        ), '-'),
                    DealerPriceText =
                        ISNULL((
                            SELECT TOP 1 CONVERT(NVARCHAR(20), pp.DealerPurchasePrice)
                            FROM ProductUnitOptions uo
                            INNER JOIN ProductPrices pp
                                ON pp.ProductOptionId = uo.ProductOptionId
                            WHERE uo.ProductVariantId = v.ProductVariantId
                            AND uo.IsActive = 1
                            AND pp.IsActive = 1
                            ORDER BY uo.ProductOptionId,
                                     pp.EffectiveFrom DESC
                        ), '-'),
                    ShopPriceText =
                        ISNULL((
                            SELECT TOP 1 CONVERT(NVARCHAR(20), pp.ShopSellingPrice)
                            FROM ProductUnitOptions uo
                            INNER JOIN ProductPrices pp
                                ON pp.ProductOptionId = uo.ProductOptionId
                            WHERE uo.ProductVariantId = v.ProductVariantId
                            AND uo.IsActive = 1
                            AND pp.IsActive = 1
                            ORDER BY uo.ProductOptionId,
                                     pp.EffectiveFrom DESC
                        ), '-')
                FROM ProductVariants v
                WHERE v.ProductId = @ProductId
                ORDER BY v.SortOrder, v.VariantName";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@ProductId",
                        SqlDbType.Int).Value =
                        ProductId;

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        gvVariants.DataSource = dt;
                        gvVariants.DataBind();
                    }
                }
            }
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
    }
}
