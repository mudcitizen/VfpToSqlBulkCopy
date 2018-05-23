namespace VfpToSqlBulkCopy.Logging.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DropUPloadHeaderEnd : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.UploadHeaders", "End");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UploadHeaders", "End", c => c.DateTime());
        }
    }
}
