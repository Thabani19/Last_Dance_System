using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Last_Dance_System.Models
{
    public class PackagePayment
    {
        [Key]
        public int PackagePaymentId { get; set; }


        // =========================================================
        // STUDENT PACKAGE
        // =========================================================

        [Required]
        public int StudentPackageId { get; set; }

        [ForeignKey("StudentPackageId")]
        public virtual StudentPackage StudentPackage { get; set; }


        // =========================================================
        // STUDENT
        // =========================================================

        [Required]
        public string RegistrationId { get; set; }

        [ForeignKey("RegistrationId")]
        public virtual Registration Registration { get; set; }


        // =========================================================
        // PAYMENT
        // =========================================================

        [Required]
        [Range(0.01, 1000000)]
        public decimal Amount { get; set; }


        [Required]
        [StringLength(30)]
        public string PaymentMethod { get; set; }


        [Required]
        [StringLength(20)]
        public string PaymentStatus { get; set; }


        // =========================================================
        // TRANSACTION
        // =========================================================

        [StringLength(100)]
        public string TransactionReference { get; set; }


        public DateTime PaymentDate { get; set; }


        [StringLength(500)]
        public string Notes { get; set; }
    }
}