using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility.VfpToSqlBulkCopy.Utility.EventHandlers
{
    public interface ITableProcessorEventHandler
    {
        void HandleTableProcessorBegin(Object sender, TableProcessorBeginEventArgs args);
        void HandleTableProcessorEnd(Object sender, TableProcessorEndEventArgs args);
        void HandleTableProcessorException(Object sender, TableProcessorExceptionEventArgs args);
    }
}
