using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Last_Dance_System.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }


        // =========================================================
        // STUDENT
        // =========================================================

        [Required]
        public string RegistrationId { get; set; }

        [ForeignKey("RegistrationId")]
        public virtual Registration Registration { get; set; }


        // =========================================================
        // STUDENT PACKAGE
        // =========================================================

        public int? StudentPackageId { get; set; }

        [ForeignKey("StudentPackageId")]
        public virtual StudentPackage StudentPackage { get; set; }


        // =========================================================
        // LESSON SCHEDULE
        // =========================================================

        [Required]
        public int LessonScheduleId { get; set; }

        [ForeignKey("LessonScheduleId")]
        public virtual LessonSchedule LessonSchedule { get; set; }


        // =========================================================
        // BOOKING INFORMATION
        // =========================================================

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }


        // =========================================================
        // ATTENDANCE
        // =========================================================

        [Required]
        [StringLength(20)]
        public string AttendanceStatus { get; set; }

        public DateTime? AttendanceConfirmedDate { get; set; }


        // =========================================================
        // NOTES
        // =========================================================

        [StringLength(500)]
        public string Notes { get; set; }


    }
}