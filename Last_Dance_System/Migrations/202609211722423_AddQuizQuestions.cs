namespace Last_Dance_System.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddQuizQuestions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QuizQuestions",
                c => new
                    {
                        QuizQuestionId = c.Int(nullable: false, identity: true),
                        LearnerLessonId = c.Int(nullable: false),
                        Question = c.String(nullable: false),
                        OptionA = c.String(nullable: false),
                        OptionB = c.String(nullable: false),
                        OptionC = c.String(nullable: false),
                        OptionD = c.String(nullable: false),
                        CorrectAnswer = c.String(nullable: false),
                        QuestionOrder = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.QuizQuestionId)
                .ForeignKey("dbo.LearnerLessons", t => t.LearnerLessonId, cascadeDelete: true)
                .Index(t => t.LearnerLessonId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuizQuestions", "LearnerLessonId", "dbo.LearnerLessons");
            DropIndex("dbo.QuizQuestions", new[] { "LearnerLessonId" });
            DropTable("dbo.QuizQuestions");
        }
    }
}
