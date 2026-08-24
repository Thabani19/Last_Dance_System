using System;

namespace Last_Dance_System.Models
{
    public class StudentDashboardViewModel
    {
        // =========================================================
        // STUDENT
        // =========================================================

        public string StudentID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }


        // =========================================================
        // LESSON STATISTICS
        // =========================================================

        public int TotalLessons { get; set; }

        public int CompletedLessons { get; set; }

        public int RemainingLessons { get; set; }

        public int ProgressPercentage { get; set; }


        // =========================================================
        // UPCOMING LESSON
        // =========================================================

        public bool HasUpcomingLesson { get; set; }

        public int? UpcomingLessonID { get; set; }

        public DateTime? UpcomingLessonDate { get; set; }

        public string UpcomingLessonTime { get; set; }

        public string UpcomingLessonType { get; set; }

        public string UpcomingInstructor { get; set; }

        public string UpcomingVehicle { get; set; }

        public string UpcomingLocation { get; set; }

        // NEW
        public string BookingStatus { get; set; }


        // =========================================================
        // PAYMENT
        // =========================================================

        public string PaymentStatus { get; set; }

        public decimal TotalPaid { get; set; }

        public decimal OutstandingAmount { get; set; }


        // =========================================================
        // DAILY REVIEW
        // =========================================================

        public bool HasPendingReview { get; set; }

        public int? ReviewLessonID { get; set; }


        // =========================================================
        // INSTRUCTOR FEEDBACK
        // =========================================================

        public bool HasInstructorFeedback { get; set; }

        public string LatestFeedback { get; set; }
    }
}