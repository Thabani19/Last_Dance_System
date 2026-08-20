using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Last_Dance_System.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        // Student
        [Required]
        public string RegistrationId { get; set; }

        [ForeignKey("RegistrationId")]
        public virtual Registration Registration { get; set; }

        // Lesson Schedule
        [Required]
        public int LessonScheduleId { get; set; }

        [ForeignKey("LessonScheduleId")]
        public virtual LessonSchedule LessonSchedule { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }
    }
}