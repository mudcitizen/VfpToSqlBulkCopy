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
        public void Upload(String sourceTableName, String destinationTableName, String sourceConnectionName, String destinationConnectionName)
        {

            DataTable dataTable = null;

            // Date Null Scrub
            // Deleted 

            using (OleDbConnection sourceConnection = new OleDbConnection(Helper.GetConnectionString(sourceConnectionName)))
            {
                using (OleDbCommand dbcmd = new OleDbCommand(String.Format("SELECT * FROM {0}", sourceTableName)))
                {
                    sourceConnection.Open();
                    dataTable = new DataTable();
                    dataTable.Load(dbcmd.ExecuteReader());
                    sourceConnection.Close();
                }
            }

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
