using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Last_Dance_System.Models;
using Last_Dance_System.ViewModels;
using System.Data.Entity;

namespace Last_Dance_System.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        // Step 1: choose package (LessonType)
        public ActionResult ChoosePackage()
        {
            var lessonTypes = _db.LessonTypes.OrderBy(lt => lt.LessonTypeName).ToList();
            return View(lessonTypes);
        }

        // Step 2: choose schedule for selected package
        public ActionResult ChooseSchedule(int lessonTypeId)
        {
            var schedules = _db.LessonSchedules
                .Where(ls => ls.LessonTypeId == lessonTypeId && ls.LessonDate >= DateTime.Today)
                .Include(ls => ls.Instructor)
                .Include(ls => ls.Vehicle)
                .OrderBy(ls => ls.LessonDate)
                .ToList();

            var vm = new BookingCreateViewModel
            {
                LessonTypeId = lessonTypeId,
                AvailableSchedules = schedules
            };

            return View(vm);
        }

        // Create booking and redirect to payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateBooking(int lessonScheduleId)
        {
            var aspNetUserId = User.Identity.GetUserId();
            var registration = _db.Registrations.SingleOrDefault(r => r.ApplicationUserId == aspNetUserId);
            if (registration == null)
                return RedirectToAction("Index", "Dashboard");

            var schedule = _db.LessonSchedules.Find(lessonScheduleId);
            if (schedule == null)
                return HttpNotFound();

            var booking = new Booking
            {
                RegistrationId = registration.RegistrationId,
                LessonScheduleId = lessonScheduleId,
                BookingDate = DateTime.Now,
                Status = "PendingPayment"
            };

            _db.Bookings.Add(booking);
            _db.SaveChanges();

            // Redirect to payment to complete transaction
            return RedirectToAction("Checkout", "Payment", new { bookingId = booking.BookingId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}