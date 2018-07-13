using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VfpToSqlBulkCopy.Utility;
using VfpToSqlBulkCopy.Utility.TableProcessors;
using VfpToSqlBulkCopy.Utility.Events;

namespace VfpToSqlBulkCopy.Utility.Tests
{
    [TestClass]
    public class Experiments
    {
        const String VfpConnectionName = "Host";
        const String SqlConnectionName = "Sql";
        const String TestDbConnectionString = @"Data Source=(local);Initial Catalog=test;Integrated Security=True";

        const String LaptopHostConnectionString = @"Provider=VFPOLEDB.1;Data Source=d:\vfptosql\vhost;Collating Sequence=general;DELETED=False;";


        const String EssexHostConnectionString = @"Provider=VFPOLEDB.1;Data Source=D:\Essex\Hostdema;Collating Sequence=general;DELETED=False;";
        const String EssexSqlConnectionString = @"Data Source=(local);Initial Catalog=Essex_22_000211;Integrated Security=True";

        public TestContext TestContext { get; set; }


        [TestMethod]
        public void TestNullScrubOnTableWithNoCharacterFields()
        {
            const String tableName = "IN_UNILK";
            const String sqlConnStr = "Data Source=(local);Initial Catalog=NoRows_22_000211;Integrated Security=True";

            ITableProcessor tp = new NullCharacterScrubber();
            tp.Process(LaptopHostConnectionString, tableName, sqlConnStr, tableName);
        }

        [TestMethod]
        public void TestNumericScrubProcessorOnLargeTable()
        {
            const String tableName = "DP_CHRVL";
            const String connStr = @"Provider=VFPOLEDB.1;Data Source=I:\Kohler\HOSTDEMO;Collating Sequence=general;DELETED=False;";
            NumericScrubProcessor tp = new NumericScrubProcessor();
            tp.Process(connStr, tableName, null,null);
            foreach (String cmdStr in tp.CommandStrings)
                TestContext.WriteLine(cmdStr);
            TestContext.WriteLine("Here boss");
        }

        [TestMethod]
        public void TestNullCharacterScrubber()
        {
        }

        [TestMethod]
        public void TestSchemaBuilder()
        {
            //ITableProcessor tp = new NumericScrubProcessor();
            //tp.Process(LaptopHostConnectionString, "RS_POLCY",null,null);
            OleDbSchemaProvider sp = new OleDbSchemaProvider();
            Dictionary<String, OleDbColumnDefinition> schema = sp.GetSchema(LaptopHostConnectionString, "RS_POLCY");
            foreach (KeyValuePair<String, OleDbColumnDefinition> kvp in schema)
            {
                TestContext.WriteLine(String.Format("{0} - {1}", kvp.Key, kvp.Value.ToString()));
            }
            TestContext.WriteLine("Here boss");
        }
        [TestMethod]
        public void TestNumericScrubProcessor()
        {
            /*
             * 
             * Here is the VFP code to generate test data
             * 
             * CLOSE DATABASES 
             * USE RS_POLCY Exclusive
             * ZAP 
             * FOR lni = 1 TO 3
               * LOCAL lcI
               * lcI = ALLTRIM(STR(lni)) 
               * SCATTER NAME loRec BLANK 
               * lc = 'loRec.Advance{1} = 1000'
               * lc = STRTRAN(lc,'{1}',lci) 
               * &lc 
               * lc = 'loRec.TimeUnit{1} = 1000'
               * lc = STRTRAN(lc,'{1}',lcI) 
               * &lc 
               * lc = 'loRec.Percent{1} = 100.0'
               * lc = STRTRAN(lc,'{1}',lcI) 
               * &lc 
               * INSERT INTO RS_POLCY FROM NAME loRec 
             * NEXT 
             * 
             * LIST 
             * USE 
             * RETURN 
             */

            NumericScrubProcessor nsp = new NumericScrubProcessor(new ConstantBatchSizeProvider(1));
            nsp.Process(LaptopHostConnectionString, "RS_POLCY", null, null);
            foreach (String cmdStr in nsp.CommandStrings)
            {
                TestContext.WriteLine(cmdStr);
            }
            TestContext.WriteLine("Here boss");
        }


        [TestMethod]
        public void TestPathStuff()
        {
            String dir = @"D:\VFPTOSQL\VHOST";
            String table = "IN_WATRM";
            String fn = Path.Combine(dir, Path.ChangeExtension(table, "DBF"));
            TestContext.WriteLine(fn);

        }

        [TestMethod]
        public void TestAppSettingsKeyNotFound()
        {
            var appSettings = ConfigurationManager.AppSettings;
            String value;
            try
            {
                value = appSettings["WTFO"];
            }
            catch (Exception ex)
            {
                value = ex.ToString();
            }

            Debug.WriteLine(value);


        }


        [TestMethod]
        public void TestUploadStringWithAsciiZero()
        {
            //String backGround = File.ReadAllText(@"C:\Temp\1.pdf");

            //String insertCmdStr = "insert into pdfBackground (background) values (@backGround)";
            //using (SqlConnection sqlConn = new SqlConnection(TestDbConnectionString))
            //{
            //    sqlConn.Open();
            //    using (SqlCommand insertCmd = new SqlCommand(insertCmdStr, sqlConn))
            //    {
            //        insertCmd.Parameters.AddWithValue("@backGround", backGround);
            //        insertCmd.ExecuteNonQuery();
            //    }
            //    sqlConn.Close();
            //}
        }

        [TestMethod]
        public void TestTextFileEventHandler()
        {
            const String fileName = @"c:\temp\events.log";
            BeginUploadEventArgs uploadBeginArgs = new BeginUploadEventArgs();
            uploadBeginArgs.ConnectionStrings = new List<String>() { "connectionString1", "connectionString2" };
            RestartParameter restartParm = new RestartParameter();
            restartParm.TableName = "FUNG";
            uploadBeginArgs.RestartParameter = restartParm;

            IUploadEventHandler uploadEventHandler = new TextFileEventHandler(fileName);
            uploadEventHandler.HandleUploadBegin(null, uploadBeginArgs);

        }


    


        void WriteBoth(String txt)
        {
            String s = DateTime.Now.ToLongTimeString() + " " + txt;
            System.Diagnostics.Debug.WriteLine(s);
            TestContext.WriteLine(s);
        }
    }

    class ConstantBatchSizeProvider : IBatchSizeProvider
    {
        int BatchSize;
        internal ConstantBatchSizeProvider(int batchSize)
        {
            BatchSize = batchSize;
        }
        public int GetBatchSize(string tableName)
        {
            return BatchSize;
        }
    }
}
