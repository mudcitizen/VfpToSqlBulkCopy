using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VfpToSqlBulkCopy.Utility.CommandStringProviders;

namespace VfpToSqlBulkCopy.Utility.TableProcessors
{
    public class NullDateProcessor : ITableProcessor
    {
        public void Process(string sourceConnectionString, string sourceTableName, string destinationConnectionString, string destinationTableName)
        {
            ICommandStringProvider csp = new UpdateDateCommandStringProvider();
            destinationTableName = Helper.GetDestinationTableName(destinationTableName);
            String updateCmdStr = csp.GetCommandString(destinationConnectionString, destinationTableName);
            if (!String.IsNullOrEmpty(updateCmdStr))
            {
                Helper.ExecuteSqlNonQuery(destinationConnectionString, updateCmdStr);
            }
        }
    }
}
