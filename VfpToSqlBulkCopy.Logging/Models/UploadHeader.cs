using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VfpToSqlBulkCopy.Logging.Models
{
    public class UploadHeader
    {
        public int Id { get; set; }
        public DateTime? Begin { get; set; }
        public String ConnectionStrings { get; set; }
        public String RestartDetails { get; set; }

        public ICollection<UploadDetail> UploadDetails { get; set; }
    }
}
