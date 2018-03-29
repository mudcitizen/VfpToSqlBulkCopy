using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VfpToSqlBulkCopy.Utility.Tests
{
    [TestClass]
    public class Experiments
    {
        const String VfpConnectionName = "Host";

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestGetSchema()
        {
            Dictionary<String, OleDbColumnDefinition> schema = new OleDbSchemaProvider().GetSchema(VfpConnectionName, "in_res");
            foreach (KeyValuePair<String, OleDbColumnDefinition> kvp in schema)
            {
                TestContext.WriteLine(kvp.Value.ToString());
            }
            //using (OleDbConnection conn = new OleDbConnection(Helper.GetConnectionString(VfpConnectionName)))
            //{
            //    conn.Open();
            //    DataTable schemaTable, dataTypes;
            //    String[] restrictions = new String[] {null,null,"in_res",null};

            //    schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, restrictions);
            //    dataTypes = conn.GetOleDbSchemaTable(OleDbSchemaGuid.DbInfoLiterals, null);

            //    const String ColumnColumnName = "COLUMN_NAME";
            //    const String DataTypeColumnName = "DATA_TYPE";

            //    foreach (DataRow row in schemaTable.Rows)
            //    {
            //        OleDbType dbType = (OleDbType)row[DataTypeColumnName];
            //        String s = String.Format("Column - {0} ; DataType - {1} ; DataTypeName - {2}", row[ColumnColumnName].ToString(), row[DataTypeColumnName].ToString(),dbType);
            //        TestContext.WriteLine(s);
            //    }

            //    TestContext.WriteLine("whoo hoo");
            //}
        }

    }
}
