using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Last_Dance_System.Models
{
    public class InstructorDashboardViewModel
    {
        public int LessonsToday { get; set; }
        public int UpcomingLessons { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalStudents { get; set; }
    }

    public class ScheduleStudentsViewModel
    {
        public string ActiveTab { get; set; } = "Upcoming";
        public List<LessonItemDto> UpcomingLessons { get; set; } = new List<LessonItemDto>();
        public List<LessonItemDto> HistoryLessons { get; set; } = new List<LessonItemDto>();
        public List<StudentRosterDto> AssignedStudents { get; set; } = new List<StudentRosterDto>();
    }
    public class InstructorProfileViewModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string Phone { get; set; }

        [Display(Name = "Instructor License")]
        public string LicenseNumber { get; set; }

        [Display(Name = "Assigned Vehicle")]
        public string VehicleAssigned { get; set; }
    }

    public class LessonItemDto
    {
        public int LessonId { get; set; }
        public string StudentName { get; set; }
        public string LessonType { get; set; }
        public DateTime LessonDate { get; set; }
        public string VehicleInfo { get; set; }
        public string Status { get; set; }
        public bool IsConfirmed { get; set; }
        public string Notes { get; set; }
    }

    public class StudentRosterDto
    {
        public int StudentId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
    }
}