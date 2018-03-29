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

            // Deleted 
            //DataTable dataTable = null;
            //using (OleDbConnection sourceConnection = new OleDbConnection(Helper.GetConnectionString(sourceConnectionName)))
            //{
            //    using (OleDbCommand dbcmd = new OleDbCommand(selectCommandString,sourceConnection))
            //    {
            //        sourceConnection.Open();
            //        dataTable = new DataTable();
            //        dataTable.Load(dbcmd.ExecuteReader());
            //        sourceConnection.Close();
            //    }
            //}

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
        }
    }
}
