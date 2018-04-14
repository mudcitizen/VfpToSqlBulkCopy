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
    public class TableUploader
    {
        private readonly int BatchSize = 25000;

        const string VfpDbfFileExtension = "DBF";
        const string VfpFptFileExtension = "FPT";

        public event EventHandler<TableUploadBeginEventArgs> TableUploadBegin;
        public event EventHandler<TableUploadEndEventArgs> TableUploadEnd;
        public event EventHandler<TableUploadErrorEventArgs> TableUploadError;

        public void Upload(String sourceConnectionString, String sourceTableName, String destinationConnectionString)
        {
            Upload(sourceConnectionString, sourceTableName, destinationConnectionString, sourceTableName.Replace('-', '_'));
        }

        public void Upload(String sourceConnectionString, String sourceTableName, String destinationConnectionString, String destinationTableName)
        {
            OnTableUploadBegin(sourceTableName);
            try
            {
                Process(sourceConnectionString, sourceTableName, destinationConnectionString, destinationTableName);
            }
            catch (Exception ex)
            {
                OnTableUploadError(sourceTableName, ex);
            }
            OnTableUploadEnd(sourceTableName);
        }

        private void Process(String sourceConnectionString, String sourceTableName, String destinationConnectionString, String destinationTableName)
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

                #region ASCII 0 in memo
                VfpConnectionStringBuilder vfpConStrBldr = new VfpConnectionStringBuilder(sourceConnectionString);
                String vfpFileName = Path.Combine(vfpConnStrBldr.DataSource, Path.ChangeExtension(sourceTableName,"DBF"));

                SqlConnectionStringBuilder sqlConStrBldr = new SqlConnectionStringBuilder(destinationConnectionString);
                
                Ivfptosqlbulkcopy com = new vfptosqlbulkcopyClass();
                String result = com.UploadMemos(vfpFileName, destinationTableName, sqlConStrBldr.DataSource, sqlConStrBldr.InitialCatalog);

                #endregion

                #region Update SqlDeleted
                const String recnoParm = "@recno";
                dataTable = Helper.GetOleDbDataTable(sourceConnectionString, String.Format("SELECT RECNO() AS RecNo FROM {0} WHERE DELETED()", sourceTableName));
                if (dataTable.Rows.Count != 1)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        String cmdStr = String.Format("UPDATE {0} SET {1} = 1 WHERE {2} = {3}", destinationTableName, Constants.DILayer.DeletedColumnName, Constants.DILayer.RecnoColumnName, recnoParm);
                        using (SqlCommand cmd = new SqlCommand(cmdStr, destinationConnection))
                        {
                            cmd.Parameters.AddWithValue(recnoParm, row[0].ToString());
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                #endregion

                #region NullDates
                ICommandStringProvider csp = new UpdateDateCommandStringProvider();
                String updateCmdStr = csp.GetCommandString(destinationConnectionString, destinationTableName);
                if (!String.IsNullOrEmpty(updateCmdStr))
                {
                    Helper.ExecuteSqlNonQuery(destinationConnectionString, updateCmdStr);
                }
                #endregion

                destinationConnection.Close();
            }

        }

        void DeleteDbf(String fileName)
        {
            IList<String> fileExtensions = new List<String> { VfpDbfFileExtension, VfpFptFileExtension };
            foreach (String fileExension in fileExtensions)
            {
                String fn = Path.ChangeExtension(fileName, fileExension);
                if (File.Exists(fn))
                    File.Delete(fn);
            }
        }

        #region EventPublishers
        protected virtual void OnTableUploadBegin(String tableName)
        {
            EventHandler<TableUploadBeginEventArgs> handler = TableUploadBegin;
            if (handler != null)
            {
                TableUploadBeginEventArgs args = new TableUploadBeginEventArgs(tableName);
                handler(this, args);
            }
        }
        protected virtual void OnTableUploadEnd(String tableName)
        {
            EventHandler<TableUploadEndEventArgs> handler = TableUploadEnd;
            if (handler != null)
            {
                TableUploadEndEventArgs args = new TableUploadEndEventArgs(tableName);
                handler(this, args);
            }
        }

        protected virtual void OnTableUploadError(String tableName, Exception exception)
        {
            EventHandler<TableUploadErrorEventArgs> handler = TableUploadError;
            if (handler != null)
            {
                TableUploadErrorEventArgs args = new TableUploadErrorEventArgs(tableName, exception);
                handler(this, args);
            }
        }

        #endregion

    }
}
