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
        public void TestNumericScrubProcessor()
        {
            //ITableProcessor tp = new NumericScrubProcessor();
            //tp.Process(LaptopHostConnectionString, "RS_POLCY",null,null);
            NumericScrubProcessor tp = new NumericScrubProcessor();
            tp.Process(LaptopHostConnectionString, "RS_POLCY", null, null);
            foreach (String cmdStr in tp.CommandStrings)
            {
                TestContext.WriteLine(cmdStr);
            }
            TestContext.WriteLine("Here boss");
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
        public void TestReadNumericOverlows()
        {
            //ITableProcessor tp = new NumericScrubProcessor();
            //tp.Process(LaptopHostConnectionString, "RS_POLCY",null,null);
            OleDbSchemaProvider sp = new OleDbSchemaProvider();
            Dictionary<String, OleDbColumnDefinition> schema = sp.GetSchema(LaptopHostConnectionString, "RS_POLCY");
            //DataTable dt = Helper.GetOleDbDataTable(LaptopHostConnectionString, "SELECT IIF(BETWEEN(Percent1,99.99,-99.99),Percent1,0) AS Percent1,Percent2, Percent3 FROM RS_POLCY");
            //DataRow dr = dt.Rows[0];
            
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
            uploadEventHandler.HandleUploadBegin(null,uploadBeginArgs);

        }


        [TestMethod]
        public void TestLinkedServerUpload()
        {
            String dataSource = @"D:\GDPR\VHOST\";
            //String dataSource = @"D:\Essex\HostDema\";
            String tableName = "IN_MSG";
            String dropTable = "if exists(SELECT Table_Name FROM test.INFORMATION_SCHEMA.Tables where Table_Name = 'wtf')   drop table test.dbo.wtf";
            String dropLinkedServer = "if exists(select * from sys.servers where name = N'VFPTOSQL') EXEC master.sys.sp_dropserver 'VFPTOSQL','droplogins'";
            String addLinkedServer = String.Format(@"EXEC master.dbo.sp_addlinkedserver @server = N'VFPTOSQL', @srvproduct=N'', @provider=N'VFPOLEDB', @datasrc=N'{0}'", dataSource);
            String uploadData = String.Format("select * into test.dbo.wtf from OpenQuery(VFPTOSQL,'SELECT * FROM {0}')", tableName);


            using (SqlConnection sqlConn = new SqlConnection(TestDbConnectionString))
            {
                sqlConn.Open();
                // Drop Table
                using (SqlCommand sqlCmd = new SqlCommand(dropTable, sqlConn))
                {
                    sqlCmd.ExecuteNonQuery();
                }
                // Drop LinkedServer 
                using (SqlCommand sqlCmd = new SqlCommand(dropLinkedServer, sqlConn))
                {
                    sqlCmd.ExecuteNonQuery();
                }
                // Add LinkedServer 
                using (SqlCommand sqlCmd = new SqlCommand(addLinkedServer, sqlConn))
                {
                    sqlCmd.ExecuteNonQuery();
                }

                // Upload
                using (SqlCommand sqlCmd = new SqlCommand(uploadData, sqlConn))
                {
                    sqlCmd.CommandTimeout = 0;
                    sqlCmd.ExecuteNonQuery();
                }

                sqlConn.Close();
            }
        }



        void WriteBoth(String txt)
        {
            String s = DateTime.Now.ToLongTimeString() + " " + txt;
            System.Diagnostics.Debug.WriteLine(s);
            TestContext.WriteLine(s);
        }
    }
}
