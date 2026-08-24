namespace Last_Dance_System.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPackagePayments : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Payments", "StudentPackageId", c => c.Int());
            CreateIndex("dbo.Payments", "StudentPackageId");
            AddForeignKey("dbo.Payments", "StudentPackageId", "dbo.StudentPackages", "StudentPackageId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Payments", "StudentPackageId", "dbo.StudentPackages");
            DropIndex("dbo.Payments", new[] { "StudentPackageId" });
            DropColumn("dbo.Payments", "StudentPackageId");
        }
    }
}
