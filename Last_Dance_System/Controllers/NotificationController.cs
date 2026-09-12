using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Last_Dance_System.Models;

namespace Last_Dance_System.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private ApplicationDbContext db =
            new ApplicationDbContext();


        // ============================================================
        // STUDENT NOTIFICATIONS
        // ============================================================

        public ActionResult Index()
        {
            string userId =
                User.Identity.GetUserId();


            // ========================================================
            // FIND LOGGED-IN STUDENT
            // ========================================================

            var student =
                db.Registrations
                .FirstOrDefault(r =>
                    r.ApplicationUserId == userId);


            if (student == null)
            {
                return RedirectToAction(
                    "Register",
                    "Account");
            }


            // ========================================================
            // GET STUDENT NOTIFICATIONS
            // ========================================================

            var notifications =
                db.Notifications
                .Where(n =>
                    n.RegistrationId ==
                    student.RegistrationId)
                .OrderByDescending(n =>
                    n.CreatedAt)
                .ToList();


            return View(notifications);
        }


        // ============================================================
        // MARK NOTIFICATION AS READ
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAsRead(int id)
        {
            string userId =
                User.Identity.GetUserId();


            var student =
                db.Registrations
                .FirstOrDefault(r =>
                    r.ApplicationUserId == userId);


            if (student == null)
            {
                return RedirectToAction(
                    "Register",
                    "Account");
            }


            var notification =
                db.Notifications
                .FirstOrDefault(n =>
                    n.NotificationId == id &&

                    n.RegistrationId ==
                    student.RegistrationId);


            if (notification != null)
            {
                notification.IsRead = true;

                notification.ReadAt =
                    DateTime.Now;

                db.SaveChanges();
            }


            return RedirectToAction(
                "Index");
        }


        // ============================================================
        // MARK ALL AS READ
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAllAsRead()
        {
            string userId =
                User.Identity.GetUserId();


            var student =
                db.Registrations
                .FirstOrDefault(r =>
                    r.ApplicationUserId == userId);


            if (student == null)
            {
                return RedirectToAction(
                    "Register",
                    "Account");
            }


            var notifications =
                db.Notifications
                .Where(n =>
                    n.RegistrationId ==
                    student.RegistrationId &&

                    !n.IsRead)
                .ToList();


            foreach (var notification in notifications)
            {
                notification.IsRead = true;

                notification.ReadAt =
                    DateTime.Now;
            }


            db.SaveChanges();


            return RedirectToAction(
                "Index");
        }


        // ============================================================
        // DISPOSE
        // ============================================================

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