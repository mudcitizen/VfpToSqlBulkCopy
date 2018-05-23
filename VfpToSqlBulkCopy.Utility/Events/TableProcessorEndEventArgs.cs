using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility.Events
{
    public class TableProcessorEndEventArgs : TableProcessorBeginEventArgs
    {
        public TableProcessorEndEventArgs(String tableName) : base(tableName) { }
    }
}
