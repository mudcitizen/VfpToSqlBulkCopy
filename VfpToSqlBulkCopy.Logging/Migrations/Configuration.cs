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
         * Adding a migration - an example of a success story
         * 
         * - Changed Model - Added UploadDetails.ClassName
         * - Rebuild solution
         * - Open PM Console
         * - Make VfpToSqlBulkCopy.Logging the default project
         * - add-migration AddUploadDetailsClassName -verbose -connectionStringName "EfUpload"
         * 
         * The AddUploadDetailsClassName (in 201808171816464_AddUploadDetailsClassName) as added
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
