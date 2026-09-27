
namespace Last_Dance_System.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddLicenseCodeToVehicle : DbMigration
    {
        public override void Up()
        {
            // =========================================================
            // VEHICLE LICENCE CODE
            // =========================================================
            //
            // Add the column as nullable first because existing
            // vehicles already exist in the database.
            //
            AddColumn(
                "dbo.Vehicles",
                "LicenseCode",
                c => c.String(nullable: true, maxLength: 20)
            );

            // =========================================================
            // LESSON PACKAGE LICENCE CODE
            // =========================================================
            //
            // Driving packages can specify the licence code they
            // require. Learner Theory Package can leave this blank.
            //
            AddColumn(
                "dbo.LessonPackages",
                "RequiredLicenseCode",
                c => c.String(maxLength: 20)
            );
        }

        public override void Down()
        {
            DropColumn(
                "dbo.LessonPackages",
                "RequiredLicenseCode"
            );

            DropColumn(
                "dbo.Vehicles",
                "LicenseCode"
            );
        }
    }
}

