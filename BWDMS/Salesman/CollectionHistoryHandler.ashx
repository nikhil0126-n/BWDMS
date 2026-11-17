<%@ WebHandler Language="C#" Class="CollectionHistoryHandler" %>

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Script.Serialization;
using BWDMS.Data;

public class CollectionHistoryHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

        if (context.Session["UserId"] == null ||
            context.Session["UserRole"] == null ||
            !context.Session["UserRole"].ToString().Equals("Salesman", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 401;
            context.Response.Write("[]");
            return;
        }

        int salesmanId = Convert.ToInt32(context.Session["UserId"]);
        string shopIdStr = context.Request.QueryString["shop"];

        if (string.IsNullOrEmpty(shopIdStr) || !int.TryParse(shopIdStr, out int shopId))
        {
            context.Response.Write("[]");
            return;
        }

        // Verify shop belongs to this salesman
        string verifyQuery = @"
            SELECT COUNT(*) FROM Shops s
            WHERE s.ShopId = @ShopId
              AND s.IsActive = 1
              AND (
                s.RouteScheduleId IN (SELECT RouteScheduleId FROM RouteSchedules WHERE SalesmanId = @SalesmanId)
                OR s.RouteId IN (SELECT RouteId FROM RouteSchedules WHERE SalesmanId = @SalesmanId)
              )";

        using (SqlConnection con = DatabaseHelper.GetConnection())
        {
            con.Open();

            using (SqlCommand verifyCmd = new SqlCommand(verifyQuery, con))
            {
                verifyCmd.Parameters.Add("@ShopId", SqlDbType.Int).Value = shopId;
                verifyCmd.Parameters.Add("@SalesmanId", SqlDbType.Int).Value = salesmanId;

                int count = (int)verifyCmd.ExecuteScalar();
                if (count == 0)
                {
                    context.Response.Write("[]");
                    return;
                }
            }

            // Get collection history
            string query = @"
                SELECT
                    CollectionDate,
                    CollectionTime,
                    Amount,
                    PaymentMode,
                    ReferenceNumber,
                    Notes,
                    CreatedAt
                FROM Collections
                WHERE ShopId = @ShopId
                ORDER BY CollectionDate DESC, CollectionTime DESC";

            List<object> history = new List<object>();

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.Add("@ShopId", SqlDbType.Int).Value = shopId;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        history.Add(new
                        {
                            date = Convert.ToDateTime(reader["CollectionDate"]).ToString("dd MMM yyyy"),
                            time = reader["CollectionTime"].ToString(),
                            amount = Convert.ToDecimal(reader["Amount"]),
                            mode = reader["PaymentMode"].ToString(),
                            ref = reader["ReferenceNumber"] == DBNull.Value ? "" : reader["ReferenceNumber"].ToString(),
                            notes = reader["Notes"] == DBNull.Value ? "" : reader["Notes"].ToString()
                        });
                    }
                }
            }

            var serializer = new JavaScriptSerializer();
            context.Response.Write(serializer.Serialize(history));
        }
    }

    public bool IsReusable => false;
}