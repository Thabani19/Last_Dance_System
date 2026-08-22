using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Last_Dance_System.Models;

namespace Last_Dance_System.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // ============================================================
        // BOOKING LIST
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

            var bookings = db.Bookings
                .Where(b => b.RegistrationId == student.RegistrationId)
                .OrderByDescending(b => b.BookingDate)
                .ToList();

            return View(bookings);
        }


        // ============================================================
        // CREATE BOOKING - GET
        // ============================================================

        public ActionResult Create()
        {
            string userId = User.Identity.GetUserId();

            var student = db.Registrations
                .FirstOrDefault(r => r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction("Register", "Account");
            }

            // Only show available future lesson schedules
            var schedules = db.LessonSchedules
                .Where(s =>
                    s.Status == "Available" &&
                    s.LessonDate >= DateTime.Today
                )
                .OrderBy(s => s.LessonDate)
                .ThenBy(s => s.StartTime)
                .ToList();

            return View(schedules);
        }


        // ============================================================
        // CREATE BOOKING - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int lessonScheduleId)
        {
            string userId = User.Identity.GetUserId();

            var student = db.Registrations
                .FirstOrDefault(r => r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction("Register", "Account");
            }

            // Find selected schedule
            var schedule = db.LessonSchedules
                .FirstOrDefault(s =>
                    s.LessonScheduleId == lessonScheduleId &&
                    s.Status == "Available");

            if (schedule == null)
            {
                TempData["Error"] =
                    "This lesson is no longer available.";

                return RedirectToAction("Create");
            }


            // ========================================================
            // PREVENT DUPLICATE BOOKING
            // ========================================================

            bool alreadyBooked = db.Bookings.Any(b =>
                b.RegistrationId == student.RegistrationId &&
                b.LessonScheduleId == lessonScheduleId &&
                b.Status != "Cancelled");

            if (alreadyBooked)
            {
                TempData["Error"] =
                    "You have already booked this lesson.";

                return RedirectToAction("Index");
            }


            // ========================================================
            // CREATE BOOKING
            // ========================================================

            var booking = new Booking
            {
                RegistrationId = student.RegistrationId,

                LessonScheduleId = schedule.LessonScheduleId,

                BookingDate = DateTime.Now,

                // Important:
                // Booking is NOT confirmed until payment.
                Status = "PendingPayment",

                Notes = "Booking created. Payment required."
            };

            db.Bookings.Add(booking);

            db.SaveChanges();


            // ========================================================
            // CREATE LESSON PROGRESS
            // ========================================================

            var lessonProgress = new LessonProgress
            {
                RegistrationId = student.RegistrationId,

                BookingId = booking.BookingId,

                LessonStatus = "PendingPayment",

                ProgressPercentage = 0,

                CheckInCode = new Random().Next(100000, 999999).ToString()
            };

            db.LessonProgresses.Add(lessonProgress);

            db.SaveChanges();


            // ========================================================
            // REDIRECT TO PAYMENT
            // ========================================================

            return RedirectToAction(
                "Index",
                "Payment",
                new { bookingId = booking.BookingId }
            );
        }


        // ============================================================
        // CANCEL BOOKING
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            string userId = User.Identity.GetUserId();

            var student = db.Registrations
                .FirstOrDefault(r => r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var booking = db.Bookings
                .FirstOrDefault(b =>
                    b.BookingId == id &&
                    b.RegistrationId == student.RegistrationId);

            if (booking == null)
            {
                return HttpNotFound();
            }


            // Don't allow cancellation of completed lessons
            if (booking.Status == "Completed")
            {
                TempData["Error"] =
                    "A completed lesson cannot be cancelled.";

                return RedirectToAction("Index");
            }


            booking.Status = "Cancelled";


            // Update lesson progress
            var progress = db.LessonProgresses
                .FirstOrDefault(lp => lp.BookingId == booking.BookingId);

            if (progress != null)
            {
                progress.LessonStatus = "Cancelled";
            }


            db.SaveChanges();

            return RedirectToAction("Index");
        }


        // ============================================================
        // DETAILS
        // ============================================================

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

            var lessonProgress = db.LessonProgresses
                .FirstOrDefault(lp => lp.BookingId == booking.BookingId);

            // Generate checkout code after 30 minutes
            if (lessonProgress != null &&
                lessonProgress.CheckInConfirmed &&
                !lessonProgress.CheckOutConfirmed &&
                lessonProgress.CheckInTime.HasValue &&
                DateTime.Now >= lessonProgress.CheckInTime.Value.AddMinutes(30) &&
                string.IsNullOrEmpty(lessonProgress.CheckOutCode))
            {
                lessonProgress.CheckOutCode =
                    new Random().Next(100000, 999999).ToString();

                db.SaveChanges();
            }

            ViewBag.LessonProgress = lessonProgress;

            return View(booking);
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