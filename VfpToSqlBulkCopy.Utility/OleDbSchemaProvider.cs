using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace VfpToSqlBulkCopy.Utility
{
    public class OleDbSchemaProvider
    {
        public Dictionary<String, OleDbColumnDefinition> GetSchema(String connectionString, String tableName)
        {
            Dictionary<String, OleDbColumnDefinition> schema = new Dictionary<String, OleDbColumnDefinition>();
            connectionString = new VfpConnectionStringBuilder(connectionString).ConnectionString;

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                String[] restrictions = new String[] { null, null, tableName, null };
                DataTable  schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, restrictions);

                foreach (DataRow row in schemaTable.Rows)
                {
                    String name = row[Constants.OleDbSchemaColumnNames.Column].ToString();
                    OleDbType dbType = (OleDbType)row[Constants.OleDbSchemaColumnNames.DataType];
                    //if (name.ToUpper().StartsWith("PERCENT"))
                    //{
                    //    object np = row[15];
                    //    object ns = row[16];
                    //}

                    int? numericPrecision;
                    short? numericScale;
                    if (dbType.ToString() == Constants.OleDbTypeNames.Numeric)
                    {
                        numericPrecision = (int?)row[Constants.OleDbSchemaColumnNames.NumericPrecision];
                        numericScale = (short?)row[Constants.OleDbSchemaColumnNames.NumericScale];
                    }
                    else
                    {
                        numericPrecision = null;
                        numericScale = null;
                    }

                    OleDbColumnDefinition colDef = new OleDbColumnDefinition() { Name = name, Type = dbType, NumericPrecision = numericPrecision, NumericScale = numericScale };
                    schema.Add(colDef.Name,colDef);
                }
                conn.Close();
            }

            return schema;
        }

    }
}
