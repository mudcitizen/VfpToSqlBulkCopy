using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace VfpToSqlBulkCopy.Utility
{
    public class Helper
    {
        public static String GetConnectionString(String connectionName)
        {
            String connStr = ConfigurationManager.ConnectionStrings[connectionName].ToString();
            return connStr;
        }
    }
}
