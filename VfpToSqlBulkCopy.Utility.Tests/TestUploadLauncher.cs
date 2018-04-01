using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VfpToSqlBulkCopy.Utility.Tests
{
    [TestClass]
    public class TestUploadLauncher
    {
        const String LaptopHostConnectionString = @"Provider=VFPOLEDB.1;Data Source=D:\VfpToSql\vhost;Collating Sequence=general;DELETED=False;";
        const String LaptopPosConnectionString = @"Provider=VFPOLEDB.1;Data Source=D:\VfpToSql\vpos;Collating Sequence=general;DELETED=False;";
        const String LaptopSqlConnectionString = @"Data Source=(local);Initial Catalog=NoRows_22_000211;Integrated Security=True";

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestLaptopUpload()
        {

            Helper.ExecuteSqlNonQuery(LaptopSqlConnectionString, @"USE[master] RESTORE DATABASE[NoRows_22_000211] FROM DISK = N'D:\VFPTOSQL\VHOST\NoRows_22_000211.bak' WITH FILE = 1, NOUNLOAD, STATS = 5");

            IDictionary<String, String> connStrs = new Dictionary<String, String>();
            connStrs.Add(Constants.ConnectionNames.Sql, LaptopSqlConnectionString);
            connStrs.Add(Constants.ConnectionNames.Host, LaptopHostConnectionString);
            connStrs.Add(Constants.ConnectionNames.POS, LaptopPosConnectionString);

            //RestartDetails restartDetails = new RestartDetails() { ConnectionName = Constants.ConnectionNames.POS, TableName = "PSDRLOG" };
            UploadLauncher ul = new UploadLauncher(connStrs,null);
            TestContext.WriteLine(String.Format("{0} ; {1}", DateTime.Now.ToLongTimeString(), "Begin"));
            ul.Launch();
            TestContext.WriteLine(String.Format("{0} ; {1}", DateTime.Now.ToLongTimeString(), "End"));
            TestContext.WriteLine("Done");

        }
        [TestMethod]
        public void TestEssexUpload()
        {


            const String HostConnectionString = @"Provider=VFPOLEDB.1;Data Source=D:\Essex\Hostdema;Collating Sequence=general;DELETED=False;";
            const String SqlConnectionString = @"Data Source=(local);Initial Catalog=Essex_22_000211;Integrated Security=True";

            //Helper.ExecuteSqlNonQuery(SqlConnectionString, @"USE[master] RESTORE DATABASE[Essex_22_000211] FROM DISK = N'D:\Essex\Hostdema\Essex_22_000211.bak' WITH FILE = 1, NOUNLOAD, STATS = 5");

            IDictionary<String, String> connStrs = new Dictionary<String, String>();
            connStrs.Add(Constants.ConnectionNames.Sql, SqlConnectionString);
            connStrs.Add(Constants.ConnectionNames.Host, HostConnectionString);

            RestartParameter restartParm = new RestartParameter() { ConnectionName = Constants.ConnectionNames.Host, TableName = "IN_HRES" };
            UploadLauncher ul = new UploadLauncher(connStrs,restartParm);
            TestContext.WriteLine(String.Format("{0} ; {1}", DateTime.Now.ToLongTimeString(), "Begin"));
            ul.Launch();
            TestContext.WriteLine(String.Format("{0} ; {1}", DateTime.Now.ToLongTimeString(), "End"));
            TestContext.WriteLine("Done");

        }


    }
}
