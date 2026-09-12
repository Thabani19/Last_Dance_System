using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Last_Dance_System.Models;

namespace Last_Dance_System.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminLessonPackagesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        // =========================================================
        // INDEX
        // =========================================================

        public ActionResult Index()
        {
            var packages = db.LessonPackages
                .OrderByDescending(p => p.IsActive)
                .ThenBy(p => p.PackageName)
                .ToList();

            return View(packages);
        }


        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public ActionResult Create()
        {
            var package = new LessonPackage
            {
                IsActive = true
            };

            return View(package);
        }


        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LessonPackage package)
        {
            try
            {
                // -------------------------------------------------
                // VALIDATE PACKAGE NAME
                // -------------------------------------------------

                if (string.IsNullOrWhiteSpace(package.PackageName))
                {
                    ModelState.AddModelError(
                        "PackageName",
                        "Please enter a package name."
                    );
                }


                // -------------------------------------------------
                // VALIDATE NUMBER OF LESSONS
                // -------------------------------------------------

                if (package.NumberOfLessons <= 0)
                {
                    ModelState.AddModelError(
                        "NumberOfLessons",
                        "Number of lessons must be greater than 0."
                    );
                }


                // -------------------------------------------------
                // VALIDATE PRICE
                // -------------------------------------------------

                if (package.Price <= 0)
                {
                    ModelState.AddModelError(
                        "Price",
                        "Package price must be greater than R0.00."
                    );
                }


                // -------------------------------------------------
                // CHECK DUPLICATE PACKAGE NAME
                // -------------------------------------------------

                var duplicatePackage = db.LessonPackages
                    .Any(p =>
                        p.PackageName.ToLower() ==
                        package.PackageName.ToLower()
                    );

                if (duplicatePackage)
                {
                    ModelState.AddModelError(
                        "PackageName",
                        "A lesson package with this name already exists."
                    );
                }


                // -------------------------------------------------
                // RETURN VIEW IF VALIDATION FAILS
                // -------------------------------------------------

                if (!ModelState.IsValid)
                {
                    return View(package);
                }


                // -------------------------------------------------
                // CLEAN PACKAGE NAME
                // -------------------------------------------------

                package.PackageName =
                    package.PackageName.Trim();


                // -------------------------------------------------
                // DEFAULT ACTIVE
                // -------------------------------------------------

                package.IsActive = true;


                // -------------------------------------------------
                // SAVE
                // -------------------------------------------------

                db.LessonPackages.Add(package);

                db.SaveChanges();


                TempData["SuccessMessage"] =
                    "Lesson package created successfully.";


                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] =
                    "An error occurred while creating the lesson package.";

                return View(package);
            }
        }


        // =========================================================
        // DETAILS - GET
        // =========================================================

        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest
                );
            }


            var package = db.LessonPackages
                .FirstOrDefault(p =>
                    p.LessonPackageId == id
                );


            if (package == null)
            {
                return HttpNotFound();
            }


            return View(package);
        }


        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest
                );
            }


            var package = db.LessonPackages
                .FirstOrDefault(p =>
                    p.LessonPackageId == id
                );


            if (package == null)
            {
                return HttpNotFound();
            }


            return View(package);
        }


        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(LessonPackage package)
        {
            try
            {
                // -------------------------------------------------
                // VALIDATE PACKAGE NAME
                // -------------------------------------------------

                if (string.IsNullOrWhiteSpace(package.PackageName))
                {
                    ModelState.AddModelError(
                        "PackageName",
                        "Please enter a package name."
                    );
                }


                // -------------------------------------------------
                // VALIDATE LESSONS
                // -------------------------------------------------

                if (package.NumberOfLessons <= 0)
                {
                    ModelState.AddModelError(
                        "NumberOfLessons",
                        "Number of lessons must be greater than 0."
                    );
                }


                // -------------------------------------------------
                // VALIDATE PRICE
                // -------------------------------------------------

                if (package.Price <= 0)
                {
                    ModelState.AddModelError(
                        "Price",
                        "Package price must be greater than R0.00."
                    );
                }


                // -------------------------------------------------
                // CHECK EXISTING PACKAGE
                // -------------------------------------------------

                var existingPackage = db.LessonPackages
                    .FirstOrDefault(p =>
                        p.LessonPackageId ==
                        package.LessonPackageId
                    );


                if (existingPackage == null)
                {
                    return HttpNotFound();
                }


                // -------------------------------------------------
                // CHECK DUPLICATE NAME
                // -------------------------------------------------

                var duplicatePackage = db.LessonPackages
                    .Any(p =>
                        p.LessonPackageId !=
                        package.LessonPackageId
                        &&
                        p.PackageName.ToLower() ==
                        package.PackageName.ToLower()
                    );


                if (duplicatePackage)
                {
                    ModelState.AddModelError(
                        "PackageName",
                        "Another lesson package already uses this name."
                    );
                }


                // -------------------------------------------------
                // RETURN IF INVALID
                // -------------------------------------------------

                if (!ModelState.IsValid)
                {
                    return View(package);
                }


                // -------------------------------------------------
                // UPDATE FIELDS
                // -------------------------------------------------

                existingPackage.PackageName =
                    package.PackageName.Trim();

                existingPackage.NumberOfLessons =
                    package.NumberOfLessons;

                existingPackage.Price =
                    package.Price;

                existingPackage.Description =
                    package.Description;

                existingPackage.IsActive =
                    package.IsActive;


                // -------------------------------------------------
                // SAVE
                // -------------------------------------------------

                db.Entry(existingPackage).State =
                    EntityState.Modified;

                db.SaveChanges();


                TempData["SuccessMessage"] =
                    "Lesson package updated successfully.";


                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] =
                    "An error occurred while updating the lesson package.";

                return View(package);
            }
        }


        // =========================================================
        // DELETE - GET
        // =========================================================

        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest
                );
            }


            var package = db.LessonPackages
                .FirstOrDefault(p =>
                    p.LessonPackageId == id
                );


            if (package == null)
            {
                return HttpNotFound();
            }


            return View(package);
        }


        // =========================================================
        // DELETE - POST
        // =========================================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var package = db.LessonPackages
                    .FirstOrDefault(p =>
                        p.LessonPackageId == id
                    );


                if (package == null)
                {
                    TempData["ErrorMessage"] =
                        "The lesson package could not be found.";

                    return RedirectToAction("Index");
                }


                // -------------------------------------------------
                // CHECK WHETHER PACKAGE IS BEING USED
                // -------------------------------------------------

                var packageIsUsed =
                    db.StudentPackages
                    .Any(sp =>
                        sp.LessonPackageId == id
                    );


                if (packageIsUsed)
                {
                    TempData["ErrorMessage"] =
                        "This package cannot be deleted because it is already being used by a student. Deactivate it instead.";

                    return RedirectToAction("Index");
                }


                // -------------------------------------------------
                // DELETE
                // -------------------------------------------------

                db.LessonPackages.Remove(package);

                db.SaveChanges();


                TempData["SuccessMessage"] =
                    "Lesson package deleted successfully.";


                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] =
                    "The lesson package could not be deleted.";

                return RedirectToAction("Index");
            }
        }


        // =========================================================
        // ACTIVATE / DEACTIVATE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            try
            {
                var package = db.LessonPackages
                    .FirstOrDefault(p =>
                        p.LessonPackageId == id
                    );


                if (package == null)
                {
                    TempData["ErrorMessage"] =
                        "The lesson package could not be found.";

                    return RedirectToAction("Index");
                }


                // -------------------------------------------------
                // TOGGLE STATUS
                // -------------------------------------------------

                package.IsActive =
                    !package.IsActive;


                db.Entry(package).State =
                    EntityState.Modified;

                db.SaveChanges();


                // -------------------------------------------------
                // SUCCESS MESSAGE
                // -------------------------------------------------

                if (package.IsActive)
                {
                    TempData["SuccessMessage"] =
                        "Lesson package activated successfully.";
                }
                else
                {
                    TempData["SuccessMessage"] =
                        "Lesson package deactivated successfully.";
                }


                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] =
                    "An error occurred while changing the package status.";

                return RedirectToAction("Index");
            }
        }


        // =========================================================
        // ACTIVATE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Activate(int id)
        {
            try
            {
                var package = db.LessonPackages
                    .FirstOrDefault(p =>
                        p.LessonPackageId == id
                    );


                if (package == null)
                {
                    TempData["ErrorMessage"] =
                        "The lesson package could not be found.";

                    return RedirectToAction("Index");
                }


                package.IsActive = true;

                db.Entry(package).State =
                    EntityState.Modified;

                db.SaveChanges();


                TempData["SuccessMessage"] =
                    "Lesson package activated successfully.";


                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] =
                    "Unable to activate the lesson package.";

                return RedirectToAction("Index");
            }
        }


        // =========================================================
        // DEACTIVATE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deactivate(int id)
        {
            try
            {
                var package = db.LessonPackages
                    .FirstOrDefault(p =>
                        p.LessonPackageId == id
                    );


                if (package == null)
                {
                    TempData["ErrorMessage"] =
                        "The lesson package could not be found.";

                    return RedirectToAction("Index");
                }


                package.IsActive = false;

                db.Entry(package).State =
                    EntityState.Modified;

                db.SaveChanges();


                TempData["SuccessMessage"] =
                    "Lesson package deactivated successfully.";


                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] =
                    "Unable to deactivate the lesson package.";

                return RedirectToAction("Index");
            }
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