using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility
{
    public class DiTableOrDefaultBatchSizeProvider : IBatchSizeProvider
    {
        IBatchSizeProvider defaultBatchSizeProvider;
        IBatchSizeProvider ditableBatchSizeProvider;

        public DiTableOrDefaultBatchSizeProvider(String hostConnectionString)
        {
            defaultBatchSizeProvider = new DefaultBatchSizeProvider();
            ditableBatchSizeProvider = new DiTableBatchSizeProvider(hostConnectionString);
        }

        public int GetBatchSize(string tableName)
        {
            int batchSize = ditableBatchSizeProvider.GetBatchSize(tableName);
            if (batchSize == 0)
                batchSize = defaultBatchSizeProvider.GetBatchSize(tableName);
            return batchSize;
        }
    }
}
