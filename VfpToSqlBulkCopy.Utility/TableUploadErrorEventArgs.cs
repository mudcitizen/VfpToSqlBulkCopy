using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility
{
    public class TableUploadErrorEventArgs
    {
        public TableUploadErrorEventArgs(String tableName, Exception exception)
        {
            TableName = tableName;
            Exception = exception;
        }
        public String TableName { get; }
        public Exception Exception { get; }

    }
}
