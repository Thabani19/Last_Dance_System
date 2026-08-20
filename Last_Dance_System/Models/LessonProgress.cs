using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Last_Dance_System.Models
{
    public class LessonProgress
    {
        [Key]
        public int LessonProgressId { get; set; }

        // Student
        [Required]
        public string RegistrationId { get; set; }

        [ForeignKey("RegistrationId")]
        public virtual Registration Registration { get; set; }

        // Booking
        [Required]
        public int BookingId { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }

        [Required]
        [StringLength(20)]
        public string LessonStatus { get; set; }

        [StringLength(1000)]
        public string InstructorNotes { get; set; }

        public DateTime? CompletionDate { get; set; }

        [Range(0, 100)]
        public int? ProgressPercentage { get; set; }
    }
}