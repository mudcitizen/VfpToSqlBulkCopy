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
        [TestMethod]
        public void TestAllPOS()
        {
            RestartParameter parm = new RestartParameter() { ConnectionName = POSConnection, TableName = "" };
            bool result = parm.SatisfiesFilter(POSConnection, "ABC");
            Assert.IsTrue(result);

        }

        [TestMethod]
        public void TestTableNameWithUnderBar()
        {
            /*

            The code in RestartParameter does character-by-character comparison - which
             will give the expected result only if we use machine collation
              
             In VFP 
             - SELECT table from ditable WHERE table like 'SY%' 
             - INDEX ON table TAG generalCol COLLATE 'general'
             - LIST the SY_ rows are at the top of the list 
              Record#  TABLE     
                  10  SY_COMM   
                  11  SY_LOG    
                   2  SYCCTYP   
                  12  SYCFGCHD  
                  13  SYCFGCHH  
                  
                 
             - INDEX ON table TAG MachineCol COLLATE 'machine'
             - LIST the SY_ rows are at the bottom of the list 
               Record#  TABLE     
                21  SYSHRESC  
                22  SYSHRHDR  
                9   SYSTEM    
                10  SY_COMM   
                11  SY_LOG    

             */

            const String tableRoot = "SYCFGCH";
            RestartParameter parm = new RestartParameter();
            parm.ConnectionName = Constants.ConnectionNames.Host;
            parm.TableName = tableRoot + "H";

            Assert.IsFalse(parm.SatisfiesFilter(Constants.ConnectionNames.Host, tableRoot + "D"));
            Assert.IsTrue(parm.SatisfiesFilter(Constants.ConnectionNames.Host, parm.TableName));
            Assert.IsTrue(parm.SatisfiesFilter(Constants.ConnectionNames.Host, tableRoot + "I"));
            Assert.IsTrue(parm.SatisfiesFilter(Constants.ConnectionNames.Host, "SY_LOG"));

            // 

        }
    }
}
