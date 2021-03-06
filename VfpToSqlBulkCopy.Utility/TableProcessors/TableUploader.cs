﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Configuration;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using vfptosqlbulkcopy;

namespace VfpToSqlBulkCopy.Utility.TableProcessors
{
    public class TableUploader : ITableProcessor
    {

        int BatchSize;
        private IBatchSizeProvider BatchSizeProvider;

        public TableUploader() : this(null) {  }

        public TableUploader(IBatchSizeProvider batchSizeProvider)
        {
            BatchSizeProvider = batchSizeProvider ?? new DefaultBatchSizeProvider();
        }


        public void Process(String sourceConnectionString, String sourceTableName, String destinationConnectionString, String destinationTableName)
        {
            destinationTableName = Helper.GetDestinationTableName(destinationTableName);
            int recordCount = Convert.ToInt32(Helper.GetOleDbScaler(sourceConnectionString, "SELECT COUNT(*) FROM " + sourceTableName));
            BatchSize = BatchSizeProvider.GetBatchSize(sourceTableName);
                
            DataTable dataTable = null;

            using (SqlConnection destinationConnection = new SqlConnection(destinationConnectionString))
            {
                destinationConnection.Open();

                #region Upload

                VfpConnectionStringBuilder vfpConnStrBldr = new VfpConnectionStringBuilder(sourceConnectionString);
                sourceConnectionString = vfpConnStrBldr.ConnectionString;
                String vfpFolderName = vfpConnStrBldr.DataSource;

                using (OleDbConnection sourceConnection = new OleDbConnection(sourceConnectionString))
                {
                    sourceConnection.Open();

                    int minRecno, maxRecno, recsUploaded;
                    recsUploaded = 0;

                    while (true)
                    {
                        minRecno = recsUploaded;
                        maxRecno = minRecno + BatchSize;

                        // Pull rows from VFP
                        String cmdStr = String.Format("SELECT * FROM {0} WHERE RECNO() > {1} AND RECNO() <= {2}", sourceTableName, Convert.ToString(minRecno), Convert.ToString(maxRecno));
                        dataTable = Helper.GetOleDbDataTable(sourceConnectionString, cmdStr);
                        recsUploaded = recsUploaded + dataTable.Rows.Count;

                        // Push rows to SQL 
                        // D # 19055 As instantiated, our SqlBulkCopy won't fire triggers.  And doing what we need to do 
                        // isn't as simple as firing triggers - because as part of the overall upload we TRUNCATE TABLE <> - 
                        // which does not fire the DELETE trigger...
                        using (SqlBulkCopy copier = new SqlBulkCopy(destinationConnection))
                        {
                            DataTableReader dtReader = dataTable.CreateDataReader();
                            copier.BulkCopyTimeout = 0;
                            copier.DestinationTableName = destinationTableName;
                            copier.WriteToServer(dataTable);
                            dtReader.Close();
                        }

                        if (recsUploaded >= recordCount)
                            break;
                    }

                    sourceConnection.Close();
                }

                #endregion
                                
                destinationConnection.Close();
            }

        }

        public int GetBatchSize()
        {
            return BatchSize;
        }
  

    }
}
