using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility
{
    public interface ITableNameProvider
    {
        IEnumerable<String> GetTables(String connectionName, String connectionString);
    }
}
