using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility
{
    public class DefaultBatchSizeProvider : IBatchSizeProvider
    {
        public int GetBatchSize(String tableName)
        {
            return Constants.DefaultBatchSize;
        }
    }
}
