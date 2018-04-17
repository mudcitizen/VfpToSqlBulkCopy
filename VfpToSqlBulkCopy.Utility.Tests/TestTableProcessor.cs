using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VfpToSqlBulkCopy.Utility.Tests
{
    [TestClass]
    public class TestTableProcessor
    {
        const String bogusVfpConnectionString = @"Provider=VFPOLEDB.1;Data Source=D:\VfpToSql\vhost;Collating Sequence=general";
        String VfpConnectionString
        { get
            {
                VfpConnectionStringBuilder bldr = new VfpConnectionStringBuilder(bogusVfpConnectionString);
                return bldr.ConnectionString;
            }
        }
            
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
            tp.Process(bogusVfpConnectionString, tableName, SqlConnectionString,tableName);

            // Make sure rowcounts are correct
            int actualRowCount = Convert.ToInt32(Helper.GetSqlScaler(SqlConnectionString, getCountCommandString));
            int expectedRowCount = Convert.ToInt32(Helper.GetOleDbScaler(VfpConnectionString, getCountCommandString));
            Assert.AreEqual(actualRowCount, expectedRowCount);

            // Check handling of Deleted() 
            int sqlDeletedRowCount = Convert.ToInt32(Helper.GetSqlScaler(SqlConnectionString, getCountCommandString + String.Format(" WHERE {0} = 1",Constants.DILayer.DeletedColumnName)));
            Assert.AreEqual(sqlDeletedRowCount, vfpDeletedRowCount);

            // Check for nullDates
            String nullDateRowCountCmdStr = getCountCommandString + " WHERE mscldate is null or msBegin is null or msDate is null";
            int nullDateRowCount = Convert.ToInt32(Helper.GetSqlScaler(SqlConnectionString, nullDateRowCountCmdStr));
            Assert.IsTrue(nullDateRowCount > 0);
            // Check for non nullDates
            nullDateRowCountCmdStr = getCountCommandString + " WHERE mscldate is not null or msBegin is not null or msDate is not null";
            nullDateRowCount = Convert.ToInt32(Helper.GetSqlScaler(SqlConnectionString, nullDateRowCountCmdStr));
            Assert.IsTrue(nullDateRowCount > 0);


        }

        [TestMethod]
        public void TestUploadOfTableWithDateFieldsThatAreReservedWords()
        {
            TableProcessor tp = new TableProcessor();
            const String tableName = "SGIBKHDR";
            tp.Process(VfpConnectionString, tableName, SqlConnectionString,tableName);
            TestContext.WriteLine("TestUploadOfTableWithDateFieldsThatAreReservedWords complete");
        }

        [TestMethod]
         public void TestUploadithAsciiZero()
        {
            const String tableName = "IN_WATRM";
            Helper.ExecuteSqlNonQuery(SqlConnectionString,"DELETE FROM " + tableName);

            TableProcessor tp = new TableProcessor();
            tp.Process(VfpConnectionString, tableName, SqlConnectionString,tableName);

            DataTable dt = Helper.GetSqlDataTable(SqlConnectionString, "select charindex(char(0),cast(background as varchar(max))) from " + tableName);
            foreach (DataRow row in dt.Rows)
                Assert.IsTrue(Convert.ToInt32(row[0]) > 0);

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
                Assert.IsTrue(upperActual.Contains(kvp.Value.Name + ",") || upperActual.Contains(kvp.Value.Name + " "));
            }

            // Can we run it?  Turns out - we can't.  VFP raises Error # 1890
            // if you issued SELECT IIF(EMPTY(mscldate),null,mscldate) FROM in_msg
            // on the Laptop
            DataTable dt = Helper.GetOleDbDataTable(VfpConnectionString, actual);

            TestContext.WriteLine(actual);
           
        }

        [TestMethod]
        public void TestBatchSizeReset()
        {
            TableUploader tu = new TableUploader();
            int batchSizeBefore = tu.GetBatchSize();
            const String tableName = "IN_WMAIL";
            tu.Process(VfpConnectionString, tableName, SqlConnectionString, tableName);
            int batchSizeAfter = tu.GetBatchSize();
            Assert.IsTrue(batchSizeAfter > 0);
            Assert.IsTrue(batchSizeAfter != batchSizeBefore); 
        }

        [TestMethod]
        public void TestHelperGetDestinationTableName()
        {
            String tableNameIn = "TA-AGT";
            String expected = "TA_AGT";
            String actual = Helper.GetDestinationTableName(tableNameIn);
            Assert.AreNotEqual(actual, tableNameIn);
            Assert.AreEqual(actual, expected);
            actual = Helper.GetDestinationTableName(expected);
            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void TestEssexInWMailUpload()
        {
            VfpConnectionStringBuilder vfpConnStrBldr = new VfpConnectionStringBuilder(VfpConnectionString);
            vfpConnStrBldr.DataSource = @"D:\Essex\Hostdema\";
            String vfpConnStr = vfpConnStrBldr.ConnectionString;

            String sqlConnStr = "Data Source = (local); Initial Catalog = Essex_22_000211; Integrated Security = True";
            const String tableName = "IN_WMAIL";
            Helper.ExecuteSqlNonQuery(sqlConnStr, "DELETE FROM " + tableName);

            string expectedRecordCountCmdStr = "SELECT COUNT(*) FROM " + tableName;
            int expectedRecordCount = Convert.ToInt32(Helper.GetOleDbScaler(vfpConnStr, expectedRecordCountCmdStr));

            TableUploader tu = new TableUploader();
            tu.Process(vfpConnStr, tableName, sqlConnStr, tableName);
            int actualRecordCount = Convert.ToInt32(Helper.GetSqlScaler(sqlConnStr, expectedRecordCountCmdStr));

            Assert.AreEqual(expectedRecordCount, actualRecordCount);

        }

    }
}
