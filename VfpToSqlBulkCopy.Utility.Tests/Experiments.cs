using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VfpToSqlBulkCopy.Utility.Tests
{
    [TestClass]
    public class Experiments
    {
        const String VfpConnectionName = "Host";

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestGetSchema()
        {
            Dictionary<String, OleDbColumnDefinition> schema = new OleDbSchemaProvider().GetSchema(VfpConnectionName, "in_res");
            foreach (KeyValuePair<String, OleDbColumnDefinition> kvp in schema)
            {
                TestContext.WriteLine(kvp.Value.ToString());
            }
        }

        [TestMethod]
        public void TestCommand()
        {
            String cmdStr = "SELECT GuestNum,ResNo,Level,IIF(EMPTY(Cancel),null,Cancel) as Cancel,IIF(DELETED(),1,0) AS SqlDeleted FROM IN_RES";
            cmdStr = "select MSNUMB, MSTYPE, MSGNUM, MSRNUM, IIF(EMPTY(MSDATE),null,MSDATE) as MSDATE, MSTIME, MSUSER, IIF(EMPTY(MSCLDATE),null,MSCLDATE) as MSCLDATE, MSCLTIME, MSCLUSER, MSPRINT, MSTO, MSFLAG, MSNOTTYP, IIF(EMPTY(MSBEGIN),null,MSBEGIN) as MSBEGIN, MSDAY, MSBEGTIME, MSENDTIME, MSTXT, MSPRI, MSSTAT, MSIPADDR, MSAUTOCLS, IIF(DELETED(),1,0) AS SqlDeleted from IN_MSG";
            
            using (OleDbConnection conn = new OleDbConnection(Helper.GetConnectionString(VfpConnectionName)))
            {
                using (OleDbCommand cmd = new OleDbCommand(cmdStr, conn))
                {
                    conn.Open();
                    DataTable dt;
                    dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    conn.Close();
                    TestContext.WriteLine("HH");

                }

            }

        }

    }
}
