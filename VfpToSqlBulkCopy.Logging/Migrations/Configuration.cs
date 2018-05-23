namespace VfpToSqlBulkCopy.Logging.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<VfpToSqlBulkCopy.Logging.EfContexts.UploadContext>
    {
        /*
         * Update-Database -ConnectionString 'Data Source=(local);Initial Catalog=EfUpload;Integrated Security=True' -ConnectionProviderName 'System.Data.SqlClient' -verbose
         * 
         *     <add name="EFUpload" connectionString="Data Source=(local);Initial Catalog=EfUpload;Integrated Security=True" providerName="System.Data.SqlClient" />

         * 
         * 
         * 
         */

        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "VfpToSqlBulkCopy.Logging.EfContexts.UploadContext";
        }

        protected override void Seed(VfpToSqlBulkCopy.Logging.EfContexts.UploadContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
        }
    }
}
