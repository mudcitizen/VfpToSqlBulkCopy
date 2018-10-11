using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vfptosqlbulkcopy;

namespace VfpToSqlBulkCopy.Utility.TableProcessors
{
    public class ConversionActionProcessor : ITableProcessor
    {

        private String HostPath;
        public ConversionActionProcessor()
        {
            String hostConnStr = Helper.GetConnectionString(Constants.ConnectionNames.Host);
            VfpConnectionStringBuilder vfpConnStrBldr = new VfpConnectionStringBuilder(hostConnStr);
            HostPath = vfpConnStrBldr.DataSource;
        }

        public void Process(string sourceConnectionString, string sourceTableName, string destinationConnectionString, string destinationTableName)
        {
            destinationTableName = Helper.GetDestinationTableName(destinationTableName);

            SqlConnectionStringBuilder sqlConnStrBldr = new SqlConnectionStringBuilder(destinationConnectionString);

            Ivfptosqlbulkcopy com = new vfptosqlbulkcopyClass();
            com.SetPath(HostPath);
            String result = com.ProcessConversionActions(destinationTableName, sqlConnStrBldr.DataSource, sqlConnStrBldr.InitialCatalog);

            if (!String.IsNullOrEmpty(result))
                throw new Exception(result);
        }
    }
}
