namespace Last_Dance_System.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPaymentMethodToStudentPackage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StudentPackages", "PaymentMethod", c => c.String(nullable: false, maxLength: 30));
        }
        
        public override void Down()
        {
            DropColumn("dbo.StudentPackages", "PaymentMethod");
        }
    }
}
