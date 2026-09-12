namespace Last_Dance_System.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddInstructorFeedback : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InstructorFeedbacks",
                c => new
                    {
                        InstructorFeedbackId = c.Int(nullable: false, identity: true),
                        BookingId = c.Int(nullable: false),
                        InstructorId = c.Int(nullable: false),
                        RegistrationId = c.String(nullable: false, maxLength: 13),
                        Feedback = c.String(nullable: false, maxLength: 2000),
                        AreasToImprove = c.String(maxLength: 1000),
                        Recommendations = c.String(maxLength: 1000),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.InstructorFeedbackId)
                .ForeignKey("dbo.Bookings", t => t.BookingId)
                .ForeignKey("dbo.Instructors", t => t.InstructorId)
                .ForeignKey("dbo.Registrations", t => t.RegistrationId)
                .Index(t => t.BookingId)
                .Index(t => t.InstructorId)
                .Index(t => t.RegistrationId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.InstructorFeedbacks", "RegistrationId", "dbo.Registrations");
            DropForeignKey("dbo.InstructorFeedbacks", "InstructorId", "dbo.Instructors");
            DropForeignKey("dbo.InstructorFeedbacks", "BookingId", "dbo.Bookings");
            DropIndex("dbo.InstructorFeedbacks", new[] { "RegistrationId" });
            DropIndex("dbo.InstructorFeedbacks", new[] { "InstructorId" });
            DropIndex("dbo.InstructorFeedbacks", new[] { "BookingId" });
            DropTable("dbo.InstructorFeedbacks");
        }
    }
}
