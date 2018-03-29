using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace VfpToSqlBulkCopy.Utility
{
    public class OleDbSchemaProvider
    {
        public Dictionary<String, OleDbColumnDefinition> GetSchema(String connectionName, String tableName)
        {
            Dictionary<String, OleDbColumnDefinition> schema = new Dictionary<String, OleDbColumnDefinition>();
            using (OleDbConnection conn = new OleDbConnection(Helper.GetConnectionString(connectionName)))
            {
                conn.Open();
                String[] restrictions = new String[] { null, null, tableName, null };
                DataTable  schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, restrictions);

                foreach (DataRow row in schemaTable.Rows)
                {
                    String name = row[Constants.OleDbSchemaColumnNames.Column].ToString();
                    OleDbType dbType = (OleDbType)row[Constants.OleDbSchemaColumnNames.DataType];
                    OleDbColumnDefinition colDef = new OleDbColumnDefinition() { Name = name, Type = dbType };
                    schema.Add(colDef.Name,colDef);
                }
                conn.Close();
            }

            return schema;
        }

    }
}
