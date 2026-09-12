using Last_Dance_System.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Last_Dance_System.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class InstructorDashboardController : Controller
    {
        private readonly ApplicationDbContext db;

        public InstructorDashboardController()
        {
            db = new ApplicationDbContext();
        }


        // =========================================================
        // INSTRUCTOR DASHBOARD
        // =========================================================

        [HttpGet]
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();

            var instructor = db.Instructors
                .FirstOrDefault(i =>
                    i.ApplicationUserId == userId);

            if (instructor == null)
            {
                AuthenticationManagerSignOut();

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            ViewBag.Instructor = instructor;


            // =====================================================
            // FIND ASSIGNED VEHICLE
            // =====================================================

            Vehicle assignedVehicle = null;

            if (instructor.VehicleId.HasValue)
            {
                assignedVehicle = db.Vehicles
                    .FirstOrDefault(v =>
                        v.VehicleId ==
                        instructor.VehicleId.Value);
            }

            ViewBag.AssignedVehicle = assignedVehicle;


            // =====================================================
            // VEHICLE STATUS
            // =====================================================

            if (assignedVehicle != null)
            {
                ViewBag.VehicleIsActive =
                    assignedVehicle.IsActive;

                ViewBag.VehicleStatus =
                    assignedVehicle.Status;
            }
            else
            {
                ViewBag.VehicleIsActive = false;
                ViewBag.VehicleStatus =
                    "No Vehicle Assigned";
            }


            // =====================================================
            // UNREAD NOTIFICATIONS
            // =====================================================

            ViewBag.UnreadNotificationCount =
                db.Notifications.Count(n =>
                    n.InstructorId ==
                    instructor.InstructorId
                    &&
                    !n.IsRead);


            var today = DateTime.Today;


            // =====================================================
            // TODAY'S LESSONS
            // =====================================================

            var todaysLessons = db.LessonSchedules

                .Include(l =>
                    l.Bookings.Select(b =>
                        b.Registration))

                .Include(l =>
                    l.LessonType)

                .Include(l =>
                    l.Vehicle)

                .Where(l =>
                    l.InstructorId ==
                        instructor.InstructorId

                    &&

                    l.LessonDate == today

                    &&

                    l.Bookings.Any(b =>
                        b.Status != "Cancelled"
                        &&
                        b.Status != "Completed"))

                .OrderBy(l =>
                    l.StartTime)

                .ToList();


            // =====================================================
            // UPCOMING LESSONS
            // =====================================================

            var upcomingLessons = db.LessonSchedules

                .Include(l =>
                    l.Bookings.Select(b =>
                        b.Registration))

                .Include(l =>
                    l.LessonType)

                .Include(l =>
                    l.Vehicle)

                .Where(l =>
                    l.InstructorId ==
                        instructor.InstructorId

                    &&

                    l.LessonDate > today

                    &&

                    l.Bookings.Any(b =>
                        b.Status != "Cancelled"
                        &&
                        b.Status != "Completed"))

                .OrderBy(l =>
                    l.LessonDate)

                .ThenBy(l =>
                    l.StartTime)

                .ToList();


            // =====================================================
            // COMPLETED LESSONS
            // =====================================================

            var completedLessons = db.LessonSchedules

                .Include(l =>
                    l.Bookings.Select(b =>
                        b.Registration))

                .Include(l =>
                    l.LessonType)

                .Include(l =>
                    l.Vehicle)

                .Where(l =>
                    l.InstructorId ==
                        instructor.InstructorId

                    &&

                    l.Bookings.Any(b =>
                        b.Status == "Completed"))

                .OrderByDescending(l =>
                    l.LessonDate)

                .ThenByDescending(l =>
                    l.StartTime)

                .ToList();


            // =====================================================
            // SEND DATA TO DASHBOARD
            // =====================================================

            ViewBag.TodaysLessons =
                todaysLessons;

            ViewBag.UpcomingLessonList =
                upcomingLessons;

            ViewBag.CompletedLessons =
                completedLessons;


            return View();
        }



        // =========================================================
        // MY LESSONS
        // =========================================================

        [HttpGet]
        public ActionResult MyLessons()
        {
            var userId =
                User.Identity.GetUserId();


            // =====================================================
            // FIND INSTRUCTOR
            // =====================================================

            var instructor = db.Instructors
                .Include(i => i.Vehicle)
                .FirstOrDefault(i =>
                    i.ApplicationUserId == userId);


            if (instructor == null)
            {
                AuthenticationManagerSignOut();

                return RedirectToAction(
                    "Login",
                    "Account");
            }


            ViewBag.Instructor =
                instructor;


            // =====================================================
            // GET ASSIGNED VEHICLE
            // =====================================================

            Vehicle assignedVehicle = null;

            if (instructor.VehicleId.HasValue)
            {
                assignedVehicle = db.Vehicles
                    .FirstOrDefault(v =>
                        v.VehicleId ==
                        instructor.VehicleId.Value);
            }

            ViewBag.AssignedVehicle =
                assignedVehicle;


            ViewBag.VehicleIsActive =
                assignedVehicle != null &&
                assignedVehicle.IsActive;


            // =====================================================
            // GET INSTRUCTOR LESSONS
            // =====================================================

            var lessons = db.LessonSchedules

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


            return View(lessons);
        }



        // =========================================================
        // INSTRUCTOR NOTIFICATIONS
        // =========================================================

        [HttpGet]
        public ActionResult Notifications()
        {
            var userId =
                User.Identity.GetUserId();


            // =====================================================
            // FIND LOGGED-IN INSTRUCTOR
            // =====================================================

            var instructor = db.Instructors
                .FirstOrDefault(i =>
                    i.ApplicationUserId == userId);


            if (instructor == null)
            {
                AuthenticationManagerSignOut();

                return RedirectToAction(
                    "Login",
                    "Account");
            }


            // =====================================================
            // GET ONLY THIS INSTRUCTOR'S NOTIFICATIONS
            // =====================================================

            var notifications = db.Notifications
                .Where(n =>
                    n.InstructorId ==
                    instructor.InstructorId)

                .OrderByDescending(n =>
                    n.CreatedAt)

                .ToList();


            // =====================================================
            // INSTRUCTOR INFORMATION
            // =====================================================

            ViewBag.Instructor =
                instructor;


            // =====================================================
            // UNREAD COUNT
            // =====================================================

            ViewBag.UnreadNotificationCount =
                notifications.Count(n =>
                    !n.IsRead);


            return View(notifications);
        }



        // =========================================================
        // MARK ONE NOTIFICATION AS READ
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkNotificationAsRead(
            int id)
        {
            var userId =
                User.Identity.GetUserId();


            // =====================================================
            // FIND LOGGED-IN INSTRUCTOR
            // =====================================================

            var instructor = db.Instructors
                .FirstOrDefault(i =>
                    i.ApplicationUserId == userId);


            if (instructor == null)
            {
                AuthenticationManagerSignOut();

                return RedirectToAction(
                    "Login",
                    "Account");
            }


            // =====================================================
            // FIND NOTIFICATION
            // =====================================================

            var notification = db.Notifications
                .FirstOrDefault(n =>
                    n.NotificationId == id

                    &&

                    n.InstructorId ==
                    instructor.InstructorId);


            if (notification == null)
            {
                return HttpNotFound();
            }


            // =====================================================
            // MARK AS READ
            // =====================================================

            if (!notification.IsRead)
            {
                notification.IsRead = true;

                notification.ReadAt =
                    DateTime.Now;

                db.SaveChanges();
            }


            // =====================================================
            // RETURN TO NOTIFICATIONS
            // =====================================================

            return RedirectToAction(
                "Notifications");
        }



        // =========================================================
        // MARK ALL NOTIFICATIONS AS READ
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAllNotificationsAsRead()
        {
            var userId =
                User.Identity.GetUserId();


            // =====================================================
            // FIND LOGGED-IN INSTRUCTOR
            // =====================================================

            var instructor = db.Instructors
                .FirstOrDefault(i =>
                    i.ApplicationUserId == userId);


            if (instructor == null)
            {
                AuthenticationManagerSignOut();

                return RedirectToAction(
                    "Login",
                    "Account");
            }


            // =====================================================
            // GET UNREAD NOTIFICATIONS
            // =====================================================

            var notifications = db.Notifications
                .Where(n =>
                    n.InstructorId ==
                    instructor.InstructorId

                    &&

                    !n.IsRead)

                .ToList();


            // =====================================================
            // MARK ALL AS READ
            // =====================================================

            foreach (var notification in notifications)
            {
                notification.IsRead = true;

                notification.ReadAt =
                    DateTime.Now;
            }


            // =====================================================
            // SAVE
            // =====================================================

            db.SaveChanges();


            TempData["NotificationSuccess"] =
                "All notifications have been marked as read.";


            return RedirectToAction(
                "Notifications");
        }



        // =========================================================
        // INSTRUCTOR PROFILE
        // =========================================================

        [HttpGet]
        public ActionResult Profile()
        {
            var userId =
                User.Identity.GetUserId();


            var instructor = db.Instructors
                .FirstOrDefault(i =>
                    i.ApplicationUserId == userId);


            if (instructor == null)
            {
                AuthenticationManagerSignOut();

                return RedirectToAction(
                    "Login",
                    "Account");
            }


            ViewBag.Instructor =
                instructor;


            // =====================================================
            // ASSIGNED VEHICLE
            // =====================================================

            Vehicle assignedVehicle = null;

            if (instructor.VehicleId.HasValue)
            {
                assignedVehicle = db.Vehicles
                    .FirstOrDefault(v =>
                        v.VehicleId ==
                        instructor.VehicleId.Value);
            }

            ViewBag.AssignedVehicle =
                assignedVehicle;


            return View(instructor);
        }



        // =========================================================
        // EDIT INSTRUCTOR PROFILE - GET
        // =========================================================

        [HttpGet]
        public ActionResult EditProfile()
        {
            var userId =
                User.Identity.GetUserId();


            var instructor = db.Instructors
                .FirstOrDefault(i =>
                    i.ApplicationUserId == userId);


            if (instructor == null)
            {
                AuthenticationManagerSignOut();

                return RedirectToAction(
                    "Login",
                    "Account");
            }


            return View(instructor);
        }



        // =========================================================
        // EDIT INSTRUCTOR PROFILE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(
            Instructor model)
        {
            var userId =
                User.Identity.GetUserId();


            var instructor = db.Instructors
                .FirstOrDefault(i =>
                    i.ApplicationUserId == userId);


            if (instructor == null)
            {
                AuthenticationManagerSignOut();

                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            instructor.FirstName =
                model.FirstName;

            instructor.LastName =
                model.LastName;

            instructor.Phone =
                model.Phone;

            instructor.Email =
                model.Email;

            instructor.LicenseNumber =
                model.LicenseNumber;

            instructor.Experience =
                model.Experience;


            db.Entry(instructor)
                .State =
                EntityState.Modified;


            db.SaveChanges();


            return RedirectToAction(
                "Profile");
        }


        
// =========================================================
// GIVE INSTRUCTOR FEEDBACK - GET
// =========================================================

[HttpGet]
public ActionResult GiveFeedback(int bookingId)
        {
            var userId =
                User.Identity.GetUserId();


            // =====================================================
            // FIND LOGGED-IN INSTRUCTOR
            // =====================================================

            var instructor = db.Instructors
                .FirstOrDefault(i =>
                    i.ApplicationUserId == userId);


            if (instructor == null)
            {
                AuthenticationManagerSignOut();

                return RedirectToAction(
                    "Login",
                    "Account");
            }


            // =====================================================
            // FIND BOOKING
            // =====================================================

            var booking = db.Bookings
                .Include(b => b.Registration)
                .Include(b => b.LessonSchedule)
                .FirstOrDefault(b =>
                    b.BookingId == bookingId);


            if (booking == null)
            {
                return HttpNotFound();
            }


            // =====================================================
            // SECURITY:
            // MAKE SURE LESSON BELONGS TO THIS INSTRUCTOR
            // =====================================================

            if (booking.LessonSchedule == null ||
                booking.LessonSchedule.InstructorId !=
                instructor.InstructorId)
            {
                return new HttpUnauthorizedResult();
            }


            // =====================================================
            // ONLY COMPLETED LESSONS CAN RECEIVE FEEDBACK
            // =====================================================

            if (!string.Equals(
                booking.Status,
                "Completed",
                StringComparison.OrdinalIgnoreCase))
            {
                TempData["FeedbackError"] =
                    "Feedback can only be submitted for completed lessons.";

                return RedirectToAction("MyLessons");
            }


            // =====================================================
            // CHECK IF FEEDBACK ALREADY EXISTS
            // =====================================================

            var existingFeedback =
                db.InstructorFeedbacks
                    .FirstOrDefault(f =>
                        f.BookingId == booking.BookingId);


            if (existingFeedback != null)
            {
                TempData["FeedbackError"] =
                    "Feedback has already been submitted for this lesson.";

                return RedirectToAction("MyLessons");
            }


            // =====================================================
            // CREATE NEW FEEDBACK MODEL
            // =====================================================

            var model = new InstructorFeedback
            {
                BookingId =
                    booking.BookingId,

                InstructorId =
                    instructor.InstructorId,

                RegistrationId =
                    booking.RegistrationId,

                CreatedDate =
                    DateTime.Now
            };


            // =====================================================
            // SEND INFORMATION TO VIEW
            // =====================================================

            ViewBag.Instructor =
                instructor;

            ViewBag.Booking =
                booking;


            return View(model);
        }



        // =========================================================
        // GIVE INSTRUCTOR FEEDBACK - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GiveFeedback(
            InstructorFeedback model)
        {
            var userId =
                User.Identity.GetUserId();


            // =====================================================
            // FIND LOGGED-IN INSTRUCTOR
            // =====================================================

            var instructor = db.Instructors
                .FirstOrDefault(i =>
                    i.ApplicationUserId == userId);


            if (instructor == null)
            {
                AuthenticationManagerSignOut();

                return RedirectToAction(
                    "Login",
                    "Account");
            }


            // =====================================================
            // FIND BOOKING
            // =====================================================

            var booking = db.Bookings
                .Include(b => b.Registration)
                .Include(b => b.LessonSchedule)
                .FirstOrDefault(b =>
                    b.BookingId == model.BookingId);


            if (booking == null)
            {
                return HttpNotFound();
            }


            // =====================================================
            // SECURITY:
            // MAKE SURE LESSON BELONGS TO THIS INSTRUCTOR
            // =====================================================

            if (booking.LessonSchedule == null ||
                booking.LessonSchedule.InstructorId !=
                instructor.InstructorId)
            {
                return new HttpUnauthorizedResult();
            }


            // =====================================================
            // ONLY COMPLETED LESSONS CAN RECEIVE FEEDBACK
            // =====================================================

            if (!string.Equals(
                booking.Status,
                "Completed",
                StringComparison.OrdinalIgnoreCase))
            {
                TempData["FeedbackError"] =
                    "Feedback can only be submitted for completed lessons.";

                return RedirectToAction("MyLessons");
            }


            // =====================================================
            // CHECK IF FEEDBACK ALREADY EXISTS
            // =====================================================

            var existingFeedback =
                db.InstructorFeedbacks
                    .FirstOrDefault(f =>
                        f.BookingId == booking.BookingId);


            if (existingFeedback != null)
            {
                TempData["FeedbackError"] =
                    "Feedback has already been submitted for this lesson.";

                return RedirectToAction("MyLessons");
            }


            // =====================================================
            // VALIDATE FEEDBACK
            // =====================================================

            if (string.IsNullOrWhiteSpace(model.Feedback))
            {
                ModelState.AddModelError(
                    "Feedback",
                    "Please enter feedback for the student.");
            }


            // =====================================================
            // RETURN FORM IF VALIDATION FAILS
            // =====================================================

            if (!ModelState.IsValid)
            {
                ViewBag.Instructor =
                    instructor;

                ViewBag.Booking =
                    booking;

                return View(model);
            }


            // =====================================================
            // FORCE CORRECT RELATIONSHIPS
            // =====================================================

            model.InstructorId =
                instructor.InstructorId;

            model.RegistrationId =
                booking.RegistrationId;

            model.BookingId =
                booking.BookingId;

            model.CreatedDate =
                DateTime.Now;


            // =====================================================
            // SAVE FEEDBACK
            // =====================================================

            db.InstructorFeedbacks.Add(model);

            db.SaveChanges();


            // =====================================================
            // SUCCESS MESSAGE
            // =====================================================

            TempData["FeedbackSuccess"] =
                "Instructor feedback has been submitted successfully.";


            // =====================================================
            // RETURN TO MY LESSONS
            // =====================================================

            return RedirectToAction(
                "MyLessons");
        }


        // =========================================================
        // SIGN OUT
        // =========================================================

        private void AuthenticationManagerSignOut()
        {
            HttpContext
                .GetOwinContext()
                .Authentication
                .SignOut(
                    Microsoft.AspNet.Identity
                        .DefaultAuthenticationTypes
                        .ApplicationCookie);
        }



        // =========================================================
        // DISPOSE DATABASE
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