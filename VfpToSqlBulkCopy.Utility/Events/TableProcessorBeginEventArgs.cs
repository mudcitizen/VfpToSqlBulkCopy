using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility.Events
{
    public class TableProcessorBeginEventArgs : BaseTableProcessorEventArgs
    {
        public TableProcessorBeginEventArgs(String tableName, String className) : base(tableName, className) { }
    }
}
