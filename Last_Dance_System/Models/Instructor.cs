using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Last_Dance_System.Models
{
    public class Instructor
    {
        [Key]
        public int InstructorId { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [StringLength(15)]
        public string Phone { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        public string LicenseNumber { get; set; }

        [Range(0, 50)]
        public int Experience { get; set; }

        public bool IsActive { get; set; }

        // Connection to ASP.NET Identity
        public string ApplicationUserId { get; set; }

        public virtual ApplicationUser ApplicationUser { get; set; }

        // Navigation property
        public virtual ICollection<LessonSchedule> LessonSchedules { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }

    }
}