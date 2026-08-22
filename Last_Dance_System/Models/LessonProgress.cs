using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Last_Dance_System.Models
{
    public class LessonProgress
    {
        [Key]
        public int LessonProgressId { get; set; }

        // =====================================================
        // STUDENT
        // =====================================================

        [Required]
        public string RegistrationId { get; set; }

        [ForeignKey("RegistrationId")]
        public virtual Registration Registration { get; set; }


        // =====================================================
        // BOOKING
        // =====================================================

        [Required]
        public int BookingId { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }


        // =====================================================
        // LESSON STATUS
        // =====================================================

        [Required]
        [StringLength(20)]
        public string LessonStatus { get; set; }


        // =====================================================
        // SYSTEM-GENERATED CHECK-IN CODE
        // =====================================================

        [StringLength(10)]
        public string CheckInCode { get; set; }

        public bool CheckInConfirmed { get; set; }

        public DateTime? CheckInTime { get; set; }


        // =====================================================
        // SYSTEM-GENERATED CHECK-OUT CODE
        // =====================================================

        [StringLength(10)]
        public string CheckOutCode { get; set; }

        public bool CheckOutConfirmed { get; set; }

        public DateTime? CheckOutTime { get; set; }


        // =====================================================
        // INSTRUCTOR FEEDBACK
        // =====================================================

        [StringLength(1000)]
        public string InstructorNotes { get; set; }


        // =====================================================
        // COMPLETION
        // =====================================================

        public DateTime? CompletionDate { get; set; }

        [Range(0, 100)]
        public int? ProgressPercentage { get; set; }
    }
}