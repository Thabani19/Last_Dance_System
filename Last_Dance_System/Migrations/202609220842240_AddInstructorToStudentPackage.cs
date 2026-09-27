namespace Last_Dance_System.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddInstructorToStudentPackage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StudentPackages", "InstructorId", c => c.Int());
            CreateIndex("dbo.StudentPackages", "InstructorId");
            AddForeignKey("dbo.StudentPackages", "InstructorId", "dbo.Instructors", "InstructorId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StudentPackages", "InstructorId", "dbo.Instructors");
            DropIndex("dbo.StudentPackages", new[] { "InstructorId" });
            DropColumn("dbo.StudentPackages", "InstructorId");
        }
    }
}
