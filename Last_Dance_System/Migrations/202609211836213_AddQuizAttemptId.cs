namespace Last_Dance_System.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddQuizAttemptId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizResults", "AttemptId", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizResults", "AttemptId");
        }
    }
}
