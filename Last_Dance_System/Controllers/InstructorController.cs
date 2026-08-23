using Last_Dance_System.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Last_Dance_System.Controllers
{
    //[Authorize(Roles = "Instructor")]// Temporary commented for testing
    public class InstructorController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private int GetCurrentInstructorId()
        {
            string userEmail = User.Identity.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
                return 0;
            }

            var instructor = db.Instructors.FirstOrDefault(i => i.Email == userEmail);
            return instructor != null ? instructor.InstructorId : 0;
        }

        private void AutoMarkAbsentLessons(int instructorId)
        {
            DateTime today = DateTime.Today;

            // Find past scheduled lessons without progress entries
            var overdueSchedules = db .LessonSchedules
                .Where(s => s.InstructorId == instructorId && s.LessonDate < today)
                .ToList();

            foreach (var schedule in overdueSchedules)
            {
                var booking = db.Bookings.FirstOrDefault(b => b.LessonScheduleId == schedule.LessonScheduleId);
                if (booking != null)
                {
                    var progress = db.LessonProgresses.FirstOrDefault(p => p.BookingId == booking.BookingId);
                    if (progress == null)
                    {
                        db.LessonProgresses.Add(new LessonProgress
                        {
                            BookingId = booking.BookingId,
                            RegistrationId = booking.RegistrationId,
                            LessonStatus = "ABSENT",
                            CompletionDate = DateTime.Now,
                            InstructorNotes = "Auto-marked absent by system."
                        });
                    }
                }
            }
            db.SaveChanges();
        }

        public ActionResult Dashboard()
        {
            int instructorId = GetCurrentInstructorId();
            AutoMarkAbsentLessons(instructorId);

            DateTime today = DateTime.Today;

            var vm = new InstructorDashboardViewModel
            {
                LessonsToday = db.LessonSchedules.Count(s => s.InstructorId == instructorId && DbFunctions.TruncateTime(s.LessonDate) == today),
                UpcomingLessons = db.LessonSchedules.Count(s => s.InstructorId == instructorId && s.LessonDate >= today),
                CompletedLessons = db.LessonProgresses.Count(p => p.Booking.LessonSchedule.InstructorId == instructorId && p.LessonStatus == "COMPLETED"),
                TotalStudents = db.Bookings.Where(b => b.LessonSchedule.InstructorId == instructorId).Select(b => b.RegistrationId).Distinct().Count()
            };

            return View("Dashboard", vm);
        }

        public ActionResult Schedule(string tab = "Upcoming")
        {
            int instructorId = GetCurrentInstructorId();
            AutoMarkAbsentLessons(instructorId);

            var vm = new ScheduleStudentsViewModel { ActiveTab = tab };

            // 1. Upcoming Lessons
            var upcomingSchedules = db.LessonSchedules
                .Where(s => s.InstructorId == instructorId && s.LessonDate >= DateTime.Today)
                .OrderBy(s => s.LessonDate)
                .ToList();

            vm.UpcomingLessons = upcomingSchedules.Select(s =>
            {
                var booking = db.Bookings.FirstOrDefault(b => b.LessonScheduleId == s.LessonScheduleId);
                var progress = booking != null ? db.LessonProgresses.FirstOrDefault(p => p.BookingId == booking.BookingId) : null;

                string studentFullName = booking?.Registration != null
                ? $"{booking.Registration.FirstName} {booking.Registration.LastName}"
                : "Unassigned";

                string studentPhone = booking?.Registration?.Phone ?? "";

                return new LessonItemDto
                {
                    LessonId = s.LessonScheduleId,
                    StudentName = studentFullName,
                    LessonType = s.LessonType?.LessonTypeName ?? "Driving Lesson",
                    LessonDate = s.LessonDate,
                    VehicleInfo = s.Vehicle != null
                        ? $"{s.Vehicle.Model} ({s.Vehicle.RegistrationNumber}) - {(booking?.Registration?.Phone ?? "")}"
                        : "No Vehicle Assigned",
                    IsConfirmed = booking?.Status == "CONFIRMED",
                    Status = progress?.LessonStatus ?? "Scheduled"
                };
            }).ToList();

            // 2. Lesson History
            var historyProgress = db.LessonProgresses
                .Where(p => p.Booking.LessonSchedule.InstructorId == instructorId)
                .OrderByDescending(p => p.Booking.LessonSchedule.LessonDate)
                .ToList();

            vm.HistoryLessons = historyProgress.Select(p => new LessonItemDto
            {
                LessonId = p.Booking?.LessonScheduleId ?? 0,
                StudentName = p.Registration != null
                    ? (p.Registration.FirstName + " " + p.Registration.LastName)
                    : (p.Booking?.Registration != null ? (p.Booking.Registration.FirstName + " " + p.Booking.Registration.LastName) : "N/A"),
                LessonType = p.Booking?.LessonSchedule?.LessonType?.LessonTypeName ?? "Driving Lesson",
                LessonDate = p.Booking?.LessonSchedule?.LessonDate ?? DateTime.MinValue,
                Status = p.LessonStatus,
                Notes = p.InstructorNotes
            }).ToList();

            // 3. Assigned Students Roster
            var assignedBookings = db.Bookings
                .Where(b => b.LessonSchedule.InstructorId == instructorId && b.Registration != null)
                .ToList();


            vm.AssignedStudents = assignedBookings
                .Select(b => b.Registration)
                .GroupBy(r => r.RegistrationId)
                .Select(g => g.First())
                .Select(s => new StudentRosterDto
                {
                    StudentId =int.TryParse( s.RegistrationId, out int id) ? id : 0, // If StudentRosterDto.StudentId is string. If int, use: Convert.ToInt32(s.RegistrationId)
                    FullName = s.FirstName + " " + s.LastName,
                    Email = s.Email,
                    Phone = s.Phone, // Uses s.Phone instead of s.PhoneNumber
                    CompletedLessons = db.LessonProgresses.Count(p => p.RegistrationId == s.RegistrationId && p.LessonStatus == "COMPLETED"),
                    TotalLessons = db.Bookings.Count(b => b.RegistrationId == s.RegistrationId && b.LessonSchedule.InstructorId == instructorId)
                }).ToList();

            return View("Schedule", vm);
        }

        // GET: Instructor/Profile
        [HttpGet]
        public ActionResult Profile()
        {
            string userEmail = User.Identity.Name;
            var instructor = db.Instructors.FirstOrDefault(i => i.Email == userEmail);

            if (instructor == null)
            {
                return HttpNotFound("Instructor record not found in database.");
            }

            var model = new InstructorProfileViewModel
            {
                FullName = $"{instructor.FirstName} {instructor.LastName}",
                Email = instructor.Email,
                Phone = instructor.Phone,
                LicenseNumber = instructor.LicenseNumber,
                VehicleAssigned ="None Assigned"
            };

            return View(model);
        }

        // POST: Instructor/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Profile(InstructorProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string userEmail = User.Identity.Name;
            var instructor = db.Instructors.FirstOrDefault(i => i.Email == userEmail);

            if (instructor == null)
            {
                return HttpNotFound();
            }

            // Split FullName back into FirstName and LastName if required by your model
            if (!string.IsNullOrEmpty(model.FullName) && model.FullName.Contains(" "))
            {
                var names = model.FullName.Split(new[] { ' ' }, 2);
                instructor.FirstName = names[0];
                instructor.LastName = names[1];
            }
            else
            {
                instructor.FirstName = model.FullName;
            }

            instructor.Phone = model.Phone;

            db.Entry(instructor).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        // POST: Instructor/SubmitFeedback
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitFeedback(int lessonId, string feedbackNotes)
        {
            var lesson = db.LessonSchedules.FirstOrDefault(l => l.LessonScheduleId == lessonId);
            if (lesson != null && !string.IsNullOrEmpty(feedbackNotes))
            {
                // Update lesson status or feedback field
                lesson.Status = feedbackNotes;
                db.Entry(lesson).State = System.Data.Entity.EntityState.Modified;

                // Optionally save to Review entity
                var review = new Review
                {
                    Comment = feedbackNotes,
                    ReviewDate = System.DateTime.Now,
                    InstructorId = GetCurrentInstructorId()
                };
                db.Reviews.Add(review);

                db.SaveChanges();
                TempData["SuccessMessage"] = "Lesson checkout completed and feedback saved successfully!";
            }

            return RedirectToAction("Schedule", new { tab = "Upcoming" });
        }
        // POST: Instructor/SubmitGeneralFeedback
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitGeneralFeedback(string feedbackType, string comments)
        {
            int InstructorId = GetCurrentInstructorId();

            if (!string.IsNullOrEmpty(comments))
            {
                var review = new Review
                {
                    Comment = $"[{feedbackType}] {comments}",
                    ReviewDate = System.DateTime.Now,
                    InstructorId = InstructorId
                };

                db.Reviews.Add(review);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Thank you! Your feedback has been submitted successfully.";
            }

            return Redirect(Request.UrlReferrer?.ToString() ?? Url.Action("Dashboard", "Instructor"));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckOut(int lessonId)
        {
            var lesson = db.LessonSchedules.FirstOrDefault(l => l.LessonScheduleId == lessonId);
            if (lesson != null)
            {
                lesson.Status = "Completed"; // or update status accordingly
                db.SaveChanges();

                // Pass lesson ID to prompt required feedback on reload
                TempData["PromptLessonFeedback"] = lessonId;
            }

            return RedirectToAction("Schedule");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkStatus(int scheduleId, string status)
        {
            var booking = db.Bookings.FirstOrDefault(b => b.LessonScheduleId == scheduleId);
            if (booking != null)
            {
                var progress = db.LessonProgresses.FirstOrDefault(p => p.BookingId == booking.BookingId);
                if (progress == null)
                {
                    db.LessonProgresses.Add(new LessonProgress
                    {
                        BookingId = booking.BookingId,
                        RegistrationId = booking.RegistrationId,
                        LessonStatus = status,
                        CompletionDate = DateTime.Now
                    });
                }
                else
                {
                    progress.LessonStatus = status;
                    progress.CompletionDate = DateTime.Now;
                }
                db.SaveChanges();
            }

            return RedirectToAction("Schedule", new { tab = "Upcoming" });
        }
    }

}