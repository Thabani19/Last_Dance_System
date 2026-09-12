
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Last_Dance_System.Models;

namespace Last_Dance_System.Controllers
{
    [Authorize]
    public class StudentFeedbackController : Controller
    {
        private readonly ApplicationDbContext db =
            new ApplicationDbContext();


        // =========================================================
        // HELPER: GET LOGGED-IN STUDENT
        // =========================================================

        private Registration GetLoggedInStudent()
        {
            string applicationUserId =
                User.Identity.GetUserId();

            if (string.IsNullOrWhiteSpace(applicationUserId))
            {
                return null;
            }

            return db.Registrations
                .FirstOrDefault(r =>
                    r.ApplicationUserId == applicationUserId);
        }


        // =========================================================
        // DETAILS
        //
        // Student views feedback for ONE lesson.
        //
        // IMPORTANT:
        // We identify ownership using the BOOKING's
        // RegistrationId rather than trusting only the
        // InstructorFeedback RegistrationId.
        // =========================================================

        [HttpGet]
        public ActionResult Details(int? bookingId)
        {
            // =====================================================
            // CHECK BOOKING ID
            // =====================================================

            if (!bookingId.HasValue)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest,
                    "Booking ID is required.");
            }


            // =====================================================
            // GET LOGGED-IN STUDENT
            // =====================================================

            var student = GetLoggedInStudent();

            if (student == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.Forbidden,
                    "Student account is not linked.");
            }


            // =====================================================
            // FIND FEEDBACK
            //
            // SECURITY:
            //
            // 1. Feedback must belong to this booking.
            // 2. Booking must belong to logged-in student.
            //
            // This prevents students from changing bookingId
            // in the URL and seeing another student's feedback.
            // =====================================================

            var feedback =
                db.InstructorFeedbacks

                .Include(f => f.Booking)

                .Include(f =>
                    f.Booking.LessonSchedule)

                .Include(f =>
                    f.Booking.LessonSchedule.LessonType)

                .Include(f =>
                    f.Booking.LessonSchedule.Instructor)

                .Include(f =>
                    f.Booking.LessonSchedule.Vehicle)

                .Include(f =>
                    f.Instructor)

                .Include(f =>
                    f.Registration)

                .FirstOrDefault(f =>
                    f.BookingId == bookingId.Value
                    &&
                    f.Booking.RegistrationId ==
                    student.RegistrationId);


            // =====================================================
            // NO FEEDBACK FOUND
            // =====================================================

            if (feedback == null)
            {
                TempData["FeedbackMessage"] =
                    "No instructor feedback has been submitted for this lesson yet.";

                return RedirectToAction(
                    "MyLessons",
                    "Dashboard");
            }


            // =====================================================
            // RETURN FEEDBACK
            // =====================================================

            return View(feedback);
        }



        // =========================================================
        // INDEX
        //
        // Shows ALL feedback belonging to the logged-in student.
        // =========================================================

        [HttpGet]
        public ActionResult Index()
        {
            // =====================================================
            // GET LOGGED-IN STUDENT
            // =====================================================

            var student = GetLoggedInStudent();

            if (student == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.Forbidden,
                    "Student account is not linked.");
            }


            // =====================================================
            // GET ALL FEEDBACK
            //
            // IMPORTANT:
            // We use Booking.RegistrationId to determine ownership.
            //
            // This is more reliable because the Booking is the
            // actual lesson belonging to the student.
            // =====================================================

            var feedback =
                db.InstructorFeedbacks

                .Include(f => f.Booking)

                .Include(f =>
                    f.Booking.LessonSchedule)

                .Include(f =>
                    f.Booking.LessonSchedule.LessonType)

                .Include(f =>
                    f.Booking.LessonSchedule.Instructor)

                .Include(f =>
                    f.Booking.LessonSchedule.Vehicle)

                .Include(f =>
                    f.Instructor)

                .Include(f =>
                    f.Registration)

                .Where(f =>
                    f.Booking.RegistrationId ==
                    student.RegistrationId)

                .OrderByDescending(f =>
                    f.CreatedDate)

                .ToList();


            // =====================================================
            // RETURN VIEW
            // =====================================================

            return View(feedback);
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

