using System;
using System.Data;
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

            if (IsPostBack)
            {
                return;
            }

            LoadOrder();
        }


        private void LoadOrder()
        {
            try
            {
                // TODO: replace with real query once Orders tables are final.
                // DataTable header = DbHelper.ExecuteDataTable(
                //     "SELECT ... FROM Orders WHERE OrderId = @OrderId AND DealerId = @DealerId",
                //     new SqlParameter("@OrderId", OrderId),
                //     new SqlParameter("@DealerId", DealerId));

                // DataTable lines = DbHelper.ExecuteDataTable(...);

                // if (lines.Rows.Count == 0)
                // {
                //     lblNoLines.Visible = true;
                // }
                // else
                // {
                //     rptLines.DataSource = lines;
                //     rptLines.DataBind();
                // }
            }
            catch (Exception ex)
            {
                lblMessage.Visible = true;
                lblMessage.CssClass = "alert alert-danger d-block";
                lblMessage.Text = "Unable to load order: " + ex.Message;
            }
        }
    }
}
