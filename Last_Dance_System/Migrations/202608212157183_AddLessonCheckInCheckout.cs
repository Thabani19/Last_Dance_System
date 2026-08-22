namespace Last_Dance_System.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLessonCheckInCheckout : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LessonProgresses", "CheckInCode", c => c.String(maxLength: 10));
            AddColumn("dbo.LessonProgresses", "CheckInTime", c => c.DateTime());
            AddColumn("dbo.LessonProgresses", "CheckOutCode", c => c.String(maxLength: 10));
            AddColumn("dbo.LessonProgresses", "CheckOutTime", c => c.DateTime());
            AddColumn("dbo.LessonProgresses", "DurationMinutes", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.LessonProgresses", "DurationMinutes");
            DropColumn("dbo.LessonProgresses", "CheckOutTime");
            DropColumn("dbo.LessonProgresses", "CheckOutCode");
            DropColumn("dbo.LessonProgresses", "CheckInTime");
            DropColumn("dbo.LessonProgresses", "CheckInCode");
        }
    }
}
