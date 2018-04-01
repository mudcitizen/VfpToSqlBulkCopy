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
        public void TestBatch()
        {

            const String HostConnectionString = @"Provider=VFPOLEDB.1;Data Source=D:\Essex\Hostdema;Collating Sequence=general;DELETED=False;";
            const String SqlConnectionString = @"Data Source=(local);Initial Catalog=Essex_22_000211;Integrated Security=True";

            const String restoreSqlDb = @"USE [master] RESTORE DATABASE [Essex_22_000211] FROM  DISK = N'D:\ESSEX\HOSTDEMA\Essex_22_000211.bak' WITH  FILE = 1,  NOUNLOAD,  STATS = 5";
            Helper.ExecuteSqlNonQuery(SqlConnectionString, restoreSqlDb);


            ITableNameProvider tableNameProvider = new TableNameProvider(HostConnectionString);
            IEnumerable<String> tableNames = tableNameProvider.GetTables(Constants.ConnectionNames.Host, HostConnectionString);

            foreach (String tableName in tableNames)
            {
                UploadTable(tableName,HostConnectionString,SqlConnectionString);
            }


        }

        [TestMethod]
        public void TestFung()
        {
            TableProcessor tp = new TableProcessor();
            tp.Upload(EssexHostConnectionString, "FUNG", EssexSqlConnectionString);
        }

        void UploadTable(String sourceTableName, String HostConnectionString,String SqlConnectionString)
        {

            String destinationTableName = sourceTableName.Trim().ToUpper().Replace('-', '_');
            Helper.ExecuteSqlNonQuery(SqlConnectionString, "delete from " + destinationTableName);

            const int batchSize = 50000;
            int recordCount = Convert.ToInt32(Helper.GetOleDbScaler(HostConnectionString, "SELECT COUNT(*) FROM " + sourceTableName));


            WriteBoth("UploadTable - " + sourceTableName + " - Begin");
            using (SqlConnection destinationConnection = new SqlConnection(SqlConnectionString))
            {
                destinationConnection.Open();

                using (OleDbConnection sourceConnection = new OleDbConnection(HostConnectionString))
                {
                    sourceConnection.Open();

                    int minRecno, maxRecno, recsUploaded;
                    recsUploaded = 0;

                    DataTable dataTable = null;
                    while (true)
                    {
                        minRecno = recsUploaded;
                        maxRecno = minRecno + batchSize;

                        // Pull rows from VFP
                        String cmdStr = String.Format("SELECT * FROM {0} WHERE RECNO() > {1} AND RECNO() <= {2}", sourceTableName, Convert.ToString(minRecno), Convert.ToString(maxRecno));
                        dataTable = Helper.GetOleDbDataTable(HostConnectionString, cmdStr);
                        recsUploaded = recsUploaded + dataTable.Rows.Count;

                        // Push rows to SQL 
                        using (SqlBulkCopy copier = new SqlBulkCopy(destinationConnection))
                        {
                            DataTableReader dtReader = dataTable.CreateDataReader();
                            copier.BulkCopyTimeout = 0;
                            copier.DestinationTableName = destinationTableName;
                            copier.WriteToServer(dataTable);
                            dtReader.Close();
                        }

                        if (recsUploaded >= recordCount)
                            break;
                    }

                    sourceConnection.Close();
                }

                destinationConnection.Close();

            }

            WriteBoth("UploadTable - " + sourceTableName + " - End");
        }

        void WriteBoth(String txt)
        {
            String s = DateTime.Now.ToLongTimeString() +" " + txt;
            System.Diagnostics.Debug.WriteLine(s);
            TestContext.WriteLine(s);
        }
    }
}
