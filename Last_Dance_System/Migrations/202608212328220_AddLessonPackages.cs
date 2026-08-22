namespace Last_Dance_System.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLessonPackages : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LessonPackages",
                c => new
                    {
                        LessonPackageId = c.Int(nullable: false, identity: true),
                        PackageName = c.String(nullable: false, maxLength: 100),
                        NumberOfLessons = c.Int(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Description = c.String(maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.LessonPackageId);
            
            CreateTable(
                "dbo.StudentPackages",
                c => new
                    {
                        StudentPackageId = c.Int(nullable: false, identity: true),
                        RegistrationId = c.String(nullable: false, maxLength: 13),
                        LessonPackageId = c.Int(nullable: false),
                        LessonsPurchased = c.Int(nullable: false),
                        LessonsRemaining = c.Int(nullable: false),
                        PaymentStatus = c.String(nullable: false, maxLength: 20),
                        PurchaseDate = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.StudentPackageId)
                .ForeignKey("dbo.LessonPackages", t => t.LessonPackageId)
                .ForeignKey("dbo.Registrations", t => t.RegistrationId)
                .Index(t => t.RegistrationId)
                .Index(t => t.LessonPackageId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StudentPackages", "RegistrationId", "dbo.Registrations");
            DropForeignKey("dbo.StudentPackages", "LessonPackageId", "dbo.LessonPackages");
            DropIndex("dbo.StudentPackages", new[] { "LessonPackageId" });
            DropIndex("dbo.StudentPackages", new[] { "RegistrationId" });
            DropTable("dbo.StudentPackages");
            DropTable("dbo.LessonPackages");
        }
    }
}
