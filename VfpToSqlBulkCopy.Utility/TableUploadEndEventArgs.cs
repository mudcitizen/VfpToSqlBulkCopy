using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility
{
    public class TableUploadEndEventArgs : TableUploadBeginEventArgs
    {
        public TableUploadEndEventArgs(String tableName) : base(tableName) { }
    }
}
