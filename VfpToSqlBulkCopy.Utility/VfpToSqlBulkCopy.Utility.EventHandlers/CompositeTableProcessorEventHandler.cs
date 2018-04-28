using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility.VfpToSqlBulkCopy.Utility.EventHandlers
{
    public class CompositeTableProcessorEventHandler : ITableProcessorEventHandler
    {
        IEnumerable<ITableProcessorEventHandler> Handlers;

        public CompositeTableProcessorEventHandler(IEnumerable<ITableProcessorEventHandler> handlers)
        {
            if (handlers == null)
                throw new ArgumentNullException("Handlers is a requirement parameter");

            Handlers = handlers;
        }
        public void HandleTableProcessorBegin(object sender, TableProcessorBeginEventArgs args)
        {
            foreach (ITableProcessorEventHandler handler in Handlers)
                handler.HandleTableProcessorBegin(sender, args);
        }

        public void HandleTableProcessorEnd(object sender, TableProcessorEndEventArgs args)
        {
            foreach (ITableProcessorEventHandler handler in Handlers)
                handler.HandleTableProcessorEnd(sender, args);
        }


        public void HandleTableProcessorException(object sender, TableProcessorExceptionEventArgs args)
        {
            foreach (ITableProcessorEventHandler handler in Handlers)
                handler.HandleTableProcessorException(sender, args);
        }
    }
}
