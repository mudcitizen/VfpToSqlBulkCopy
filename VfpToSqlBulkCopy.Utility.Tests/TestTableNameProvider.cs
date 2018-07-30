using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VfpToSqlBulkCopy.Utility.Tests
{
    [TestClass]
    public class TestTableNameProvider
    {

        public TestContext TestContext { get; set; }
        const String LaptopHostConnectionString = @"Provider=VFPOLEDB.1;Data Source=D:\VfpToSql\vhost;";
        [TestMethod]
        public void TestMethod1()
        {
            RestartParameter rs = new RestartParameter() { ConnectionName = Constants.ConnectionNames.Host, TableName = "IN_H" };
            ITableNameProvider tnp = new TableNameProvider(LaptopHostConnectionString, rs.SatisfiesFilter);
            IEnumerable<String> tables = tnp.GetTables(Constants.ConnectionNames.Host, LaptopHostConnectionString);
            foreach (String s in tables)
                TestContext.WriteLine(s);
            TestContext.WriteLine("Done");

        }
    }
}
