using Last_Dance_System.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Last_Dance_System.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext db;

        public AdminController()
        {
            db = new ApplicationDbContext();
        }


        // =========================================================
        // ADMIN DASHBOARD
        // =========================================================

        [HttpGet]
        public ActionResult Index()
        {
            var totalStudents = db.Registrations.Count();

            var totalInstructors = db.Instructors.Count();

            var activeInstructors = db.Instructors
                .Count(i => i.IsActive);

            var totalVehicles = db.Vehicles.Count();

            var totalBookings = db.Bookings.Count();

            var bookedLessons = db.Bookings
                .Count(b =>
                    b.Status != null &&
                    b.Status == "Booked");

            var completedLessons = db.Bookings
                .Count(b =>
                    b.Status != null &&
                    b.Status == "Completed");

            var cancelledLessons = db.Bookings
                .Count(b =>
                    b.Status != null &&
                    b.Status == "Cancelled");

            var activeStudentPackages = db.StudentPackages
                .Count(p =>
                    p.IsActive &&
                    !p.IsCancelled);

            var totalSchedules = db.LessonSchedules.Count();

            var today = DateTime.Today;

            var upcomingLessons = db.LessonSchedules
                .Count(l =>
                    l.LessonDate >= today &&
                    l.Status != "Cancelled" &&
                    l.Status != "Completed");

            var recentBookings = db.Bookings
                .Include(b => b.Registration)
                .Include(b => b.LessonSchedule)
                .OrderByDescending(b => b.BookingDate)
                .Take(5)
                .ToList();


            ViewBag.TotalStudents =
                totalStudents;

            ViewBag.TotalInstructors =
                totalInstructors;

            ViewBag.ActiveInstructors =
                activeInstructors;

            ViewBag.TotalVehicles =
                totalVehicles;

            ViewBag.TotalBookings =
                totalBookings;

            ViewBag.BookedLessons =
                bookedLessons;

            ViewBag.CompletedLessons =
                completedLessons;

            ViewBag.CancelledLessons =
                cancelledLessons;

            ViewBag.ActiveStudentPackages =
                activeStudentPackages;

            ViewBag.TotalSchedules =
                totalSchedules;

            ViewBag.UpcomingLessons =
                upcomingLessons;

            ViewBag.RecentBookings =
                recentBookings;


            return View();
        }


        // =========================================================
        // LESSON PACKAGES
        // =========================================================

        [HttpGet]
        public ActionResult LessonPackages()
        {
            var packages = db.LessonPackages
                .OrderBy(p => p.PackageName)
                .ToList();

            return View(packages);
        }


        // =========================================================
        // LESSON SCHEDULES
        // =========================================================

        [HttpGet]
        public ActionResult LessonSchedules()
        {
            var schedules = db.LessonSchedules
                .Include(l => l.LessonType)
                .Include(l => l.Instructor)
                .Include(l => l.Vehicle)
                .OrderBy(l => l.LessonDate)
                .ThenBy(l => l.StartTime)
                .ToList();

            return View(schedules);
        }


        // =========================================================
        // STUDENTS
        // =========================================================

        [HttpGet]
        public ActionResult Students()
        {
            var students = db.Registrations
                .OrderBy(s => s.FirstName)
                .ThenBy(s => s.LastName)
                .ToList();

            return View(students);
        }


        // =========================================================
        // INSTRUCTORS - LIST
        // =========================================================

        [HttpGet]
        public ActionResult Instructors()
        {
            var instructors = db.Instructors
                .Include(i => i.Vehicle)
                .OrderBy(i => i.FirstName)
                .ThenBy(i => i.LastName)
                .ToList();

            return View(instructors);
        }


        // =========================================================
        // INSTRUCTORS - EDIT GET
        // =========================================================

        [HttpGet]
        public ActionResult EditInstructor(int id)
        {
            var instructor = db.Instructors
                .Include(i => i.Vehicle)
                .FirstOrDefault(i =>
                    i.InstructorId == id);

            if (instructor == null)
            {
                return HttpNotFound();
            }


            // -----------------------------------------------------
            // AVAILABLE VEHICLES
            //
            // Show:
            //
            // 1. Active vehicles
            //
            // AND
            //
            // 2. Vehicles that are not assigned to another
            //    instructor
            //
            // The instructor's current vehicle is also included.
            // -----------------------------------------------------

            var availableVehicles = db.Vehicles
                .Where(v =>
                    v.IsActive &&
                    (
                        !db.Instructors.Any(i =>
                            i.VehicleId == v.VehicleId &&
                            i.InstructorId != instructor.InstructorId)
                        ||
                        v.VehicleId == instructor.VehicleId
                    ))
                .OrderBy(v => v.Make)
                .ThenBy(v => v.Model)
                .ToList();


            ViewBag.Vehicles = new SelectList(
                availableVehicles,
                "VehicleId",
                "RegistrationNumber",
                instructor.VehicleId
            );


            return View(instructor);
        }


        // =========================================================
        // INSTRUCTORS - EDIT POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditInstructor(Instructor instructor)
        {
            if (instructor == null)
            {
                return HttpNotFound();
            }


            // -----------------------------------------------------
            // REMOVE NAVIGATION PROPERTY VALIDATION
            // -----------------------------------------------------

            ModelState.Remove("ApplicationUser");

            ModelState.Remove("Vehicle");

            ModelState.Remove("LessonSchedules");

            ModelState.Remove("Reviews");


            // -----------------------------------------------------
            // FIND EXISTING INSTRUCTOR
            // -----------------------------------------------------

            var existingInstructor = db.Instructors
                .FirstOrDefault(i =>
                    i.InstructorId == instructor.InstructorId);

            if (existingInstructor == null)
            {
                return HttpNotFound();
            }


            // -----------------------------------------------------
            // CLEAN INPUT
            // -----------------------------------------------------

            instructor.FirstName =
                instructor.FirstName?.Trim();

            instructor.LastName =
                instructor.LastName?.Trim();

            instructor.Phone =
                instructor.Phone?.Trim();

            instructor.Email =
                instructor.Email?.Trim();

            instructor.LicenseNumber =
                instructor.LicenseNumber?.Trim();


            // =====================================================
            // VEHICLE VALIDATION
            // =====================================================

            if (instructor.VehicleId.HasValue)
            {
                // -------------------------------------------------
                // CHECK VEHICLE EXISTS AND IS ACTIVE
                // -------------------------------------------------

                var vehicle = db.Vehicles
                    .FirstOrDefault(v =>
                        v.VehicleId ==
                        instructor.VehicleId.Value
                        &&
                        v.IsActive);

                if (vehicle == null)
                {
                    ModelState.AddModelError(
                        "VehicleId",
                        "The selected vehicle is not active or does not exist."
                    );
                }
                else
                {
                    // ---------------------------------------------
                    // CHECK IF ANOTHER INSTRUCTOR HAS THIS VEHICLE
                    // ---------------------------------------------

                    var vehicleAlreadyAssigned =
                        db.Instructors.Any(i =>
                            i.VehicleId == vehicle.VehicleId
                            &&
                            i.InstructorId !=
                            instructor.InstructorId);


                    if (vehicleAlreadyAssigned)
                    {
                        ModelState.AddModelError(
                            "VehicleId",
                            "This vehicle is already assigned to another instructor."
                        );
                    }
                }
            }


            // =====================================================
            // MODEL VALIDATION
            // =====================================================

            if (!ModelState.IsValid)
            {
                // Rebuild dropdown so the page does not crash
                // when validation fails.

                var availableVehicles = db.Vehicles
                    .Where(v =>
                        v.IsActive &&
                        (
                            !db.Instructors.Any(i =>
                                i.VehicleId == v.VehicleId &&
                                i.InstructorId !=
                                instructor.InstructorId)
                            ||
                            v.VehicleId ==
                            instructor.VehicleId
                        ))
                    .OrderBy(v => v.Make)
                    .ThenBy(v => v.Model)
                    .ToList();


                ViewBag.Vehicles = new SelectList(
                    availableVehicles,
                    "VehicleId",
                    "RegistrationNumber",
                    instructor.VehicleId
                );


                return View(instructor);
            }


            // =====================================================
            // UPDATE INSTRUCTOR INFORMATION
            // =====================================================

            existingInstructor.FirstName =
                instructor.FirstName;

            existingInstructor.LastName =
                instructor.LastName;

            existingInstructor.Phone =
                instructor.Phone;

            existingInstructor.Email =
                instructor.Email;

            existingInstructor.LicenseNumber =
                instructor.LicenseNumber;

            existingInstructor.Experience =
                instructor.Experience;

            existingInstructor.IsActive =
                instructor.IsActive;


            // =====================================================
            // UPDATE VEHICLE
            // =====================================================

            existingInstructor.VehicleId =
                instructor.VehicleId;


            // =====================================================
            // SAVE CHANGES
            // =====================================================

            db.SaveChanges();


            TempData["SuccessMessage"] =
                "Instructor updated successfully.";


            return RedirectToAction("Instructors");
        }


        // =========================================================
        // VEHICLES - LIST
        // =========================================================

        [HttpGet]
        public ActionResult Vehicles()
        {
            var vehicles = db.Vehicles
                .OrderBy(v => v.Make)
                .ThenBy(v => v.Model)
                .ToList();

            return View(vehicles);
        }


        // =========================================================
        // VEHICLES - CREATE
        // =========================================================

        [HttpGet]
        public ActionResult CreateVehicle()
        {
            var vehicle = new Vehicle
            {
                Status = "Available",
                IsActive = true
            };

            return View(vehicle);
        }


        // =========================================================
        // VEHICLES - CREATE POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateVehicle(Vehicle vehicle)
        {
            if (vehicle == null)
            {
                return View();
            }


            // -----------------------------------------------------
            // DEFAULT STATUS
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(vehicle.Status))
            {
                vehicle.Status = "Available";
            }


            // -----------------------------------------------------
            // CLEAN INPUT
            // -----------------------------------------------------

            vehicle.Make =
                vehicle.Make?.Trim();

            vehicle.Model =
                vehicle.Model?.Trim();

            vehicle.RegistrationNumber =
                vehicle.RegistrationNumber?.Trim();

            vehicle.VehicleType =
                vehicle.VehicleType?.Trim();

            vehicle.Status =
                vehicle.Status?.Trim();


            // -----------------------------------------------------
            // CHECK DUPLICATE REGISTRATION NUMBER
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(
                vehicle.RegistrationNumber))
            {
                var registrationExists =
                    db.Vehicles.Any(v =>
                        v.RegistrationNumber ==
                        vehicle.RegistrationNumber);

                if (registrationExists)
                {
                    ModelState.AddModelError(
                        "RegistrationNumber",
                        "A vehicle with this registration number already exists."
                    );
                }
            }


            // -----------------------------------------------------
            // VALIDATION
            // -----------------------------------------------------

            if (!ModelState.IsValid)
            {
                return View(vehicle);
            }


            // -----------------------------------------------------
            // SAVE VEHICLE
            // -----------------------------------------------------

            db.Vehicles.Add(vehicle);

            db.SaveChanges();


            TempData["SuccessMessage"] =
                "Vehicle added successfully.";


            return RedirectToAction("Vehicles");
        }


        // =========================================================
        // VEHICLES - EDIT GET
        // =========================================================

        [HttpGet]
        public ActionResult EditVehicle(int id)
        {
            var vehicle = db.Vehicles
                .FirstOrDefault(v =>
                    v.VehicleId == id);

            if (vehicle == null)
            {
                return HttpNotFound();
            }

            return View(vehicle);
        }


        // =========================================================
        // VEHICLES - EDIT POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditVehicle(Vehicle vehicle)
        {
            if (vehicle == null)
            {
                return HttpNotFound();
            }


            // -----------------------------------------------------
            // CLEAN INPUT
            // -----------------------------------------------------

            vehicle.Make =
                vehicle.Make?.Trim();

            vehicle.Model =
                vehicle.Model?.Trim();

            vehicle.RegistrationNumber =
                vehicle.RegistrationNumber?.Trim();

            vehicle.VehicleType =
                vehicle.VehicleType?.Trim();

            vehicle.Status =
                vehicle.Status?.Trim();


            // -----------------------------------------------------
            // CHECK DUPLICATE REGISTRATION NUMBER
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(
                vehicle.RegistrationNumber))
            {
                var registrationExists =
                    db.Vehicles.Any(v =>
                        v.RegistrationNumber ==
                        vehicle.RegistrationNumber
                        &&
                        v.VehicleId !=
                        vehicle.VehicleId);

                if (registrationExists)
                {
                    ModelState.AddModelError(
                        "RegistrationNumber",
                        "Another vehicle already uses this registration number."
                    );
                }
            }


            // -----------------------------------------------------
            // VALIDATION
            // -----------------------------------------------------

            if (!ModelState.IsValid)
            {
                return View(vehicle);
            }


            // -----------------------------------------------------
            // FIND EXISTING VEHICLE
            // -----------------------------------------------------

            var existingVehicle = db.Vehicles
                .FirstOrDefault(v =>
                    v.VehicleId ==
                    vehicle.VehicleId);

            if (existingVehicle == null)
            {
                return HttpNotFound();
            }


            // -----------------------------------------------------
            // UPDATE VEHICLE
            // -----------------------------------------------------

            existingVehicle.Make =
                vehicle.Make;

            existingVehicle.Model =
                vehicle.Model;

            existingVehicle.RegistrationNumber =
                vehicle.RegistrationNumber;

            existingVehicle.VehicleType =
                vehicle.VehicleType;

            existingVehicle.Status =
                vehicle.Status;

            existingVehicle.CurrentMileage =
                vehicle.CurrentMileage;

            existingVehicle.IsActive =
                vehicle.IsActive;


            // -----------------------------------------------------
            // IMPORTANT
            //
            // If a vehicle becomes inactive, it remains assigned
            // to the instructor, but cannot be assigned to another
            // instructor because EditInstructor only shows active
            // vehicles.
            // -----------------------------------------------------

            db.SaveChanges();


            TempData["SuccessMessage"] =
                "Vehicle updated successfully.";


            return RedirectToAction("Vehicles");
        }


        // =========================================================
        // VEHICLES - ACTIVATE / DEACTIVATE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleVehicleStatus(int id)
        {
            var vehicle = db.Vehicles
                .FirstOrDefault(v =>
                    v.VehicleId == id);

            if (vehicle == null)
            {
                return HttpNotFound();
            }


            vehicle.IsActive =
                !vehicle.IsActive;


            if (!vehicle.IsActive)
            {
                vehicle.Status =
                    "Unavailable";
            }
            else
            {
                vehicle.Status =
                    "Available";
            }


            db.SaveChanges();


            TempData["SuccessMessage"] =
                vehicle.IsActive
                ? "Vehicle activated successfully."
                : "Vehicle deactivated successfully.";


            return RedirectToAction("Vehicles");
        }


        // =========================================================
        // BOOKINGS
        // =========================================================

        [HttpGet]
        public ActionResult Bookings()
        {
            var bookings = db.Bookings
                .Include(b => b.Registration)
                .Include(b => b.StudentPackage)
                .Include(b => b.LessonSchedule)
                .Include(b => b.LessonSchedule.Instructor)
                .Include(b => b.LessonSchedule.Vehicle)
                .OrderByDescending(
                    b => b.LessonSchedule.LessonDate)
                .ThenBy(
                    b => b.LessonSchedule.StartTime)
                .ToList();

            return View(bookings);
        }


        // =========================================================
        // PAYMENTS
        // =========================================================

        [HttpGet]
        public ActionResult Payments()
        {
            var payments = db.Payments
                .Include(p => p.Registration)
                .Include(p => p.Booking)
                .Include(p => p.StudentPackage)
                .OrderByDescending(p => p.PaymentDate)
                .ToList();

            return View(payments);
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