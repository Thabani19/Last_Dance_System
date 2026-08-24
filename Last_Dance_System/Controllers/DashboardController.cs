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

            var student = db.Registrations
                .FirstOrDefault(r => r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction("Register", "Account");
            }


            // ========================================================
            // GET STUDENT BOOKINGS
            // ========================================================

            var bookings = db.Bookings
                .Where(b => b.RegistrationId == student.RegistrationId)
                .OrderByDescending(b => b.BookingDate)
                .ToList();


            // ========================================================
            // LESSON STATISTICS
            // ========================================================

            int totalLessons = bookings.Count;

            int completedLessons = bookings
                .Count(b => b.Status == "Completed");

            int remainingLessons = bookings
                .Count(b =>
                    b.Status != "Completed" &&
                    b.Status != "Cancelled");


            int progressPercentage = totalLessons > 0
                ? (completedLessons * 100) / totalLessons
                : 0;


            // ========================================================
            // UPCOMING BOOKING
            // ========================================================

            var upcomingBooking = bookings
                .Where(b =>
                    b.LessonSchedule != null &&
                    b.LessonSchedule.LessonDate >= DateTime.Today &&
                    b.Status != "Cancelled" &&
                    b.Status != "Completed")
                .OrderBy(b => b.LessonSchedule.LessonDate)
                .ThenBy(b => b.LessonSchedule.StartTime)
                .FirstOrDefault();


            // ========================================================
            // DEFAULT UPCOMING INFORMATION
            // ========================================================

            DateTime? upcomingLessonDate = null;

            int? upcomingLessonID = null;

            string upcomingLessonTime = "Not Scheduled";

            string upcomingLessonType = "No Lesson";

            string upcomingInstructor = "Not Assigned";

            string upcomingVehicle = "Not Assigned";

            string bookingStatus = "No Active Booking";


            // ========================================================
            // LOAD UPCOMING LESSON
            // ========================================================

            if (upcomingBooking != null &&
                upcomingBooking.LessonSchedule != null)
            {
                var schedule = upcomingBooking.LessonSchedule;

                upcomingLessonID = upcomingBooking.BookingId;

                upcomingLessonDate = schedule.LessonDate;


                upcomingLessonTime =
                    schedule.StartTime.ToString(@"hh\:mm")
                    + " - " +
                    schedule.EndTime.ToString(@"hh\:mm");


                // ====================================================
                // LESSON TYPE
                // ====================================================

                if (schedule.LessonType != null)
                {
                    upcomingLessonType =
                        schedule.LessonType.LessonTypeName;
                }


                // ====================================================
                // INSTRUCTOR
                // ====================================================

                if (schedule.Instructor != null)
                {
                    upcomingInstructor =
                        schedule.Instructor.FirstName
                        + " "
                        + schedule.Instructor.LastName;
                }


                // ====================================================
                // VEHICLE
                // ====================================================

                if (schedule.Vehicle != null)
                {
                    upcomingVehicle =
                        schedule.Vehicle.Make
                        + " "
                        + schedule.Vehicle.Model
                        + " ("
                        + schedule.Vehicle.RegistrationNumber
                        + ")";
                }


                // ====================================================
                // BOOKING STATUS
                // ====================================================

                bookingStatus = upcomingBooking.Status;
            }


            // ========================================================
            // CREATE DASHBOARD MODEL
            // ========================================================

            var model = new StudentDashboardViewModel
            {
                StudentID = student.RegistrationId.ToString(),

                FirstName = student.FirstName,

                LastName = student.LastName,

                Email = student.Email,


                TotalLessons = totalLessons,

                CompletedLessons = completedLessons,

                RemainingLessons = remainingLessons,

                ProgressPercentage = progressPercentage,


                HasUpcomingLesson =
                    upcomingBooking != null,

                UpcomingLessonID =
                    upcomingLessonID,

                UpcomingLessonDate =
                    upcomingLessonDate,

                UpcomingLessonTime =
                    upcomingLessonTime,

                UpcomingLessonType =
                    upcomingLessonType,

                UpcomingInstructor =
                    upcomingInstructor,

                UpcomingVehicle =
                    upcomingVehicle,

                UpcomingLocation =
                    "Driving School",

                BookingStatus =
                    bookingStatus,


                // ====================================================
                // PAYMENT
                // ====================================================

                PaymentStatus = "Payment Required",

                TotalPaid = 0m,

                OutstandingAmount = 0m,


                // ====================================================
                // REVIEW
                // ====================================================

                HasPendingReview = false,

                ReviewLessonID = null,


                // ====================================================
                // FEEDBACK
                // ====================================================

                HasInstructorFeedback = false,

                LatestFeedback =
                    "No instructor feedback available."
            };


            return View(model);
        }



        // ============================================================
        // PROFILE
        // ============================================================

        public ActionResult Profile()
        {
            string userId = User.Identity.GetUserId();

            var student = db.Registrations
                .FirstOrDefault(r =>
                    r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction("Register", "Account");
            }

            return View(student);
        }



        // ============================================================
        // EDIT PROFILE - GET
        // ============================================================

        [HttpGet]
        public ActionResult EditProfile()
        {
            string userId = User.Identity.GetUserId();

            var student = db.Registrations
                .FirstOrDefault(r =>
                    r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction("Register", "Account");
            }

            return View(student);
        }



        // ============================================================
        // EDIT PROFILE - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(
            [Bind(Include =
                "RegistrationId,FirstName,LastName,Gender,DateOfBirth,Phone,Email,Address")]
            Registration student)
        {
            string userId = User.Identity.GetUserId();


            // ========================================================
            // FIND ORIGINAL STUDENT
            // ========================================================

            var existingStudent = db.Registrations
                .FirstOrDefault(r =>
                    r.RegistrationId == student.RegistrationId &&
                    r.ApplicationUserId == userId);


            // ========================================================
            // SECURITY CHECK
            // ========================================================

            if (existingStudent == null)
            {
                return HttpNotFound();
            }


            // ========================================================
            // VALIDATION
            // ========================================================

            if (!ModelState.IsValid)
            {
                return View(student);
            }


            // ========================================================
            // UPDATE INFORMATION
            // ========================================================

            existingStudent.FirstName =
                student.FirstName;

            existingStudent.LastName =
                student.LastName;

            existingStudent.Gender =
                student.Gender;

            existingStudent.DateOfBirth =
                student.DateOfBirth;

            existingStudent.Phone =
                student.Phone;

            existingStudent.Email =
                student.Email;

            existingStudent.Address =
                student.Address;


            // ========================================================
            // SAVE
            // ========================================================

            db.SaveChanges();


            // ========================================================
            // RETURN TO PROFILE
            // ========================================================

            return RedirectToAction("Profile");
        }



        // ============================================================
        // DISPOSE
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