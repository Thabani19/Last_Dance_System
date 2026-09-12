namespace Last_Dance_System.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddInstructorNotifications : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Notifications", new[] { "RegistrationId" });
            AddColumn("dbo.Notifications", "InstructorId", c => c.Int());
            AlterColumn("dbo.Notifications", "RegistrationId", c => c.String(maxLength: 13));
            CreateIndex("dbo.Notifications", "RegistrationId");
            CreateIndex("dbo.Notifications", "InstructorId");
            AddForeignKey("dbo.Notifications", "InstructorId", "dbo.Instructors", "InstructorId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Notifications", "InstructorId", "dbo.Instructors");
            DropIndex("dbo.Notifications", new[] { "InstructorId" });
            DropIndex("dbo.Notifications", new[] { "RegistrationId" });
            AlterColumn("dbo.Notifications", "RegistrationId", c => c.String(nullable: false, maxLength: 13));
            DropColumn("dbo.Notifications", "InstructorId");
            CreateIndex("dbo.Notifications", "RegistrationId");
        }
    }
}
