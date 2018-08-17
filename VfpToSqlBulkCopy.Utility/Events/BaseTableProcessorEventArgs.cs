using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility.Events
{
    public class BaseTableProcessorEventArgs
    {
        public String TableName { get; }
        public String ClassName { get; }
        public DateTime When { get; }

        public BaseTableProcessorEventArgs(String tableName, String className)
        {
            TableName = tableName;
            ClassName = className;
            When = DateTime.Now;
        }

        public override string ToString()
        {
            return String.Format("When - {0} ; TableName - {1} ; ClassName - {2}", When.ToLongTimeString(), TableName, ClassName);
        }
    }
}
