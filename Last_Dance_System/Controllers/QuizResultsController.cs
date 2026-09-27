
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Last_Dance_System.Models;

namespace Last_Dance_System.Controllers
{
    [Authorize(Roles = "Student")]
    public class QuizResultsController : Controller
    {
        private ApplicationDbContext db =
            new ApplicationDbContext();


        // =========================================================
        // CHECK LEARNER THEORY PACKAGE ACCESS
        // =========================================================
        //
        // Quiz results are part of the Learner Theory Package.
        //
        // The student must have:
        //
        // 1. A Learner Theory Package
        // 2. Payment status = Paid
        // 3. Package = Active
        // 4. Package = Not Cancelled
        //
        // LessonsRemaining is NOT checked.
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
        // GET: QuizResults
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
                    "before you can access your quiz results.";

                return RedirectToAction(
                    "Index",
                    "Package");
            }


            // =====================================================
            // GET CURRENT STUDENT
            // =====================================================

            string userId =
                User.Identity.GetUserId();


            // Find the Registration belonging to
            // the currently logged-in student.
            var registration =
                db.Registrations
                    .FirstOrDefault(r =>
                        r.ApplicationUserId == userId);


            // =====================================================
            // CHECK REGISTRATION
            // =====================================================

            if (registration == null)
            {
                ViewBag.TotalAttempts = 0;

                ViewBag.AverageScore = 0;

                ViewBag.BestScore = 0;

                ViewBag.AveragePercentage = 0;

                return View(
                    new List<QuizResult>());
            }


            // =====================================================
            // GET THIS STUDENT'S QUIZ RESULTS
            // =====================================================
            //
            // Include LearnerLesson so the view can
            // display the lesson title.
            // =====================================================

            var results =
                db.QuizResults

                    .Include(r =>
                        r.LearnerLesson)

                    .Where(r =>
                        r.RegistrationId ==
                        registration.RegistrationId)

                    .OrderByDescending(r =>
                        r.DateTaken)

                    .ToList();


            // =====================================================
            // CALCULATE STATISTICS
            // =====================================================

            int totalAttempts =
                results.Count;

            int averageScore = 0;

            int bestScore = 0;

            int averagePercentage = 0;


            if (results.Any())
            {
                averageScore =
                    (int)Math.Round(
                        results.Average(r =>
                            r.Score));

                bestScore =
                    results.Max(r =>
                        r.Score);

                averagePercentage =
                    (int)Math.Round(
                        results.Average(r =>
                            r.Percentage));
            }


            // =====================================================
            // SEND STATISTICS TO VIEW
            // =====================================================

            ViewBag.TotalAttempts =
                totalAttempts;

            ViewBag.AverageScore =
                averageScore;

            ViewBag.BestScore =
                bestScore;

            ViewBag.AveragePercentage =
                averagePercentage;


            // =====================================================
            // RETURN RESULTS
            // =====================================================

            return View(results);
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

