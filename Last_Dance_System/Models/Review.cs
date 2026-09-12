using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Last_Dance_System.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }


        // =========================================================
        // BOOKING / LESSON BEING REVIEWED
        // =========================================================

        [Required]
        public int BookingId { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }


        // =========================================================
        // STUDENT
        // =========================================================

        // Student connected to the review
        public string RegistrationId { get; set; }

        [ForeignKey("RegistrationId")]
        public virtual Registration Registration { get; set; }


        // =========================================================
        // INSTRUCTOR BEING REVIEWED
        // =========================================================

        public int? InstructorId { get; set; }

        [ForeignKey("InstructorId")]
        public virtual Instructor Instructor { get; set; }


        // =========================================================
        // INSTRUCTOR WHO SUBMITTED THE REVIEW
        // =========================================================

        public int? ReviewerInstructorId { get; set; }

        [ForeignKey("ReviewerInstructorId")]
        public virtual Instructor ReviewerInstructor { get; set; }


        // =========================================================
        // VEHICLE
        // =========================================================

        public int? VehicleId { get; set; }

        [ForeignKey("VehicleId")]
        public virtual Vehicle Vehicle { get; set; }


        // =========================================================
        // REVIEW TYPE
        // =========================================================

        [Required]
        [StringLength(30)]
        public string ReviewType { get; set; }


        // =========================================================
        // RATING
        // =========================================================

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }


        // =========================================================
        // COMMENT
        // =========================================================

        [Required]
        [StringLength(1000)]
        public string Comment { get; set; }


        // =========================================================
        // REVIEW DATE
        // =========================================================

        public DateTime ReviewDate { get; set; }


        // =========================================================
        // APPROVAL
        // =========================================================

        public bool IsApproved { get; set; }
    }
}