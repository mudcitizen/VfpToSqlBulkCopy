using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VfpToSqlBulkCopy.Logging.Models;

namespace VfpToSqlBulkCopy.Logging.EfContexts
{
    public class UploadContext : DbContext
    {
        public UploadContext() : base("EFUpload") { Init();  }
        public DbSet<UploadHeader> UploadHeaders { get; set; }
        public DbSet<UploadDetail> UploadDetails { get; set; }

        private void Init() {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<UploadContext, Migrations.Configuration>());
        }
    }
}
