using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VfpToSqlBulkCopy.Utility.Tests
{
    [TestClass]
    public class TestTableProcessor
    {
        const String VfpConnectionString = @"Provider=VFPOLEDB.1;Data Source=D:\VfpToSql\vhost;Collating Sequence=general;DELETED=False;";
        const String SqlConnectionString = @"Data Source=(local);Initial Catalog=NoRows_22_000211;Integrated Security=True";

        public TestContext TestContext { get; set; }


        [TestMethod]
        public void TestBasicUpload()
        {
            const String tableName = "IN_MSG";
            String getCountCommandString = "SELECT COUNT(*) FROM " + tableName;
            int vfpRowCount = Convert.ToInt32(Helper.GetOleDbScaler(VfpConnectionString, getCountCommandString));
            int vfpNonDeletedRowCount = Convert.ToInt32(Helper.GetOleDbScaler(VfpConnectionString, getCountCommandString + " WHERE NOT DELETED()"));
            int vfpDeletedRowCount = Convert.ToInt32(Helper.GetOleDbScaler(VfpConnectionString, getCountCommandString + " WHERE DELETED()"));
            Assert.IsTrue(vfpNonDeletedRowCount > 0, "Expected 1 or more non-deleted rows");
            Assert.IsTrue(vfpDeletedRowCount > 0, "Expected 1 or more deleted rows");

            // Clear out the SQL table
            Helper.ExecuteSqlNonQuery(SqlConnectionString, "DELETE FROM " + tableName);
            Assert.AreEqual(0, (int)Helper.GetSqlScaler(SqlConnectionString, getCountCommandString));

            // Import from VFP
            TableProcessor tp = new TableProcessor();
            tp.Upload(VfpConnectionString, tableName, SqlConnectionString);

            // Make sure rowcounts are correct
            int actualRowCount = Convert.ToInt32(Helper.GetSqlScaler(SqlConnectionString, getCountCommandString));
            int expectedRowCount = Convert.ToInt32(Helper.GetOleDbScaler(VfpConnectionString, getCountCommandString));
            Assert.AreEqual(actualRowCount, expectedRowCount);

            // Check handling of Deleted() 
            int sqlDeletedRowCount = Convert.ToInt32(Helper.GetSqlScaler(SqlConnectionString, getCountCommandString + String.Format(" WHERE {0} = 1",Constants.DILayer.DeletedColumnName)));
            Assert.AreEqual(sqlDeletedRowCount, vfpDeletedRowCount);
        }

        [TestMethod]
        public void TestUploadOfTableWithDateFieldsThatAreReservedWords()
        {
            TableProcessor tp = new TableProcessor();
            tp.Upload(VfpConnectionString, "SGIBKHDR", SqlConnectionString);
            TestContext.WriteLine("TestUploadOfTableWithDateFieldsThatAreReservedWords complete");
        }

        [TestMethod]
        public void TestBasicCommandStringProvider()
        {
            const string tableName = "IN_MSG";
            ICommandStringProvider csp = new SelectCommandStringProvider();
            String actual = csp.GetCommandString(VfpConnectionString, tableName);
            String upperActual = actual.ToUpper();

            Assert.IsTrue(upperActual.StartsWith("SELECT "));
            Assert.IsTrue(upperActual.Replace(" ",String.Empty).EndsWith("FROM"+tableName));

            Dictionary<String, OleDbColumnDefinition> schema = new OleDbSchemaProvider().GetSchema(VfpConnectionString, tableName);

            foreach (KeyValuePair<String, OleDbColumnDefinition> kvp in schema)
            {
                Assert.IsTrue(upperActual.Contains(kvp.Value.Name + ","));
            }

            // Can we run it?  Turns out - we can't.  VFP raises Error # 1890
            // if you issued SELECT IIF(EMPTY(mscldate),null,mscldate) FROM in_msg
            // on the Laptop
            DataTable dt = Helper.GetOleDbDataTable(VfpConnectionString, actual);

            TestContext.WriteLine(actual);
           

        }
    }
}
