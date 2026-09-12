using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Last_Dance_System.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }


        // =========================================================
        // STUDENT
        // =========================================================

        [Required]
        public string RegistrationId { get; set; }

        [ForeignKey("RegistrationId")]
        public virtual Registration Registration { get; set; }


        // =========================================================
        // LESSON BOOKING
        // =========================================================

        // Nullable because package payments
        // do not have a BookingId.

        public int? BookingId { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }


        // =========================================================
        // STUDENT PACKAGE
        // =========================================================

        // Nullable because normal lesson payments
        // do not have a StudentPackageId.

        public int? StudentPackageId { get; set; }

        [ForeignKey("StudentPackageId")]
        public virtual StudentPackage StudentPackage { get; set; }


        // =========================================================
        // PAYMENT INFORMATION
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


        [StringLength(100)]
        public string TransactionReference { get; set; }


        public DateTime PaymentDate { get; set; }


        [StringLength(500)]
        public string Notes { get; set; }
    }
}