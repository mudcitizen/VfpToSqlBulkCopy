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
        public void Upload(String sourceConnectionName, String sourceTableName, String destinationConnectionName)
        {
            Upload(sourceConnectionName, sourceTableName, destinationConnectionName, sourceTableName.Replace('-', '_'), new SelectCommandStringProvider());
        }


        public void Upload(String sourceConnectionName, String sourceTableName, String destinationConnectionName, String destinationTableName, ICommandStringProvider commandStringProvider)
        {

            if (String.IsNullOrEmpty(destinationTableName))
                destinationTableName = sourceTableName.Replace("-", "_");

            if (commandStringProvider == null)
                commandStringProvider = new SelectCommandStringProvider();

            // Date Null Scrub
            // Deleted 
            String selectCommandString = commandStringProvider.GetCommandString(sourceConnectionName, sourceTableName);

            DataTable dataTable = Helper.GetOleDbDataTable(sourceConnectionName, selectCommandString);

            using (SqlConnection destinationConnection = new SqlConnection(Helper.GetConnectionString(destinationConnectionName)))
            {
                destinationConnection.Open();

                using (SqlBulkCopy copier = new SqlBulkCopy(destinationConnection))
                {
                    DataTableReader dtReader = dataTable.CreateDataReader();
                    copier.DestinationTableName = destinationTableName;
                    copier.WriteToServer(dataTable);
                    dtReader.Close();
                }

                #region Update SqlDeleted
                const String recnoParm = "@recno";
                dataTable = Helper.GetOleDbDataTable(sourceConnectionName, String.Format("SELECT RECNO() AS RecNo FROM {0} WHERE DELETED()", sourceTableName));
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
                String updateCmdStr = csp.GetCommandString(destinationConnectionName,destinationTableName);
                if (!String.IsNullOrEmpty(updateCmdStr))
                {
                    Helper.ExecuteSqlNonQuery(destinationConnectionName, updateCmdStr);
                }
                #endregion


                destinationConnection.Close();

            }

        }
    }
}
