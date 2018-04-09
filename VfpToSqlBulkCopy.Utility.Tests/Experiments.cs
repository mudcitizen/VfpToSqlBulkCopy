using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vfptosqlbulkcopy;

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
        public void TestPathStuff()
        {
            String dir = @"D:\VFPTOSQL\VHOST";
            String table = "IN_WATRM";
            String fn = Path.Combine(dir, Path.ChangeExtension(table, "DBF"));
            TestContext.WriteLine(fn);

        }

        [TestMethod]
        public void TestCOMClient()
        {

            Ivfptosqlbulkcopy com = new vfptosqlbulkcopyClass();
            const String tableIn = @"D:\VHOST\IN_WATRM.DBF";
            const String folderOut = @"C:\TEMP\MEMOOUT\";
            const String tableOut = "MemoProblems";


            String result = com.ListMemos(tableIn, folderOut, tableOut);

            foreach (String fn in Directory.GetFiles(folderOut))
            {
                TestContext.WriteLine(fn);
            }


        }


        [TestMethod]
        public void TestDataTableWithNullChrInMemo()
        {

            const String sqlConnectionString = @"Data Source=(local);Initial Catalog=test;Integrated Security=True";

            String cmdStr = "SELECT RECNO() as RecNo, Background,len(background) FROM IN_WATRM";
            const String vfpConnectionString = @"Provider=VFPOLEDB.1;Data Source=D:\VfpToSql\vhost;Collating Sequence=general;DELETED=False;NULL=YES";
            DataTable result = null;
            using (OleDbConnection conn = new OleDbConnection(vfpConnectionString))
            {
                using (OleDbCommand cmd = new OleDbCommand(cmdStr, conn))
                {
                    conn.Open();
                    result = new DataTable();
                    result.Load(cmd.ExecuteReader());
                    foreach (DataRow row in result.Rows)
                    {
                        String insertCmdStr = "insert into test (background) values (@back)";
                        using (SqlConnection sqlConn = new SqlConnection(sqlConnectionString))
                        {
                            sqlConn.Open();
                            using (SqlCommand insertCmd = new SqlCommand(insertCmdStr, sqlConn))
                            {
                                int strLen = row[1].ToString().Length;
                                String bkgrnd = row[1].ToString();
                                int bkgrndLen = bkgrnd.Length;
                                insertCmd.Parameters.AddWithValue("@back", row[1]);
                                insertCmd.ExecuteNonQuery();
                            }
                            sqlConn.Close();
                        }
                    }
                    conn.Close();
                }
            }

            Helper.ExecuteSqlNonQuery(EssexSqlConnectionString, "DELETE FROM IN_MISC");
            TableUploader tp = new TableUploader();
            tp.Upload(EssexHostConnectionString, "in_misc", EssexSqlConnectionString);
            WriteBoth("Uploaded IN_MISC");
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
