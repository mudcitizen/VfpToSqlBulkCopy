using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VfpToSqlBulkCopy.Utility.Tests
{
    [TestClass]
    public class TestVfpConnectionStringBuilder
    {
        TestContext TestContext { get; set; }

        [TestMethod]
        public void TestConnStringHasDeleted()
        {
            IList<String> clauses = new List<String>()
            {
                "Provider=VFPOLEDB.1",
                @"Data Source=D:\VfpToSql\vhost",
                "Collating Sequence=general"
            };
            String delim = ";";
            StringBuilder sb = new StringBuilder();
            foreach (String clause in clauses)
            {
                sb.Append(delim + clause);
                delim = ";";
            }
            String connStr = sb.ToString();


            OleDbConnectionStringBuilder oleDbBldr = new OleDbConnectionStringBuilder(connStr);
            String ds = oleDbBldr.DataSource;
            Assert.IsFalse(String.IsNullOrEmpty(ds));

            VfpConnectionStringBuilder vfpBldr = new VfpConnectionStringBuilder(connStr);
            String actual = vfpBldr.ConnectionString.Replace(" ",String.Empty);
            clauses.Add("DELETED = False");
            foreach (String clause in clauses)
            {
                Assert.IsTrue(actual.ToUpper().Contains(clause.ToUpper().Replace(" ", String.Empty)));
            }

            TestContext.WriteLine(actual);

        }
    }
}
