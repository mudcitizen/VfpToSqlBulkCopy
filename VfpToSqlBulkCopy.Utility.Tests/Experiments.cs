using System;
using System.Collections.Generic;
using System.Configuration;
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

            ConnectionStringSettings css = ConfigurationManager.ConnectionStrings["host"];
            String s;
            if (css == null)
            {
                s = "Null boss";
            }
            else
            {

                OleDbConnectionStringBuilder bldr = new OleDbConnectionStringBuilder(css.ConnectionString);
                Object ds;
                bldr.TryGetValue("Data Source", out ds);
                s = String.Format("Name - {0} ; String - {1} ; DataSource - {2}",css.Name,css.ConnectionString,ds);



            }
            TestContext.WriteLine(s);
        }

    }
}
