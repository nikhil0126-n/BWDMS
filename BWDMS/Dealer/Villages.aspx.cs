using System;
using System.Data;
using System.Data.SqlClient;
using BWDMS.Data;

namespace BWDMS.Dealer
{
    public partial class Villages : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {

            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            Response.Cache.SetExpires(
                DateTime.UtcNow.AddDays(-1));

            Response.Cache.SetRevalidation(
                System.Web.HttpCacheRevalidation.AllCaches);



            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }


            if (Session["UserRole"] == null ||
                Session["UserRole"].ToString() != "Dealer")
            {
                Response.Redirect("~/Account/Login.aspx");
                return;
            }



            if (!IsPostBack)
            {
                LoadVillages();
            }
        }



        private void LoadVillages()
        {
            try
            {
                string query = @"
                    SELECT
                        VillageId,
                        VillageName,
                        Taluka,
                        District,
                        Pincode,
                        IsActive
                    FROM Villages
                    ORDER BY
                        VillageName ASC";


                using (SqlConnection con =
                    DatabaseHelper.GetConnection())
                {
                    con.Open();


                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        using (SqlDataAdapter da =
                            new SqlDataAdapter(cmd))
                        {
                            DataTable dt =
                                new DataTable();


                            da.Fill(dt);


                            gvVillages.DataSource =
                                dt;

                            gvVillages.DataBind();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Write(
                    "<script>alert('Error loading villages: " +
                    Server.HtmlEncode(ex.Message) +
                    "');</script>");
            }
        }
    }
}