using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Last_Dance_System.Models;

namespace Last_Dance_System.Controllers
{
    [Authorize]
    public class LessonsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // =========================================================
        // MY LESSONS
        // =========================================================
        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();

            var student = db.Registrations
                .FirstOrDefault(r => r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction("Register", "Account");
            }

            var bookings = db.Bookings
                .Where(b =>
                    b.RegistrationId == student.RegistrationId &&
                    b.Status != "Cancelled")
                .OrderByDescending(b => b.LessonSchedule.LessonDate)
                .ToList();

            // =====================================================
            // GENERATE CHECK-IN CODES FOR CONFIRMED LESSONS
            // =====================================================

            foreach (var booking in bookings)
            {
                var lesson = db.LessonProgresses
                    .FirstOrDefault(l =>
                        l.BookingId == booking.BookingId);

                if (lesson == null)
                    continue;

                // Only confirmed lessons
                if (booking.Status != "Confirmed")
                    continue;

                // Generate check-in code if it doesn't exist
                if (string.IsNullOrEmpty(lesson.CheckInCode))
                {
                    lesson.CheckInCode = GenerateCode();
                }

                // Generate checkout code only after check-in
                // and after 30 minutes have passed
                if (lesson.CheckInConfirmed &&
                    lesson.CheckInTime.HasValue &&
                    DateTime.Now >= lesson.CheckInTime.Value.AddMinutes(30) &&
                    string.IsNullOrEmpty(lesson.CheckOutCode))
                {
                    lesson.CheckOutCode = GenerateCode();
                }
            }

            db.SaveChanges();

            return View(bookings);
        }


        // =========================================================
        // LESSON DETAILS
        // =========================================================
        public ActionResult Details(int id)
        {
            string userId = User.Identity.GetUserId();

            var student = db.Registrations
                .FirstOrDefault(r => r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction("Register", "Account");
            }

            var booking = db.Bookings
                .FirstOrDefault(b =>
                    b.BookingId == id &&
                    b.RegistrationId == student.RegistrationId);

            if (booking == null)
            {
                return HttpNotFound();
            }

            var lesson = db.LessonProgresses
                .FirstOrDefault(l =>
                    l.BookingId == booking.BookingId);

            if (lesson == null)
            {
                TempData["Error"] = "Lesson progress record was not found.";
                return RedirectToAction("Index");
            }

            // =====================================================
            // GENERATE CHECK-IN CODE
            // =====================================================

            if (booking.Status == "Confirmed" &&
                string.IsNullOrEmpty(lesson.CheckInCode))
            {
                lesson.CheckInCode = GenerateCode();

                db.SaveChanges();
            }

            // =====================================================
            // GENERATE CHECK-OUT CODE
            // ONLY AFTER 30 MINUTES
            // =====================================================

            if (lesson.CheckInConfirmed &&
                lesson.CheckInTime.HasValue &&
                DateTime.Now >= lesson.CheckInTime.Value.AddMinutes(30) &&
                string.IsNullOrEmpty(lesson.CheckOutCode))
            {
                lesson.CheckOutCode = GenerateCode();

                db.SaveChanges();
            }

            ViewBag.LessonProgress = lesson;

            return View(booking);
        }


        // =========================================================
        // CHECK-IN
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckIn(
            int id,
            string checkInCode)
        {
            string userId = User.Identity.GetUserId();

            var student = db.Registrations
                .FirstOrDefault(r =>
                    r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction("Register", "Account");
            }

            var booking = db.Bookings
                .FirstOrDefault(b =>
                    b.BookingId == id &&
                    b.RegistrationId == student.RegistrationId);

            if (booking == null)
            {
                return HttpNotFound();
            }

            // =====================================================
            // BOOKING MUST BE CONFIRMED
            // =====================================================

            if (booking.Status != "Confirmed")
            {
                TempData["Error"] =
                    "This lesson has not been confirmed.";

                return RedirectToAction(
                    "Details",
                    new { id = id });
            }

            var lesson = db.LessonProgresses
                .FirstOrDefault(l =>
                    l.BookingId == booking.BookingId);

            if (lesson == null)
            {
                TempData["Error"] =
                    "Lesson record was not found.";

                return RedirectToAction(
                    "Details",
                    new { id = id });
            }

            // =====================================================
            // ALREADY CHECKED IN
            // =====================================================

            if (lesson.CheckInConfirmed)
            {
                TempData["Error"] =
                    "You have already checked in.";

                return RedirectToAction(
                    "Details",
                    new { id = id });
            }

            // =====================================================
            // CHECK CODE
            // =====================================================

            if (string.IsNullOrWhiteSpace(checkInCode) ||
                lesson.CheckInCode != checkInCode.Trim())
            {
                TempData["Error"] =
                    "Invalid check-in code.";

                return RedirectToAction(
                    "Details",
                    new { id = id });
            }

            // =====================================================
            // CONFIRM CHECK-IN
            // =====================================================

            lesson.CheckInConfirmed = true;

            lesson.CheckInTime = DateTime.Now;

            lesson.LessonStatus = "In Progress";

            lesson.ProgressPercentage = 0;

            db.SaveChanges();

            TempData["Success"] =
                "Check-in successful. Your 30-minute lesson has started.";

            return RedirectToAction(
                "Details",
                new { id = id });
        }


        // =========================================================
        // CHECK-OUT
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckOut(
            int id,
            string checkOutCode)
        {
            string userId = User.Identity.GetUserId();

            var student = db.Registrations
                .FirstOrDefault(r =>
                    r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction(
                    "Register",
                    "Account");
            }

            var booking = db.Bookings
                .FirstOrDefault(b =>
                    b.BookingId == id &&
                    b.RegistrationId == student.RegistrationId);

            if (booking == null)
            {
                return HttpNotFound();
            }

            var lesson = db.LessonProgresses
                .FirstOrDefault(l =>
                    l.BookingId == booking.BookingId);

            if (lesson == null)
            {
                TempData["Error"] =
                    "Lesson record was not found.";

                return RedirectToAction(
                    "Details",
                    new { id = id });
            }

            // =====================================================
            // MUST CHECK IN FIRST
            // =====================================================

            if (!lesson.CheckInConfirmed ||
                !lesson.CheckInTime.HasValue)
            {
                TempData["Error"] =
                    "You must check in before checking out.";

                return RedirectToAction(
                    "Details",
                    new { id = id });
            }

            // =====================================================
            // CHECK 30 MINUTES
            // =====================================================

            DateTime lessonEndTime =
                lesson.CheckInTime.Value.AddMinutes(30);

            if (DateTime.Now < lessonEndTime)
            {
                TimeSpan remaining =
                    lessonEndTime - DateTime.Now;

                TempData["Error"] =
                    "Your lesson is still in progress. " +
                    "Please complete the full 30 minutes.";

                return RedirectToAction(
                    "Details",
                    new { id = id });
            }

            // =====================================================
            // GENERATE CHECKOUT CODE IF NEEDED
            // =====================================================

            if (string.IsNullOrEmpty(lesson.CheckOutCode))
            {
                lesson.CheckOutCode = GenerateCode();

                db.SaveChanges();
            }

            // =====================================================
            // VALIDATE CHECKOUT CODE
            // =====================================================

            if (string.IsNullOrWhiteSpace(checkOutCode) ||
                lesson.CheckOutCode != checkOutCode.Trim())
            {
                TempData["Error"] =
                    "Invalid checkout code.";

                return RedirectToAction(
                    "Details",
                    new { id = id });
            }

            // =====================================================
            // COMPLETE LESSON
            // =====================================================

            lesson.CheckOutConfirmed = true;

            lesson.CheckOutTime = DateTime.Now;

            lesson.LessonStatus = "Completed";

            lesson.CompletionDate = DateTime.Now;

            lesson.ProgressPercentage = 100;

            booking.Status = "Completed";

            db.SaveChanges();

            TempData["Success"] =
                "Lesson completed successfully.";

            return RedirectToAction(
                "Details",
                new { id = id });
        }


        // =========================================================
        // SYSTEM CODE GENERATOR
        // =========================================================
        private string GenerateCode()
        {
            Random random = new Random();

            return random.Next(100000, 999999)
                .ToString();
        }


        // =========================================================
        // DISPOSE
        // =========================================================
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