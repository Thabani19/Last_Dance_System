using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Last_Dance_System.Models
{
    public class StudentPackage
    {
        [Key]
        public int StudentPackageId { get; set; }


        // =========================================================
        // STUDENT
        // =========================================================

        [Required]
        public string RegistrationId { get; set; }

        [ForeignKey("RegistrationId")]
        public virtual Registration Registration { get; set; }


        // =========================================================
        // PACKAGE
        // =========================================================

        [Required]
        public int LessonPackageId { get; set; }

        [ForeignKey("LessonPackageId")]
        public virtual LessonPackage LessonPackage { get; set; }


        // =========================================================
        // LESSON CREDITS
        // =========================================================

        [Required]
        public int LessonsPurchased { get; set; }

        [Required]
        public int LessonsRemaining { get; set; }


        // =========================================================
        // PAYMENT
        // =========================================================

        [Required]
        [StringLength(30)]
        public string PaymentMethod { get; set; }

        [Required]
        [StringLength(20)]
        public string PaymentStatus { get; set; }


        // =========================================================
        // PURCHASE INFORMATION
        // =========================================================

        public DateTime PurchaseDate { get; set; }

        public bool IsActive { get; set; }


        // =========================================================
        // CANCELLATION
        // =========================================================

        public bool IsCancelled { get; set; }

        public DateTime? CancellationDate { get; set; }
    }
}