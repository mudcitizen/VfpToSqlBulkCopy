using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility.Events
{
    public interface IUploadEventHandler
    {
        void HandleUploadBegin(Object sender, BeginUploadEventArgs args);
        void HandleUploadEnd(Object sender, EndUploadEventArgs args);
        void HandleTableProcessorBegin(Object sender, TableProcessorBeginEventArgs args);
        void HandleTableProcessorEnd(Object sender, TableProcessorEndEventArgs args);
        void HandleTableProcessorException(Object sender, TableProcessorExceptionEventArgs args);
    }
}
