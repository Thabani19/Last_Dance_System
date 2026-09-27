using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Last_Dance_System.Models;

namespace Last_Dance_System.Controllers
{
    [Authorize(Roles = "Student")]
    public class QuizController : Controller
    {
        private ApplicationDbContext db =
            new ApplicationDbContext();


        // =========================================================
        // CHECK LEARNER THEORY PACKAGE ACCESS
        // =========================================================
        //
        // A student can access quizzes only when they have:
        //
        // 1. A Learner Theory Package
        // 2. Paid for the package
        // 3. An active package
        // 4. The package has not been cancelled
        //
        // LessonsRemaining is NOT checked.
        //
        // One R299 Learner Theory Package unlocks:
        // - All learner lessons
        // - All quizzes
        // - Quiz results
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
        // GET: Quiz
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
                    "before you can access quizzes.";

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
        // GET: Quiz/Start
        // =========================================================

        public ActionResult Start(int? lessonId)
        {
            // =====================================================
            // CHECK PAYMENT ACCESS
            // =====================================================

            if (!HasLearnerTheoryAccess())
            {
                TempData["Error"] =
                    "You need to purchase the Learner Theory Package " +
                    "before you can start a quiz.";

                return RedirectToAction(
                    "Index",
                    "Package");
            }


            // =====================================================
            // VALIDATE LESSON ID
            // =====================================================

            if (lessonId == null)
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
                            lessonId
                        &&
                        l.IsActive);

            if (lesson == null)
            {
                return HttpNotFound();
            }


            // =====================================================
            // GET QUIZ QUESTIONS
            // =====================================================

            var questions =
                db.QuizQuestions
                    .Where(q =>
                        q.LearnerLessonId ==
                            lessonId
                        &&
                        q.IsActive)
                    .OrderBy(q =>
                        q.QuestionOrder)
                    .ToList();

            if (!questions.Any())
            {
                return HttpNotFound();
            }


            // =====================================================
            // CREATE QUIZ ATTEMPT ID
            // =====================================================

            string attemptId =
                Guid.NewGuid().ToString();


            // =====================================================
            // SEND INFORMATION TO VIEW
            // =====================================================

            ViewBag.LessonTitle =
                lesson.Title;

            ViewBag.LessonId =
                lesson.LearnerLessonId;

            ViewBag.AttemptId =
                attemptId;


            return View(questions);
        }


        // =========================================================
        // POST: Quiz/Submit
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Submit(
            int lessonId,
            string attemptId,
            FormCollection answers)
        {
            // =====================================================
            // CHECK PAYMENT ACCESS
            // =====================================================

            if (!HasLearnerTheoryAccess())
            {
                TempData["Error"] =
                    "You need to purchase the Learner Theory Package " +
                    "before you can submit a quiz.";

                return RedirectToAction(
                    "Index",
                    "Package");
            }


            // =====================================================
            // GET CURRENT STUDENT
            // =====================================================

            string userId =
                User.Identity.GetUserId();

            var registration =
                db.Registrations
                    .FirstOrDefault(r =>
                        r.ApplicationUserId ==
                            userId);

            if (registration == null)
            {
                return RedirectToAction(
                    "Register",
                    "Account");
            }


            // =====================================================
            // VALIDATE ATTEMPT ID
            // =====================================================

            if (string.IsNullOrWhiteSpace(
                attemptId))
            {
                attemptId =
                    Guid.NewGuid().ToString();
            }


            // =====================================================
            // FIND LESSON
            // =====================================================

            var lesson =
                db.LearnerLessons
                    .FirstOrDefault(l =>
                        l.LearnerLessonId ==
                            lessonId
                        &&
                        l.IsActive);

            if (lesson == null)
            {
                return HttpNotFound();
            }


            // =====================================================
            // GET QUIZ QUESTIONS
            // =====================================================

            var questions =
                db.QuizQuestions
                    .Where(q =>
                        q.LearnerLessonId ==
                            lessonId
                        &&
                        q.IsActive)
                    .OrderBy(q =>
                        q.QuestionOrder)
                    .ToList();

            if (!questions.Any())
            {
                return HttpNotFound();
            }


            // =====================================================
            // CHECK IF ATTEMPT WAS ALREADY SAVED
            // =====================================================

            var existingResult =
                db.QuizResults
                    .FirstOrDefault(r =>
                        r.AttemptId ==
                            attemptId
                        &&
                        r.RegistrationId ==
                            registration.RegistrationId);

            if (existingResult != null)
            {
                return RedirectToAction(
                    "Result",
                    new
                    {
                        id =
                            existingResult.QuizResultId
                    });
            }


            // =====================================================
            // CALCULATE SCORE
            // =====================================================

            int score = 0;


            // =====================================================
            // CHECK EACH ANSWER
            // =====================================================

            foreach (var question in questions)
            {
                string answer =
                    answers[
                        "Question_" +
                        question.QuizQuestionId];

                if (!string.IsNullOrWhiteSpace(
                    answer))
                {
                    answer =
                        answer.Trim();

                    string correctAnswer =
                        question.CorrectAnswer;

                    if (!string.IsNullOrWhiteSpace(
                        correctAnswer))
                    {
                        correctAnswer =
                            correctAnswer.Trim();
                    }

                    if (string.Equals(
                        answer,
                        correctAnswer,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        score++;
                    }
                }
            }


            // =====================================================
            // CALCULATE TOTAL AND PERCENTAGE
            // =====================================================

            int totalQuestions =
                questions.Count;

            int percentage = 0;

            if (totalQuestions > 0)
            {
                percentage =
                    (score * 100) /
                    totalQuestions;
            }


            // =====================================================
            // CREATE QUIZ RESULT
            // =====================================================

            QuizResult quizResult =
                new QuizResult
                {
                    RegistrationId =
                        registration.RegistrationId,

                    LearnerLessonId =
                        lesson.LearnerLessonId,

                    AttemptId =
                        attemptId,

                    Score =
                        score,

                    TotalQuestions =
                        totalQuestions,

                    Percentage =
                        percentage,

                    DateTaken =
                        DateTime.Now
                };


            // =====================================================
            // SAVE RESULT
            // =====================================================

            db.QuizResults.Add(
                quizResult);

            int rowsSaved =
                db.SaveChanges();

            if (rowsSaved <= 0)
            {
                return Content(
                    "ERROR: Quiz result was not saved to the database.");
            }


            // =====================================================
            // SHOW RESULT
            // =====================================================

            return RedirectToAction(
                "Result",
                new
                {
                    id =
                        quizResult.QuizResultId
                });
        }


        // =========================================================
        // GET: Quiz/Result
        // =========================================================

        public ActionResult Result(int? id)
        {
            // =====================================================
            // CHECK PAYMENT ACCESS
            // =====================================================

            if (!HasLearnerTheoryAccess())
            {
                TempData["Error"] =
                    "You need an active Learner Theory Package " +
                    "to access quiz results.";

                return RedirectToAction(
                    "Index",
                    "Package");
            }


            // =====================================================
            // VALIDATE RESULT ID
            // =====================================================

            if (id == null)
            {
                return new HttpStatusCodeResult(
                    System.Net.HttpStatusCode.BadRequest);
            }


            // =====================================================
            // GET CURRENT STUDENT
            // =====================================================

            string userId =
                User.Identity.GetUserId();

            var registration =
                db.Registrations
                    .FirstOrDefault(r =>
                        r.ApplicationUserId ==
                            userId);

            if (registration == null)
            {
                return RedirectToAction(
                    "Register",
                    "Account");
            }


            // =====================================================
            // GET ONLY THIS STUDENT'S RESULT
            // =====================================================

            var result =
                db.QuizResults
                    .FirstOrDefault(r =>
                        r.QuizResultId == id
                        &&
                        r.RegistrationId ==
                            registration.RegistrationId);

            if (result == null)
            {
                return HttpNotFound();
            }


            // =====================================================
            // GET LESSON
            // =====================================================

            var lesson =
                db.LearnerLessons
                    .FirstOrDefault(l =>
                        l.LearnerLessonId ==
                            result.LearnerLessonId);

            if (lesson != null)
            {
                ViewBag.LessonTitle =
                    lesson.Title;

                ViewBag.LessonId =
                    lesson.LearnerLessonId;
            }


            // =====================================================
            // SEND RESULT INFORMATION TO VIEW
            // =====================================================

            ViewBag.Score =
                result.Score;

            ViewBag.TotalQuestions =
                result.TotalQuestions;

            ViewBag.Percentage =
                result.Percentage;

            ViewBag.DateTaken =
                result.DateTaken;


            return View("Result");
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

