namespace VfpToSqlBulkCopy.Logging.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUploadDetailsClassName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UploadDetails", "ClassName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UploadDetails", "ClassName");
        }
    }
}
