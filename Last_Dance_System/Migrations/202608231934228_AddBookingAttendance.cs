namespace Last_Dance_System.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBookingAttendance : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Bookings", "AttendanceStatus", c => c.String(nullable: false, maxLength: 20));
            AddColumn("dbo.Bookings", "AttendanceConfirmedDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Bookings", "AttendanceConfirmedDate");
            DropColumn("dbo.Bookings", "AttendanceStatus");
        }
    }
}
