using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Last_Dance_System.Models;

namespace Last_Dance_System.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // ============================================================
        // STUDENT DASHBOARD
        // ============================================================

        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();

            // Find the logged-in student's registration
            var student = db.Registrations
                .FirstOrDefault(r => r.ApplicationUserId == userId);

            // If no registration exists, send them to registration
            if (student == null)
            {
                return RedirectToAction("Register", "Account");
            }

            // ========================================================
            // CREATE DASHBOARD VIEW MODEL
            // ========================================================

            var model = new StudentDashboardViewModel
            {
                // Student information
                RegistrationId = student.RegistrationId,

                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,

                // Also populate the names used by the dashboard
                StudentName = student.FirstName,
                StudentSurname = student.LastName,
                StudentEmail = student.Email,

                // ====================================================
                // BOOKING
                // ====================================================

                BookingStatus = "No Active Booking",

                CourseName = "No Course",

                InstructorName = "Automatically Assigned",

                // ====================================================
                // PAYMENT
                // ====================================================

                PaymentStatus = "Payment Required",

                // ====================================================
                // LESSONS
                // ====================================================

                TotalLessons = 0,

                CompletedLessons = 0,

                RemainingLessons = 0,

                // ====================================================
                // PROGRESS
                // ====================================================

                ProgressPercentage = 0,

                // ====================================================
                // NEXT LESSON
                // ====================================================

                NextLessonDate = null,

                NextLessonTime = "Not Scheduled",

                // ====================================================
                // REVIEWS
                // ====================================================

                DailyReview = "No daily review available.",

                InstructorFeedback = "No instructor feedback available.",

                OverallReview = "Your overall review will become available after completing your package."
            };

            // ============================================================
            // RETURN MODEL TO VIEW
            // ============================================================

            return View(model);
        }

        // ============================================================
        // DISPOSE DATABASE
        // ============================================================

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}