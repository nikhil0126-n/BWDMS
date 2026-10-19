using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using BWDMS.Data;

namespace BWDMS.Admin
{
    public partial class Products : System.Web.UI.Page
    {
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


            if (!IsPostBack)
            {
                LoadCategoryFilter();
                LoadProducts();

                if (Request.QueryString["saved"] == "1")
                {
                    ShowMessage("Product saved successfully.", true);
                }
            }
        }


        // ==============================================
        // CATEGORY FILTER DROPDOWN
        // ==============================================

        private void LoadCategoryFilter()
        {
            string query = @"
                SELECT CategoryId, CategoryName
                FROM ProductCategories
                ORDER BY SortOrder, CategoryName";

            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        ddlCategory.DataSource = dt;
                        ddlCategory.DataTextField = "CategoryName";
                        ddlCategory.DataValueField = "CategoryId";
                        ddlCategory.DataBind();

                        ddlCategory.Items.Insert(
                            0,
                            new ListItem(
                                "All Categories",
                                ""));
                    }
                }
            }
        }


        // ==============================================
        // LOAD PRODUCTS
        // ==============================================

        private void LoadProducts()
        {
            string search = txtSearch.Text.Trim();

            string categoryId = ddlCategory.SelectedValue;


            string query = @"
                SELECT
                    p.ProductId,
                    p.ProductCode,
                    p.ProductName,
                    p.IsActive,
                    c.CategoryName,
                    VariantCount =
                    (
                        SELECT COUNT(*)
                        FROM ProductVariants v
                        WHERE v.ProductId = p.ProductId
                    ),
                    UnitCount =
                    (
                        SELECT COUNT(*)
                        FROM ProductUnitOptions uo
                        INNER JOIN ProductVariants v2
                            ON v2.ProductVariantId = uo.ProductVariantId
                        WHERE v2.ProductId = p.ProductId
                        AND uo.IsActive = 1
                    )
                FROM Products p
                INNER JOIN ProductCategories c
                    ON c.CategoryId = p.CategoryId
                WHERE
                (
                    p.ProductName LIKE @Search
                    OR ISNULL(p.ProductCode, '') LIKE @Search
                )
                AND
                (
                    @CategoryId = ''
                    OR CONVERT(NVARCHAR(20), p.CategoryId) = @CategoryId
                )
                ORDER BY p.ProductName";


            using (SqlConnection con =
                DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue(
                        "@Search",
                        "%" + search + "%");

                    cmd.Parameters.AddWithValue(
                        "@CategoryId",
                        categoryId);

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        gvProducts.DataSource = dt;
                        gvProducts.DataBind();

                        litPager.Text = GridPager.Html(gvProducts);
                    }
                }
            }
        }


        protected void txtSearch_TextChanged(
            object sender,
            EventArgs e)
        {
            gvProducts.PageIndex = 0;

            LoadProducts();
        }


        protected void ddlCategory_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            gvProducts.PageIndex = 0;

            LoadProducts();
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

        protected void gvProducts_PageIndexChanging(
            object sender,
            System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            gvProducts.PageIndex = e.NewPageIndex;

            LoadProducts();
        }


        // The pager bar under the grid links back to the grid with a
        // "Page$N" argument. Event validation only accepts arguments that
        // were registered while rendering, which the grid's own (hidden)
        // pager would normally do, so register them here.
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            GridPager.RegisterForEventValidation(gvProducts);

            base.Render(writer);
        }
    }
}
