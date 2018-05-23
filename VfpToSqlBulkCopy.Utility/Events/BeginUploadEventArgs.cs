using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Utility.Events
{
    public class BeginUploadEventArgs
    {
        public RestartParameter RestartParameter { get; set; }
        public IEnumerable<String> ConnectionStrings { get; set; }

    }
}
