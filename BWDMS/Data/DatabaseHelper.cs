using System.Configuration;
using System.Data.SqlClient;

namespace BWDMS.Data
{
    public static class DatabaseHelper
    {
        // Web.config names the string "Database1", after the .mdf it
        // attaches; Deploy-Database.ps1 and the README look the string
        // up by that same name.
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