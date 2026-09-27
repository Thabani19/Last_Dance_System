using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Last_Dance_System.Models
{
    public class Vehicle
    {
        [Key]
        public int VehicleId { get; set; }

        [Required]
        [StringLength(50)]
        public string Make { get; set; }

        [Required]
        [StringLength(50)]
        public string Model { get; set; }

        [Required]
        [StringLength(20)]
        public string RegistrationNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string VehicleType { get; set; }

        // =========================================================
        // LICENCE CODE
        // =========================================================
        //
        // Examples:
        // Code 8
        // Code 10
        // Code 14
        //
        // This determines which driving packages this vehicle
        // can be used for.
        // =========================================================

        [Required]
        [StringLength(20)]
        [Display(Name = "Licence Code")]
        public string LicenseCode { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        public int? CurrentMileage { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<Review> Reviews { get; set; }

        // Instructors using this vehicle
        public virtual ICollection<Instructor> Instructors { get; set; }
    }
}

