using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Last_Dance_System.Models;
using Last_Dance_System.Services;
using SvcEmail = Last_Dance_System.Services.EmailService; // use explicit alias to avoid name collision

namespace Last_Dance_System.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();
        private readonly PaymentProcessor _paymentProcessor = new PaymentProcessor();
        private readonly SvcEmail _emailService = new SvcEmail();

        // Show checkout page (you will scaffold a view that posts token or uses simulation)
        public ActionResult Checkout(int bookingId)
        {
            var booking = _db.Bookings.Find(bookingId);
            if (booking == null) return HttpNotFound();

            // Only allow owner to pay
            var aspNetUserId = User.Identity.GetUserId();
            var registration = _db.Registrations.Find(booking.RegistrationId);
            if (registration == null || registration.ApplicationUserId != aspNetUserId)
                return new HttpUnauthorizedResult();

            // You may expose amount via LessonType.Price or Course price
            var amount = booking.LessonSchedule != null ? booking.LessonSchedule.LessonType.Price : 0m;
            // eager-load for view
            _db.Entry(booking).Reference(b => b.LessonSchedule).Load();
            _db.Entry(booking.LessonSchedule).Reference(ls => ls.LessonType).Load();

            ViewBag.BookingId = bookingId;
            ViewBag.Amount = booking.LessonSchedule.LessonType.Price;
            ViewBag.Simulate = _paymentProcessor.Simulate;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Pay(int bookingId, string stripeToken = null)
        {
            var booking = _db.Bookings.Find(bookingId);
            if (booking == null) return HttpNotFound();

            var registration = _db.Registrations.Find(booking.RegistrationId);
            if (registration == null) return HttpNotFound();

            // Load price
            _db.Entry(booking).Reference(b => b.LessonSchedule).Load();
            _db.Entry(booking.LessonSchedule).Reference(ls => ls.LessonType).Load();
            var amount = booking.LessonSchedule.LessonType.Price;

            var description = $"Booking {booking.BookingId} for {registration.LastName}";

            var (success, transactionId, message) = await _paymentProcessor.ProcessPaymentAsync(
                amount, "usd", stripeToken ?? "simulated", description);

            var payment = new Payment
            {
                RegistrationId = registration.RegistrationId,
                BookingId = booking.BookingId,
                PaymentMethod = _paymentProcessor.Simulate ? "Simulated" : "Stripe",
                PaymentStatus = success ? "Completed" : "Failed",
                PaymentDate = DateTime.Now
            };
            _db.Payments.Add(payment);

            if (success)
            {
                booking.Status = "Paid";
            }
            else
            {
                booking.Status = "PaymentFailed";
            }

            _db.SaveChanges();

            // Send confirmation email on success
            if (success)
            {
                await _emailService.SendBookingConfirmationAsync(registration, booking);
            }

            TempData["PaymentMessage"] = success ? "Payment successful. Confirmation sent by email." : $"Payment failed: {message}";
            return RedirectToAction("Index", "Dashboard");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}