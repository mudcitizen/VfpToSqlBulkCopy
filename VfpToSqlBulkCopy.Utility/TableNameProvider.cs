using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility
{
    public class TableNameProvider : ITableNameProvider
    {

        String HostConnectionString;

        // 1st String is a Connection Name ; 2nd String is a TableName
        Func<String, String, Boolean> TableNameFilter;

        public TableNameProvider(String hostConnectionString) : this(hostConnectionString, null) { }

        public TableNameProvider(String hostConnectionString,Func<String, String, Boolean> tableNameFilter)
        {
            HostConnectionString = hostConnectionString;
            TableNameFilter = tableNameFilter;
        }

        public IEnumerable<string> GetTables(string connectionName, string connectionString)
        {
            IList<String> tables = new List<String>();

            String cmdStr = String.Format("SELECT TABLE FROM DITABLE WHERE UPPER(ClassName) NOT LIKE 'V%' AND UPPER(IndxDbf) LIKE '{0}%' ORDER BY TABLE", connectionName.Equals(Constants.ConnectionNames.Host, StringComparison.InvariantCultureIgnoreCase) ? 'I' : 'P');
            DataTable dt = Helper.GetOleDbDataTable(HostConnectionString, cmdStr);

            OleDbConnectionStringBuilder bldr = new OleDbConnectionStringBuilder(connectionString);
            Object obj = null;
            bldr.TryGetValue("Data Source", out obj);

            if (obj == null)
                throw new ApplicationException("No Data Source for connection name " + connectionName);

            String directoryName = obj.ToString();

            if (!Directory.Exists(directoryName))
                throw new ApplicationException("Data Source does not exist for connection name " + connectionName);

            // TODO Ensure connectionString has DELETED=False
            foreach (DataRow row in dt.Rows)
            {
                String tableName = row[0].ToString().Trim();

                if ((TableNameFilter != null) && (!TableNameFilter(connectionName,tableName)))
                    continue;

                String fileName = Path.Combine(directoryName, tableName + ".DBF");
                if (File.Exists(fileName))
                    tables.Add(tableName);
            }

            return tables;
        }
    }
}
