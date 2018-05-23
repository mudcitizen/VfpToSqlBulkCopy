using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility.Events
{
    public class CompositeEventHandler : IUploadEventHandler
    {
        IEnumerable<IUploadEventHandler> Handlers;

        public CompositeEventHandler(IEnumerable<IUploadEventHandler> handlers)
        {
            if (handlers == null)
                throw new ArgumentNullException("Handlers is a requirement parameter");

            Handlers = handlers;
        }
        public void HandleTableProcessorBegin(object sender, TableProcessorBeginEventArgs args)
        {
            foreach (IUploadEventHandler handler in Handlers)
                handler.HandleTableProcessorBegin(sender, args);
        }

        public void HandleTableProcessorEnd(object sender, TableProcessorEndEventArgs args)
        {
            foreach (IUploadEventHandler handler in Handlers)
                handler.HandleTableProcessorEnd(sender, args);
        }


        public void HandleTableProcessorException(object sender, TableProcessorExceptionEventArgs args)
        {
            foreach (IUploadEventHandler handler in Handlers)
                handler.HandleTableProcessorException(sender, args);
        }

        public void HandleUploadBegin(object sender, BeginUploadEventArgs args)
        {
            foreach (IUploadEventHandler handler in Handlers)
                handler.HandleUploadBegin(sender, args);
        }

        public void HandleUploadEnd(object sender, EndUploadEventArgs args)
        {
            foreach (IUploadEventHandler handler in Handlers)
                handler.HandleUploadEnd(sender, args);
        }
    }
}
