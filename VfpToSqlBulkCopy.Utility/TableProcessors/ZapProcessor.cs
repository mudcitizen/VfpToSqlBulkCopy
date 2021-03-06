﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility.TableProcessors
{
    [Obsolete("Use TruncateTableProcessor instead (because it is much faster)")]
    public class ZapProcessor : ITableProcessor
    {
        public void Process(string sourceConnectionString, string sourceTableName, string destinationConnectionString, string destinationTableName)
        {
            destinationTableName = Helper.GetDestinationTableName(destinationTableName);
            Helper.ExecuteSqlNonQuery(destinationConnectionString, "DELETE FROM " + destinationTableName);
        }
    }
}
