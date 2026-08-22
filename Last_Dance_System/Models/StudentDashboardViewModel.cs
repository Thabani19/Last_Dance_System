using System;

namespace Last_Dance_System.Models
{
    public class StudentDashboardViewModel
    {
        // ============================================================
        // STUDENT INFORMATION
        // ============================================================

        public string RegistrationId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        // Compatibility properties used by the dashboard
        public string StudentName { get; set; }

        public string StudentSurname { get; set; }

        public string StudentEmail { get; set; }


        // ============================================================
        // BOOKING
        // ============================================================

        public string BookingStatus { get; set; }

        public string CourseName { get; set; }

        public string InstructorName { get; set; }


        // ============================================================
        // PAYMENT
        // ============================================================

        public string PaymentStatus { get; set; }


        // ============================================================
        // LESSONS
        // ============================================================

        public int TotalLessons { get; set; }

        public int CompletedLessons { get; set; }

        public int RemainingLessons { get; set; }

        public int UpcomingLessons { get; set; }


        // ============================================================
        // PROGRESS
        // ============================================================

        public int ProgressPercentage { get; set; }


        // ============================================================
        // NEXT LESSON
        // ============================================================

        public DateTime? NextLessonDate { get; set; }

        public string NextLessonTime { get; set; }

        // These are used by your current Dashboard view
        public NextLessonViewModel NextLesson { get; set; }


        // ============================================================
        // REVIEWS & FEEDBACK
        // ============================================================

        public string DailyReview { get; set; }

        public string InstructorFeedback { get; set; }

        public string OverallReview { get; set; }
    }


    // ================================================================
    // NEXT LESSON
    // ================================================================

    public class NextLessonViewModel
    {
        public int Day { get; set; }

        public string Month { get; set; }

        public string Date { get; set; }

        public string Time { get; set; }

        public string Location { get; set; }

        public string Instructor { get; set; }

        public string Vehicle { get; set; }
    }
}