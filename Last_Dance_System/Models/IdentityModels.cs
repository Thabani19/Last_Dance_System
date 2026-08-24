using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Last_Dance_System.Models
{
    // =============================================================
    // APPLICATION USER
    // =============================================================

    public class ApplicationUser : IdentityUser
    {
        public virtual Registration Registration { get; set; }


        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(
            UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(
                this,
                DefaultAuthenticationTypes.ApplicationCookie);

            return userIdentity;
        }
    }


    // =============================================================
    // APPLICATION DATABASE CONTEXT
    // =============================================================

    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base(
                "DefaultConnection",
                throwIfV1Schema: false)
        {
        }


        // =========================================================
        // DATABASE TABLES
        // =========================================================

        public DbSet<Registration> Registrations { get; set; }

        public DbSet<Instructor> Instructors { get; set; }

        public DbSet<Administrator> Administrators { get; set; }

        public DbSet<Course> Courses { get; set; }

        public DbSet<StudentCourse> StudentCourses { get; set; }

        public DbSet<Vehicle> Vehicles { get; set; }

        public DbSet<LessonType> LessonTypes { get; set; }

        public DbSet<LessonSchedule> LessonSchedules { get; set; }

        public DbSet<LessonPackage> LessonPackages { get; set; }

        public DbSet<StudentPackage> StudentPackages { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<LessonProgress> LessonProgresses { get; set; }

        public DbSet<Cancellation> Cancellations { get; set; }

        public DbSet<Review> Reviews { get; set; }

        public DbSet<Notification> Notifications { get; set; }


        // =========================================================
        // MODEL CONFIGURATION
        // =========================================================

        protected override void OnModelCreating(
            DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // =====================================================
            // REGISTRATION ↔ APPLICATION USER
            // =====================================================

            modelBuilder.Entity<Registration>()
                .HasOptional(r => r.ApplicationUser)
                .WithOptionalDependent(u => u.Registration);


            // =====================================================
            // INSTRUCTOR ↔ APPLICATION USER
            // =====================================================

            modelBuilder.Entity<Instructor>()
                .HasOptional(i => i.ApplicationUser)
                .WithMany()
                .HasForeignKey(i => i.ApplicationUserId)
                .WillCascadeOnDelete(false);


            // =====================================================
            // ADMINISTRATOR ↔ APPLICATION USER
            // =====================================================

            modelBuilder.Entity<Administrator>()
                .HasOptional(a => a.ApplicationUser)
                .WithMany()
                .HasForeignKey(a => a.ApplicationUserId)
                .WillCascadeOnDelete(false);


            // =====================================================
            // REGISTRATION → BOOKING
            // =====================================================

            modelBuilder.Entity<Booking>()
                .HasRequired(b => b.Registration)
                .WithMany(r => r.Bookings)
                .HasForeignKey(b => b.RegistrationId)
                .WillCascadeOnDelete(false);


            // =====================================================
            // LESSON SCHEDULE → BOOKING
            // =====================================================

            modelBuilder.Entity<Booking>()
                .HasRequired(b => b.LessonSchedule)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.LessonScheduleId)
                .WillCascadeOnDelete(false);


            // =====================================================
            // REGISTRATION → PAYMENT
            // =====================================================

            modelBuilder.Entity<Payment>()
                .HasRequired(p => p.Registration)
                .WithMany(r => r.Payments)
                .HasForeignKey(p => p.RegistrationId)
                .WillCascadeOnDelete(false);


            // =====================================================
            // BOOKING → PAYMENT
            // =====================================================
            //
            // OPTIONAL
            //
            // Normal lesson payments have a BookingId.
            //
            // Package payments do NOT have a BookingId.
            //
            // Therefore BookingId must be nullable.
            // =====================================================

            modelBuilder.Entity<Payment>()
                .HasOptional(p => p.Booking)
                .WithMany()
                .HasForeignKey(p => p.BookingId)
                .WillCascadeOnDelete(false);


            // =====================================================
            // STUDENT PACKAGE → PAYMENT
            // =====================================================
            //
            // OPTIONAL
            //
            // Package payments have a StudentPackageId.
            //
            // Normal lesson payments do NOT have a
            // StudentPackageId.
            //
            // Therefore StudentPackageId must be nullable.
            // =====================================================

            modelBuilder.Entity<Payment>()
                .HasOptional(p => p.StudentPackage)
                .WithMany()
                .HasForeignKey(p => p.StudentPackageId)
                .WillCascadeOnDelete(false);


            // =====================================================
            // REGISTRATION → LESSON PROGRESS
            // =====================================================

            modelBuilder.Entity<LessonProgress>()
                .HasRequired(lp => lp.Registration)
                .WithMany(r => r.LessonProgresses)
                .HasForeignKey(lp => lp.RegistrationId)
                .WillCascadeOnDelete(false);


            // =====================================================
            // BOOKING → LESSON PROGRESS
            // =====================================================

            modelBuilder.Entity<LessonProgress>()
                .HasRequired(lp => lp.Booking)
                .WithMany()
                .HasForeignKey(lp => lp.BookingId)
                .WillCascadeOnDelete(false);


            // =====================================================
            // REGISTRATION → CANCELLATION
            // =====================================================

            modelBuilder.Entity<Cancellation>()
                .HasRequired(c => c.Registration)
                .WithMany(r => r.Cancellations)
                .HasForeignKey(c => c.RegistrationId)
                .WillCascadeOnDelete(false);


            // =====================================================
            // BOOKING → CANCELLATION
            // =====================================================

            modelBuilder.Entity<Cancellation>()
                .HasRequired(c => c.Booking)
                .WithMany()
                .HasForeignKey(c => c.BookingId)
                .WillCascadeOnDelete(false);


            // =====================================================
            // NEW LESSON SCHEDULE → CANCELLATION
            // =====================================================

            modelBuilder.Entity<Cancellation>()
                .HasOptional(c => c.NewLessonSchedule)
                .WithMany()
                .HasForeignKey(c => c.NewLessonScheduleId)
                .WillCascadeOnDelete(false);


            // =====================================================
            // REGISTRATION → REVIEW
            // =====================================================

            modelBuilder.Entity<Review>()
                .HasRequired(r => r.Registration)
                .WithMany(s => s.Reviews)
                .HasForeignKey(r => r.RegistrationId)
                .WillCascadeOnDelete(false);


            // =====================================================
            // INSTRUCTOR → REVIEW
            // =====================================================

            modelBuilder.Entity<Review>()
                .HasOptional(r => r.Instructor)
                .WithMany(i => i.Reviews)
                .HasForeignKey(r => r.InstructorId)
                .WillCascadeOnDelete(false);


            // =====================================================
            // VEHICLE → REVIEW
            // =====================================================

            modelBuilder.Entity<Review>()
                .HasOptional(r => r.Vehicle)
                .WithMany(v => v.Reviews)
                .HasForeignKey(r => r.VehicleId)
                .WillCascadeOnDelete(false);


            // =====================================================
            // REGISTRATION → NOTIFICATION
            // =====================================================

            modelBuilder.Entity<Notification>()
                .HasRequired(n => n.Registration)
                .WithMany(r => r.Notifications)
                .HasForeignKey(n => n.RegistrationId)
                .WillCascadeOnDelete(false);


            // =====================================================
            // COURSE → STUDENT COURSE
            // =====================================================

            modelBuilder.Entity<StudentCourse>()
                .HasRequired(sc => sc.Course)
                .WithMany(c => c.StudentCourses)
                .HasForeignKey(sc => sc.CourseId)
                .WillCascadeOnDelete(false);


            // =====================================================
            // REGISTRATION → STUDENT COURSE
            // =====================================================

            modelBuilder.Entity<StudentCourse>()
                .HasRequired(sc => sc.Registration)
                .WithMany()
                .HasForeignKey(sc => sc.RegistrationId)
                .WillCascadeOnDelete(false);


            // =====================================================
            // LESSON TYPE → LESSON SCHEDULE
            // =====================================================

            modelBuilder.Entity<LessonSchedule>()
                .HasRequired(ls => ls.LessonType)
                .WithMany(lt => lt.LessonSchedules)
                .HasForeignKey(ls => ls.LessonTypeId)
                .WillCascadeOnDelete(false);


            // =====================================================
            // INSTRUCTOR → LESSON SCHEDULE
            // =====================================================

            modelBuilder.Entity<LessonSchedule>()
                .HasRequired(ls => ls.Instructor)
                .WithMany(i => i.LessonSchedules)
                .HasForeignKey(ls => ls.InstructorId)
                .WillCascadeOnDelete(false);


            // =====================================================
            // VEHICLE → LESSON SCHEDULE
            // =====================================================

            modelBuilder.Entity<LessonSchedule>()
                .HasRequired(ls => ls.Vehicle)
                .WithMany()
                .HasForeignKey(ls => ls.VehicleId)
                .WillCascadeOnDelete(false);


            // =====================================================
            // LESSON PACKAGE → STUDENT PACKAGE
            // =====================================================

            modelBuilder.Entity<StudentPackage>()
                .HasRequired(sp => sp.LessonPackage)
                .WithMany(lp => lp.StudentPackages)
                .HasForeignKey(sp => sp.LessonPackageId)
                .WillCascadeOnDelete(false);


            // =====================================================
            // REGISTRATION → STUDENT PACKAGE
            // =====================================================

            modelBuilder.Entity<StudentPackage>()
                .HasRequired(sp => sp.Registration)
                .WithMany()
                .HasForeignKey(sp => sp.RegistrationId)
                .WillCascadeOnDelete(false);
        }


        // =========================================================
        // CREATE DATABASE CONTEXT
        // =========================================================

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}