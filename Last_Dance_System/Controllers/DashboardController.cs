using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Last_Dance_System.Models;

namespace Last_Dance_System.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private ApplicationDbContext db =
            new ApplicationDbContext();


        // ============================================================
        // STUDENT DASHBOARD
        // ============================================================

        public ActionResult Index()
        {
            // ========================================================
            // GET LOGGED-IN USER
            // ========================================================

            string userId =
                User.Identity.GetUserId();


            // ========================================================
            // FIND STUDENT
            // ========================================================

            var student =
                db.Registrations
                .FirstOrDefault(r =>
                    r.ApplicationUserId == userId);


            // ========================================================
            // STUDENT NOT FOUND
            // ========================================================

            if (student == null)
            {
                return RedirectToAction(
                    "Register",
                    "Account");
            }


            // ========================================================
            // STUDENT NOTIFICATIONS
            // ========================================================

            var studentNotifications =
                db.Notifications
                .Where(n =>
                    n.RegistrationId ==
                    student.RegistrationId)
                .OrderByDescending(n =>
                    n.CreatedAt)
                .ToList();


            int unreadNotificationCount =
                studentNotifications.Count(n =>
                    !n.IsRead);


            // ========================================================
            // GET STUDENT BOOKINGS
            // ========================================================

            var bookings =
                db.Bookings
                .Include("LessonSchedule")
                .Include("LessonSchedule.LessonType")
                .Include("LessonSchedule.Instructor")
                .Include("LessonSchedule.Vehicle")
                .Where(b =>
                    b.RegistrationId ==
                    student.RegistrationId)
                .OrderByDescending(b =>
                    b.BookingDate)
                .ToList();


            // ========================================================
            // CANCELLED LESSONS
            // ========================================================

            int cancelledLessons =
                bookings.Count(b =>
                    b.Status != null &&
                    b.Status.Equals(
                        "Cancelled",
                        StringComparison.OrdinalIgnoreCase));


            // ========================================================
            // UPCOMING BOOKINGS
            // ========================================================

            var upcomingBookings =
                bookings
                .Where(b =>
                    b.LessonSchedule != null &&

                    b.LessonSchedule.LessonDate >=
                    DateTime.Today &&

                    b.Status != null &&

                    !b.Status.Equals(
                        "Cancelled",
                        StringComparison.OrdinalIgnoreCase) &&

                    !b.Status.Equals(
                        "Completed",
                        StringComparison.OrdinalIgnoreCase))
                .OrderBy(b =>
                    b.LessonSchedule.LessonDate)
                .ThenBy(b =>
                    b.LessonSchedule.StartTime)
                .ToList();


            int upcomingLessonsCount =
                upcomingBookings.Count;


            // ========================================================
            // NEXT UPCOMING LESSON
            // ========================================================

            var upcomingBooking =
                upcomingBookings.FirstOrDefault();


            // ========================================================
            // DEFAULT UPCOMING INFORMATION
            // ========================================================

            DateTime? upcomingLessonDate =
                null;

            int? upcomingLessonID =
                null;

            string upcomingLessonTime =
                "Not Scheduled";

            string upcomingLessonType =
                "Driving Lesson";

            string upcomingInstructor =
                "Not Assigned";

            string upcomingVehicle =
                "Not Assigned";

            string upcomingLocation =
                "Driving School";

            string bookingStatus =
                "No Active Booking";


            // ========================================================
            // LOAD UPCOMING LESSON
            // ========================================================

            if (upcomingBooking != null &&
                upcomingBooking.LessonSchedule != null)
            {
                var schedule =
                    upcomingBooking.LessonSchedule;


                upcomingLessonID =
                    upcomingBooking.BookingId;


                upcomingLessonDate =
                    schedule.LessonDate;


                upcomingLessonTime =
                    schedule.StartTime.ToString(@"hh\:mm")
                    + " - "
                    + schedule.EndTime.ToString(@"hh\:mm");


                // ====================================================
                // LESSON TYPE
                // ====================================================

                if (schedule.LessonType != null)
                {
                    upcomingLessonType =
                        schedule.LessonType.LessonTypeName;
                }


                // ====================================================
                // INSTRUCTOR
                // ====================================================

                if (schedule.Instructor != null)
                {
                    upcomingInstructor =
                        (
                            schedule.Instructor.FirstName
                            + " "
                            + schedule.Instructor.LastName
                        ).Trim();

                    if (string.IsNullOrWhiteSpace(
                        upcomingInstructor))
                    {
                        upcomingInstructor =
                            "Not Assigned";
                    }
                }


                // ====================================================
                // VEHICLE
                // ====================================================

                if (schedule.Vehicle != null)
                {
                    upcomingVehicle =
                        (
                            schedule.Vehicle.Make
                            + " "
                            + schedule.Vehicle.Model
                            + " ("
                            + schedule.Vehicle.RegistrationNumber
                            + ")"
                        ).Trim();
                }


                // ====================================================
                // BOOKING STATUS
                // ====================================================

                bookingStatus =
                    upcomingBooking.Status;
            }


            // ========================================================
            // CURRENT ACTIVE PACKAGE
            // ========================================================

            var currentPackage =
                db.StudentPackages
                .Include("LessonPackage")
                .Where(sp =>
                    sp.RegistrationId ==
                    student.RegistrationId &&

                    sp.PaymentStatus != null &&

                    sp.PaymentStatus.Equals(
                        "Paid",
                        StringComparison.OrdinalIgnoreCase) &&

                    sp.IsActive &&

                    !sp.IsCancelled)
                .OrderByDescending(sp =>
                    sp.PurchaseDate)
                .FirstOrDefault();


            // ========================================================
            // LESSON STATISTICS
            // ========================================================

            int totalLessons = 0;

            int completedLessons = 0;

            int remainingLessons = 0;

            int progressPercentage = 0;


            if (currentPackage != null)
            {
                totalLessons =
                    currentPackage.LessonsPurchased;


                remainingLessons =
                    currentPackage.LessonsRemaining;


                completedLessons =
                    totalLessons -
                    remainingLessons;


                if (completedLessons < 0)
                {
                    completedLessons = 0;
                }


                if (totalLessons > 0)
                {
                    progressPercentage =
                        (completedLessons * 100)
                        / totalLessons;
                }


                if (progressPercentage < 0)
                {
                    progressPercentage = 0;
                }


                if (progressPercentage > 100)
                {
                    progressPercentage = 100;
                }
            }


            // ========================================================
            // DEFAULT PACKAGE INFORMATION
            // ========================================================

            bool hasActivePackage =
                currentPackage != null;


            int? currentStudentPackageID =
                null;


            string currentPackageName =
                "No Active Package";


            int currentPackageLessons =
                0;


            int currentPackageLessonsRemaining =
                0;


            string currentPackagePaymentStatus =
                "No Package";


            decimal currentPackagePrice =
                0m;


            int packageProgressPercentage =
                0;


            // ========================================================
            // LOAD PACKAGE
            // ========================================================

            if (currentPackage != null)
            {
                currentStudentPackageID =
                    currentPackage.StudentPackageId;


                currentPackageLessons =
                    currentPackage.LessonsPurchased;


                currentPackageLessonsRemaining =
                    currentPackage.LessonsRemaining;


                currentPackagePaymentStatus =
                    currentPackage.PaymentStatus;


                if (currentPackage.LessonPackage != null)
                {
                    currentPackageName =
                        currentPackage
                        .LessonPackage
                        .PackageName;


                    currentPackagePrice =
                        currentPackage
                        .LessonPackage
                        .Price;
                }


                // ====================================================
                // PACKAGE PROGRESS
                // ====================================================

                if (currentPackageLessons > 0)
                {
                    int lessonsUsed =
                        currentPackageLessons
                        - currentPackageLessonsRemaining;


                    packageProgressPercentage =
                        (lessonsUsed * 100)
                        / currentPackageLessons;


                    if (packageProgressPercentage < 0)
                    {
                        packageProgressPercentage = 0;
                    }


                    if (packageProgressPercentage > 100)
                    {
                        packageProgressPercentage = 100;
                    }
                }
            }


            // ========================================================
            // PAYMENT INFORMATION
            // ========================================================

            string paymentStatus =
                "No Package";


            decimal totalPaid =
                0m;


            decimal outstandingAmount =
                0m;


            if (currentPackage != null)
            {
                paymentStatus =
                    currentPackage.PaymentStatus;


                if (currentPackage.PaymentStatus != null &&
                    currentPackage.PaymentStatus.Equals(
                        "Paid",
                        StringComparison.OrdinalIgnoreCase))
                {
                    totalPaid =
                        currentPackagePrice;


                    outstandingAmount =
                        0m;
                }
                else
                {
                    totalPaid =
                        0m;


                    outstandingAmount =
                        currentPackagePrice;
                }
            }


            // ========================================================
            // INSTRUCTOR FEEDBACK
            // ========================================================
            //
            // IMPORTANT:
            // Instructor feedback is saved by ReviewController
            // into the Reviews table.
            //
            // Therefore we MUST read Review.Comment here instead
            // of LessonProgress.InstructorNotes.
            //
            // Instructor reviews are identified by:
            //
            // ReviewType = "InstructorSession"
            //
            // ========================================================

            var latestInstructorFeedback =
                db.Reviews
                .Include("Booking")
                .Include("Booking.LessonSchedule")
                .Include("Booking.LessonSchedule.Instructor")
                .Where(r =>
                    r.RegistrationId ==
                    student.RegistrationId &&

                    r.ReviewType ==
                    "InstructorSession" &&

                    r.Comment != null &&

                    r.Comment != "")
                .OrderByDescending(r =>
                    r.ReviewDate)
                .FirstOrDefault();


            bool hasInstructorFeedback =
                latestInstructorFeedback != null;


            string latestFeedbackText =
                "No instructor feedback available.";


            string feedbackInstructor =
                "Instructor";


            DateTime? feedbackDate =
                null;


            if (latestInstructorFeedback != null)
            {
                // ====================================================
                // FEEDBACK COMMENT
                // ====================================================

                latestFeedbackText =
                    latestInstructorFeedback.Comment;


                // ====================================================
                // FEEDBACK DATE
                // ====================================================

                feedbackDate =
                    latestInstructorFeedback.ReviewDate;


                // ====================================================
                // INSTRUCTOR NAME
                // ====================================================

                if (latestInstructorFeedback.Booking != null &&
                    latestInstructorFeedback.Booking.LessonSchedule != null &&
                    latestInstructorFeedback.Booking.LessonSchedule.Instructor != null)
                {
                    feedbackInstructor =
                        (
                            latestInstructorFeedback
                            .Booking
                            .LessonSchedule
                            .Instructor
                            .FirstName

                            + " "

                            +

                            latestInstructorFeedback
                            .Booking
                            .LessonSchedule
                            .Instructor
                            .LastName
                        ).Trim();
                }


                // ====================================================
                // FALLBACK TO REVIEWER INSTRUCTOR
                // ====================================================

                if (string.IsNullOrWhiteSpace(
                    feedbackInstructor))
                {
                    feedbackInstructor =
                        "Instructor";
                }
            }


            // ========================================================
            // PENDING REVIEW
            // ========================================================
            //
            // IMPORTANT:
            // Do NOT count every Review anymore.
            //
            // Instructor reviews use:
            // ReviewType = "InstructorSession"
            //
            // Student reviews should be counted separately.
            //
            // We count reviews that are NOT instructor-session reviews.
            //
            // ========================================================

            int reviewCount =
                db.Reviews
                .Count(r =>
                    r.RegistrationId ==
                    student.RegistrationId &&

                    r.ReviewType !=
                    "InstructorSession");


            // ========================================================
            // COMPLETED BOOKING
            // ========================================================

            var completedBooking =
                bookings
                .Where(b =>
                    b.Status != null &&
                    b.Status.Equals(
                        "Completed",
                        StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(b =>
                    b.LessonSchedule != null
                        ? b.LessonSchedule.LessonDate
                        : b.BookingDate)
                .FirstOrDefault();


            bool hasPendingReview =
                completedBooking != null &&
                reviewCount < completedLessons;


            int? reviewLessonID =
                null;


            if (hasPendingReview &&
                completedBooking != null)
            {
                reviewLessonID =
                    completedBooking.BookingId;
            }


            // ========================================================
            // CREATE VIEW MODEL
            // ========================================================

            var model =
                new StudentDashboardViewModel
                {
                    // =================================================
                    // STUDENT
                    // =================================================

                    StudentID =
                        student.RegistrationId.ToString(),

                    FirstName =
                        student.FirstName,

                    LastName =
                        student.LastName,

                    Email =
                        student.Email,


                    // =================================================
                    // NOTIFICATIONS
                    // =================================================

                    UnreadNotificationCount =
                        unreadNotificationCount,

                    Notifications =
                        studentNotifications,


                    // =================================================
                    // LESSON STATISTICS
                    // =================================================

                    TotalLessons =
                        totalLessons,

                    CompletedLessons =
                        completedLessons,

                    RemainingLessons =
                        remainingLessons,

                    ProgressPercentage =
                        progressPercentage,

                    CancelledLessons =
                        cancelledLessons,


                    // =================================================
                    // UPCOMING LESSON
                    // =================================================

                    HasUpcomingLesson =
                        upcomingBooking != null,

                    UpcomingLessonID =
                        upcomingLessonID,

                    UpcomingLessonDate =
                        upcomingLessonDate,

                    UpcomingLessonTime =
                        upcomingLessonTime,

                    UpcomingLessonType =
                        upcomingLessonType,

                    UpcomingInstructor =
                        upcomingInstructor,

                    UpcomingVehicle =
                        upcomingVehicle,

                    UpcomingLocation =
                        upcomingLocation,

                    BookingStatus =
                        bookingStatus,

                    UpcomingLessonsCount =
                        upcomingLessonsCount,


                    // =================================================
                    // PACKAGE
                    // =================================================

                    HasActivePackage =
                        hasActivePackage,

                    CurrentStudentPackageID =
                        currentStudentPackageID,

                    CurrentPackageName =
                        currentPackageName,

                    CurrentPackageLessons =
                        currentPackageLessons,

                    CurrentPackageLessonsRemaining =
                        currentPackageLessonsRemaining,

                    CurrentPackagePaymentStatus =
                        currentPackagePaymentStatus,

                    CurrentPackagePrice =
                        currentPackagePrice,

                    PackageProgressPercentage =
                        packageProgressPercentage,


                    // =================================================
                    // PAYMENT
                    // =================================================

                    PaymentStatus =
                        paymentStatus,

                    TotalPaid =
                        totalPaid,

                    OutstandingAmount =
                        outstandingAmount,


                    // =================================================
                    // REVIEW
                    // =================================================

                    HasPendingReview =
                        hasPendingReview,

                    ReviewLessonID =
                        reviewLessonID,


                    // =================================================
                    // INSTRUCTOR FEEDBACK
                    // =================================================

                    HasInstructorFeedback =
                        hasInstructorFeedback,

                    LatestFeedback =
                        latestFeedbackText,

                    FeedbackInstructor =
                        feedbackInstructor,

                    FeedbackDate =
                        feedbackDate
                };


            // ========================================================
            // RETURN DASHBOARD
            // ========================================================

            return View(model);
        }
        // ============================================================
        // MY LESSONS
        // ============================================================

        public ActionResult MyLessons()
        {
            // ========================================================
            // GET LOGGED-IN USER
            // ========================================================

            string userId =
                User.Identity.GetUserId();


            // ========================================================
            // FIND STUDENT
            // ========================================================

            var student =
                db.Registrations
                .FirstOrDefault(r =>
                    r.ApplicationUserId == userId);


            // ========================================================
            // STUDENT NOT FOUND
            // ========================================================

            if (student == null)
            {
                return RedirectToAction(
                    "Register",
                    "Account");
            }


            // ========================================================
            // GET ALL STUDENT BOOKINGS
            // ========================================================

            var bookings =
                db.Bookings
                .Include("LessonSchedule")
                .Include("LessonSchedule.LessonType")
                .Include("LessonSchedule.Instructor")
                .Include("LessonSchedule.Vehicle")
                .Where(b =>
                    b.RegistrationId ==
                    student.RegistrationId)
                .OrderByDescending(b =>
                    b.LessonSchedule.LessonDate)
                .ThenByDescending(b =>
                    b.LessonSchedule.StartTime)
                .ToList();


            // ========================================================
            // RETURN MY LESSONS
            // ========================================================

            return View(bookings);
        }

        // ============================================================
        // PROFILE
        // ============================================================

        public ActionResult Profile()
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


            return View(student);
        }


        // ============================================================
        // EDIT PROFILE - GET
        // ============================================================

        [HttpGet]
        public ActionResult EditProfile()
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


            return View(student);
        }


        // ============================================================
        // EDIT PROFILE - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(
            [Bind(Include =
                "RegistrationId,FirstName,LastName,Gender,DateOfBirth,Phone,Email,Address")]
            Registration student)
        {
            string userId =
                User.Identity.GetUserId();


            var existingStudent =
                db.Registrations
                .FirstOrDefault(r =>
                    r.RegistrationId ==
                    student.RegistrationId &&

                    r.ApplicationUserId ==
                    userId);


            if (existingStudent == null)
            {
                return HttpNotFound();
            }


            if (!ModelState.IsValid)
            {
                return View(student);
            }


            existingStudent.FirstName =
                student.FirstName;

            existingStudent.LastName =
                student.LastName;

            existingStudent.Gender =
                student.Gender;

            existingStudent.DateOfBirth =
                student.DateOfBirth;

            existingStudent.Phone =
                student.Phone;

            existingStudent.Email =
                student.Email;

            existingStudent.Address =
                student.Address;


            db.SaveChanges();


            return RedirectToAction(
                "Profile");
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