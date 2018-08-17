using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VfpToSqlBulkCopy.Logging.EfContexts;
using VfpToSqlBulkCopy.Logging.Models;
using VfpToSqlBulkCopy.Utility.Events;

namespace VfpToSqlBulkCopy.Utility.Events
{
    public class SqlEventHandler : IUploadEventHandler
    {

        UploadHeader Header;
        UploadDetail Detail;
        UploadContext Context;
        public SqlEventHandler()
        {
            Context = new UploadContext();
        }


        public void HandleTableProcessorBegin(object sender, TableProcessorBeginEventArgs args)
        {
            Detail = new UploadDetail() { TableName = args.TableName, ClassName = args.ClassName, Begin = DateTime.Now, UploadHeader = Header };
            Context.UploadDetails.Add(Detail);
            Context.SaveChanges();
        }

        public void HandleTableProcessorEnd(object sender, TableProcessorEndEventArgs args)
        {
            UploadDetail detailToUpdate;
            if (args.ClassName == Detail.ClassName)
                detailToUpdate = Detail;
            else
                detailToUpdate = Context.UploadDetails.Where(detail => detail.UploadHeader.Id == Header.Id && detail.TableName == args.TableName && detail.ClassName == args.ClassName).First();
            detailToUpdate.End = DateTime.Now;
            Context.SaveChanges();
        }

        public void HandleTableProcessorException(object sender, TableProcessorExceptionEventArgs args)
        {
            Detail.Exception = args.Exception.ToString();
            Context.SaveChanges();
        }

        public void HandleUploadBegin(object sender, BeginUploadEventArgs args)
        {
            Header = new UploadHeader();
            Header.Begin = DateTime.Now;
            StringBuilder sb = new StringBuilder();
            foreach (String connStr in args.ConnectionStrings)
                sb.AppendLine(connStr);
            Header.ConnectionStrings = sb.ToString();
            Header.RestartDetails = args.RestartParameter == null ? String.Empty : args.RestartParameter.ToString();
            Context.UploadHeaders.Add(Header);
            Context.SaveChanges();

        }

        public void HandleUploadEnd(object sender, EndUploadEventArgs args)
        {

            Context.SaveChanges();
        }
    }
}
