using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.Common;

namespace VfpToSqlBulkCopy.Utility
{
    public class Helper
    {
        public static String GetConnectionString(String connectionName)
        {
            ConnectionStringSettings css = ConfigurationManager.ConnectionStrings[connectionName];
            if (css == null)
                return null;
            else
                return css.ConnectionString;
        }

        public static Object GetOleDbScaler(String connectionString, string cmdStr)
        {
            Object result = null;
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                using (OleDbCommand cmd = new OleDbCommand(cmdStr, conn))
                {
                    conn.Open();
                    result = cmd.ExecuteScalar();
                    conn.Close();
                }
            }
            return result;
        }
        public static DataTable GetOleDbDataTable(String connectionString, string cmdStr)
        {
            DataTable result = null;
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                using (OleDbCommand cmd = new OleDbCommand(cmdStr, conn))
                {
                    conn.Open();
                    result = new DataTable();
                    result.Load(cmd.ExecuteReader());
                    conn.Close();
                }
            }
            return result;
        }
        public static DataTable GetSqlDataTable(String connectionString, string cmdStr)
        {
            DataTable result = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(cmdStr, conn))
                {
                    conn.Open();
                    cmd.CommandTimeout = 0;
                    result = new DataTable();
                    result.Load(cmd.ExecuteReader());
                    conn.Close();
                }
            }
            return result;
        }

        public static Object GetSqlScaler(String connectionString, string cmdStr)
        {
            Object result = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(cmdStr, conn))
                {
                    conn.Open();
                    cmd.CommandTimeout = 0;
                    result = cmd.ExecuteScalar();
                    conn.Close();
                }
            }
            return result;
        }
        public static void ExecuteOleDbNonQuery(String connectionString, string cmdStr)
        {
            Object result = null;
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                using (OleDbCommand cmd = new OleDbCommand(cmdStr, conn))
                {
                    conn.Open();
                    result = cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }
        public static void ExecuteSqlNonQuery(String connectionString, string cmdStr)
        {
            Object result = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(cmdStr, conn))
                {
                    conn.Open();
                    cmd.CommandTimeout = 0;
                    result = cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public static String GetDestinationTableName(String tableName)
        {
            return tableName.Replace('-', '_');
        }
    }
}
