using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility.Events
{
    public class TableProcessorBeginEventArgs
    {
        public TableProcessorBeginEventArgs(String tableName)
        {
            TableName = tableName;
            When = DateTime.Now;
        }
        public String TableName { get; }
        public DateTime When { get; }

        public override string ToString()
        {
            return String.Format("TableName - {0} ; When - {1}", TableName, When.ToLongTimeString());
        }
    }
}
