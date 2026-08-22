using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Last_Dance_System.Models;

namespace Last_Dance_System.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        // ============================================================
        // PAYMENT PAGE
        // ============================================================

        [HttpGet]
        public ActionResult Create(int bookingId)
        {
            string userId = User.Identity.GetUserId();

            var student = db.Registrations
                .FirstOrDefault(r => r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction("Register", "Account");
            }


            // Make sure booking belongs to logged-in student
            var booking = db.Bookings
                .FirstOrDefault(b =>
                    b.BookingId == bookingId &&
                    b.RegistrationId == student.RegistrationId);

            if (booking == null)
            {
                return HttpNotFound();
            }


            // Don't allow payment for an already confirmed booking
            if (booking.Status == "Confirmed")
            {
                return RedirectToAction("Index", "Lessons");
            }


            // Get lesson information
            var schedule = db.LessonSchedules
                .FirstOrDefault(s =>
                    s.LessonScheduleId == booking.LessonScheduleId);

            if (schedule == null)
            {
                return HttpNotFound();
            }


            // Get price from LessonType
            decimal amount = schedule.LessonType.Price;


            ViewBag.BookingId = booking.BookingId;
            ViewBag.Amount = amount;
            ViewBag.LessonDate = schedule.LessonDate;
            ViewBag.StartTime = schedule.StartTime;
            ViewBag.EndTime = schedule.EndTime;

            if (schedule.Instructor != null)
            {
                ViewBag.InstructorName =
                    schedule.Instructor.FirstName + " " +
                    schedule.Instructor.LastName;
            }
            else
            {
                ViewBag.InstructorName = "Automatically Assigned";
            }


            return View();
        }


        // ============================================================
        // PROCESS PAYMENT
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            int bookingId,
            string paymentMethod)
        {
            string userId = User.Identity.GetUserId();

            var student = db.Registrations
                .FirstOrDefault(r => r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction("Register", "Account");
            }


            // Find booking belonging to student
            var booking = db.Bookings
                .FirstOrDefault(b =>
                    b.BookingId == bookingId &&
                    b.RegistrationId == student.RegistrationId);

            if (booking == null)
            {
                return HttpNotFound();
            }


            // Prevent duplicate payment
            if (booking.Status == "Confirmed")
            {
                return RedirectToAction("Index", "Lessons");
            }


            // Get lesson schedule
            var schedule = db.LessonSchedules
                .FirstOrDefault(s =>
                    s.LessonScheduleId == booking.LessonScheduleId);

            if (schedule == null)
            {
                return HttpNotFound();
            }


            // ========================================================
            // GET LESSON PRICE
            // ========================================================

            decimal amount = schedule.LessonType.Price;


            // ========================================================
            // CREATE PAYMENT
            // ========================================================

            var payment = new Payment
            {
                RegistrationId = student.RegistrationId,

                BookingId = booking.BookingId,

                Amount = amount,

                PaymentMethod = paymentMethod,

                PaymentStatus = "Paid",

                TransactionReference =
                    "TXN-" + Guid.NewGuid()
                    .ToString("N")
                    .Substring(0, 10)
                    .ToUpper(),

                PaymentDate = DateTime.Now,

                Notes = "Payment processed successfully."
            };

            db.Payments.Add(payment);

            // ========================================================
            // CONFIRM BOOKING
            // ========================================================

            booking.Status = "Confirmed";

            // ========================================================
            // CONFIRM LESSON PROGRESS
            // ========================================================

            var lessonProgress = db.LessonProgresses
                .FirstOrDefault(lp => lp.BookingId == booking.BookingId);

            if (lessonProgress != null)
            {
                lessonProgress.LessonStatus = "Confirmed";
            }
            // ========================================================
            // UPDATE LESSON PROGRESS
            // ========================================================

            var progress = db.LessonProgresses
                .FirstOrDefault(lp => lp.BookingId == booking.BookingId);

            if (progress != null)
            {
                progress.LessonStatus = "Confirmed";
                progress.ProgressPercentage = 0;
            }


            // ========================================================
            // SAVE
            // ========================================================

            db.SaveChanges();


            // ========================================================
            // SAVE
            // ========================================================

            db.SaveChanges();


            // ========================================================
            // REDIRECT TO LESSONS
            // ========================================================

            TempData["Success"] =
                "Payment successful. Your lesson has been confirmed.";

            return RedirectToAction("Index", "Lessons");
        }


        // ============================================================
        // PAYMENT HISTORY
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


            var payments = db.Payments
                .Where(p =>
                    p.RegistrationId == student.RegistrationId)
                .OrderByDescending(p => p.PaymentDate)
                .ToList();


            return View(payments);
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