using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VfpToSqlBulkCopy.Utility;

namespace VfpToSqlBulkCopy.Logging
{
    public interface ITableUploadEventHandler
    {
        void HandleTableUploadBegin(Object sender, TableUploadBeginEventArgs args);
        void HandleTableUploadEnd(Object sender, TableUploadEndEventArgs args);
        void HandleTableUploadError(Object sender, TableUploadErrorEventArgs args);
    }
}
