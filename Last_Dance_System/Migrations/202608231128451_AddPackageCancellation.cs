namespace Last_Dance_System.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPackageCancellation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StudentPackages", "IsCancelled", c => c.Boolean(nullable: false));
            AddColumn("dbo.StudentPackages", "CancellationDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.StudentPackages", "CancellationDate");
            DropColumn("dbo.StudentPackages", "IsCancelled");
        }
    }
}
