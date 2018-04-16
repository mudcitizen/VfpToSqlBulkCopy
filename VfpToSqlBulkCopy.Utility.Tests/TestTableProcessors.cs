using System;
using System.Data.SqlClient;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VfpToSqlBulkCopy.Utility.Tests
{
    [TestClass]
    public class TestTableProcessors
    {

        const String TableName = "TestTable";
        const String DbName = "NoRows_22_000211";
        String DropTableCommandString => "DROP TABLE " + GetQualifiedTableName();

        public TestContext TestContext { get; set; }

  


        [TestInitialize]
        public void Setup()
        {
            //String cmd = String.Format("IF EXISTS(SELECT Table_Name FROM {0}.INFORMATION_SCHEMA"
            StringBuilder sb = new StringBuilder();
            String checkSchemaCmd = String.Format("SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{1}'", DbName, TableName);
            sb.AppendLine("IF EXISTS( " + checkSchemaCmd + ") " + DropTableCommandString);
            sb.AppendLine("CREATE TABLE {0}.DBO.{1} (RecNo int, DateFld date, SqlDeleted bit)");
            for (int i = 1; i <= 10; i++)
            {
                String dateVal = ((i % 2) == 0) ? Constants.SqlDateMinValue : "20180101";
                sb.AppendLine(String.Format("INSERT INTO {0}.DBO.{1} (RecNo,DateFld,SqlDeleted) VALUES ({2},'{3}',0)", DbName, TableName, Convert.ToString(i), dateVal));
            }

            String cmds = String.Format(sb.ToString(), DbName, TableName);
            String connStr = GetConnectionString();
            Helper.ExecuteSqlNonQuery(connStr, cmds);
            int rows = Convert.ToInt32(Helper.GetSqlScaler(connStr, checkSchemaCmd));
            Assert.AreEqual(1, rows);
        }

        [TestCleanup]
        public void Teardown()
        {
        }

        [TestMethod]
        public void TestZapProcessor()
        {
            // Make sure there are rows
            String cmdStr = "SELECT COUNT(*) FROM " + GetQualifiedTableName();
            Assert.IsTrue(GetRowCount(cmdStr) > 0);
            ITableProcessor zapper = new ZapProcessor();
            zapper.Process(null, null, GetConnectionString(), TableName);
            Assert.IsTrue(GetRowCount(cmdStr) == 0);
        }

        [TestMethod]
        public void TestNullDateProcessor()
        {
            // Make sure there are rows
            String cmdStr = "SELECT COUNT(*) FROM " + GetQualifiedTableName() + " WHERE DateFld is Null" ;
            Assert.IsTrue(GetRowCount(cmdStr) == 0);
            ITableProcessor tableProcessor  = new NullDateProcessor();
            tableProcessor.Process(null, null, GetConnectionString(), TableName);
            Assert.IsTrue(GetRowCount(cmdStr) > 0);
        }


        String GetQualifiedTableName()
        {
            return DbName + ".dbo." + TableName;
        }

        String GetConnectionString()
        {
            SqlConnectionStringBuilder bldr = new SqlConnectionStringBuilder();
            bldr.DataSource = "(local)";
            bldr.InitialCatalog = DbName;
            bldr.IntegratedSecurity = true;
            return bldr.ConnectionString;
        }

        int GetRowCount(String cmdStr)
        {
            String connStr = GetConnectionString();
            return Convert.ToInt32(Helper.GetSqlScaler(connStr, cmdStr));

        }


    }
}
