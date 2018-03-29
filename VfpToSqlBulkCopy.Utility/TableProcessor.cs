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
            Upload(sourceConnectionName, sourceTableName, destinationConnectionName, sourceTableName.Replace('-', '_'), new DefaultCommandStringProvider());
        }


        public void Upload(String sourceConnectionName, String sourceTableName, String destinationConnectionName, String destinationTableName, ICommandStringProvider commandStringProvider)
        {

            if (String.IsNullOrEmpty(destinationTableName))
                destinationTableName = sourceTableName.Replace("-", "_");

            if (commandStringProvider == null)
                commandStringProvider = new DefaultCommandStringProvider();

            // Date Null Scrub
            // Deleted 
            String selectCommandString = commandStringProvider.GetCommandString(sourceConnectionName, sourceTableName);

            DataTable dataTable = Helper.GetOleDbDataTable(sourceConnectionName, selectCommandString);

            using (SqlConnection destinationConnection = new SqlConnection(Helper.GetConnectionString(destinationConnectionName)))
            {
                using (SqlBulkCopy copier = new SqlBulkCopy(destinationConnection))
                {
                    destinationConnection.Open();
                    DataTableReader dtReader = dataTable.CreateDataReader();
                    copier.DestinationTableName = destinationTableName;
                    copier.WriteToServer(dataTable);
                    dtReader.Close();
                    destinationConnection.Close();
                }
            }

            const String recnoParm = "@recno";
            dataTable = Helper.GetOleDbDataTable(sourceConnectionName, String.Format("SELECT RECNO() AS RecNo FROM {0} WHERE DELETED()", sourceTableName));
            using (SqlConnection conn = new SqlConnection(Helper.GetConnectionString(destinationConnectionName)))
            {
                conn.Open();
                foreach (DataRow row in dataTable.Rows)
                {
                    String cmdStr = String.Format("UPDATE {0} SET {1} = 1 WHERE {2} = {3}", destinationTableName, Constants.DILayer.DeletedColumnName, Constants.DILayer.RecnoColumnName, recnoParm);
                    using (SqlCommand cmd = new SqlCommand(cmdStr, conn))
                    {
                        cmd.Parameters.AddWithValue(recnoParm, row[0].ToString());
                        cmd.ExecuteNonQuery();
                    }
                }
                conn.Close();
            }
        }
    }
}
