using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility.Events
{
    public class TableProcessorEndEventArgs : BaseTableProcessorEventArgs
    {
        public TableProcessorEndEventArgs(String tableName, String className) : base(tableName, className) { }
        public TableProcessorEndEventArgs(BaseTableProcessorEventArgs args) : base(args.TableName, args.ClassName) { }
    }
}
