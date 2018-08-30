using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility.TableProcessors
{
    public class TruncateTableProcessor : ITableProcessor
    {
        public void Process(string sourceConnectionString, string sourceTableName, string destinationConnectionString, string destinationTableName)
        {
            destinationTableName = Helper.GetDestinationTableName(destinationTableName);
            Helper.ExecuteSqlNonQuery(destinationConnectionString, "TRUNCATE TABLE " + destinationTableName);
        }
    }
}
