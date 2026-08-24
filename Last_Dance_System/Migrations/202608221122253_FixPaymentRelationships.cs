namespace Last_Dance_System.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixPaymentRelationships : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Payments", new[] { "BookingId" });
            AlterColumn("dbo.Payments", "BookingId", c => c.Int());
            CreateIndex("dbo.Payments", "BookingId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Payments", new[] { "BookingId" });
            AlterColumn("dbo.Payments", "BookingId", c => c.Int(nullable: false));
            CreateIndex("dbo.Payments", "BookingId");
        }
    }
}
