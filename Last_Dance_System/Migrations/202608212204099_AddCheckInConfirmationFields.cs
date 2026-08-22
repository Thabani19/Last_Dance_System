namespace Last_Dance_System.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCheckInConfirmationFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LessonProgresses", "CheckInConfirmed", c => c.Boolean(nullable: false));
            AddColumn("dbo.LessonProgresses", "CheckOutConfirmed", c => c.Boolean(nullable: false));
            DropColumn("dbo.LessonProgresses", "DurationMinutes");
        }
        
        public override void Down()
        {
            AddColumn("dbo.LessonProgresses", "DurationMinutes", c => c.Int());
            DropColumn("dbo.LessonProgresses", "CheckOutConfirmed");
            DropColumn("dbo.LessonProgresses", "CheckInConfirmed");
        }
    }
}
