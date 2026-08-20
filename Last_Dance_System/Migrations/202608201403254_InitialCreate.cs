namespace Last_Dance_System.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Administrators",
                c => new
                    {
                        AdministratorId = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false, maxLength: 50),
                        LastName = c.String(nullable: false, maxLength: 50),
                        Email = c.String(nullable: false, maxLength: 100),
                        Phone = c.String(nullable: false, maxLength: 15),
                        IsActive = c.Boolean(nullable: false),
                        ApplicationUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.AdministratorId)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUserId)
                .Index(t => t.ApplicationUserId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Registrations",
                c => new
                    {
                        RegistrationId = c.String(nullable: false, maxLength: 13),
                        FirstName = c.String(nullable: false, maxLength: 50),
                        LastName = c.String(nullable: false, maxLength: 50),
                        Gender = c.String(nullable: false, maxLength: 10),
                        DateOfBirth = c.DateTime(nullable: false),
                        Phone = c.String(nullable: false, maxLength: 15),
                        Email = c.String(nullable: false, maxLength: 100),
                        Address = c.String(nullable: false, maxLength: 255),
                        RegistrationDate = c.DateTime(nullable: false),
                        ApplicationUserId = c.String(),
                        ApplicationUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.RegistrationId)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id)
                .Index(t => t.ApplicationUser_Id);
            
            CreateTable(
                "dbo.Bookings",
                c => new
                    {
                        BookingId = c.Int(nullable: false, identity: true),
                        RegistrationId = c.String(nullable: false, maxLength: 13),
                        LessonScheduleId = c.Int(nullable: false),
                        BookingDate = c.DateTime(nullable: false),
                        Status = c.String(nullable: false, maxLength: 20),
                        Notes = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.BookingId)
                .ForeignKey("dbo.LessonSchedules", t => t.LessonScheduleId)
                .ForeignKey("dbo.Registrations", t => t.RegistrationId)
                .Index(t => t.RegistrationId)
                .Index(t => t.LessonScheduleId);
            
            CreateTable(
                "dbo.LessonSchedules",
                c => new
                    {
                        LessonScheduleId = c.Int(nullable: false, identity: true),
                        LessonDate = c.DateTime(nullable: false),
                        StartTime = c.Time(nullable: false, precision: 7),
                        EndTime = c.Time(nullable: false, precision: 7),
                        LessonTypeId = c.Int(nullable: false),
                        InstructorId = c.Int(nullable: false),
                        VehicleId = c.Int(nullable: false),
                        Status = c.String(nullable: false, maxLength: 20),
                    })
                .PrimaryKey(t => t.LessonScheduleId)
                .ForeignKey("dbo.Instructors", t => t.InstructorId)
                .ForeignKey("dbo.LessonTypes", t => t.LessonTypeId)
                .ForeignKey("dbo.Vehicles", t => t.VehicleId)
                .Index(t => t.LessonTypeId)
                .Index(t => t.InstructorId)
                .Index(t => t.VehicleId);
            
            CreateTable(
                "dbo.Instructors",
                c => new
                    {
                        InstructorId = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false, maxLength: 50),
                        LastName = c.String(nullable: false, maxLength: 50),
                        Phone = c.String(nullable: false, maxLength: 15),
                        Email = c.String(nullable: false, maxLength: 100),
                        LicenseNumber = c.String(nullable: false, maxLength: 50),
                        Experience = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        ApplicationUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.InstructorId)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUserId)
                .Index(t => t.ApplicationUserId);
            
            CreateTable(
                "dbo.Reviews",
                c => new
                    {
                        ReviewId = c.Int(nullable: false, identity: true),
                        RegistrationId = c.String(nullable: false, maxLength: 13),
                        InstructorId = c.Int(),
                        VehicleId = c.Int(),
                        ReviewType = c.String(nullable: false, maxLength: 20),
                        Rating = c.Int(nullable: false),
                        Comment = c.String(nullable: false, maxLength: 1000),
                        ReviewDate = c.DateTime(nullable: false),
                        IsApproved = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ReviewId)
                .ForeignKey("dbo.Instructors", t => t.InstructorId)
                .ForeignKey("dbo.Registrations", t => t.RegistrationId)
                .ForeignKey("dbo.Vehicles", t => t.VehicleId)
                .Index(t => t.RegistrationId)
                .Index(t => t.InstructorId)
                .Index(t => t.VehicleId);
            
            CreateTable(
                "dbo.Vehicles",
                c => new
                    {
                        VehicleId = c.Int(nullable: false, identity: true),
                        Make = c.String(nullable: false, maxLength: 50),
                        Model = c.String(nullable: false, maxLength: 50),
                        RegistrationNumber = c.String(nullable: false, maxLength: 20),
                        VehicleType = c.String(nullable: false, maxLength: 50),
                        Status = c.String(nullable: false, maxLength: 20),
                        CurrentMileage = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.VehicleId);
            
            CreateTable(
                "dbo.LessonTypes",
                c => new
                    {
                        LessonTypeId = c.Int(nullable: false, identity: true),
                        LessonTypeName = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        DurationMinutes = c.Int(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.LessonTypeId);
            
            CreateTable(
                "dbo.Cancellations",
                c => new
                    {
                        CancellationId = c.Int(nullable: false, identity: true),
                        BookingId = c.Int(nullable: false),
                        RegistrationId = c.String(nullable: false, maxLength: 13),
                        ActionType = c.String(nullable: false, maxLength: 20),
                        Reason = c.String(nullable: false, maxLength: 500),
                        ActionDate = c.DateTime(nullable: false),
                        RequestedBy = c.String(maxLength: 20),
                        NewLessonScheduleId = c.Int(),
                    })
                .PrimaryKey(t => t.CancellationId)
                .ForeignKey("dbo.Bookings", t => t.BookingId)
                .ForeignKey("dbo.LessonSchedules", t => t.NewLessonScheduleId)
                .ForeignKey("dbo.Registrations", t => t.RegistrationId)
                .Index(t => t.BookingId)
                .Index(t => t.RegistrationId)
                .Index(t => t.NewLessonScheduleId);
            
            CreateTable(
                "dbo.LessonProgresses",
                c => new
                    {
                        LessonProgressId = c.Int(nullable: false, identity: true),
                        RegistrationId = c.String(nullable: false, maxLength: 13),
                        BookingId = c.Int(nullable: false),
                        LessonStatus = c.String(nullable: false, maxLength: 20),
                        InstructorNotes = c.String(maxLength: 1000),
                        CompletionDate = c.DateTime(),
                        ProgressPercentage = c.Int(),
                    })
                .PrimaryKey(t => t.LessonProgressId)
                .ForeignKey("dbo.Bookings", t => t.BookingId)
                .ForeignKey("dbo.Registrations", t => t.RegistrationId)
                .Index(t => t.RegistrationId)
                .Index(t => t.BookingId);
            
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        NotificationId = c.Int(nullable: false, identity: true),
                        RegistrationId = c.String(nullable: false, maxLength: 13),
                        Title = c.String(nullable: false, maxLength: 100),
                        Message = c.String(nullable: false, maxLength: 1000),
                        NotificationType = c.String(nullable: false, maxLength: 30),
                        IsRead = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        ReadAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.NotificationId)
                .ForeignKey("dbo.Registrations", t => t.RegistrationId)
                .Index(t => t.RegistrationId);
            
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        PaymentId = c.Int(nullable: false, identity: true),
                        RegistrationId = c.String(nullable: false, maxLength: 13),
                        BookingId = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaymentMethod = c.String(nullable: false, maxLength: 30),
                        PaymentStatus = c.String(nullable: false, maxLength: 20),
                        TransactionReference = c.String(maxLength: 100),
                        PaymentDate = c.DateTime(nullable: false),
                        Notes = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.PaymentId)
                .ForeignKey("dbo.Bookings", t => t.BookingId)
                .ForeignKey("dbo.Registrations", t => t.RegistrationId)
                .Index(t => t.RegistrationId)
                .Index(t => t.BookingId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Courses",
                c => new
                    {
                        CourseId = c.Int(nullable: false, identity: true),
                        CourseName = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Duration = c.Int(nullable: false),
                        Status = c.String(nullable: false, maxLength: 20),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.CourseId);
            
            CreateTable(
                "dbo.StudentCourses",
                c => new
                    {
                        StudentCourseId = c.Int(nullable: false, identity: true),
                        RegistrationId = c.String(nullable: false, maxLength: 13),
                        CourseId = c.Int(nullable: false),
                        EnrolmentDate = c.DateTime(nullable: false),
                        Status = c.String(nullable: false, maxLength: 20),
                        CompletionDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.StudentCourseId)
                .ForeignKey("dbo.Courses", t => t.CourseId)
                .ForeignKey("dbo.Registrations", t => t.RegistrationId)
                .Index(t => t.RegistrationId)
                .Index(t => t.CourseId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.StudentCourses", "RegistrationId", "dbo.Registrations");
            DropForeignKey("dbo.StudentCourses", "CourseId", "dbo.Courses");
            DropForeignKey("dbo.Administrators", "ApplicationUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Payments", "RegistrationId", "dbo.Registrations");
            DropForeignKey("dbo.Payments", "BookingId", "dbo.Bookings");
            DropForeignKey("dbo.Notifications", "RegistrationId", "dbo.Registrations");
            DropForeignKey("dbo.LessonProgresses", "RegistrationId", "dbo.Registrations");
            DropForeignKey("dbo.LessonProgresses", "BookingId", "dbo.Bookings");
            DropForeignKey("dbo.Cancellations", "RegistrationId", "dbo.Registrations");
            DropForeignKey("dbo.Cancellations", "NewLessonScheduleId", "dbo.LessonSchedules");
            DropForeignKey("dbo.Cancellations", "BookingId", "dbo.Bookings");
            DropForeignKey("dbo.Bookings", "RegistrationId", "dbo.Registrations");
            DropForeignKey("dbo.Bookings", "LessonScheduleId", "dbo.LessonSchedules");
            DropForeignKey("dbo.LessonSchedules", "VehicleId", "dbo.Vehicles");
            DropForeignKey("dbo.LessonSchedules", "LessonTypeId", "dbo.LessonTypes");
            DropForeignKey("dbo.LessonSchedules", "InstructorId", "dbo.Instructors");
            DropForeignKey("dbo.Reviews", "VehicleId", "dbo.Vehicles");
            DropForeignKey("dbo.Reviews", "RegistrationId", "dbo.Registrations");
            DropForeignKey("dbo.Reviews", "InstructorId", "dbo.Instructors");
            DropForeignKey("dbo.Instructors", "ApplicationUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Registrations", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.StudentCourses", new[] { "CourseId" });
            DropIndex("dbo.StudentCourses", new[] { "RegistrationId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.Payments", new[] { "BookingId" });
            DropIndex("dbo.Payments", new[] { "RegistrationId" });
            DropIndex("dbo.Notifications", new[] { "RegistrationId" });
            DropIndex("dbo.LessonProgresses", new[] { "BookingId" });
            DropIndex("dbo.LessonProgresses", new[] { "RegistrationId" });
            DropIndex("dbo.Cancellations", new[] { "NewLessonScheduleId" });
            DropIndex("dbo.Cancellations", new[] { "RegistrationId" });
            DropIndex("dbo.Cancellations", new[] { "BookingId" });
            DropIndex("dbo.Reviews", new[] { "VehicleId" });
            DropIndex("dbo.Reviews", new[] { "InstructorId" });
            DropIndex("dbo.Reviews", new[] { "RegistrationId" });
            DropIndex("dbo.Instructors", new[] { "ApplicationUserId" });
            DropIndex("dbo.LessonSchedules", new[] { "VehicleId" });
            DropIndex("dbo.LessonSchedules", new[] { "InstructorId" });
            DropIndex("dbo.LessonSchedules", new[] { "LessonTypeId" });
            DropIndex("dbo.Bookings", new[] { "LessonScheduleId" });
            DropIndex("dbo.Bookings", new[] { "RegistrationId" });
            DropIndex("dbo.Registrations", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Administrators", new[] { "ApplicationUserId" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.StudentCourses");
            DropTable("dbo.Courses");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.Payments");
            DropTable("dbo.Notifications");
            DropTable("dbo.LessonProgresses");
            DropTable("dbo.Cancellations");
            DropTable("dbo.LessonTypes");
            DropTable("dbo.Vehicles");
            DropTable("dbo.Reviews");
            DropTable("dbo.Instructors");
            DropTable("dbo.LessonSchedules");
            DropTable("dbo.Bookings");
            DropTable("dbo.Registrations");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Administrators");
        }
    }
}
