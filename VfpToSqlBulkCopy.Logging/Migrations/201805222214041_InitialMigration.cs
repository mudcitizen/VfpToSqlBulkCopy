namespace VfpToSqlBulkCopy.Logging.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UploadDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TableName = c.String(),
                        Begin = c.DateTime(),
                        End = c.DateTime(),
                        Exception = c.String(),
                        UploadHeader_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UploadHeaders", t => t.UploadHeader_Id)
                .Index(t => t.UploadHeader_Id);
            
            CreateTable(
                "dbo.UploadHeaders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Begin = c.DateTime(),
                        End = c.DateTime(),
                        ConnectionStrings = c.String(),
                        RestartDetails = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UploadDetails", "UploadHeader_Id", "dbo.UploadHeaders");
            DropIndex("dbo.UploadDetails", new[] { "UploadHeader_Id" });
            DropTable("dbo.UploadHeaders");
            DropTable("dbo.UploadDetails");
        }
    }
}
