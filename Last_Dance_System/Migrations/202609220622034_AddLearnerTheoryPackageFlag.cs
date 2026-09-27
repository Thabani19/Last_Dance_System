namespace Last_Dance_System.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLearnerTheoryPackageFlag : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LessonPackages", "IsLearnerTheoryPackage", c => c.Boolean(nullable: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.LessonPackages", "IsLearnerTheoryPackage");
        }
    }
}
