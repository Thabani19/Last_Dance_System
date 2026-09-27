using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Last_Dance_System.Models;
using Last_Dance_System.Services;

namespace Last_Dance_System.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private ApplicationDbContext db =
            new ApplicationDbContext();


        // =========================================================
        // AUTOMATICALLY GENERATE LESSON SCHEDULES
        // =========================================================

        private void GenerateLessonSchedules()
        {
            DateTime startDate =
                DateTime.Today;

            DateTime endDate =
                startDate.AddDays(30);


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
            // GET ACTIVE INSTRUCTORS WITH ASSIGNED VEHICLES
            // =====================================================

            var instructors =
                db.Instructors
                .Include(i => i.Vehicle)
                .Where(i =>
                    i.IsActive &&
                    i.VehicleId.HasValue &&
                    i.Vehicle != null &&
                    i.Vehicle.IsActive)
                .ToList();


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
                // EACH INSTRUCTOR
                // =================================================

                foreach (var instructor in instructors)
                {
                    if (!instructor.VehicleId.HasValue)
                    {
                        continue;
                    }


                    if (instructor.Vehicle == null)
                    {
                        continue;
                    }


                    if (!instructor.Vehicle.IsActive)
                    {
                        continue;
                    }


                    int vehicleId =
                        instructor.VehicleId.Value;


                    // =================================================
                    // GENERATE TIME SLOTS
                    // =================================================

                    foreach (TimeSpan startTime in startTimes)
                    {
                        TimeSpan endTime =
                            startTime.Add(
                                TimeSpan.FromMinutes(30));


                        // =============================================
                        // CHECK IF SLOT ALREADY EXISTS
                        // =============================================

                        bool exists =
                            db.LessonSchedules.Any(s =>
                                s.LessonDate == date &&
                                s.StartTime == startTime &&
                                s.EndTime == endTime &&
                                s.LessonTypeId == 1 &&
                                s.InstructorId ==
                                    instructor.InstructorId &&
                                s.VehicleId ==
                                    vehicleId);


                        // =============================================
                        // CREATE NEW SLOT
                        // =============================================

                        if (!exists)
                        {
                            var schedule =
                                new LessonSchedule
                                {
                                    LessonDate =
                                        date,

                                    StartTime =
                                        startTime,

                                    EndTime =
                                        endTime,

                                    LessonTypeId =
                                        1,

                                    InstructorId =
                                        instructor.InstructorId,

                                    VehicleId =
                                        vehicleId,

                                    Status =
                                        "Available"
                                };

                            db.LessonSchedules.Add(schedule);
                        }
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
            string userId =
                User.Identity.GetUserId();


            // =====================================================
            // FIND STUDENT
            // =====================================================

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


            // =====================================================
            // FIND CURRENT ACTIVE PAID DRIVING PACKAGE
            // =====================================================
            //
            // The Learner Theory Package is excluded.
            //
            // Only a normal driving package can be used for
            // practical driving lesson bookings.
            // =====================================================

            var package =
                db.StudentPackages
                .Include(p => p.LessonPackage)
                .Where(p =>
                    p.RegistrationId ==
                        student.RegistrationId &&

                    p.IsActive &&

                    !p.IsCancelled &&

                    p.PaymentStatus == "Paid" &&

                    p.LessonsRemaining > 0 &&

                    p.LessonPackage != null &&

                    p.LessonPackage.IsActive &&

                    !p.LessonPackage.IsLearnerTheoryPackage &&

                    !string.IsNullOrEmpty(
                        p.LessonPackage.RequiredLicenseCode))
                .OrderByDescending(p =>
                    p.PurchaseDate)
                .FirstOrDefault();


            // =====================================================
            // NO ACTIVE DRIVING PACKAGE
            // =====================================================

            if (package == null)
            {
                TempData["BookingError"] =
                    "You need to purchase an active driving package before booking a driving lesson.";

                return RedirectToAction(
                    "Index",
                    "Package");
            }


            // =====================================================
            // GET REQUIRED LICENCE CODE
            // =====================================================

            string requiredLicenseCode =
                package.LessonPackage
                    .RequiredLicenseCode
                    .Trim();


            // =====================================================
            // GENERATE NEXT 30 DAYS
            // =====================================================

            GenerateLessonSchedules();


            // =====================================================
            // CALCULATE BOOKING DATES
            // =====================================================

            DateTime today =
                DateTime.Today;

            DateTime bookingEndDate =
                today.AddDays(30);

            DateTime now =
                DateTime.Now;


            // =====================================================
            // GET AVAILABLE LESSON SCHEDULES
            // =====================================================
            //
            // IMPORTANT:
            //
            // The student does NOT choose the instructor.
            //
            // The system only shows schedules where the instructor's
            // assigned vehicle matches the student's package code.
            //
            // Example:
            //
            // Code 8 package
            //       ↓
            // Code 8 vehicle
            //       ↓
            // Code 8 instructor
            //
            // Code 10 package
            //       ↓
            // Code 10 vehicle
            //       ↓
            // Code 10 instructor
            //
            // Code 14 package
            //       ↓
            // Code 14 vehicle
            //       ↓
            // Code 14 instructor
            // =====================================================

            var schedules =
                db.LessonSchedules
                .Include(s => s.Instructor)
                .Include(s => s.Vehicle)
                .Include(s => s.LessonType)
                .Where(s =>
                    s.LessonDate >= today &&

                    s.LessonDate <= bookingEndDate &&

                    s.Status == "Available" &&

                    s.Instructor != null &&

                    s.Instructor.IsActive &&

                    s.Instructor.VehicleId.HasValue &&

                    s.Vehicle != null &&

                    s.Vehicle.IsActive &&

                    s.VehicleId ==
                        s.Instructor.VehicleId &&

                    s.Vehicle.LicenseCode != null &&

                    s.Vehicle.LicenseCode ==
                        requiredLicenseCode &&

                    !s.Bookings.Any(b =>
                        b.Status != "Cancelled"))
                .OrderBy(s =>
                    s.LessonDate)
                .ThenBy(s =>
                    s.StartTime)
                .ToList()
                .Where(s =>
                {
                    DateTime lessonEndDateTime =
                        s.LessonDate.Date
                        .Add(s.EndTime);

                    return lessonEndDateTime > now;
                })
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

            ViewBag.RequiredLicenseCode =
                requiredLicenseCode;


            return View(schedules);
        }


        // =========================================================
        // CREATE BOOKING - POST
        // =========================================================

       
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Create(
    DateTime LessonDate,
    TimeSpan StartTime,
    string Notes)
        {
            string userId = User.Identity.GetUserId();

            var student = db.Registrations
                .FirstOrDefault(r =>
                    r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction(
                    "Register",
                    "Account");
            }


            // =========================================================
            // FIND THE STUDENT'S ACTIVE PAID DRIVING PACKAGE
            // =========================================================

            var package = db.StudentPackages
                .Include(p => p.LessonPackage)
                .Where(p =>
                    p.RegistrationId == student.RegistrationId &&
                    p.IsActive &&
                    !p.IsCancelled &&
                    p.PaymentStatus == "Paid" &&
                    p.LessonsRemaining > 0 &&
                    p.LessonPackage != null &&
                    !p.LessonPackage.IsLearnerTheoryPackage &&
                    p.LessonPackage.IsActive &&
                    !string.IsNullOrEmpty(
                        p.LessonPackage.RequiredLicenseCode))
                .OrderByDescending(p => p.PurchaseDate)
                .FirstOrDefault();


            if (package == null)
            {
                TempData["BookingError"] =
                    "You need an active paid driving package with available lessons before booking.";

                return RedirectToAction(
                    "Create",
                    "Booking");
            }


            string requiredLicenseCode =
                package.LessonPackage
                    .RequiredLicenseCode
                    .Trim();


            // =========================================================
            // VALIDATE THE DATE AND TIME
            // =========================================================

            DateTime requestedDate =
                LessonDate.Date;


            DateTime now =
                DateTime.Now;


            if (requestedDate < DateTime.Today)
            {
                TempData["BookingError"] =
                    "You cannot book a lesson for a date that has already passed.";

                return RedirectToAction(
                    "Create",
                    "Booking");
            }


            if (requestedDate > DateTime.Today.AddDays(30))
            {
                TempData["BookingError"] =
                    "Lessons can only be booked within the next 30 days.";

                return RedirectToAction(
                    "Create",
                    "Booking");
            }


            if (requestedDate.DayOfWeek == DayOfWeek.Sunday)
            {
                TempData["BookingError"] =
                    "Driving lessons are not available on Sundays.";

                return RedirectToAction(
                    "Create",
                    "Booking");
            }


            // =========================================================
            // GENERATE SCHEDULES IF NECESSARY
            // =========================================================

            GenerateLessonSchedules();


            // =========================================================
            // AUTOMATICALLY FIND THE APPROPRIATE INSTRUCTOR
            // =========================================================
            //
            // The student does NOT select an instructor.
            //
            // The system looks for:
            //
            // 1. The requested date
            // 2. The requested time
            // 3. An available schedule
            // 4. An active instructor
            // 5. An active vehicle
            // 6. The instructor's vehicle
            // 7. The vehicle's licence code matching
            //    the student's package
            //
            // If several instructors are available, the first
            // matching available instructor is assigned.
            // =========================================================

            var schedule = db.LessonSchedules
                .Include(s => s.Instructor)
                .Include(s => s.Vehicle)
                .FirstOrDefault(s =>
                    s.LessonDate == requestedDate &&
                    s.StartTime == StartTime &&
                    s.Status == "Available" &&

                    s.Instructor != null &&
                    s.Instructor.IsActive &&

                    s.Instructor.VehicleId.HasValue &&

                    s.Vehicle != null &&
                    s.Vehicle.IsActive &&

                    s.VehicleId == s.Instructor.VehicleId &&

                    s.Vehicle.LicenseCode != null &&

                    s.Vehicle.LicenseCode == requiredLicenseCode &&

                    !s.Bookings.Any(b =>
                        b.Status != "Cancelled"));


            // =========================================================
            // NO MATCHING INSTRUCTOR / VEHICLE
            // =========================================================

            if (schedule == null)
            {
                TempData["BookingError"] =
                    "No instructor is currently available for the selected time with the required licence category. Please choose another time.";

                return RedirectToAction(
                    "Create",
                    "Booking");
            }


            // =========================================================
            // DOUBLE-CHECK THE INSTRUCTOR
            // =========================================================

            if (schedule.Instructor == null)
            {
                TempData["BookingError"] =
                    "The selected lesson does not have an instructor assigned.";

                return RedirectToAction(
                    "Create",
                    "Booking");
            }


            if (!schedule.Instructor.IsActive)
            {
                TempData["BookingError"] =
                    "The assigned instructor is no longer active.";

                return RedirectToAction(
                    "Create",
                    "Booking");
            }


            if (!schedule.Instructor.VehicleId.HasValue)
            {
                TempData["BookingError"] =
                    "The assigned instructor does not currently have a vehicle.";

                return RedirectToAction(
                    "Create",
                    "Booking");
            }


            if (schedule.Vehicle == null)
            {
                TempData["BookingError"] =
                    "The assigned vehicle could not be found.";

                return RedirectToAction(
                    "Create",
                    "Booking");
            }


            if (!schedule.Vehicle.IsActive)
            {
                TempData["BookingError"] =
                    "The assigned vehicle is no longer active.";

                return RedirectToAction(
                    "Create",
                    "Booking");
            }


            if (schedule.VehicleId !=
                schedule.Instructor.VehicleId.Value)
            {
                TempData["BookingError"] =
                    "The instructor and vehicle assignment is invalid.";

                return RedirectToAction(
                    "Create",
                    "Booking");
            }


            // =========================================================
            // CHECK LICENCE CODE
            // =========================================================

            if (string.IsNullOrWhiteSpace(
                schedule.Vehicle.LicenseCode))
            {
                TempData["BookingError"] =
                    "The assigned vehicle does not have a licence category.";

                return RedirectToAction(
                    "Create",
                    "Booking");
            }


            if (!string.Equals(
                schedule.Vehicle.LicenseCode.Trim(),
                requiredLicenseCode,
                StringComparison.OrdinalIgnoreCase))
            {
                TempData["BookingError"] =
                    "The assigned vehicle does not match your driving package licence category.";

                return RedirectToAction(
                    "Create",
                    "Booking");
            }


            // =========================================================
            // MAKE SURE THE LESSON HAS NOT ENDED
            // =========================================================

            DateTime lessonEndDateTime =
                schedule.LessonDate.Date
                .Add(schedule.EndTime);


            if (now >= lessonEndDateTime)
            {
                TempData["BookingError"] =
                    "This lesson time has already passed. Please choose another time.";

                return RedirectToAction(
                    "Create",
                    "Booking");
            }


            // =========================================================
            // CHECK IF THE SLOT HAS ALREADY BEEN BOOKED
            // =========================================================

            bool lessonAlreadyBooked =
                db.Bookings.Any(b =>
                    b.LessonScheduleId ==
                        schedule.LessonScheduleId &&
                    b.Status != "Cancelled");


            if (lessonAlreadyBooked)
            {
                TempData["BookingError"] =
                    "This lesson time has just been booked by another student. Please choose another time.";

                return RedirectToAction(
                    "Create",
                    "Booking");
            }


            // =========================================================
            // CHECK IF THIS STUDENT ALREADY BOOKED THIS SLOT
            // =========================================================

            bool alreadyBookedByStudent =
                db.Bookings.Any(b =>
                    b.RegistrationId ==
                        student.RegistrationId &&

                    b.LessonScheduleId ==
                        schedule.LessonScheduleId &&

                    b.Status != "Cancelled");


            if (alreadyBookedByStudent)
            {
                TempData["BookingError"] =
                    "You have already booked this lesson.";

                return RedirectToAction(
                    "Create",
                    "Booking");
            }


            // =========================================================
            // CREATE BOOKING
            // =========================================================

            var booking = new Booking
            {
                RegistrationId =
                    student.RegistrationId,

                StudentPackageId =
                    package.StudentPackageId,

                LessonScheduleId =
                    schedule.LessonScheduleId,

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


            // Mark the selected schedule as booked.
            schedule.Status =
                "Booked";


            // Consume one lesson from the student's package.
            package.LessonsRemaining -= 1;


            // Save everything together.
            db.SaveChanges();


            // =========================================================
            // SUCCESS MESSAGE
            // =========================================================

            TempData["BookingSuccess"] =
                "Your lesson has been booked successfully. An appropriate instructor has been automatically assigned to you.";


            return RedirectToAction(
                "MyLessons",
                "Booking");
        }



        // =========================================================
        // AUTOMATICALLY COMPLETE FINISHED LESSONS
        // =========================================================

        private void UpdateCompletedLessons(
            string registrationId)
        {
            var bookings =
                db.Bookings
                .Include(b => b.LessonSchedule)
                .Where(b =>
                    b.RegistrationId ==
                        registrationId &&

                    b.Status == "Booked")
                .ToList();


            DateTime now =
                DateTime.Now;


            foreach (var booking in bookings)
            {
                if (booking.LessonSchedule == null)
                {
                    continue;
                }


                DateTime lessonEndDateTime =
                    booking.LessonSchedule
                        .LessonDate
                        .Date
                        .Add(
                            booking.LessonSchedule.EndTime);


                if (now >= lessonEndDateTime)
                {
                    booking.Status =
                        "Completed";

                    booking.LessonSchedule.Status =
                        "Completed";
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
            string userId =
                User.Identity.GetUserId();


            // =====================================================
            // FIND STUDENT
            // =====================================================

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


            // =====================================================
            // AUTOMATICALLY COMPLETE FINISHED LESSONS
            // =====================================================

            UpdateCompletedLessons(
                student.RegistrationId);


            // =====================================================
            // GET BOOKINGS
            // =====================================================

            var lessons =
                db.Bookings
                .Include(b => b.LessonSchedule)
                .Include(b => b.LessonSchedule.Instructor)
                .Include(b => b.LessonSchedule.Vehicle)
                .Include(b => b.LessonSchedule.LessonType)
                .Where(b =>
                    b.RegistrationId ==
                        student.RegistrationId)
                .OrderByDescending(b =>
                    b.LessonSchedule.LessonDate)
                .ThenByDescending(b =>
                    b.LessonSchedule.StartTime)
                .ToList();


            // =====================================================
            // GET ACTIVE PACKAGE
            // =====================================================

            var package =
                db.StudentPackages
                .Include(p => p.LessonPackage)
                .Where(p =>
                    p.RegistrationId ==
                        student.RegistrationId &&

                    p.IsActive &&

                    p.PaymentStatus == "Paid" &&

                    !p.IsCancelled)
                .OrderByDescending(p =>
                    p.PurchaseDate)
                .FirstOrDefault();


            // =====================================================
            // PACKAGE INFORMATION
            // =====================================================

            if (package != null &&
                package.LessonPackage != null)
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
        public ActionResult ConfirmAttendance(
            int id)
        {
            string userId =
                User.Identity.GetUserId();


            // =====================================================
            // FIND STUDENT
            // =====================================================

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


            // =====================================================
            // FIND BOOKING
            // =====================================================

            var booking =
                db.Bookings
                .Include(b => b.LessonSchedule)
                .FirstOrDefault(b =>
                    b.BookingId == id &&

                    b.RegistrationId ==
                        student.RegistrationId);


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

                return RedirectToAction(
                    "MyLessons");
            }


            // =====================================================
            // CHECK LESSON SCHEDULE
            // =====================================================

            if (booking.LessonSchedule == null)
            {
                TempData["BookingError"] =
                    "The lesson schedule could not be found.";

                return RedirectToAction(
                    "MyLessons");
            }


            // =====================================================
            // CALCULATE LESSON END
            // =====================================================

            DateTime lessonEndDateTime =
                booking.LessonSchedule
                    .LessonDate
                    .Date
                    .Add(
                        booking.LessonSchedule.EndTime);


            // =====================================================
            // MAKE SURE LESSON HAS ENDED
            // =====================================================

            if (DateTime.Now <
                lessonEndDateTime)
            {
                TempData["BookingError"] =
                    "You can only confirm attendance after the lesson has ended.";

                return RedirectToAction(
                    "MyLessons");
            }


            // =====================================================
            // CHECK IF ALREADY CONFIRMED
            // =====================================================

            if (booking.AttendanceStatus ==
                "Attended")
            {
                TempData["BookingError"] =
                    "Attendance has already been confirmed.";

                return RedirectToAction(
                    "MyLessons");
            }


            // =====================================================
            // CONFIRM ATTENDANCE
            // =====================================================

            booking.AttendanceStatus =
                "Attended";

            booking.AttendanceConfirmedDate =
                DateTime.Now;


            // =====================================================
            // MARK LESSON COMPLETED
            // =====================================================

            booking.Status =
                "Completed";


            if (booking.LessonSchedule != null)
            {
                booking.LessonSchedule.Status =
                    "Completed";
            }


            // =====================================================
            // SAVE
            // =====================================================

            db.SaveChanges();


            // =====================================================
            // SUCCESS MESSAGE
            // =====================================================

            TempData["BookingSuccess"] =
                "Your attendance has been confirmed successfully!";


            return RedirectToAction(
                "MyLessons");
        }


        // =========================================================
        // INSTRUCTOR - COMPLETE LESSON
        // =========================================================

        [Authorize(Roles = "Instructor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CompleteLesson(
            int id)
        {
            string userId =
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
            // FIND BOOKING
            // =====================================================

            var booking =
                db.Bookings
                .Include("LessonSchedule")
                .FirstOrDefault(b =>
                    b.BookingId == id &&

                    b.LessonSchedule.InstructorId ==
                        instructor.InstructorId);


            if (booking == null)
            {
                return HttpNotFound();
            }


            // =====================================================
            // CHECK IF ALREADY COMPLETED
            // =====================================================

            if (booking.Status == "Completed")
            {
                TempData["BookingError"] =
                    "This lesson has already been completed.";

                return RedirectToAction(
                    "Index",
                    "InstructorDashboard");
            }


            // =====================================================
            // CHECK IF CANCELLED
            // =====================================================

            if (booking.Status == "Cancelled")
            {
                TempData["BookingError"] =
                    "A cancelled lesson cannot be completed.";

                return RedirectToAction(
                    "Index",
                    "InstructorDashboard");
            }


            // =====================================================
            // MARK BOOKING COMPLETED
            // =====================================================

            booking.Status =
                "Completed";


            // =====================================================
            // MARK STUDENT ATTENDED
            // =====================================================

            booking.AttendanceStatus =
                "Attended";

            booking.AttendanceConfirmedDate =
                DateTime.Now;


            // =====================================================
            // MARK LESSON SCHEDULE COMPLETED
            // =====================================================

            if (booking.LessonSchedule != null)
            {
                booking.LessonSchedule.Status =
                    "Completed";
            }


            // =====================================================
            // SAVE
            // =====================================================

            db.SaveChanges();


            // =====================================================
            // SUCCESS MESSAGE
            // =====================================================

            TempData["BookingSuccess"] =
                "Lesson has been marked as completed successfully.";


            return RedirectToAction(
                "Index",
                "InstructorDashboard");
        }


        // =========================================================
        // CANCEL BOOKING
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(
            int id)
        {
            string userId =
                User.Identity.GetUserId();


            // =====================================================
            // FIND STUDENT
            // =====================================================

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


            // =====================================================
            // FIND BOOKING
            // =====================================================

            var booking =
                db.Bookings
                .Include(b => b.LessonSchedule)
                .Include(b => b.LessonSchedule.Instructor)
                .Include(b => b.LessonSchedule.Vehicle)
                .FirstOrDefault(b =>
                    b.BookingId == id &&

                    b.RegistrationId ==
                        student.RegistrationId);


            if (booking == null)
            {
                return HttpNotFound();
            }


            // =====================================================
            // ALREADY CANCELLED
            // =====================================================

            if (booking.Status == "Cancelled")
            {
                return RedirectToAction(
                    "MyLessons");
            }


            // =====================================================
            // DO NOT CANCEL COMPLETED LESSON
            // =====================================================

            if (booking.Status == "Completed")
            {
                TempData["BookingError"] =
                    "A completed lesson cannot be cancelled.";

                return RedirectToAction(
                    "MyLessons");
            }


            // =====================================================
            // STORE INFORMATION
            // =====================================================

            var lesson =
                booking.LessonSchedule;

            var instructor =
                lesson != null
                    ? lesson.Instructor
                    : null;


            // =====================================================
            // CANCEL BOOKING
            // =====================================================

            booking.Status =
                "Cancelled";


            // =====================================================
            // MAKE LESSON AVAILABLE AGAIN
            // =====================================================

            if (booking.LessonSchedule != null)
            {
                booking.LessonSchedule.Status =
                    "Available";
            }


            // =====================================================
            // RETURN LESSON CREDIT
            // =====================================================

            if (booking.StudentPackageId.HasValue)
            {
                var package =
                    db.StudentPackages
                    .FirstOrDefault(p =>
                        p.StudentPackageId ==
                            booking.StudentPackageId.Value);


                if (package != null &&
                    package.PaymentStatus == "Paid" &&
                    package.IsActive &&
                    !package.IsCancelled)
                {
                    package.LessonsRemaining += 1;
                }
            }


            // =====================================================
            // SAVE DATABASE CHANGES
            // =====================================================

            db.SaveChanges();


            // =====================================================
            // SEND EMAIL TO INSTRUCTOR
            // =====================================================

            bool instructorEmailSent =
                SendStudentCancellationEmail(
                    student,
                    instructor,
                    lesson);


            // =====================================================
            // CREATE IN-SYSTEM NOTIFICATION
            // =====================================================

            bool instructorNotificationCreated =
                CreateInstructorCancellationNotification(
                    student,
                    instructor,
                    lesson);


            // =====================================================
            // SUCCESS MESSAGE
            // =====================================================

            if (instructorEmailSent &&
                instructorNotificationCreated)
            {
                TempData["BookingSuccess"] =
                    "Your lesson has been cancelled successfully. " +
                    "Your lesson credit has been returned and " +
                    "your instructor has been notified by email and " +
                    "through the system.";
            }
            else if (instructorEmailSent)
            {
                TempData["BookingSuccess"] =
                    "Your lesson has been cancelled successfully. " +
                    "Your lesson credit has been returned and " +
                    "your instructor has been notified by email.";
            }
            else if (instructorNotificationCreated)
            {
                TempData["BookingSuccess"] =
                    "Your lesson has been cancelled successfully. " +
                    "Your lesson credit has been returned and " +
                    "your instructor has been notified through the system.";
            }
            else
            {
                TempData["BookingSuccess"] =
                    "Your lesson has been cancelled and the lesson " +
                    "credit has been returned.";
            }


            return RedirectToAction(
                "MyLessons");
        }


        // =========================================================
        // CREATE IN-SYSTEM NOTIFICATION FOR INSTRUCTOR
        // =========================================================

        private bool CreateInstructorCancellationNotification(
            Registration student,
            Instructor instructor,
            LessonSchedule lesson)
        {
            try
            {
                // =================================================
                // CHECK REQUIRED INFORMATION
                // =================================================

                if (student == null)
                {
                    return false;
                }


                if (instructor == null)
                {
                    return false;
                }


                if (lesson == null)
                {
                    return false;
                }


                // =================================================
                // STUDENT NAME
                // =================================================

                string studentName =
                    student.FirstName +
                    " " +
                    student.LastName;


                // =================================================
                // LESSON DATE
                // =================================================

                string lessonDate =
                    lesson.LessonDate
                    .ToString(
                        "dddd, dd MMMM yyyy");


                // =================================================
                // LESSON TIME
                // =================================================

                string startTime =
                    DateTime.Today
                    .Add(lesson.StartTime)
                    .ToString("HH:mm");

                string endTime =
                    DateTime.Today
                    .Add(lesson.EndTime)
                    .ToString("HH:mm");


                // =================================================
                // CREATE NOTIFICATION
                // =================================================

                var notification =
                    new Notification
                    {
                        RegistrationId =
                            null,

                        InstructorId =
                            instructor.InstructorId,

                        Title =
                            "Lesson Cancelled",

                        Message =
                            studentName +
                            " has cancelled their driving lesson " +
                            "scheduled for " +
                            lessonDate +
                            " from " +
                            startTime +
                            " to " +
                            endTime +
                            ". The lesson slot is now available " +
                            "for another student.",

                        NotificationType =
                            "Cancellation",

                        IsRead =
                            false,

                        CreatedAt =
                            DateTime.Now,

                        ReadAt =
                            null
                    };


                // =================================================
                // SAVE NOTIFICATION
                // =================================================

                db.Notifications.Add(
                    notification);

                db.SaveChanges();


                return true;
            }
            catch (Exception)
            {
                // =================================================
                // NOTIFICATION FAILURE MUST NOT UNDO CANCELLATION
                // =================================================

                return false;
            }
        }


        // =========================================================
        // SEND STUDENT CANCELLATION EMAIL TO INSTRUCTOR
        // =========================================================

        private bool SendStudentCancellationEmail(
            Registration student,
            Instructor instructor,
            LessonSchedule lesson)
        {
            try
            {
                // =================================================
                // CHECK REQUIRED INFORMATION
                // =================================================

                if (student == null)
                {
                    return false;
                }


                if (instructor == null)
                {
                    return false;
                }


                if (lesson == null)
                {
                    return false;
                }


                // =================================================
                // FIND INSTRUCTOR APPLICATION USER
                // =================================================

                var instructorUser =
                    db.Users
                    .FirstOrDefault(u =>
                        u.Id ==
                        instructor.ApplicationUserId);


                if (instructorUser == null)
                {
                    return false;
                }


                // =================================================
                // CHECK INSTRUCTOR EMAIL
                // =================================================

                if (string.IsNullOrWhiteSpace(
                    instructorUser.Email))
                {
                    return false;
                }


                // =================================================
                // FORMAT LESSON DATE
                // =================================================

                string lessonDate =
                    lesson.LessonDate
                    .ToString(
                        "dddd, dd MMMM yyyy");


                // =================================================
                // FORMAT LESSON TIME
                // =================================================

                string startTime =
                    DateTime.Today
                    .Add(lesson.StartTime)
                    .ToString("HH:mm");

                string endTime =
                    DateTime.Today
                    .Add(lesson.EndTime)
                    .ToString("HH:mm");


                // =================================================
                // STUDENT NAME
                // =================================================

                string studentName =
                    student.FirstName +
                    " " +
                    student.LastName;


                // =================================================
                // INSTRUCTOR NAME
                // =================================================

                string instructorName =
                    instructor.FirstName +
                    " " +
                    instructor.LastName;


                // =================================================
                // VEHICLE INFORMATION
                // =================================================

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


                // =================================================
                // EMAIL SUBJECT
                // =================================================

                string subject =
                    "Student Cancelled Driving Lesson - Last Dance Driving School";


                // =================================================
                // EMAIL BODY
                // =================================================

                string body =
                    "Dear " +
                    instructor.FirstName +
                    "," +

                    Environment.NewLine +
                    Environment.NewLine +

                    "This is to inform you that a student has " +
                    "cancelled their scheduled driving lesson." +

                    Environment.NewLine +
                    Environment.NewLine +

                    "LESSON DETAILS" +

                    Environment.NewLine +

                    "Student: " +
                    studentName +

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

                    "The student cancelled this lesson through " +
                    "the Last Dance Driving School system." +

                    Environment.NewLine +
                    Environment.NewLine +

                    "The lesson slot has been made available again " +
                    "for another student to book." +

                    Environment.NewLine +
                    Environment.NewLine +

                    "The student's lesson credit has also been " +
                    "returned to their lesson package." +

                    Environment.NewLine +
                    Environment.NewLine +

                    "Kind regards," +

                    Environment.NewLine +

                    "Last Dance Driving School";


                // =================================================
                // SEND EMAIL
                // =================================================

                Last_Dance_System.Services.EmailService emailService =
                    new Last_Dance_System.Services.EmailService();


                emailService.SendEmail(
                    instructorUser.Email,
                    subject,
                    body);


                return true;
            }
            catch (Exception)
            {
                // =================================================
                // EMAIL FAILURE MUST NOT UNDO CANCELLATION
                // =================================================

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