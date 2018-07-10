using System;
using System.Data.SqlClient;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VfpToSqlBulkCopy.Utility.TableProcessors;

namespace VfpToSqlBulkCopy.Utility.Tests
{
    [TestClass]
    public class TestTableProcessors
    {

        const String TableName = "TestTable";
        const String DbName = "NoRows_22_000211";
        String DropTableCommandString => "DROP TABLE " + GetQualifiedTableName();

        public TestContext TestContext { get; set; }

        const String CharFieldConstant = "The rain in spain";

        [TestInitialize]
        public void Setup()
        {
            //String cmd = String.Format("IF EXISTS(SELECT Table_Name FROM {0}.INFORMATION_SCHEMA"

            StringBuilder sb = new StringBuilder();
            String checkSchemaCmd = String.Format("SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{1}'", DbName, TableName);
            sb.AppendLine("IF EXISTS( " + checkSchemaCmd + ") " + DropTableCommandString);
            sb.AppendLine("CREATE TABLE {0}.DBO.{1} (RecNo int, DateFld date, SqlDeleted bit,charFld char(20) not null, SqlRecNo int IDENTITY(1,1))");
            for (int i = 1; i <= 10; i++)
            {
                String dateVal = ((i % 2) == 0) ? Constants.SqlDateMinValue : "20180101";
                String chrVal = ((i % 2) == 0) ? CharFieldConstant : CharFieldConstant.Replace(' ', '\0');
                //String chrVal = CharFieldConstant ;
                sb.AppendLine(String.Format("INSERT INTO {0}.DBO.{1} (RecNo,DateFld,SqlDeleted,charfld) VALUES ({2},'{3}',0,'{4}')", DbName, TableName, Convert.ToString(i), dateVal,chrVal));
            }

            String cmds = String.Format(sb.ToString(), DbName, TableName);
            System.IO.File.WriteAllText(@"C:\Temp\test.sql", cmds);
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
        public void TestNullCharacterScrubber()
        {
            // Make sure there are rows

            String baseCmdStr = "SELECT COUNT(*) FROM " + GetQualifiedTableName() + " WHERE CHARINDEX(CHAR(0),CharFld) {0} 0";
            String hasNullsCmdStr = String.Format(baseCmdStr, ">");
            String noNullsCmdStr = String.Format(baseCmdStr, "=");
            Assert.IsTrue(GetRowCount(hasNullsCmdStr) > 0);
            Assert.IsTrue(GetRowCount(noNullsCmdStr) > 0);
            ITableProcessor tp = new NullCharacterScrubber();
            tp.Process(null, null, GetConnectionString(), TableName);
            Assert.IsTrue(GetRowCount(hasNullsCmdStr) == 0);
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
