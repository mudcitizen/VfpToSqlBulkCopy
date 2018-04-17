using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vfptosqlbulkcopy;

namespace VfpToSqlBulkCopy.Utility
{
    public class AsciiZeroMemoProcessor : ITableProcessor
    {
        public void Process(string sourceConnectionString, string sourceTableName, string destinationConnectionString, string destinationTableName)
        {
            destinationTableName = Helper.GetDestinationTableName(destinationTableName);
            VfpConnectionStringBuilder vfpConnStrBldr = new VfpConnectionStringBuilder(sourceConnectionString);
            String vfpFileName = Path.Combine(vfpConnStrBldr.DataSource, Path.ChangeExtension(sourceTableName, "DBF"));

            SqlConnectionStringBuilder sqlConStrBldr = new SqlConnectionStringBuilder(destinationConnectionString);

            Ivfptosqlbulkcopy com = new vfptosqlbulkcopyClass();
            String result = com.UploadMemos(vfpFileName, destinationTableName, sqlConStrBldr.DataSource, sqlConStrBldr.InitialCatalog);
        }
    }
}
