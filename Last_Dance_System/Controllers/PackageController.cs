using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Last_Dance_System.Models;

namespace Last_Dance_System.Controllers
{
    [Authorize]
    public class PackageController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // =========================================================
        // AVAILABLE PACKAGES
        // =========================================================
        public ActionResult Index()
        {
            var packages = db.LessonPackages
                .Where(p => p.IsActive)
                .OrderBy(p => p.Price)
                .ToList();

            return View(packages);
        }


        // =========================================================
        // PACKAGE DETAILS
        // =========================================================
        public ActionResult Details(int id)
        {
            var package = db.LessonPackages
                .FirstOrDefault(p =>
                    p.LessonPackageId == id &&
                    p.IsActive);

            if (package == null)
            {
                return HttpNotFound();
            }

            return View(package);
        }


        // =========================================================
        // PURCHASE PACKAGE - GET
        // =========================================================
        public ActionResult Purchase(int id)
        {
            string userId = User.Identity.GetUserId();

            // Find logged-in student
            var student = db.Registrations
                .FirstOrDefault(r => r.ApplicationUserId == userId);

            if (student == null)
            {
                return RedirectToAction("Register", "Account");
            }

            // Find selected package
            var package = db.LessonPackages
                .FirstOrDefault(p =>
                    p.LessonPackageId == id &&
                    p.IsActive);

            if (package == null)
            {
                return HttpNotFound();
            }

            // Send package information to purchase page
            return View(package);
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