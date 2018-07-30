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
        public TestContext TestContext { get; set; }
        const String CollatingSequenceClause = "COLLATINGSEQUENCE=MACHINE";
        const String DeletedClause = "DELETED=FALSE";


        [TestMethod]
        public void TestHelperReturnsScrubbedConnectionString()
        {
            String connStr = Helper.GetConnectionString(Constants.ConnectionNames.Host).ToUpper().Replace(" ", String.Empty);
            Assert.IsTrue(connStr.Contains(CollatingSequenceClause));
            Assert.IsTrue(connStr.Contains(DeletedClause));
        }


        [TestMethod]
        public void TestConnStringHasDeleted()
        {
            IList<String> clauses = new List<String>()
            {
                String.Format("Provider={0}",Constants.ConnectionStringTerms.VfpOleDbProvider),
                @"Data Source=D:\VfpToSql\vhost",
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
            String actual = vfpBldr.ConnectionString.Replace(" ", String.Empty);
            clauses.Add("DELETED = False");
            foreach (String clause in clauses)
            {
                Assert.IsTrue(actual.ToUpper().Contains(clause.ToUpper().Replace(" ", String.Empty)));
            }

            Assert.IsTrue(actual.ToUpper().Replace(" ", String.Empty).Contains(CollatingSequenceClause));

            TestContext.WriteLine(actual);

        }
    }
}
