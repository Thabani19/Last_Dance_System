using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Last_Dance_System.Models;

namespace Last_Dance_System.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        // =========================================================
        // AUTOMATICALLY GENERATE LESSON SCHEDULES
        // =========================================================

        private void GenerateLessonSchedules()
        {
            DateTime startDate = DateTime.Today;
            DateTime endDate = startDate.AddDays(30);


            // =====================================================
            // LESSON TIMES
            // =====================================================

            TimeSpan[] startTimes =
            {
                new TimeSpan(9, 0, 0),
                new TimeSpan(10, 0, 0),
                new TimeSpan(11, 0, 0),
                new TimeSpan(13, 0, 0),
                new TimeSpan(14, 0, 0),
                new TimeSpan(15, 0, 0)
            };


            // =====================================================
            // GENERATE DAYS
            // =====================================================

            for (DateTime date = startDate;
                 date <= endDate;
                 date = date.AddDays(1))
            {
                // Skip Sunday
                if (date.DayOfWeek == DayOfWeek.Sunday)
                {
                    continue;
                }


                // =================================================
                // GENERATE TIME SLOTS
                // =================================================

                foreach (TimeSpan startTime in startTimes)
                {
                    TimeSpan endTime =
                        startTime.Add(TimeSpan.FromMinutes(30));


                    // =============================================
                    // CHECK IF THIS SLOT ALREADY EXISTS
                    // =============================================

                    bool exists = db.LessonSchedules.Any(s =>
                        s.LessonDate == date &&
                        s.StartTime == startTime &&
                        s.EndTime == endTime &&
                        s.LessonTypeId == 1 &&
                        s.InstructorId == 1 &&
                        s.VehicleId == 1);


                    // =============================================
                    // CREATE NEW SLOT
                    // =============================================

                    if (!exists)
                    {
                        var schedule = new LessonSchedule
                        {
                            LessonDate = date,
                            StartTime = startTime,
                            EndTime = endTime,

                            LessonTypeId = 1,
                            InstructorId = 1,
                            VehicleId = 1,

                            Status = "Available"
                        };

                        db.LessonSchedules.Add(schedule);
                    }
                }
            }


            // =====================================================
            // SAVE GENERATED LESSONS
            // =====================================================

            db.SaveChanges();
        }


        // =========================================================
        // BOOK A LESSON - GET
        // =========================================================

        [HttpGet]
        public ActionResult Create()
        {
            string userId = User.Identity.GetUserId();


            // =====================================================
            // FIND STUDENT
            // =====================================================

            var student = db.Registrations
                .FirstOrDefault(r =>
                    r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction("Register", "Account");
            }


            // =====================================================
            // FIND ACTIVE PAID PACKAGE
            // =====================================================

            var package = db.StudentPackages
                .Where(p =>
                    p.RegistrationId == student.RegistrationId &&
                    p.IsActive &&
                    p.PaymentStatus == "Paid" &&
                    p.LessonsRemaining > 0)
                .OrderByDescending(p => p.PurchaseDate)
                .FirstOrDefault();


            // =====================================================
            // NO ACTIVE PACKAGE
            // =====================================================

            if (package == null)
            {
                TempData["BookingError"] =
                    "You need to purchase a lesson package before booking a lesson.";

                return RedirectToAction("Index", "Package");
            }


            // =====================================================
            // GENERATE NEXT 30 DAYS
            // =====================================================

            GenerateLessonSchedules();


            // =====================================================
            // CALCULATE DATES BEFORE LINQ QUERY
            // =====================================================

            DateTime today = DateTime.Today;
            DateTime bookingEndDate = today.AddDays(30);


            // =====================================================
            // GET AVAILABLE LESSONS
            // =====================================================

            var schedules = db.LessonSchedules
                .Where(s =>
                    s.LessonDate >= today &&
                    s.LessonDate <= bookingEndDate &&
                    s.Status == "Available" &&
                    !s.Bookings.Any(b =>
                        b.Status != "Cancelled"))
                .OrderBy(s => s.LessonDate)
                .ThenBy(s => s.StartTime)
                .ToList();


            // =====================================================
            // PACKAGE INFORMATION
            // =====================================================

            ViewBag.PackageName =
                package.LessonPackage.PackageName;

            ViewBag.LessonsPurchased =
                package.LessonsPurchased;

            ViewBag.LessonsRemaining =
                package.LessonsRemaining;


            // =====================================================
            // RETURN VIEW
            // =====================================================

            return View(schedules);
        }


        // =========================================================
        // CREATE BOOKING - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            int LessonScheduleId,
            string Notes)
        {
            string userId = User.Identity.GetUserId();


            // =====================================================
            // FIND STUDENT
            // =====================================================

            var student = db.Registrations
                .FirstOrDefault(r =>
                    r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction("Register", "Account");
            }


            // =====================================================
            // FIND ACTIVE PAID PACKAGE
            // =====================================================

            var package = db.StudentPackages
                .Where(p =>
                    p.RegistrationId == student.RegistrationId &&
                    p.IsActive &&
                    p.PaymentStatus == "Paid" &&
                    p.LessonsRemaining > 0)
                .OrderByDescending(p => p.PurchaseDate)
                .FirstOrDefault();


            // =====================================================
            // PACKAGE VALIDATION
            // =====================================================

            if (package == null)
            {
                TempData["BookingError"] =
                    "You need an active paid package with available lessons before booking.";

                return RedirectToAction("Index", "Package");
            }


            // =====================================================
            // FIND SCHEDULE
            // =====================================================

            var schedule = db.LessonSchedules
                .FirstOrDefault(s =>
                    s.LessonScheduleId == LessonScheduleId);

            if (schedule == null)
            {
                TempData["BookingError"] =
                    "The selected lesson could not be found.";

                return RedirectToAction("Create");
            }


            // =====================================================
            // CHECK STATUS
            // =====================================================

            if (schedule.Status != "Available")
            {
                TempData["BookingError"] =
                    "This lesson is no longer available.";

                return RedirectToAction("Create");
            }


            // =====================================================
            // CHECK DATE
            // =====================================================

            if (schedule.LessonDate < DateTime.Today)
            {
                TempData["BookingError"] =
                    "You cannot book a lesson that has already passed.";

                return RedirectToAction("Create");
            }


            // =====================================================
            // CHECK IF ANOTHER STUDENT BOOKED IT
            // =====================================================

            bool lessonAlreadyBooked = db.Bookings
                .Any(b =>
                    b.LessonScheduleId == LessonScheduleId &&
                    b.Status != "Cancelled");

            if (lessonAlreadyBooked)
            {
                TempData["BookingError"] =
                    "This lesson has already been booked by another student.";

                return RedirectToAction("Create");
            }


            // =====================================================
            // CHECK CURRENT STUDENT
            // =====================================================

            bool alreadyBookedByStudent = db.Bookings
                .Any(b =>
                    b.RegistrationId == student.RegistrationId &&
                    b.LessonScheduleId == LessonScheduleId &&
                    b.Status != "Cancelled");

            if (alreadyBookedByStudent)
            {
                TempData["BookingError"] =
                    "You have already booked this lesson.";

                return RedirectToAction("Create");
            }


            // =====================================================
            // CREATE BOOKING
            // =====================================================

            var booking = new Booking
            {
                RegistrationId =
                    student.RegistrationId,

                StudentPackageId =
                    package.StudentPackageId,

                LessonScheduleId =
                    LessonScheduleId,

                BookingDate =
                    DateTime.Now,

                Status =
                    "Booked",

                AttendanceStatus =
                    "Not Confirmed",

                Notes =
                    Notes
            };

            db.Bookings.Add(booking);


            // =====================================================
            // MARK SCHEDULE AS BOOKED
            // =====================================================

            schedule.Status = "Booked";


            // =====================================================
            // REMOVE ONE LESSON CREDIT
            // =====================================================

            package.LessonsRemaining -= 1;


            // =====================================================
            // SAVE
            // =====================================================

            db.SaveChanges();


            // =====================================================
            // SUCCESS MESSAGE
            // =====================================================

            TempData["BookingSuccess"] =
                "Your lesson has been booked successfully!";


            return RedirectToAction("MyLessons");
        }


        // =========================================================
        // AUTOMATICALLY COMPLETE FINISHED LESSONS
        // =========================================================

        private void UpdateCompletedLessons(string registrationId)
        {
            var bookings = db.Bookings
                .Where(b =>
                    b.RegistrationId == registrationId &&
                    b.Status == "Booked")
                .ToList();

            DateTime now = DateTime.Now;

            foreach (var booking in bookings)
            {
                if (booking.LessonSchedule == null)
                {
                    continue;
                }

                DateTime lessonEndDateTime =
                    booking.LessonSchedule.LessonDate.Date
                    .Add(booking.LessonSchedule.EndTime);

                if (now >= lessonEndDateTime)
                {
                    booking.Status = "Completed";
                }
            }

            db.SaveChanges();
        }


        // =========================================================
        // MY LESSONS
        // =========================================================

        [HttpGet]
        public ActionResult MyLessons()
        {
            string userId = User.Identity.GetUserId();


            // =====================================================
            // FIND STUDENT
            // =====================================================

            var student = db.Registrations
                .FirstOrDefault(r =>
                    r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction("Register", "Account");
            }


            // =====================================================
            // AUTOMATICALLY COMPLETE FINISHED LESSONS
            // =====================================================

            UpdateCompletedLessons(student.RegistrationId);


            // =====================================================
            // GET BOOKINGS
            // =====================================================

            var lessons = db.Bookings
                .Where(b =>
                    b.RegistrationId == student.RegistrationId)
                .OrderByDescending(b =>
                    b.LessonSchedule.LessonDate)
                .ThenByDescending(b =>
                    b.LessonSchedule.StartTime)
                .ToList();


            // =====================================================
            // GET ACTIVE PACKAGE
            // =====================================================

            var package = db.StudentPackages
                .Where(p =>
                    p.RegistrationId == student.RegistrationId &&
                    p.IsActive &&
                    p.PaymentStatus == "Paid")
                .OrderByDescending(p =>
                    p.PurchaseDate)
                .FirstOrDefault();


            // =====================================================
            // PACKAGE INFORMATION
            // =====================================================

            if (package != null)
            {
                ViewBag.PackageName =
                    package.LessonPackage.PackageName;

                ViewBag.LessonsPurchased =
                    package.LessonsPurchased;

                ViewBag.LessonsRemaining =
                    package.LessonsRemaining;
            }
            else
            {
                ViewBag.PackageName =
                    "No Active Package";

                ViewBag.LessonsPurchased =
                    0;

                ViewBag.LessonsRemaining =
                    0;
            }


            return View(lessons);
        }


        // =========================================================
        // CONFIRM ATTENDANCE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmAttendance(int id)
        {
            string userId = User.Identity.GetUserId();


            // =====================================================
            // FIND STUDENT
            // =====================================================

            var student = db.Registrations
                .FirstOrDefault(r =>
                    r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction("Register", "Account");
            }


            // =====================================================
            // FIND BOOKING
            // =====================================================

            var booking = db.Bookings
                .FirstOrDefault(b =>
                    b.BookingId == id &&
                    b.RegistrationId == student.RegistrationId);

            if (booking == null)
            {
                return HttpNotFound();
            }


            // =====================================================
            // CHECK BOOKING STATUS
            // =====================================================

            if (booking.Status == "Cancelled")
            {
                TempData["BookingError"] =
                    "You cannot confirm attendance for a cancelled lesson.";

                return RedirectToAction("MyLessons");
            }


            // =====================================================
            // CHECK LESSON
            // =====================================================

            if (booking.LessonSchedule == null)
            {
                TempData["BookingError"] =
                    "The lesson schedule could not be found.";

                return RedirectToAction("MyLessons");
            }


            // =====================================================
            // CALCULATE LESSON START
            // =====================================================

            DateTime lessonStartDateTime =
                booking.LessonSchedule.LessonDate.Date
                .Add(booking.LessonSchedule.StartTime);


            // =====================================================
            // CALCULATE LESSON END
            // =====================================================

            DateTime lessonEndDateTime =
                booking.LessonSchedule.LessonDate.Date
                .Add(booking.LessonSchedule.EndTime);


            // =====================================================
            // MAKE SURE LESSON HAS ENDED
            // =====================================================

            if (DateTime.Now < lessonEndDateTime)
            {
                TempData["BookingError"] =
                    "You can only confirm attendance after the lesson has ended.";

                return RedirectToAction("MyLessons");
            }


            // =====================================================
            // CHECK IF ALREADY CONFIRMED
            // =====================================================

            if (booking.AttendanceStatus == "Attended")
            {
                TempData["BookingError"] =
                    "Attendance has already been confirmed.";

                return RedirectToAction("MyLessons");
            }


            // =====================================================
            // CONFIRM ATTENDANCE
            // =====================================================

            booking.AttendanceStatus = "Attended";

            booking.AttendanceConfirmedDate = DateTime.Now;


            // =====================================================
            // LESSON HAS ENDED
            // =====================================================

            booking.Status = "Completed";


            // =====================================================
            // SAVE
            // =====================================================

            db.SaveChanges();


            // =====================================================
            // SUCCESS MESSAGE
            // =====================================================

            TempData["BookingSuccess"] =
                "Your attendance has been confirmed successfully!";


            return RedirectToAction("MyLessons");
        }


        // =========================================================
        // CANCEL BOOKING
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            string userId = User.Identity.GetUserId();


            // =====================================================
            // FIND STUDENT
            // =====================================================

            var student = db.Registrations
                .FirstOrDefault(r =>
                    r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction("Register", "Account");
            }


            // =====================================================
            // FIND BOOKING
            // =====================================================

            var booking = db.Bookings
                .FirstOrDefault(b =>
                    b.BookingId == id &&
                    b.RegistrationId == student.RegistrationId);

            if (booking == null)
            {
                return HttpNotFound();
            }


            // =====================================================
            // ALREADY CANCELLED
            // =====================================================

            if (booking.Status == "Cancelled")
            {
                return RedirectToAction("MyLessons");
            }


            // =====================================================
            // CANCEL BOOKING
            // =====================================================

            booking.Status = "Cancelled";


            // =====================================================
            // MAKE LESSON AVAILABLE AGAIN
            // =====================================================

            if (booking.LessonSchedule != null)
            {
                booking.LessonSchedule.Status = "Available";
            }


            // =====================================================
            // RETURN LESSON CREDIT
            // =====================================================

            if (booking.StudentPackageId.HasValue)
            {
                var package = db.StudentPackages
                    .FirstOrDefault(p =>
                        p.StudentPackageId ==
                        booking.StudentPackageId.Value);

                if (package != null &&
                    package.PaymentStatus == "Paid")
                {
                    package.LessonsRemaining += 1;
                }
            }


            // =====================================================
            // SAVE
            // =====================================================

            db.SaveChanges();


            // =====================================================
            // SUCCESS MESSAGE
            // =====================================================

            TempData["BookingSuccess"] =
                "Your lesson has been cancelled and the lesson credit has been returned.";


            return RedirectToAction("MyLessons");
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