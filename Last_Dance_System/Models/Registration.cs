using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Last_Dance_System.Models
{
    public class Registration
    {
        [Key]
        [Required]
        [StringLength(13, MinimumLength = 13,
            ErrorMessage = "ID Number must be exactly 13 digits.")]
        public string RegistrationId { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [StringLength(10)]
        public string Gender { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(15)]
        public string Phone { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string Address { get; set; }

        public DateTime RegistrationDate { get; set; }

        // Connection to ASP.NET Identity
        public string ApplicationUserId { get; set; }

        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }

        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<LessonProgress> LessonProgresses { get; set; }
        public virtual ICollection<Cancellation> Cancellations { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
    }
}