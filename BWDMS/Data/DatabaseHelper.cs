using System.Configuration;
using System.Data.SqlClient;

namespace BWDMS.Data
{
    public static class DatabaseHelper
    {
        private static readonly string connectionString =
            ConfigurationManager
            .ConnectionStrings["Database1"]
            .ConnectionString;

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}