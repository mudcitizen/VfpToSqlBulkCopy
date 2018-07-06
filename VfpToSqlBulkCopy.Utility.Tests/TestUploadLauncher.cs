using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VfpToSqlBulkCopy.Utility.Events;

namespace VfpToSqlBulkCopy.Utility.Tests
{
    [TestClass]
    public class TestUploadLauncher
    {
        String LogFileName;
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
            connStrs.Add(Constants.ConnectionNames.Host, @"Provider=VFPOLEDB.1;Data Source=D:\VfpToSql\vhost;Collating Sequence=general;");
            connStrs.Add(Constants.ConnectionNames.POS, LaptopPosConnectionString);

            UploadLauncher ul = new UploadLauncher(connStrs);
            LogFileName = "Laptop_Upload.Log";
            RunUploadLauncher(ul);

        }
        [TestMethod]
        public void TestEssexUpload()
        {
            const String HostConnectionString = @"Provider=VFPOLEDB.1;Data Source=D:\Essex\Hostdema;Collating Sequence=machine;DELETED=False;";
            const String SqlConnectionString = @"Data Source=(local);Initial Catalog=Essex_22_000211;Integrated Security=True";

            Helper.ExecuteSqlNonQuery(SqlConnectionString, @"USE[master] RESTORE DATABASE[Essex_22_000211] FROM DISK = N'D:\Essex\Hostdema\Essex_22_000211.bak' WITH FILE = 1, NOUNLOAD, STATS = 5");

            IDictionary<String, String> connStrs = new Dictionary<String, String>();
            connStrs.Add(Constants.ConnectionNames.Sql, SqlConnectionString);
            connStrs.Add(Constants.ConnectionNames.Host, HostConnectionString);

            //RestartParameter restartParm = new RestartParameter() { ConnectionName = Constants.ConnectionNames.Host, TableName = "IN_HRES" };
            LogFileName = "Essex_Upload.Log";
            UploadLauncher ul = new UploadLauncher(connStrs);
            RunUploadLauncher(ul);
        }

    
        private void WriteBoth(String txt)
        {
            String s = DateTime.Now.ToLongTimeString() + " " + txt;
            System.Diagnostics.Debug.WriteLine(s);
            TestContext.WriteLine(s);
            System.IO.File.AppendAllText(LogFileName, txt + Environment.NewLine);
        }

        private void RunUploadLauncher(UploadLauncher uploadLauncher)
        {
            if (File.Exists(LogFileName))
                File.Delete(LogFileName);


            IEnumerable<IUploadEventHandler> ehs = new List<IUploadEventHandler>()
            {
                new ConsoleEventHandler(),
                new TextFileEventHandler(LogFileName)
            };
            IUploadEventHandler eh = new CompositeEventHandler(ehs);


            TestContext.WriteLine(String.Format("{0} ; {1}", DateTime.Now.ToLongTimeString(), "Begin"));
            uploadLauncher.TableProcessor.TableProcessorBegin += eh.HandleTableProcessorBegin;
            uploadLauncher.TableProcessor.TableProcessorEnd += eh.HandleTableProcessorEnd;
            uploadLauncher.TableProcessor.TableProcessorException += eh.HandleTableProcessorException;
            uploadLauncher.Launch();
            TestContext.WriteLine(String.Format("{0} ; {1}", DateTime.Now.ToLongTimeString(), "End"));
            TestContext.WriteLine("Done");

        }
    }
}
