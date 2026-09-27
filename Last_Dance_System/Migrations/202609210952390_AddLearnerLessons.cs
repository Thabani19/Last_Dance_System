namespace Last_Dance_System.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLearnerLessons : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LearnerLessons",
                c => new
                    {
                        LearnerLessonId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false),
                        Description = c.String(nullable: false),
                        Content = c.String(nullable: false),
                        LessonOrder = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.LearnerLessonId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.LearnerLessons");
        }
    }
}
