using System;
using System.Collections.Generic;
using System.Linq;
using VfpToSqlBulkCopy.Logging.EfContexts;
using VfpToSqlBulkCopy.Logging.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VfpToSqlBulkCopy.Logging.Tests
{
    [TestClass]
    public class TestUploadContext
    {
        public TestContext TestContext { get; set; }
        [TestMethod]
        public void TestCreateAndPopulate()
        {
            UploadContext context = new UploadContext();

            UploadHeader header = new UploadHeader();
            header.Begin = DateTime.Now;

            context.UploadHeaders.Add(header);

            DateTime dt = DateTime.Now;
            int minutes = 0;
            IList<String> tables = new List<String>() { "IN_GUEST", "IN_TRN", "IN_MSG", "IN_RES" };
            foreach (String table in tables.OrderBy(s=>s))
            {
                minutes++;
                UploadDetail dtl = new UploadDetail() { TableName = table, Begin = dt.AddMinutes(minutes++),End = dt.AddMinutes(minutes++),UploadHeader = header };
                context.UploadDetails.Add(dtl);
            }
            context.SaveChanges();
            TestContext.WriteLine("Done - {0}", minutes);
        }
    }
}
