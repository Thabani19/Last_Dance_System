using Last_Dance_System.Models;
using Last_Dance_System.Services;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Last_Dance_System.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class InstructorLessonsController : Controller
    {
        private readonly ApplicationDbContext db;

        public InstructorLessonsController()
        {
            db = new ApplicationDbContext();
        }


        // =========================================================
        // MY LESSONS
        // =========================================================

        [HttpGet]
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();

            var instructor =
                db.Instructors
                .Include(i => i.Vehicle)
                .FirstOrDefault(i =>
                    i.ApplicationUserId == userId);

            if (instructor == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            // =====================================================
            // GET INSTRUCTOR LESSONS
            // =====================================================

            var lessons =
                db.LessonSchedules
                .Include(l =>
                    l.Bookings.Select(b =>
                        b.Registration))
                .Include(l =>
                    l.LessonType)
                .Include(l =>
                    l.Vehicle)
                .Where(l =>
                    l.InstructorId ==
                    instructor.InstructorId)
                .OrderBy(l =>
                    l.LessonDate)
                .ThenBy(l =>
                    l.StartTime)
                .ToList();


            ViewBag.Instructor =
                instructor;


            ViewBag.AssignedVehicle =
                instructor.Vehicle;


            return View(lessons);
        }


        // =========================================================
        // LESSON DETAILS
        // =========================================================

        [HttpGet]
        public ActionResult Details(int id)
        {
            var userId =
                User.Identity.GetUserId();


            var instructor =
                db.Instructors
                .Include(i => i.Vehicle)
                .FirstOrDefault(i =>
                    i.ApplicationUserId == userId);


            if (instructor == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            // =====================================================
            // GET LESSON
            // =====================================================

            var lesson =
                db.LessonSchedules
                .Include(l =>
                    l.Bookings.Select(b =>
                        b.Registration))
                .Include(l =>
                    l.LessonType)
                .Include(l =>
                    l.Vehicle)
                .FirstOrDefault(l =>
                    l.LessonScheduleId == id &&
                    l.InstructorId ==
                    instructor.InstructorId);


            if (lesson == null)
            {
                return HttpNotFound();
            }


            ViewBag.Instructor =
                instructor;


            ViewBag.AssignedVehicle =
                instructor.Vehicle;


            // =====================================================
            // FIND ACTIVE / RELEVANT BOOKING
            // =====================================================

            var booking =
                lesson.Bookings
                .FirstOrDefault(b =>
                    !string.Equals(
                        b.Status,
                        "Cancelled",
                        StringComparison.OrdinalIgnoreCase));


            // =====================================================
            // LOAD EXISTING FEEDBACK
            // =====================================================

            LessonProgress existingProgress =
                null;


            if (booking != null)
            {
                existingProgress =
                    db.LessonProgresses
                    .FirstOrDefault(lp =>
                        lp.BookingId ==
                        booking.BookingId);
            }


            // =====================================================
            // SEND EXISTING FEEDBACK TO VIEW
            // =====================================================

            ViewBag.ExistingFeedback =
                existingProgress != null
                    ? existingProgress.InstructorNotes
                    : null;


            ViewBag.FeedbackDate =
                existingProgress != null
                    ? existingProgress.CompletionDate
                    : null;


            return View(lesson);
        }


        // =========================================================
        // SAVE INSTRUCTOR FEEDBACK
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveFeedback(
            int id,
            string instructorNotes)
        {
            var userId =
                User.Identity.GetUserId();


            // =====================================================
            // FIND INSTRUCTOR
            // =====================================================

            var instructor =
                db.Instructors
                .FirstOrDefault(i =>
                    i.ApplicationUserId == userId);


            if (instructor == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            // =====================================================
            // VALIDATE FEEDBACK
            // =====================================================

            if (string.IsNullOrWhiteSpace(
                instructorNotes))
            {
                TempData["InstructorError"] =
                    "Please enter feedback before submitting.";

                return RedirectToAction(
                    "Details",
                    new { id = id });
            }


            instructorNotes =
                instructorNotes.Trim();


            if (instructorNotes.Length > 1000)
            {
                TempData["InstructorError"] =
                    "Feedback cannot exceed 1000 characters.";

                return RedirectToAction(
                    "Details",
                    new { id = id });
            }


            // =====================================================
            // FIND LESSON
            // =====================================================

            var lesson =
                db.LessonSchedules
                .Include(l =>
                    l.Bookings.Select(b =>
                        b.Registration))
                .FirstOrDefault(l =>
                    l.LessonScheduleId == id &&
                    l.InstructorId ==
                    instructor.InstructorId);


            if (lesson == null)
            {
                TempData["InstructorError"] =
                    "The lesson could not be found.";

                return RedirectToAction(
                    "Index");
            }


            // =====================================================
            // FIND BOOKING
            // =====================================================

            var booking =
                lesson.Bookings
                .FirstOrDefault(b =>
                    !string.Equals(
                        b.Status,
                        "Cancelled",
                        StringComparison.OrdinalIgnoreCase));


            if (booking == null)
            {
                TempData["InstructorError"] =
                    "This lesson does not have an active student booking.";

                return RedirectToAction(
                    "Details",
                    new { id = id });
            }


            // =====================================================
            // FIND STUDENT
            // =====================================================

            var student =
                booking.Registration;


            if (student == null)
            {
                TempData["InstructorError"] =
                    "The student associated with this lesson could not be found.";

                return RedirectToAction(
                    "Details",
                    new { id = id });
            }


            // =====================================================
            // FIND EXISTING LESSON PROGRESS
            // =====================================================

            var lessonProgress =
                db.LessonProgresses
                .FirstOrDefault(lp =>
                    lp.BookingId ==
                    booking.BookingId);


            // =====================================================
            // CREATE IF IT DOES NOT EXIST
            // =====================================================

            if (lessonProgress == null)
            {
                lessonProgress =
                    new LessonProgress
                    {
                        RegistrationId =
                            student.RegistrationId,

                        BookingId =
                            booking.BookingId,

                        LessonStatus =
                            booking.Status ?? "Completed",

                        InstructorNotes =
                            instructorNotes,

                        CompletionDate =
                            DateTime.Now,

                        ProgressPercentage =
                            100
                    };


                db.LessonProgresses.Add(
                    lessonProgress);
            }
            else
            {
                // =================================================
                // UPDATE EXISTING FEEDBACK
                // =================================================

                lessonProgress.RegistrationId =
                    student.RegistrationId;

                lessonProgress.InstructorNotes =
                    instructorNotes;

                lessonProgress.CompletionDate =
                    DateTime.Now;

                lessonProgress.LessonStatus =
                    booking.Status ?? "Completed";

                lessonProgress.ProgressPercentage =
                    100;
            }


            // =====================================================
            // SAVE
            // =====================================================

            db.SaveChanges();


            // =====================================================
            // SUCCESS
            // =====================================================

            TempData["InstructorSuccess"] =
                "Instructor feedback has been saved successfully. " +
                "The student can now see your feedback.";


            return RedirectToAction(
                "Details",
                new { id = id });
        }


        // =========================================================
        // MARK LESSON AS COMPLETED
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkCompleted(int id)
        {
            var userId =
                User.Identity.GetUserId();


            var instructor =
                db.Instructors
                .FirstOrDefault(i =>
                    i.ApplicationUserId == userId);


            if (instructor == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            var lesson =
                db.LessonSchedules
                .Include(l =>
                    l.Bookings)
                .FirstOrDefault(l =>
                    l.LessonScheduleId == id &&
                    l.InstructorId ==
                    instructor.InstructorId);


            if (lesson == null)
            {
                TempData["InstructorError"] =
                    "The lesson could not be found.";

                return RedirectToAction(
                    "Index");
            }


            // =====================================================
            // FIND ACTIVE BOOKING
            // =====================================================

            var booking =
                lesson.Bookings
                .FirstOrDefault(b =>
                    !string.Equals(
                        b.Status,
                        "Cancelled",
                        StringComparison.OrdinalIgnoreCase));


            if (booking == null)
            {
                TempData["InstructorError"] =
                    "This lesson does not have an active student booking.";

                return RedirectToAction(
                    "Index");
            }


            // =====================================================
            // CHECK IF ALREADY COMPLETED
            // =====================================================

            if (string.Equals(
                booking.Status,
                "Completed",
                StringComparison.OrdinalIgnoreCase))
            {
                TempData["InstructorError"] =
                    "This lesson has already been completed.";

                return RedirectToAction(
                    "Index");
            }


            // =====================================================
            // CHECK LESSON END TIME
            // =====================================================

            DateTime lessonEndDateTime =
                lesson.LessonDate
                    .Date
                    .Add(lesson.EndTime);


            if (DateTime.Now < lessonEndDateTime)
            {
                TempData["InstructorError"] =
                    "This lesson cannot be marked as completed before the scheduled end time.";

                return RedirectToAction(
                    "Index");
            }


            // =====================================================
            // COMPLETE BOOKING
            // =====================================================

            booking.Status =
                "Completed";


            // =====================================================
            // COMPLETE LESSON SCHEDULE
            // =====================================================

            lesson.Status =
                "Completed";


            db.SaveChanges();


            TempData["InstructorSuccess"] =
                "The lesson has been marked as completed successfully.";


            return RedirectToAction(
                "Index");
        }


        // =========================================================
        // CANCEL LESSON
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelLesson(
            int id,
            string cancellationReason)
        {
            var userId =
                User.Identity.GetUserId();


            // =====================================================
            // FIND INSTRUCTOR
            // =====================================================

            var instructor =
                db.Instructors
                .FirstOrDefault(i =>
                    i.ApplicationUserId == userId);


            if (instructor == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            // =====================================================
            // FIND LESSON
            // =====================================================

            var lesson =
                db.LessonSchedules
                .Include(l =>
                    l.Bookings.Select(b =>
                        b.Registration))
                .Include(l =>
                    l.LessonType)
                .Include(l =>
                    l.Vehicle)
                .FirstOrDefault(l =>
                    l.LessonScheduleId == id &&
                    l.InstructorId ==
                    instructor.InstructorId);


            if (lesson == null)
            {
                TempData["InstructorError"] =
                    "The lesson could not be found.";

                return RedirectToAction(
                    "Index");
            }


            // =====================================================
            // CHECK LESSON STATUS
            // =====================================================

            if (string.Equals(
                lesson.Status,
                "Completed",
                StringComparison.OrdinalIgnoreCase))
            {
                TempData["InstructorError"] =
                    "A completed lesson cannot be cancelled.";

                return RedirectToAction(
                    "Index");
            }


            if (string.Equals(
                lesson.Status,
                "Cancelled",
                StringComparison.OrdinalIgnoreCase))
            {
                TempData["InstructorError"] =
                    "This lesson has already been cancelled.";

                return RedirectToAction(
                    "Index");
            }


            // =====================================================
            // FIND ACTIVE BOOKING
            // =====================================================

            var booking =
                lesson.Bookings
                .FirstOrDefault(b =>
                    !string.Equals(
                        b.Status,
                        "Cancelled",
                        StringComparison.OrdinalIgnoreCase)

                    &&

                    !string.Equals(
                        b.Status,
                        "Completed",
                        StringComparison.OrdinalIgnoreCase));


            if (booking == null)
            {
                TempData["InstructorError"] =
                    "This lesson does not have an active student booking.";

                return RedirectToAction(
                    "Index");
            }


            // =====================================================
            // VALIDATE CANCELLATION REASON
            // =====================================================

            if (string.IsNullOrWhiteSpace(
                cancellationReason))
            {
                TempData["InstructorError"] =
                    "Please provide a reason for cancelling the lesson.";

                return RedirectToAction(
                    "Index");
            }


            cancellationReason =
                cancellationReason.Trim();


            // =====================================================
            // GET STUDENT
            // =====================================================

            var student =
                booking.Registration;


            if (student == null)
            {
                TempData["InstructorError"] =
                    "The student associated with this lesson could not be found.";

                return RedirectToAction(
                    "Index");
            }


            // =====================================================
            // CANCEL BOOKING
            // =====================================================

            booking.Status =
                "Cancelled";


            // =====================================================
            // SAVE CANCELLATION REASON
            // =====================================================

            booking.Notes =
                "Cancelled by instructor. Reason: " +
                cancellationReason;


            // =====================================================
            // RETURN LESSON CREDIT
            // =====================================================

            if (booking.StudentPackageId.HasValue)
            {
                var studentPackage =
                    db.StudentPackages
                    .FirstOrDefault(sp =>
                        sp.StudentPackageId ==
                        booking.StudentPackageId.Value);


                if (studentPackage != null)
                {
                    studentPackage.LessonsRemaining += 1;
                }
            }


            // =====================================================
            // CANCEL LESSON SCHEDULE
            // =====================================================

            lesson.Status =
                "Cancelled";


            // =====================================================
            // SAVE DATABASE CHANGES FIRST
            // =====================================================

            db.SaveChanges();


            // =====================================================
            // SEND EMAIL TO STUDENT
            // =====================================================

            bool emailSent =
                SendCancellationEmail(
                    student,
                    instructor,
                    lesson,
                    cancellationReason);


            // =====================================================
            // SUCCESS MESSAGE
            // =====================================================

            if (emailSent)
            {
                TempData["InstructorSuccess"] =
                    "The lesson has been cancelled successfully. " +
                    "The student's lesson credit has been returned " +
                    "and an email notification has been sent.";
            }
            else
            {
                TempData["InstructorSuccess"] =
                    "The lesson has been cancelled successfully " +
                    "and the student's lesson credit has been returned. " +
                    "However, the cancellation email could not be sent.";
            }


            return RedirectToAction(
                "Index");
        }


        // =========================================================
        // SEND CANCELLATION EMAIL
        // =========================================================

        private bool SendCancellationEmail(
            Registration student,
            Instructor instructor,
            LessonSchedule lesson,
            string cancellationReason)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(
                    student.Email))
                {
                    return false;
                }


                string lessonDate =
                    lesson.LessonDate
                    .ToString("dddd, dd MMMM yyyy");


                string startTime =
                    DateTime.Today
                    .Add(lesson.StartTime)
                    .ToString("HH:mm");


                string endTime =
                    DateTime.Today
                    .Add(lesson.EndTime)
                    .ToString("HH:mm");


                string instructorName =
                    instructor.FirstName +
                    " " +
                    instructor.LastName;


                string vehicleInformation =
                    "Not specified";


                if (lesson.Vehicle != null)
                {
                    vehicleInformation =
                        lesson.Vehicle.Make +
                        " " +
                        lesson.Vehicle.Model +
                        " (" +
                        lesson.Vehicle.RegistrationNumber +
                        ")";
                }


                string subject =
                    "Driving Lesson Cancelled - Last Dance Driving School";


                string body =
                    "Dear " +
                    student.FirstName +
                    "," +

                    Environment.NewLine +
                    Environment.NewLine +

                    "We are writing to inform you that your " +
                    "driving lesson has been cancelled by your instructor." +

                    Environment.NewLine +
                    Environment.NewLine +

                    "LESSON DETAILS" +

                    Environment.NewLine +

                    "Date: " +
                    lessonDate +

                    Environment.NewLine +

                    "Time: " +
                    startTime +
                    " - " +
                    endTime +

                    Environment.NewLine +

                    "Instructor: " +
                    instructorName +

                    Environment.NewLine +

                    "Vehicle: " +
                    vehicleInformation +

                    Environment.NewLine +
                    Environment.NewLine +

                    "CANCELLATION REASON" +

                    Environment.NewLine +

                    cancellationReason +

                    Environment.NewLine +
                    Environment.NewLine +

                    "Your lesson credit has been returned to " +
                    "your lesson package." +

                    Environment.NewLine +
                    Environment.NewLine +

                    "You can log into the Last Dance Driving " +
                    "School system and book another available " +
                    "lesson." +

                    Environment.NewLine +
                    Environment.NewLine +

                    "We apologise for any inconvenience caused." +

                    Environment.NewLine +
                    Environment.NewLine +

                    "Kind regards," +

                    Environment.NewLine +

                    "Last Dance Driving School";


                Last_Dance_System.Services.EmailService emailService =
                    new Last_Dance_System.Services.EmailService();


                emailService.SendEmail(
                    student.Email,
                    subject,
                    body);


                return true;
            }
            catch (Exception)
            {
                return false;
            }
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