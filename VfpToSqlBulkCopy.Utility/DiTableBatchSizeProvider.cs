using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility
{
    public class DiTableBatchSizeProvider : IBatchSizeProvider
    {
        IDictionary<String, int> CustomBatchSizes = new Dictionary<String, int>();
        public DiTableBatchSizeProvider(String hostConnectionString)
        {
            DataTable dt = Helper.GetOleDbDataTable(hostConnectionString, "SELECT Table,BatchSize FROM DITABLE WHERE BatchSize <> 0");
            foreach (DataRow row in dt.Rows)
            {
                String table = ((String)row[0]).ToUpper();
                int batchSize = (int)row[1];
                CustomBatchSizes.Add(new KeyValuePair<String, int>(table,batchSize));
            }
        }


        public int GetBatchSize(string tableName)
        {
            tableName = tableName.ToUpper();
            int batchSize;
            if (!CustomBatchSizes.TryGetValue(tableName,out batchSize))
                batchSize = 0;
            return batchSize;
        }
    }
}
