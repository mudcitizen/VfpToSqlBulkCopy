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

                String outTableStem = destinationTableName + "_MemoProblems";
                const String fieldColumnName = "fieldname";
                const String recnoColumnName = "recno";
                const String fileNameColumnName = "filename";



                String tableIn = Path.Combine(vfpFolderName, Path.ChangeExtension(sourceTableName, "DBF"));

                String outFolder = Path.Combine(vfpFolderName, "memodata");
                String outTable = outTableStem + ".dbf";
                String outTableFileName = Path.Combine(outFolder, outTable);

                DeleteDbf(outTableFileName);

                if (!Directory.Exists(outFolder))
                    Directory.CreateDirectory(outFolder);

                Ivfptosqlbulkcopy com = new vfptosqlbulkcopyClass();
                String result = com.ListMemos(tableIn, outFolder, outTable);
                if (!String.IsNullOrEmpty(result))
                    throw new ApplicationException(result);

                // We are not storing the temp table in the Hostplus folder so we'll need 
                // a different connectionString to read it.
                VfpConnectionStringBuilder tempVpfConnBldr = new VfpConnectionStringBuilder();
                tempVpfConnBldr.DataSource = outFolder;
                dataTable = Helper.GetOleDbDataTable(tempVpfConnBldr.ConnectionString, "SELECT * FROM " + outTableStem);

                using (SqlConnection sqlConn = new SqlConnection(destinationConnectionString))
                {
                    sqlConn.Open();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        String fieldName = row[fieldColumnName].ToString();
                        int recno = Convert.ToInt32(row[recnoColumnName]);
                        String fileName = row[fileNameColumnName].ToString();
                        String memoData = File.ReadAllText(fileName);


                        String updateMemoCmdStr = String.Format("UPDATE {0} SET {1} = @{1} WHERE {2} = @{2}", destinationTableName, fieldName, Constants.DILayer.RecnoColumnName);
                        using (SqlCommand sqlCmd = new SqlCommand(updateMemoCmdStr, sqlConn))
                        {
                            sqlCmd.Parameters.AddWithValue("@" + fieldName, memoData);
                            sqlCmd.Parameters.AddWithValue("@" + Constants.DILayer.RecnoColumnName, recno.ToString());
                            sqlCmd.ExecuteNonQuery();
                        }

                        File.Delete(fileName);

                    }

                    sqlConn.Close();
                }


                DeleteDbf(outTableFileName);

                try
                {
                    Directory.Delete(outFolder, true);
                }
                catch (Exception ex)
                { }

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
