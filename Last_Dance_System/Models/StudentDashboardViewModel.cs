using System;
using System.Collections.Generic;

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

        public int CancelledLessons { get; set; }


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

        public string BookingStatus { get; set; }

        public int UpcomingLessonsCount { get; set; }


        // =========================================================
        // PACKAGE
        // =========================================================

        public bool HasActivePackage { get; set; }

        public int? CurrentStudentPackageID { get; set; }

        public string CurrentPackageName { get; set; }

        public int CurrentPackageLessons { get; set; }

        public int CurrentPackageLessonsRemaining { get; set; }

        public string CurrentPackagePaymentStatus { get; set; }

        public decimal CurrentPackagePrice { get; set; }

        public int PackageProgressPercentage { get; set; }


        // =========================================================
        // PAYMENT
        // =========================================================

        public string PaymentStatus { get; set; }

        public decimal TotalPaid { get; set; }

        public decimal OutstandingAmount { get; set; }


        // =========================================================
        // REVIEW
        // =========================================================

        public bool HasPendingReview { get; set; }

        public int? ReviewLessonID { get; set; }


        // =========================================================
        // INSTRUCTOR FEEDBACK
        // =========================================================

        public bool HasInstructorFeedback { get; set; }

        public string LatestFeedback { get; set; }

        public string FeedbackInstructor { get; set; }

        public DateTime? FeedbackDate { get; set; }


        // =========================================================
        // NOTIFICATIONS
        // =========================================================

        public int UnreadNotificationCount { get; set; }

        public List<Notification> Notifications { get; set; }
    }
}