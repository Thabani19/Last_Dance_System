using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Last_Dance_System.Models;

namespace Last_Dance_System.Controllers
{
    [Authorize]
    public class InstructorDashboardController : Controller
    {
        private readonly ApplicationDbContext db;

        public InstructorDashboardController()
        {
            db = new ApplicationDbContext();
        }


        // =========================================================
        // GET: InstructorDashboard
        // =========================================================

        public ActionResult Index()
        {
            var instructor = GetCurrentInstructor();

            if (instructor == null)
            {
                return HttpNotFound(
                    "Instructor profile was not found.");
            }

            DateTime today = DateTime.Today;
            TimeSpan currentTime = DateTime.Now.TimeOfDay;


            // =====================================================
            // ALL LESSONS BELONGING TO THIS INSTRUCTOR
            // =====================================================

            var instructorLessons = db.LessonSchedules
                .Where(l =>
                    l.InstructorId ==
                    instructor.InstructorId)
                .ToList();


            // =====================================================
            // TODAY'S LESSONS
            // =====================================================

            var todaysLessons = instructorLessons
                .Where(l =>
                    l.LessonDate.Date == today)
                .OrderBy(l => l.StartTime)
                .ToList();


            // =====================================================
            // UPCOMING LESSONS
            // =====================================================

            var upcomingLessons = instructorLessons
                .Where(l =>
                    l.LessonDate.Date > today
                    ||
                    (
                        l.LessonDate.Date == today
                        &&
                        l.StartTime > currentTime
                    ))
                .OrderBy(l => l.LessonDate)
                .ThenBy(l => l.StartTime)
                .ToList();


            // =====================================================
            // COMPLETED / ENDED LESSONS
            // =====================================================

            var completedLessons = instructorLessons
                .Where(l =>
                    l.LessonDate.Date < today
                    ||
                    (
                        l.LessonDate.Date == today
                        &&
                        l.EndTime <= currentTime
                    ))
                .OrderByDescending(l => l.LessonDate)
                .ThenByDescending(l => l.StartTime)
                .ToList();


            // =====================================================
            // BOOKED LESSONS
            // =====================================================

            var bookedLessons = instructorLessons
                .Where(l =>
                    l.Bookings.Any(b =>
                        !string.Equals(
                            b.Status,
                            "Cancelled",
                            StringComparison.OrdinalIgnoreCase)))
                .OrderBy(l => l.LessonDate)
                .ThenBy(l => l.StartTime)
                .ToList();


            // =====================================================
            // DASHBOARD STATISTICS
            // =====================================================

            ViewBag.InstructorId =
                instructor.InstructorId;

            ViewBag.InstructorName =
                instructor.FirstName +
                " " +
                instructor.LastName;

            ViewBag.TodayLessons =
                todaysLessons.Count;

            ViewBag.UpcomingLessons =
                upcomingLessons.Count;

            ViewBag.CompletedLessons =
                completedLessons.Count;

            ViewBag.TotalLessons =
                instructorLessons.Count;

            ViewBag.BookedLessons =
                bookedLessons.Count;


            // =====================================================
            // PASS LESSONS TO DASHBOARD
            // =====================================================

            ViewBag.TodaysLessons =
                todaysLessons;

            ViewBag.UpcomingLessonList =
                upcomingLessons;

            ViewBag.CompletedLessonList =
                completedLessons;


            return View();
        }


        // =========================================================
        // UPCOMING
        // =========================================================

        // GET: InstructorDashboard/Upcoming

        public ActionResult Upcoming()
        {
            var instructor = GetCurrentInstructor();

            if (instructor == null)
            {
                return HttpNotFound(
                    "Instructor profile was not found.");
            }

            DateTime today = DateTime.Today;
            TimeSpan currentTime =
                DateTime.Now.TimeOfDay;


            var lessons = db.LessonSchedules
                .Where(l =>
                    l.InstructorId ==
                    instructor.InstructorId
                    &&
                    (
                        l.LessonDate > today
                        ||
                        (
                            l.LessonDate == today
                            &&
                            l.StartTime > currentTime
                        )
                    ))
                .OrderBy(l => l.LessonDate)
                .ThenBy(l => l.StartTime)
                .ToList();


            ViewBag.InstructorName =
                instructor.FirstName +
                " " +
                instructor.LastName;


            return View(lessons);
        }


        // =========================================================
        // BOOKED LESSONS
        // =========================================================

        // GET: InstructorDashboard/Booked

        public ActionResult Booked()
        {
            var instructor = GetCurrentInstructor();

            if (instructor == null)
            {
                return HttpNotFound(
                    "Instructor profile was not found.");
            }


            DateTime today = DateTime.Today;
            TimeSpan currentTime =
                DateTime.Now.TimeOfDay;


            var lessons = db.LessonSchedules
                .Where(l =>
                    l.InstructorId ==
                    instructor.InstructorId
                    &&
                    l.Bookings.Any(b =>
                        !string.Equals(
                            b.Status,
                            "Cancelled",
                            StringComparison.OrdinalIgnoreCase))
                    &&
                    (
                        l.LessonDate > today
                        ||
                        (
                            l.LessonDate == today
                            &&
                            l.EndTime > currentTime
                        )
                    ))
                .OrderBy(l => l.LessonDate)
                .ThenBy(l => l.StartTime)
                .ToList();


            ViewBag.InstructorName =
                instructor.FirstName +
                " " +
                instructor.LastName;


            return View(lessons);
        }


        // =========================================================
        // HISTORY
        // =========================================================

        // GET: InstructorDashboard/History

        public ActionResult History()
        {
            var instructor = GetCurrentInstructor();

            if (instructor == null)
            {
                return HttpNotFound(
                    "Instructor profile was not found.");
            }


            DateTime today = DateTime.Today;
            TimeSpan currentTime =
                DateTime.Now.TimeOfDay;


            // -----------------------------------------------------
            // Only this instructor's lessons
            // -----------------------------------------------------

            var history = db.LessonSchedules
                .Where(l =>
                    l.InstructorId ==
                    instructor.InstructorId
                    &&
                    (
                        l.LessonDate < today
                        ||
                        (
                            l.LessonDate == today
                            &&
                            l.EndTime <= currentTime
                        )
                        ||
                        l.Status == "Cancelled"
                    ))
                .OrderByDescending(l => l.LessonDate)
                .ThenByDescending(l => l.StartTime)
                .ToList();


            ViewBag.InstructorName =
                instructor.FirstName +
                " " +
                instructor.LastName;


            return View(history);
        }


        // =========================================================
        // PROFILE
        // =========================================================

        // GET: InstructorDashboard/Profile

        public ActionResult Profile()
        {
            var instructor = GetCurrentInstructor();

            if (instructor == null)
            {
                return HttpNotFound(
                    "Instructor profile was not found.");
            }


            return View(instructor);
        }


        // =========================================================
        // CANCEL LESSON - GET
        // =========================================================

        // GET: InstructorDashboard/Cancel/5

        public ActionResult Cancel(int id)
        {
            var instructor = GetCurrentInstructor();

            if (instructor == null)
            {
                return HttpNotFound(
                    "Instructor profile was not found.");
            }


            // -----------------------------------------------------
            // Find booking
            // -----------------------------------------------------

            var booking = db.Bookings
                .FirstOrDefault(b =>
                    b.BookingId == id);


            if (booking == null)
            {
                return HttpNotFound(
                    "Booking was not found.");
            }


            // -----------------------------------------------------
            // SECURITY CHECK
            //
            // Make absolutely sure this lesson belongs to
            // the currently logged-in instructor.
            // -----------------------------------------------------

            if (booking.LessonSchedule == null ||
                booking.LessonSchedule.InstructorId !=
                instructor.InstructorId)
            {
                return new HttpStatusCodeResult(
                    403,
                    "You are not authorized to cancel this lesson.");
            }


            // -----------------------------------------------------
            // Already cancelled?
            // -----------------------------------------------------

            if (string.Equals(
                booking.Status,
                "Cancelled",
                StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] =
                    "This lesson has already been cancelled.";

                return RedirectToAction("Booked");
            }


            return View(booking);
        }


        // =========================================================
        // CANCEL LESSON - POST
        // =========================================================

        // POST: InstructorDashboard/Cancel

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(
            int id,
            string reason)
        {
            var instructor = GetCurrentInstructor();

            if (instructor == null)
            {
                return HttpNotFound(
                    "Instructor profile was not found.");
            }


            // -----------------------------------------------------
            // Validate reason
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] =
                    "Please provide a reason for cancelling the lesson.";

                return RedirectToAction(
                    "Cancel",
                    new { id = id });
            }


            // -----------------------------------------------------
            // Find booking
            // -----------------------------------------------------

            var booking = db.Bookings
                .FirstOrDefault(b =>
                    b.BookingId == id);


            if (booking == null)
            {
                return HttpNotFound(
                    "Booking was not found.");
            }


            // -----------------------------------------------------
            // SECURITY
            //
            // Instructor can ONLY cancel their own lesson.
            // -----------------------------------------------------

            if (booking.LessonSchedule == null ||
                booking.LessonSchedule.InstructorId !=
                instructor.InstructorId)
            {
                return new HttpStatusCodeResult(
                    403,
                    "You are not authorized to cancel this lesson.");
            }


            // -----------------------------------------------------
            // Already cancelled
            // -----------------------------------------------------

            if (string.Equals(
                booking.Status,
                "Cancelled",
                StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] =
                    "This lesson has already been cancelled.";

                return RedirectToAction("Booked");
            }


            // =====================================================
            // CANCEL BOOKING
            // =====================================================

            booking.Status = "Cancelled";


            // =====================================================
            // CANCEL LESSON SCHEDULE
            // =====================================================

            if (booking.LessonSchedule != null)
            {
                booking.LessonSchedule.Status =
                    "Cancelled";
            }


            // =====================================================
            // RETURN STUDENT CREDIT
            // =====================================================

            if (booking.StudentPackageId.HasValue)
            {
                var studentPackage =
                    db.StudentPackages
                        .FirstOrDefault(sp =>
                            sp.StudentPackageId ==
                            booking.StudentPackageId.Value);


                if (studentPackage != null)
                {
                    studentPackage.LessonsRemaining++;
                }
            }


            // =====================================================
            // CREATE CANCELLATION RECORD
            // =====================================================

            var cancellation =
                new Cancellation
                {
                    BookingId =
                        booking.BookingId,

                    RegistrationId =
                        booking.RegistrationId,

                    ActionType =
                        "Cancelled",

                    Reason =
                        reason,

                    ActionDate =
                        DateTime.Now,

                    RequestedBy =
                        "Instructor",

                    NewLessonScheduleId =
                        null
                };


            db.Cancellations.Add(
                cancellation);


            // =====================================================
            // CREATE STUDENT NOTIFICATION
            // =====================================================

            string lessonDate =
                booking.LessonSchedule != null
                    ? booking.LessonSchedule
                        .LessonDate
                        .ToString("dd MMM yyyy")
                    : "scheduled lesson";


            string lessonTime =
                booking.LessonSchedule != null
                    ? booking.LessonSchedule.StartTime
                        .ToString(@"hh\:mm")
                    : "";


            var notification =
                new Notification
                {
                    RegistrationId =
                        booking.RegistrationId,

                    Title =
                        "Lesson Cancelled by Instructor",

                    Message =
                        "Your driving lesson scheduled for "
                        + lessonDate
                        + " at "
                        + lessonTime
                        + " has been cancelled by your instructor. "
                        + "Your lesson credit has been returned. "
                        + "Reason: "
                        + reason,

                    NotificationType =
                        "Cancellation",

                    IsRead =
                        false,

                    CreatedAt =
                        DateTime.Now,

                    ReadAt =
                        null
                };


            db.Notifications.Add(
                notification);


            // =====================================================
            // SAVE EVERYTHING
            // =====================================================

            try
            {
                db.SaveChanges();
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "The lesson could not be cancelled.";

                return RedirectToAction(
                    "Booked");
            }


            TempData["Success"] =
                "Lesson cancelled successfully. "
                + "The student's lesson credit has been returned "
                + "and the student has been notified.";


            return RedirectToAction(
                "Booked");
        }


        // =========================================================
        // GET CURRENT INSTRUCTOR
        // =========================================================

        private Instructor GetCurrentInstructor()
        {
            string applicationUserId =
                User.Identity.GetUserId();


            if (string.IsNullOrEmpty(
                applicationUserId))
            {
                return null;
            }


            return db.Instructors
                .FirstOrDefault(i =>
                    i.ApplicationUserId ==
                    applicationUserId);
        }


        // =========================================================
        // DISPOSE
        // =========================================================

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}