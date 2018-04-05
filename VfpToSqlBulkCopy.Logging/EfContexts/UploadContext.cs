using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VfpToSqlBulkCopy.Logging.Models;

namespace VfpToSqlBulkCopy.Logging.EfContexts
{
    public class UploadContext : DbContext
    {
        public UploadContext() : base() { Init();  }
        public UploadContext(String connectionName) : base(connectionName) { Init(); }

        public DbSet<UploadHeader> UploadHeaders { get; set; }
        public DbSet<UploadDetail> UploadDetails { get; set; }

        private void Init() {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<UploadContext, Migrations.Configuration>());
        }
    }
}
