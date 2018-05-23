using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility
{
    public interface  ITableProcessor
    {
        void Process(String sourceConnectionString, String sourceTableName, String destinationConnectionString, String destinationTableName);    
    }
}
