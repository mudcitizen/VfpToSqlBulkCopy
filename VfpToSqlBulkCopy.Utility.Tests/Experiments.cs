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
        const String SqlConnectionName = "Sql";

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
        public void TestWhatever()
        {

            const String connName = SqlConnectionName;
            ICommandStringProvider csp = new UpdateDateCommandStringProvider();
            String s = csp.GetCommandString(connName, "IN_MSG");
            if (!String.IsNullOrEmpty(s))
            {
                TestContext.WriteLine(s);
                Helper.ExecuteSqlNonQuery(connName, s);
            }

        }

    }
}
