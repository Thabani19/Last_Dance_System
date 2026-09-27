namespace Last_Dance_System.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddQuizResults : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QuizResults",
                c => new
                    {
                        QuizResultId = c.Int(nullable: false, identity: true),
                        RegistrationId = c.String(nullable: false, maxLength: 13),
                        LearnerLessonId = c.Int(nullable: false),
                        Score = c.Int(nullable: false),
                        TotalQuestions = c.Int(nullable: false),
                        Percentage = c.Int(nullable: false),
                        DateTaken = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.QuizResultId)
                .ForeignKey("dbo.LearnerLessons", t => t.LearnerLessonId, cascadeDelete: true)
                .ForeignKey("dbo.Registrations", t => t.RegistrationId, cascadeDelete: true)
                .Index(t => t.RegistrationId)
                .Index(t => t.LearnerLessonId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuizResults", "RegistrationId", "dbo.Registrations");
            DropForeignKey("dbo.QuizResults", "LearnerLessonId", "dbo.LearnerLessons");
            DropIndex("dbo.QuizResults", new[] { "LearnerLessonId" });
            DropIndex("dbo.QuizResults", new[] { "RegistrationId" });
            DropTable("dbo.QuizResults");
        }
    }
}
