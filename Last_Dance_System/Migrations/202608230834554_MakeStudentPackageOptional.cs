namespace Last_Dance_System.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MakeStudentPackageOptional : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Bookings", "StudentPackageId", "dbo.StudentPackages");
            DropIndex("dbo.Bookings", new[] { "StudentPackageId" });
            AlterColumn("dbo.Bookings", "StudentPackageId", c => c.Int());
            CreateIndex("dbo.Bookings", "StudentPackageId");
            AddForeignKey("dbo.Bookings", "StudentPackageId", "dbo.StudentPackages", "StudentPackageId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Bookings", "StudentPackageId", "dbo.StudentPackages");
            DropIndex("dbo.Bookings", new[] { "StudentPackageId" });
            AlterColumn("dbo.Bookings", "StudentPackageId", c => c.Int(nullable: false));
            CreateIndex("dbo.Bookings", "StudentPackageId");
            AddForeignKey("dbo.Bookings", "StudentPackageId", "dbo.StudentPackages", "StudentPackageId", cascadeDelete: true);
        }
    }
}
