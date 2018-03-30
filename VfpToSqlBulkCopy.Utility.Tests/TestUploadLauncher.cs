using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VfpToSqlBulkCopy.Utility.Tests
{
    [TestClass]
    public class TestUploadLauncher
    {
        public TestContext TestContext { get; set; }
        [TestMethod]
        public void TestUpload()
        {

            Helper.ExecuteSqlNonQuery(Constants.ConnectionNames.Sql, @"USE[master] RESTORE DATABASE[NoRows_22_000211] FROM DISK = N'D:\VFPTOSQL\VHOST\NoRows_22_000211.bak' WITH FILE = 1, NOUNLOAD, STATS = 5");
            UploadLauncher ul = new UploadLauncher();
            TestContext.WriteLine(String.Format("{0} ; {1}", DateTime.Now.ToLongTimeString(), "Begin"));
            ul.Launch();
            TestContext.WriteLine(String.Format("{0} ; {1}", DateTime.Now.ToLongTimeString(), "End"));
            TestContext.WriteLine("Done");

        }

    }
}
