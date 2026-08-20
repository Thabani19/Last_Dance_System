using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Last_Dance_System.Models
{
    public class Cancellation
    {
        [Key]
        public int CancellationId { get; set; }

        // Booking being cancelled/rescheduled
        [Required]
        public int BookingId { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }

        // Student who requested the cancellation
        [Required]
        public string RegistrationId { get; set; }

        [ForeignKey("RegistrationId")]
        public virtual Registration Registration { get; set; }

        [Required]
        [StringLength(20)]
        public string ActionType { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; }

        [Required]
        public DateTime ActionDate { get; set; }

        [StringLength(20)]
        public string RequestedBy { get; set; }

        public int? NewLessonScheduleId { get; set; }

        [ForeignKey("NewLessonScheduleId")]
        public virtual LessonSchedule NewLessonSchedule { get; set; }
    }
}