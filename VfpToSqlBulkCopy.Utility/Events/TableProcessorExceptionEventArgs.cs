using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility.Events
{
    public class TableProcessorExceptionEventArgs : BaseTableProcessorEventArgs
    {
        public Exception Exception { get; }
        public TableProcessorExceptionEventArgs(String tableName, String className, Exception exception) : base(tableName,className)
        {
            Exception = exception;
        }

        public override string ToString()
        {
            return base.ToString() + " Exception - " + Exception.ToString();
        }

    }
}
