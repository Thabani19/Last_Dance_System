using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Last_Dance_System.Models
{
    public class StudentPackage
    {
        [Key]
        public int StudentPackageId { get; set; }

        // Student who purchased the package
        [Required]
        public string RegistrationId { get; set; }

        [ForeignKey("RegistrationId")]
        public virtual Registration Registration { get; set; }

        // Package purchased
        [Required]
        public int LessonPackageId { get; set; }

        [ForeignKey("LessonPackageId")]
        public virtual LessonPackage LessonPackage { get; set; }

        // Number originally purchased
        [Required]
        public int LessonsPurchased { get; set; }

        // Number still available for booking
        [Required]
        public int LessonsRemaining { get; set; }

        [Required]
        [StringLength(20)]
        public string PaymentStatus { get; set; }

        public DateTime PurchaseDate { get; set; }

        public bool IsActive { get; set; }
    }
}