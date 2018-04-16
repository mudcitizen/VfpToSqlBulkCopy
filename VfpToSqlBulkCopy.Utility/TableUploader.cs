using System;
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

namespace VfpToSqlBulkCopy.Utility
{
    public class TableUploader : ITableProcessor
    {
        private readonly int BatchSize = 25000;
            

        public void Process(String sourceConnectionString, String sourceTableName, String destinationConnectionString, String destinationTableName)
        {

            if (String.IsNullOrEmpty(destinationTableName))
                destinationTableName = sourceTableName.Replace("-", "_");

            int recordCount = Convert.ToInt32(Helper.GetOleDbScaler(sourceConnectionString, "SELECT COUNT(*) FROM " + sourceTableName));

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
  

    }
}
