using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Diagnostics;

namespace VfpToSqlBulkCopy.Utility.Tests
{
    /// <summary>
    /// Summary description for TestBatchSizeProvider
    /// </summary>
    [TestClass]
    public class TestBatchSizeProvider
    {
        public TestBatchSizeProvider()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestDiBatchSizeProvider()
        {

            String connStr = Helper.GetConnectionString("Host");
            IBatchSizeProvider bsp = new DiTableBatchSizeProvider(connStr);

            TableBatchSizePair pair = GetTableBatchSizePair(">");
            int actualBatchSize = bsp.GetBatchSize(pair.Table);
            Assert.AreEqual(pair.BatchSize, actualBatchSize);

            pair = GetTableBatchSizePair("=");
            actualBatchSize = bsp.GetBatchSize(pair.Table);
            Assert.AreEqual(0, actualBatchSize);

        }

        [TestMethod]
        public void TestDefaultSizeProvider()
        {
            int act = new DefaultBatchSizeProvider().GetBatchSize("WTF");
            int exp = Constants.DefaultBatchSize;
            Assert.AreEqual(exp,act);
        }

        [TestMethod]
        public void TestDiBatchOrDefaultSizeProvider()
        {

            String connStr = Helper.GetConnectionString("Host");
            IBatchSizeProvider testBsp = new DiTableOrDefaultBatchSizeProvider(connStr);
            IBatchSizeProvider di = new DiTableBatchSizeProvider(connStr);
            IBatchSizeProvider def = new DefaultBatchSizeProvider();

            TableBatchSizePair pair = GetTableBatchSizePair(">");
            Assert.AreEqual(testBsp.GetBatchSize(pair.Table), di.GetBatchSize(pair.Table)); 

            pair = GetTableBatchSizePair("=");
            Assert.AreEqual(testBsp.GetBatchSize(pair.Table), def.GetBatchSize(pair.Table));
            Debug.WriteLine("here boss");

        }



        TableBatchSizePair GetTableBatchSizePair(String oper)
        {
            String connStr = Helper.GetConnectionString("Host");
            IBatchSizeProvider bsp = new DiTableBatchSizeProvider(connStr);
            String cmdStr = "SELECT TOP 1 Table,BatchSize FROM DITABLE WHERE BatchSize {0} 0 ORDER BY Table";

            DataTable dt = Helper.GetOleDbDataTable(connStr, String.Format(cmdStr, oper));
            DataRow row = dt.Rows[0];
            return new TableBatchSizePair() { Table = (String)row[0], BatchSize = (int)row[1] };
        }
    }


    class TableBatchSizePair
    {
        public String Table { get; set; }
        public int BatchSize { get; set; }
    }

}
