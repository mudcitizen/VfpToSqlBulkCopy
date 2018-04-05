using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Logging.Models
{
    public class UploadDetail
    {
        public int Id { get; set; }
        public String TableName { get; set; }
        public DateTime? Begin { get; set; }
        public DateTime? End { get; set; }
        public String Exception { get; set; }
        public UploadHeader UploadHeader { get; set; }
    }
}
