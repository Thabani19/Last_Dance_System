using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Last_Dance_System.Models;
using Last_Dance_System.ViewModels;
using System.Data.Entity;

namespace Last_Dance_System.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var aspNetUserId = User.Identity.GetUserId();

            // Find registration that is linked to the current ApplicationUser
            var registration = _db.Registrations
                .SingleOrDefault(r => r.ApplicationUserId == aspNetUserId);

            if (registration == null)
                return RedirectToAction("Create", "Registration"); // or show a friendly page

            var vm = new DashboardViewModel
            {
                Registration = registration,
                Bookings = _db.Bookings
                    .Where(b => b.RegistrationId == registration.RegistrationId)
                    .Include(b => b.LessonSchedule)
                    .Include(b => b.LessonSchedule.LessonType)
                    .Include(b => b.LessonSchedule.Instructor)
                    .OrderByDescending(b => b.BookingDate)
                    .ToList(),
                Payments = _db.Payments
                    .Where(p => p.RegistrationId == registration.RegistrationId)
                    .OrderByDescending(p => p.PaymentDate)
                    .ToList(),
                LessonProgresses = _db.LessonProgresses
                    .Where(lp => lp.RegistrationId == registration.RegistrationId)
                    .Include(lp => lp.Booking)
                    .ToList(),
                Notifications = _db.Notifications
                    .Where(n => n.RegistrationId == registration.RegistrationId)
                    .ToList()
            };

            return View(vm);
        }
    }
}