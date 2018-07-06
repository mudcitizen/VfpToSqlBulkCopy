using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VfpToSqlBulkCopy.Utility.Tests
{
    [TestClass]
    public class TestRestartParameter
    {
        readonly String HostConnection = Constants.ConnectionNames.Host;
        readonly String POSConnection = Constants.ConnectionNames.POS;

        [TestMethod]
        public void TestStartsWith()
        {
            RestartParameter parm = new RestartParameter() { ConnectionName = HostConnection, TableName = "IN_" };
            Assert.IsFalse(parm.SatisfiesFilter(HostConnection, "AC_TRN"));

            Assert.IsTrue(parm.SatisfiesFilter(HostConnection, "IN_"));
            Assert.IsTrue(parm.SatisfiesFilter(HostConnection, "IN_GUEST"));
            Assert.IsTrue(parm.SatisfiesFilter(HostConnection, "WO_TASK"));
        }

        [TestMethod]
        public void TestRestartHostProcessingPOS()
        {
            RestartParameter parm = new RestartParameter() { ConnectionName = HostConnection, TableName = "IN_GUEST" };
            Assert.IsFalse(parm.SatisfiesFilter(HostConnection, "AC_TRN"));
            Assert.IsTrue(parm.SatisfiesFilter(HostConnection, "IN_RES"));
            Assert.IsTrue(parm.SatisfiesFilter(POSConnection, "PS_BANK")); 
        }
        [TestMethod]
        public void TestRestartPosProcessingHost()
        {
            RestartParameter parm = new RestartParameter() { ConnectionName = POSConnection, TableName = "PSCHK" };
            Assert.IsFalse(parm.SatisfiesFilter(HostConnection, "AC_TRN"));
            Assert.IsFalse(parm.SatisfiesFilter(HostConnection, "RS_SKED"));
            Assert.IsTrue(parm.SatisfiesFilter(POSConnection, "PS_WTF"));
        }
        [TestMethod]
        public void TestRestartPos()
        {
            RestartParameter parm = new RestartParameter() { ConnectionName = POSConnection, TableName = "PSINDX" };
            TableNameProvider tnp = new TableNameProvider(Helper.GetConnectionString(HostConnection), parm.SatisfiesFilter);
            IEnumerable<String> tables = tnp.GetTables(POSConnection, Helper.GetConnectionString(POSConnection));
            foreach (String table in tables)
            {
                Debug.WriteLine(table);
            }
            Debug.WriteLine("here boss");

        }
    }
}
