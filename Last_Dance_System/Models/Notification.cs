using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Last_Dance_System.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }


        // =========================================================
        // STUDENT RECEIVING NOTIFICATION
        // =========================================================
        //
        // Nullable because a notification can belong to either
        // a student OR an instructor.
        //
        // =========================================================

        public string RegistrationId { get; set; }

        [ForeignKey("RegistrationId")]
        public virtual Registration Registration { get; set; }


        // =========================================================
        // INSTRUCTOR RECEIVING NOTIFICATION
        // =========================================================

        public int? InstructorId { get; set; }

        [ForeignKey("InstructorId")]
        public virtual Instructor Instructor { get; set; }


        // =========================================================
        // NOTIFICATION INFORMATION
        // =========================================================

        [Required]
        [StringLength(100)]
        public string Title { get; set; }


        [Required]
        [StringLength(1000)]
        public string Message { get; set; }


        [Required]
        [StringLength(30)]
        public string NotificationType { get; set; }


        // =========================================================
        // READ STATUS
        // =========================================================

        public bool IsRead { get; set; }


        public DateTime CreatedAt { get; set; }


        public DateTime? ReadAt { get; set; }
    }
}