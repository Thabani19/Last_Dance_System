using Last_Dance_System.Models;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Last_Dance_System.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class VehicleController : Controller
    {
        private readonly ApplicationDbContext db;

        public VehicleController()
        {
            db = new ApplicationDbContext();
        }


        // =========================================================
        // VEHICLE LIST
        // =========================================================

        [HttpGet]
        public ActionResult Index()
        {
            var vehicles = db.Vehicles
                .Include(v => v.Instructors)
                .OrderBy(v => v.Make)
                .ThenBy(v => v.Model)
                .ToList();

            return View(vehicles);
        }


        // =========================================================
        // CREATE VEHICLE - GET
        // =========================================================

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }


        // =========================================================
        // CREATE VEHICLE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Vehicle model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // =====================================================
            // CHECK REGISTRATION NUMBER
            // =====================================================

            var registrationExists =
                db.Vehicles.Any(v =>
                    v.RegistrationNumber == model.RegistrationNumber);

            if (registrationExists)
            {
                ModelState.AddModelError(
                    "RegistrationNumber",
                    "A vehicle with this registration number already exists.");

                return View(model);
            }


            // =====================================================
            // DEFAULT STATUS
            // =====================================================

            model.IsActive = true;

            if (string.IsNullOrWhiteSpace(model.Status))
            {
                model.Status = "Active";
            }


            // =====================================================
            // SAVE
            // =====================================================

            db.Vehicles.Add(model);

            db.SaveChanges();


            TempData["VehicleSuccess"] =
                "Vehicle was successfully added.";


            return RedirectToAction("Index");
        }


        // =========================================================
        // EDIT VEHICLE - GET
        // =========================================================

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var vehicle = db.Vehicles
                .Include(v => v.Instructors)
                .FirstOrDefault(v =>
                    v.VehicleId == id);

            if (vehicle == null)
            {
                return HttpNotFound();
            }


            // =====================================================
            // GET ALL INSTRUCTORS
            // =====================================================

            ViewBag.Instructors =
                db.Instructors
                    .Where(i => i.IsActive)
                    .OrderBy(i => i.FirstName)
                    .ThenBy(i => i.LastName)
                    .ToList();


            return View(vehicle);
        }


        // =========================================================
        // EDIT VEHICLE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            Vehicle model,
            int? InstructorId)
        {
            // =====================================================
            // FIND VEHICLE
            // =====================================================

            var vehicle = db.Vehicles
                .Include(v => v.Instructors)
                .FirstOrDefault(v =>
                    v.VehicleId ==
                    model.VehicleId);

            if (vehicle == null)
            {
                return HttpNotFound();
            }


            // =====================================================
            // VALIDATION
            // =====================================================

            if (!ModelState.IsValid)
            {
                ViewBag.Instructors =
                    db.Instructors
                        .Where(i => i.IsActive)
                        .OrderBy(i => i.FirstName)
                        .ThenBy(i => i.LastName)
                        .ToList();

                return View(model);
            }


            // =====================================================
            // CHECK DUPLICATE REGISTRATION
            // =====================================================

            var registrationExists =
                db.Vehicles.Any(v =>
                    v.VehicleId != model.VehicleId
                    &&
                    v.RegistrationNumber ==
                    model.RegistrationNumber);

            if (registrationExists)
            {
                ModelState.AddModelError(
                    "RegistrationNumber",
                    "Another vehicle already uses this registration number.");

                ViewBag.Instructors =
                    db.Instructors
                        .Where(i => i.IsActive)
                        .OrderBy(i => i.FirstName)
                        .ThenBy(i => i.LastName)
                        .ToList();

                return View(model);
            }


            // =====================================================
            // UPDATE VEHICLE DETAILS
            // =====================================================

            vehicle.Make =
                model.Make;

            vehicle.Model =
                model.Model;

            vehicle.RegistrationNumber =
                model.RegistrationNumber;

            vehicle.VehicleType =
                model.VehicleType;

            vehicle.CurrentMileage =
                model.CurrentMileage;


            // =====================================================
            // ACTIVE / INACTIVE
            // =====================================================

            vehicle.IsActive =
                model.IsActive;


            // =====================================================
            // UPDATE STATUS
            // =====================================================

            vehicle.Status =
                model.IsActive
                    ? "Active"
                    : "Not Active";


            // =====================================================
            // REMOVE CURRENT INSTRUCTOR ASSIGNMENT
            // =====================================================

            foreach (var instructor in vehicle.Instructors.ToList())
            {
                instructor.VehicleId = null;
            }


            // =====================================================
            // ASSIGN NEW INSTRUCTOR
            // =====================================================

            if (InstructorId.HasValue)
            {
                var instructor =
                    db.Instructors
                        .FirstOrDefault(i =>
                            i.InstructorId ==
                            InstructorId.Value);

                if (instructor != null)
                {
                    instructor.VehicleId =
                        vehicle.VehicleId;
                }
            }


            // =====================================================
            // SAVE
            // =====================================================

            db.SaveChanges();


            TempData["VehicleSuccess"] =
                "Vehicle details were successfully updated.";


            return RedirectToAction("Index");
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