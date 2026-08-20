using Last_Dance_System.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;

namespace Last_Dance_System.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class InstructorController : Controller
    {
        private readonly ApplicationDbContext db;

        private ApplicationUserManager _userManager;

        public InstructorController()
        {
            db = new ApplicationDbContext();
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ??
                    HttpContext.GetOwinContext()
                    .GetUserManager<ApplicationUserManager>();
            }
        }

        // GET: Instructor/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Instructor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(
            InstructorRegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if email already exists
            var existingUser =
                await UserManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "This email address is already registered.");

                return View(model);
            }

            // Check if license number already exists
            var existingInstructor = db.Instructors
                .FirstOrDefault(i =>
                    i.LicenseNumber == model.LicenseNumber);

            if (existingInstructor != null)
            {
                ModelState.AddModelError(
                    "LicenseNumber",
                    "This license number is already registered.");

                return View(model);
            }

            // Create Identity account
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await UserManager.CreateAsync(
                user,
                model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }

                return View(model);
            }

            // Assign Instructor role
            var roleResult = await UserManager.AddToRoleAsync(
                user.Id,
                "Instructor");

            if (!roleResult.Succeeded)
            {
                await UserManager.DeleteAsync(user);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError("", error);
                }

                return View(model);
            }

            // Create Instructor record
            var instructor = new Instructor
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Phone = model.Phone,
                Email = model.Email,
                LicenseNumber = model.LicenseNumber,
                Experience = model.Experience,
                IsActive = true,

                // Link Instructor to AspNetUsers
                ApplicationUserId = user.Id
            };

            db.Instructors.Add(instructor);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception)
            {
                // If saving instructor fails,
                // remove the Identity account as well.
                await UserManager.DeleteAsync(user);

                ModelState.AddModelError(
                    "",
                    "The instructor could not be created.");

                return View(model);
            }

            return RedirectToAction("Index", "Instructor");
        }

        // GET: Instructor/Index
        public ActionResult Index()
        {
            var instructors = db.Instructors.ToList();

            return View(instructors);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();

                if (_userManager != null)
                {
                    _userManager.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}