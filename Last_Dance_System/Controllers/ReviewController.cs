
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Last_Dance_System.Models;

namespace Last_Dance_System.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext db =
            new ApplicationDbContext();


        // =========================================================
        // HELPER: GET LOGGED-IN INSTRUCTOR
        // =========================================================

        private Instructor GetLoggedInInstructor()
        {
            string applicationUserId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(applicationUserId))
            {
                return null;
            }

            return db.Instructors
                .FirstOrDefault(i =>
                    i.ApplicationUserId == applicationUserId);
        }


        // =========================================================
        // INDEX
        // Shows completed lessons belonging to this instructor
        // =========================================================

        [HttpGet]
        public ActionResult Index()
        {
            var instructor = GetLoggedInInstructor();

            if (instructor == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.Forbidden,
                    "Instructor account is not linked.");
            }

            var completedLessons = db.Bookings
                .Include(b => b.LessonSchedule)
                .Include(b => b.LessonSchedule.LessonType)
                .Include(b => b.LessonSchedule.Instructor)
                .Include(b => b.Registration)
                .Include(b => b.LessonSchedule.Vehicle)
                .Where(b =>
                    b.LessonSchedule != null &&
                    b.LessonSchedule.InstructorId ==
                        instructor.InstructorId &&
                    b.Status == "Completed")
                .OrderByDescending(b =>
                    b.LessonSchedule.LessonDate)
                .ThenByDescending(b =>
                    b.LessonSchedule.StartTime)
                .ToList();

            return View(completedLessons);
        }


        // =========================================================
        // CREATE - GET
        // Instructor opens feedback form
        // =========================================================

        [HttpGet]
        public ActionResult Create(int? bookingId)
        {
            if (!bookingId.HasValue)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest,
                    "Booking ID is required.");
            }

            var instructor = GetLoggedInInstructor();

            if (instructor == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.Forbidden,
                    "Instructor account is not linked.");
            }


            // =====================================================
            // GET BOOKING
            // =====================================================

            var booking = db.Bookings
                .Include(b => b.Registration)
                .Include(b => b.LessonSchedule)
                .Include(b => b.LessonSchedule.LessonType)
                .Include(b => b.LessonSchedule.Instructor)
                .Include(b => b.LessonSchedule.Vehicle)
                .FirstOrDefault(b =>
                    b.BookingId == bookingId.Value);

            if (booking == null)
            {
                return HttpNotFound(
                    "The lesson could not be found.");
            }


            // =====================================================
            // SECURITY
            // Lesson must belong to logged-in instructor
            // =====================================================

            if (booking.LessonSchedule == null ||
                booking.LessonSchedule.InstructorId !=
                    instructor.InstructorId)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.Forbidden,
                    "You are not assigned to this lesson.");
            }


            // =====================================================
            // STUDENT CHECK
            // =====================================================

            if (booking.Registration == null)
            {
                TempData["ReviewError"] =
                    "This lesson does not have a valid student.";

                return RedirectToAction(
                    "MyLessons",
                    "InstructorDashboard");
            }


            // =====================================================
            // ONLY COMPLETED LESSONS
            // =====================================================

            if (!string.Equals(
                    booking.Status,
                    "Completed",
                    StringComparison.OrdinalIgnoreCase))
            {
                TempData["ReviewError"] =
                    "You can only provide feedback for completed lessons.";

                return RedirectToAction(
                    "MyLessons",
                    "InstructorDashboard");
            }


            // =====================================================
            // CHECK DUPLICATE
            // =====================================================

            bool alreadyReviewed = db.Reviews.Any(r =>
                r.BookingId == booking.BookingId &&
                r.ReviewerInstructorId ==
                    instructor.InstructorId &&
                r.ReviewType == "InstructorSession");

            if (alreadyReviewed)
            {
                TempData["ReviewError"] =
                    "You have already submitted feedback for this lesson.";

                return RedirectToAction(
                    "MyLessons",
                    "InstructorDashboard");
            }


            // =====================================================
            // CREATE EMPTY REVIEW MODEL
            // =====================================================

            var review = new Review
            {
                BookingId =
                    booking.BookingId,

                RegistrationId =
                    booking.RegistrationId,

                // InstructorId is NULL because this feedback
                // is going FROM instructor TO student.
                InstructorId = null,

                // Logged-in instructor is the person
                // submitting the feedback.
                ReviewerInstructorId =
                    instructor.InstructorId,

                VehicleId =
                    booking.LessonSchedule.VehicleId,

                ReviewType =
                    "InstructorSession",

                Rating = 5,

                ReviewDate =
                    DateTime.Now,

                IsApproved = true
            };


            ViewBag.Booking = booking;
            ViewBag.Instructor = instructor;

            return View(review);
        }


        // =========================================================
        // CREATE - POST
        // Saves instructor -> student feedback
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            int bookingId,
            Review review)
        {
            var instructor = GetLoggedInInstructor();

            if (instructor == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.Forbidden,
                    "Instructor account is not linked.");
            }


            // =====================================================
            // GET BOOKING
            // =====================================================

            var booking = db.Bookings
                .Include(b => b.Registration)
                .Include(b => b.LessonSchedule)
                .Include(b => b.LessonSchedule.Vehicle)
                .FirstOrDefault(b =>
                    b.BookingId == bookingId);

            if (booking == null)
            {
                return HttpNotFound(
                    "The lesson could not be found.");
            }


            // =====================================================
            // SECURITY
            // =====================================================

            if (booking.LessonSchedule == null ||
                booking.LessonSchedule.InstructorId !=
                    instructor.InstructorId)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.Forbidden,
                    "You are not assigned to this lesson.");
            }


            // =====================================================
            // STUDENT CHECK
            // =====================================================

            if (booking.Registration == null)
            {
                TempData["ReviewError"] =
                    "This lesson does not have a valid student.";

                return RedirectToAction(
                    "MyLessons",
                    "InstructorDashboard");
            }


            // =====================================================
            // COMPLETED LESSON CHECK
            // =====================================================

            if (!string.Equals(
                    booking.Status,
                    "Completed",
                    StringComparison.OrdinalIgnoreCase))
            {
                TempData["ReviewError"] =
                    "You can only provide feedback for completed lessons.";

                return RedirectToAction(
                    "MyLessons",
                    "InstructorDashboard");
            }


            // =====================================================
            // VALIDATE RATING
            // =====================================================

            if (review.Rating < 1 ||
                review.Rating > 5)
            {
                ModelState.AddModelError(
                    "Rating",
                    "Please select a rating between 1 and 5.");
            }


            // =====================================================
            // VALIDATE COMMENT
            // =====================================================

            if (string.IsNullOrWhiteSpace(
                    review.Comment))
            {
                ModelState.AddModelError(
                    "Comment",
                    "Please enter feedback for the student.");
            }


            // =====================================================
            // CHECK DUPLICATE
            // =====================================================

            bool alreadyReviewed = db.Reviews.Any(r =>
                r.BookingId == booking.BookingId &&
                r.ReviewerInstructorId ==
                    instructor.InstructorId &&
                r.ReviewType == "InstructorSession");

            if (alreadyReviewed)
            {
                TempData["ReviewError"] =
                    "Feedback has already been submitted for this lesson.";

                return RedirectToAction(
                    "MyLessons",
                    "InstructorDashboard");
            }


            // =====================================================
            // VALIDATION FAILED
            // =====================================================

            if (!ModelState.IsValid)
            {
                ViewBag.Booking = booking;
                ViewBag.Instructor = instructor;

                return View(review);
            }


            // =====================================================
            // CREATE REVIEW
            // =====================================================

            var newReview = new Review
            {
                // Exact lesson
                BookingId =
                    booking.BookingId,

                // Student receiving feedback
                RegistrationId =
                    booking.RegistrationId,

                // Not an instructor receiving a review
                InstructorId = null,

                // Instructor who submitted feedback
                ReviewerInstructorId =
                    instructor.InstructorId,

                // Vehicle used
                VehicleId =
                    booking.LessonSchedule.VehicleId,

                // Instructor -> Student
                ReviewType =
                    "InstructorSession",

                Rating =
                    review.Rating,

                Comment =
                    review.Comment.Trim(),

                ReviewDate =
                    DateTime.Now,

                IsApproved =
                    true
            };


            // =====================================================
            // SAVE TO DATABASE
            // =====================================================

            db.Reviews.Add(newReview);

            db.SaveChanges();


            // =====================================================
            // SUCCESS MESSAGE
            // =====================================================

            TempData["ReviewSuccess"] =
                "Feedback has been submitted successfully and is now available to the student.";


            return RedirectToAction(
                "MyLessons",
                "InstructorDashboard");
        }


        // =========================================================
        // DETAILS
        // Instructor views feedback they submitted
        // =========================================================

        [HttpGet]
        public ActionResult Details(int id)
        {
            var instructor = GetLoggedInInstructor();

            if (instructor == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.Forbidden,
                    "Instructor account is not linked.");
            }


            // =====================================================
            // GET REVIEW
            // =====================================================

            var review = db.Reviews
                .Include(r => r.Booking)
                .Include(r => r.Registration)
                .Include(r => r.Instructor)
                .Include(r => r.ReviewerInstructor)
                .Include(r => r.Vehicle)
                .FirstOrDefault(r =>
                    r.ReviewId == id);

            if (review == null)
            {
                return HttpNotFound(
                    "The feedback could not be found.");
            }


            // =====================================================
            // SECURITY
            // Only the instructor who created it can view it
            // =====================================================

            if (review.ReviewerInstructorId !=
                instructor.InstructorId)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.Forbidden,
                    "You are not authorized to view this feedback.");
            }


            return View(review);
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

