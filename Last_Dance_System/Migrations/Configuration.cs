namespace Last_Dance_System.Migrations
{
    using Last_Dance_System.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Last_Dance_System.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Last_Dance_System.Models.ApplicationDbContext context)
        {
          
            // 1. Create Identity Roles
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            if (!roleManager.RoleExists("Instructor"))
            {
                roleManager.Create(new IdentityRole("Instructor"));
            }

            // 2. Create Instructor Login Account
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            string instructorEmail = "instructor@tkdriving.co.za";

            var user = userManager.FindByEmail(instructorEmail);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = instructorEmail,
                    Email = instructorEmail,
                    EmailConfirmed = true
                };

                // Password: Password123!
                var result = userManager.Create(user, "Password123!");
                if (result.Succeeded)
                {
                    userManager.AddToRole(user.Id, "Instructor");
                }
            }

            // 3. Insert Instructor Profile Details
            var instructor = context.Instructors.FirstOrDefault(i => i.Email == instructorEmail);
            if (instructor == null)
            {
                instructor = new Instructor
                {
                    ApplicationUserId = user.Id,
                    FirstName = "Asanda",
                    LastName = "Sibiya",
                    Email = instructorEmail,
                    Phone = "0821234567",
                    LicenseNumber = "Code 8 / Code 10"
                };
                context.Instructors.Add(instructor);
                context.SaveChanges();
            }

            // 4. Insert Sample Students & Bookings
            var student = context.Registrations.FirstOrDefault();
            if (student == null)
            {
                student = new Registration
                {
                    FirstName = "Mandisa",
                    LastName = "Mngomezulu",
                    Email = "Mandisa@123.com",
                    Phone = "0719876543"
                };
                context.Registrations.Add(student);
                context.SaveChanges();
            }

            // 5. Insert Lessons (Today, Upcoming, Completed)
            if (!context.LessonSchedules.Any())
            {
                // Lesson Today
                var lessonToday = new LessonSchedule
                {
                    InstructorId = instructor.InstructorId,
                    LessonDate = DateTime.Today,
                    StartTime = new TimeSpan(10, 0, 0),
                    EndTime = new TimeSpan(11, 0, 0),
                    Status = "Scheduled"
                };

                // Upcoming Lesson
                var upcomingLesson = new LessonSchedule
                {
                    InstructorId = instructor.InstructorId,
                    LessonDate = DateTime.Today.AddDays(2),
                    StartTime = new TimeSpan(14, 0, 0),
                    EndTime = new TimeSpan(15, 0, 0),
                    Status = "Scheduled"
                };

                // Add or update today's and upcoming lessons
                context.LessonSchedules.AddOrUpdate(
                    l => new { l.InstructorId, l.LessonDate, l.StartTime, l.EndTime },
                    lessonToday,
                    upcomingLesson
                );
                context.SaveChanges();

                // Completed Lesson (Progress record)
                var completedSchedule = new LessonSchedule
                {
                    InstructorId = instructor.InstructorId,
                    LessonDate = DateTime.Today.AddDays(-3),
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(10, 0, 0),
                    Status = "Completed"
                };

                context.LessonSchedules.AddOrUpdate(
                    l => new { l.InstructorId, l.LessonDate, l.StartTime, l.EndTime },
                    completedSchedule
                );
                context.SaveChanges();

                var booking = new Booking
                {
                    LessonScheduleId = completedSchedule.LessonScheduleId,
                    RegistrationId = student.RegistrationId
                };
                context.Bookings.Add(booking);
                context.SaveChanges();

                context.LessonProgresses.Add(new LessonProgress
                {
                    BookingId = booking.BookingId,
                    LessonStatus = "Completed",
                    CompletionDate = DateTime.Today.AddDays(-3)
                });
                context.SaveChanges();
            }
        }
    }
}
