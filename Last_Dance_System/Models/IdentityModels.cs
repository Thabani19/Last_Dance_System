using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Last_Dance_System.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
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

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Registration ↔ ApplicationUser
            modelBuilder.Entity<Registration>()
                .HasOptional(r => r.ApplicationUser)
                .WithOptionalDependent(u => u.Registration);

            // Instructor ↔ ApplicationUser
            modelBuilder.Entity<Instructor>()
                .HasOptional(i => i.ApplicationUser)
                .WithMany()
                .HasForeignKey(i => i.ApplicationUserId)
                .WillCascadeOnDelete(false);

            // Administrator ↔ ApplicationUser
            modelBuilder.Entity<Administrator>()
                .HasOptional(a => a.ApplicationUser)
                .WithMany()
                .HasForeignKey(a => a.ApplicationUserId)
                .WillCascadeOnDelete(false);

            // Registration → Booking
            modelBuilder.Entity<Booking>()
                .HasRequired(b => b.Registration)
                .WithMany(r => r.Bookings)
                .HasForeignKey(b => b.RegistrationId)
                .WillCascadeOnDelete(false);

            // LessonSchedule → Booking
            modelBuilder.Entity<Booking>()
                .HasRequired(b => b.LessonSchedule)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.LessonScheduleId)
                .WillCascadeOnDelete(false);

            // Registration → Payment
            modelBuilder.Entity<Payment>()
                .HasRequired(p => p.Registration)
                .WithMany(r => r.Payments)
                .HasForeignKey(p => p.RegistrationId)
                .WillCascadeOnDelete(false);

            // Booking → Payment
            modelBuilder.Entity<Payment>()
                .HasRequired(p => p.Booking)
                .WithMany()
                .HasForeignKey(p => p.BookingId)
                .WillCascadeOnDelete(false);

            // Registration → LessonProgress
            modelBuilder.Entity<LessonProgress>()
                .HasRequired(lp => lp.Registration)
                .WithMany(r => r.LessonProgresses)
                .HasForeignKey(lp => lp.RegistrationId)
                .WillCascadeOnDelete(false);

            // Booking → LessonProgress
            modelBuilder.Entity<LessonProgress>()
                .HasRequired(lp => lp.Booking)
                .WithMany()
                .HasForeignKey(lp => lp.BookingId)
                .WillCascadeOnDelete(false);

            // Registration → Cancellation
            modelBuilder.Entity<Cancellation>()
                .HasRequired(c => c.Registration)
                .WithMany(r => r.Cancellations)
                .HasForeignKey(c => c.RegistrationId)
                .WillCascadeOnDelete(false);

            // Booking → Cancellation
            modelBuilder.Entity<Cancellation>()
                .HasRequired(c => c.Booking)
                .WithMany()
                .HasForeignKey(c => c.BookingId)
                .WillCascadeOnDelete(false);

            // New LessonSchedule → Cancellation
            modelBuilder.Entity<Cancellation>()
                .HasOptional(c => c.NewLessonSchedule)
                .WithMany()
                .HasForeignKey(c => c.NewLessonScheduleId)
                .WillCascadeOnDelete(false);

            // Registration → Review
            modelBuilder.Entity<Review>()
                .HasRequired(r => r.Registration)
                .WithMany(s => s.Reviews)
                .HasForeignKey(r => r.RegistrationId)
                .WillCascadeOnDelete(false);

            // Instructor → Review
            modelBuilder.Entity<Review>()
                .HasOptional(r => r.Instructor)
                .WithMany(i => i.Reviews)
                .HasForeignKey(r => r.InstructorId)
                .WillCascadeOnDelete(false);

            // Vehicle → Review
            modelBuilder.Entity<Review>()
                .HasOptional(r => r.Vehicle)
                .WithMany(v => v.Reviews)
                .HasForeignKey(r => r.VehicleId)
                .WillCascadeOnDelete(false);

            // Registration → Notification
            modelBuilder.Entity<Notification>()
                .HasRequired(n => n.Registration)
                .WithMany(r => r.Notifications)
                .HasForeignKey(n => n.RegistrationId)
                .WillCascadeOnDelete(false);

            // Course → StudentCourse
            modelBuilder.Entity<StudentCourse>()
                .HasRequired(sc => sc.Course)
                .WithMany(c => c.StudentCourses)
                .HasForeignKey(sc => sc.CourseId)
                .WillCascadeOnDelete(false);

            // Registration → StudentCourse
            modelBuilder.Entity<StudentCourse>()
                .HasRequired(sc => sc.Registration)
                .WithMany()
                .HasForeignKey(sc => sc.RegistrationId)
                .WillCascadeOnDelete(false);

            // LessonType → LessonSchedule
            modelBuilder.Entity<LessonSchedule>()
                .HasRequired(ls => ls.LessonType)
                .WithMany(lt => lt.LessonSchedules)
                .HasForeignKey(ls => ls.LessonTypeId)
                .WillCascadeOnDelete(false);

            // Instructor → LessonSchedule
            modelBuilder.Entity<LessonSchedule>()
                .HasRequired(ls => ls.Instructor)
                .WithMany(i => i.LessonSchedules)
                .HasForeignKey(ls => ls.InstructorId)
                .WillCascadeOnDelete(false);

            // Vehicle → LessonSchedule
            modelBuilder.Entity<LessonSchedule>()
                .HasRequired(ls => ls.Vehicle)
                .WithMany()
                .HasForeignKey(ls => ls.VehicleId)
                .WillCascadeOnDelete(false);
            // LessonPackage → StudentPackage
            modelBuilder.Entity<StudentPackage>()
                .HasRequired(sp => sp.LessonPackage)
                .WithMany(lp => lp.StudentPackages)
                .HasForeignKey(sp => sp.LessonPackageId)
                .WillCascadeOnDelete(false);

            // Registration → StudentPackage
            modelBuilder.Entity<StudentPackage>()
                .HasRequired(sp => sp.Registration)
                .WithMany()
                .HasForeignKey(sp => sp.RegistrationId)
                .WillCascadeOnDelete(false);
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }

}