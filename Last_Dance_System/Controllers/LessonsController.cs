using System;
using System.Data.Entity;
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

        // ============================================================
        // MY LESSONS
        // ============================================================

        public ActionResult Index()
        {
            // Get currently logged-in Application User ID
            string userId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // ========================================================
            // FIND THE STUDENT REGISTRATION
            // ========================================================

            var student = db.Registrations
                .FirstOrDefault(r => r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction("Register", "Account");
            }

            // ========================================================
            // GET ALL BOOKINGS FOR THIS STUDENT
            // ========================================================

            var bookings = db.Bookings
                .Include(b => b.LessonSchedule)
                .Include(b => b.LessonSchedule.LessonType)
                .Include(b => b.LessonSchedule.Instructor)
                .Include(b => b.LessonSchedule.Vehicle)
                .Where(b => b.RegistrationId == student.RegistrationId)
                .OrderBy(b => b.LessonSchedule.LessonDate)
                .ThenBy(b => b.LessonSchedule.StartTime)
                .ToList();

            // ========================================================
            // SHOW ALL STUDENT BOOKINGS
            // ========================================================

            return View(bookings);
        }


        // ============================================================
        // LESSON DETAILS
        // ============================================================

        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Index");
            }

            // Get currently logged-in Application User ID
            string userId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // ========================================================
            // FIND STUDENT
            // ========================================================

            var student = db.Registrations
                .FirstOrDefault(r => r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction("Register", "Account");
            }

            // ========================================================
            // FIND BOOKING BELONGING TO THIS STUDENT
            // ========================================================

            var booking = db.Bookings
                .Include(b => b.LessonSchedule)
                .Include(b => b.LessonSchedule.LessonType)
                .Include(b => b.LessonSchedule.Instructor)
                .Include(b => b.LessonSchedule.Vehicle)
                .FirstOrDefault(b =>
                    b.BookingId == id.Value &&
                    b.RegistrationId == student.RegistrationId);

            if (booking == null)
            {
                return HttpNotFound("Lesson booking was not found.");
            }

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