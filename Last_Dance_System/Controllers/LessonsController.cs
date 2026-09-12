
using System;
using System.Collections.Generic;
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
        private readonly ApplicationDbContext db;

        public LessonsController()
        {
            db = new ApplicationDbContext();
        }

        // ============================================================
        // MY LESSONS
        // ============================================================

        [HttpGet]
        public ActionResult Index()
        {
            // ========================================================
            // GET CURRENTLY LOGGED-IN USER
            // ========================================================

            string userId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // ========================================================
            // FIND STUDENT REGISTRATION
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
                .Include(b => b.Registration)
                .Include(b => b.StudentPackage)

                .Include(b => b.LessonSchedule)
                .Include(b => b.LessonSchedule.LessonType)
                .Include(b => b.LessonSchedule.Instructor)
                .Include(b => b.LessonSchedule.Vehicle)

                .Where(b =>
                    b.RegistrationId == student.RegistrationId)

                .OrderBy(b => b.LessonSchedule.LessonDate)
                .ThenBy(b => b.LessonSchedule.StartTime)

                .ToList();

            // ========================================================
            // GET INSTRUCTOR FEEDBACK
            // ========================================================
            //
            // Instructor feedback is stored in the Review table.
            //
            // ReviewType = "InstructorSession"
            //
            // BookingId connects the feedback to the exact lesson.
            //
            // IsApproved = true means the feedback is approved and
            // can be shown to the student.
            // ========================================================

            var bookingIds = bookings
                .Select(b => b.BookingId)
                .ToList();

            var instructorFeedback = db.Reviews
                .Where(r =>
                    bookingIds.Contains(r.BookingId) &&
                    r.ReviewType == "InstructorSession" &&
                    r.IsApproved)
                .OrderByDescending(r => r.ReviewDate)
                .ToList();

            // ========================================================
            // CREATE DICTIONARY
            // ========================================================
            //
            // This allows the Razor view to quickly find the feedback
            // belonging to each BookingId.
            // ========================================================

            var instructorFeedbackByBooking =
                instructorFeedback
                    .GroupBy(r => r.BookingId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.First());

            // ========================================================
            // SEND INSTRUCTOR FEEDBACK TO VIEW
            // ========================================================

            ViewBag.InstructorFeedback =
                instructorFeedbackByBooking;

            // ========================================================
            // RETURN ALL BOOKINGS TO MY LESSONS VIEW
            // ========================================================

            return View(bookings);
        }


        // ============================================================
        // LESSON DETAILS
        // ============================================================

        [HttpGet]
        public ActionResult Details(int? id)
        {
            // ========================================================
            // VALIDATE BOOKING ID
            // ========================================================

            if (!id.HasValue)
            {
                return RedirectToAction("Index");
            }

            // ========================================================
            // GET CURRENTLY LOGGED-IN USER
            // ========================================================

            string userId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // ========================================================
            // FIND STUDENT REGISTRATION
            // ========================================================

            var student = db.Registrations
                .FirstOrDefault(r =>
                    r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction(
                    "Register",
                    "Account");
            }

            // ========================================================
            // FIND BOOKING
            //
            // IMPORTANT:
            // The booking must belong to the logged-in student.
            // ========================================================

            var booking = db.Bookings
                .Include(b => b.Registration)
                .Include(b => b.StudentPackage)

                .Include(b => b.LessonSchedule)
                .Include(b => b.LessonSchedule.LessonType)
                .Include(b => b.LessonSchedule.Instructor)
                .Include(b => b.LessonSchedule.Vehicle)

                .FirstOrDefault(b =>
                    b.BookingId == id.Value &&
                    b.RegistrationId ==
                    student.RegistrationId);

            // ========================================================
            // BOOKING NOT FOUND
            // ========================================================

            if (booking == null)
            {
                return HttpNotFound(
                    "Lesson booking was not found.");
            }

            // ========================================================
            // GET INSTRUCTOR FEEDBACK FOR THIS LESSON
            // ========================================================

            var instructorFeedback = db.Reviews
                .Include(r => r.ReviewerInstructor)
                .FirstOrDefault(r =>
                    r.BookingId == booking.BookingId &&
                    r.ReviewType == "InstructorSession" &&
                    r.IsApproved);

            // ========================================================
            // SEND FEEDBACK TO DETAILS VIEW
            // ========================================================

            ViewBag.InstructorFeedback =
                instructorFeedback;

            // ========================================================
            // RETURN DETAILS VIEW
            // ========================================================

            return View(booking);
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

