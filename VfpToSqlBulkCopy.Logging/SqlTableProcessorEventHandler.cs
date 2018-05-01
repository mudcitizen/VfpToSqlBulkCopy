using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VfpToSqlBulkCopy.Logging.EfContexts;
using VfpToSqlBulkCopy.Logging.Models;
using VfpToSqlBulkCopy.Utility;
using VfpToSqlBulkCopy.Utility.VfpToSqlBulkCopy.Utility.EventHandlers;

namespace VfpToSqlBulkCopy.Logging
{
    public class SqlTableProcessorEventHandler : ITableProcessorEventHandler
    {
        UploadContext Context;
        UploadHeader Header;


        public SqlTableProcessorEventHandler(String connectionName)
        {
            Context = new UploadContext(connectionName);
            Header = new UploadHeader();
            Header.Begin = DateTime.Now;
            Context.UploadHeaders.Add(Header);
            Context.SaveChanges();
        }

        public void HandleTableProcessorBegin(object sender, TableProcessorBeginEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void HandleTableProcessorEnd(object sender, TableProcessorEndEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void HandleTableProcessorException(object sender, TableProcessorExceptionEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
