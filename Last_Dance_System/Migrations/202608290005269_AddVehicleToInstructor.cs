namespace Last_Dance_System.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVehicleToInstructor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Instructors", "VehicleId", c => c.Int());
            CreateIndex("dbo.Instructors", "VehicleId");
            AddForeignKey("dbo.Instructors", "VehicleId", "dbo.Vehicles", "VehicleId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Instructors", "VehicleId", "dbo.Vehicles");
            DropIndex("dbo.Instructors", new[] { "VehicleId" });
            DropColumn("dbo.Instructors", "VehicleId");
        }
    }
}
