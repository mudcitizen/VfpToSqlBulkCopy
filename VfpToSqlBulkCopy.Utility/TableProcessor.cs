using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Configuration;
using System.Data.OleDb;
using System.Data.SqlClient;


namespace VfpToSqlBulkCopy.Utility
{
    public class TableProcessor
    {
        public void Upload(String sourceConnectionString, String sourceTableName, String destinationConnectionString)
        {
            Upload(sourceConnectionString, sourceTableName, destinationConnectionString, sourceTableName.Replace('-', '_'));
        }


        public void Upload(String sourceConnectionString, String sourceTableName, String destinationConnectionString, String destinationTableName)
        {

            if (String.IsNullOrEmpty(destinationTableName))
                destinationTableName = sourceTableName.Replace("-", "_");

            ICommandStringProvider commandStringProvider = new SelectCommandStringProvider();

            // Date Null Scrub
            // Deleted 
            String selectCommandString = commandStringProvider.GetCommandString(sourceConnectionString, sourceTableName);

            DataTable dataTable = Helper.GetOleDbDataTable(sourceConnectionString, selectCommandString);

            using (SqlConnection destinationConnection = new SqlConnection(destinationConnectionString))
            {
                destinationConnection.Open();

                using (SqlBulkCopy copier = new SqlBulkCopy(destinationConnection))
                {
                    DataTableReader dtReader = dataTable.CreateDataReader();
                    copier.BulkCopyTimeout = 0;
                    copier.BatchSize = 10000;
                    copier.DestinationTableName = destinationTableName;
                    copier.WriteToServer(dataTable);
                    dtReader.Close();
                }

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
    }
}
