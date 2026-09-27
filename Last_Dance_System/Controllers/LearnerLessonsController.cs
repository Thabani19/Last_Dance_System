
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Last_Dance_System.Models;

namespace Last_Dance_System.Controllers
{
    [Authorize(Roles = "Student")]
    public class LearnerLessonsController : Controller
    {
        private ApplicationDbContext db =
            new ApplicationDbContext();


        // =========================================================
        // CHECK LEARNER THEORY PACKAGE ACCESS
        // =========================================================
        //
        // A student can access learner theory only when they have:
        //
        // 1. A Learner Theory Package
        // 2. Paid for the package
        // 3. An active package
        // 4. The package has not been cancelled
        //
        // The theory package does NOT depend on LessonsRemaining.
        //
        // One purchase unlocks all learner theory lessons.
        // =========================================================

        private bool HasLearnerTheoryAccess()
        {
            string userId =
                User.Identity.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            var student =
                db.Registrations
                    .FirstOrDefault(r =>
                        r.ApplicationUserId == userId);

            if (student == null)
            {
                return false;
            }

            bool hasTheoryPackage =
                db.StudentPackages.Any(sp =>

                    sp.RegistrationId ==
                        student.RegistrationId

                    &&

                    sp.LessonPackage
                        .IsLearnerTheoryPackage == true

                    &&

                    sp.PaymentStatus ==
                        "Paid"

                    &&

                    sp.IsActive == true

                    &&

                    sp.IsCancelled == false
                );

            return hasTheoryPackage;
        }


        // =========================================================
        // GET: LearnerLessons
        // =========================================================

        public ActionResult Index()
        {
            // =====================================================
            // CHECK PAYMENT ACCESS
            // =====================================================

            if (!HasLearnerTheoryAccess())
            {
                TempData["Error"] =
                    "You need to purchase the Learner Theory Package " +
                    "before you can access the learner theory lessons.";

                return RedirectToAction(
                    "Index",
                    "Package");
            }


            // =====================================================
            // GET ACTIVE LEARNER LESSONS
            // =====================================================

            var lessons =
                db.LearnerLessons

                    .Where(l =>
                        l.IsActive)

                    .OrderBy(l =>
                        l.LessonOrder)

                    .ToList();

            return View(lessons);
        }


        // =========================================================
        // GET: LearnerLessons/Details/5
        // =========================================================

        public ActionResult Details(int? id)
        {
            // =====================================================
            // CHECK PAYMENT ACCESS
            // =====================================================

            if (!HasLearnerTheoryAccess())
            {
                TempData["Error"] =
                    "You need to purchase the Learner Theory Package " +
                    "before you can access learner theory.";

                return RedirectToAction(
                    "Index",
                    "Package");
            }


            // =====================================================
            // VALIDATE ID
            // =====================================================

            if (id == null)
            {
                return new HttpStatusCodeResult(
                    System.Net.HttpStatusCode.BadRequest);
            }


            // =====================================================
            // FIND LESSON
            // =====================================================

            var lesson =
                db.LearnerLessons

                    .FirstOrDefault(l =>
                        l.LearnerLessonId ==
                            id

                        &&

                        l.IsActive);

            if (lesson == null)
            {
                return HttpNotFound();
            }

            return View(lesson);
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

