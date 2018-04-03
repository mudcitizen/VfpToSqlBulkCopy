using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VfpToSqlBulkCopy.Utility.Tests
{
    [TestClass]
    public class Experiments
    {
        const String VfpConnectionName = "Host";
        const String SqlConnectionName = "Sql";

        const String LaptopHostConnectionString = @"Provider=VFPOLEDB.1;Data Source=d:\vfptosql\vhost;Collating Sequence=general;DELETED=False;";


        const String EssexHostConnectionString = @"Provider=VFPOLEDB.1;Data Source=D:\Essex\Hostdema;Collating Sequence=general;DELETED=False;";
        const String EssexSqlConnectionString = @"Data Source=(local);Initial Catalog=Essex_22_000211;Integrated Security=True";

        public TestContext TestContext { get; set; }


        [TestMethod]
        public void TestWhatever()
        {

            String thisTable = "MS_W";
            String restartTable = "PSCHK";
            int intResult = String.Compare(thisTable, restartTable, StringComparison.CurrentCulture);

            char[] thisArray = thisTable.ToCharArray();
            char[] restartArray = restartTable.ToCharArray();

            int iterCount = thisArray.Length < restartArray.Length ? thisArray.Length : restartArray.Length;
            Boolean greaterOrEqual = true;
            for (int i = 0; i < iterCount; i++)
            {
                if ((greaterOrEqual) && (thisArray[i] < restartArray[i]))
                {
                    greaterOrEqual = false;
                    break;
                }
            }

            TestContext.WriteLine(String.Format("GreaterOrEqual - {0}", greaterOrEqual));
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
        void WriteBoth(String txt)
        {
            String s = DateTime.Now.ToLongTimeString() + " " + txt;
            System.Diagnostics.Debug.WriteLine(s);
            TestContext.WriteLine(s);
        }
    }
}
