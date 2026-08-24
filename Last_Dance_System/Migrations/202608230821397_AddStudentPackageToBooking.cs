namespace Last_Dance_System.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStudentPackageToBooking : DbMigration
    {
        public override void Up()
        {
            // =========================================================
            // ADD STUDENT PACKAGE COLUMN
            // =========================================================

            AddColumn(
                "dbo.Bookings",
                "StudentPackageId",
                c => c.Int(nullable: true)
            );


            // =========================================================
            // LINK EXISTING BOOKINGS TO A STUDENT PACKAGE
            // =========================================================

            Sql(@"
        UPDATE B
        SET B.StudentPackageId =
        (
            SELECT TOP 1 SP.StudentPackageId
            FROM StudentPackages SP
            WHERE SP.RegistrationId = B.RegistrationId
              AND SP.PaymentStatus = 'Paid'
            ORDER BY SP.PurchaseDate DESC
        )
        FROM Bookings B
        WHERE B.StudentPackageId IS NULL
    ");


            // =========================================================
            // CREATE FOREIGN KEY
            // =========================================================

            AddForeignKey(
                "dbo.Bookings",
                "StudentPackageId",
                "dbo.StudentPackages",
                "StudentPackageId"
            );


            // =========================================================
            // CREATE INDEX
            // =========================================================

            CreateIndex(
                "dbo.Bookings",
                "StudentPackageId"
            );
        }

        public override void Down()
        {
            DropForeignKey(
                "dbo.Bookings",
                "StudentPackageId",
                "dbo.StudentPackages"
            );

            DropIndex(
                "dbo.Bookings",
                new[] { "StudentPackageId" }
            );

            DropColumn(
                "dbo.Bookings",
                "StudentPackageId"
            );
        }
    }
}
