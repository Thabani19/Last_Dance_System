
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Last_Dance_System.Models
{
    public class InstructorFeedback
    {
        // =========================================================
        // PRIMARY KEY
        // =========================================================

        [Key]
        public int InstructorFeedbackId { get; set; }


        // =========================================================
        // BOOKING / LESSON
        // =========================================================

        [Required]
        public int BookingId { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }


        // =========================================================
        // INSTRUCTOR
        // =========================================================

        [Required]
        public int InstructorId { get; set; }

        [ForeignKey("InstructorId")]
        public virtual Instructor Instructor { get; set; }


        // =========================================================
        // STUDENT
        // =========================================================

        [Required]
        public string RegistrationId { get; set; }

        [ForeignKey("RegistrationId")]
        public virtual Registration Registration { get; set; }


        // =========================================================
        // FEEDBACK
        // =========================================================

        [Required]
        [StringLength(2000)]
        public string Feedback { get; set; }


        // =========================================================
        // AREAS TO IMPROVE
        // =========================================================

        [StringLength(1000)]
        public string AreasToImprove { get; set; }


        // =========================================================
        // RECOMMENDATIONS
        // =========================================================

        [StringLength(1000)]
        public string Recommendations { get; set; }


        // =========================================================
        // DATE CREATED
        // =========================================================

        [Required]
        public DateTime CreatedDate { get; set; }
    }
}

