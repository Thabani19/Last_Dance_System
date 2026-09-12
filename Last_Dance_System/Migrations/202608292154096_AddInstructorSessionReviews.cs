namespace Last_Dance_System.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddInstructorSessionReviews : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Reviews", new[] { "RegistrationId" });
            AddColumn("dbo.Reviews", "BookingId", c => c.Int(nullable: false));
            AddColumn("dbo.Reviews", "ReviewerInstructorId", c => c.Int());
            AlterColumn("dbo.Reviews", "RegistrationId", c => c.String(maxLength: 13));
            AlterColumn("dbo.Reviews", "ReviewType", c => c.String(nullable: false, maxLength: 30));
            CreateIndex("dbo.Reviews", "BookingId");
            CreateIndex("dbo.Reviews", "RegistrationId");
            CreateIndex("dbo.Reviews", "ReviewerInstructorId");
            AddForeignKey("dbo.Reviews", "BookingId", "dbo.Bookings", "BookingId");
            AddForeignKey("dbo.Reviews", "ReviewerInstructorId", "dbo.Instructors", "InstructorId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Reviews", "ReviewerInstructorId", "dbo.Instructors");
            DropForeignKey("dbo.Reviews", "BookingId", "dbo.Bookings");
            DropIndex("dbo.Reviews", new[] { "ReviewerInstructorId" });
            DropIndex("dbo.Reviews", new[] { "RegistrationId" });
            DropIndex("dbo.Reviews", new[] { "BookingId" });
            AlterColumn("dbo.Reviews", "ReviewType", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.Reviews", "RegistrationId", c => c.String(nullable: false, maxLength: 13));
            DropColumn("dbo.Reviews", "ReviewerInstructorId");
            DropColumn("dbo.Reviews", "BookingId");
            CreateIndex("dbo.Reviews", "RegistrationId");
        }
    }
}
