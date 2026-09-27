
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Last_Dance_System.Models
{
    public class LessonPackage
    {
        [Key]
        public int LessonPackageId { get; set; }

        [Required]
        [StringLength(100)]
        public string PackageName { get; set; }

        [Required]
        [Range(1, 100)]
        public int NumberOfLessons { get; set; }

        [Required]
        [Range(0.01, 1000000)]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        // =========================================================
        // VEHICLE / LICENCE CODE
        // =========================================================
        //
        // Examples:
        // Code 8
        // Code 10
        // Code 14
        //
        // This determines which type of vehicle/instructor
        // the student is allowed to book.
        // =========================================================

        [StringLength(20)]
        [Display(Name = "Required Licence Code")]
        public string RequiredLicenseCode { get; set; }

        // =========================================================
        // LEARNER THEORY PACKAGE
        // =========================================================
        //
        // True  = This package unlocks all learner theory content.
        // False = This is a normal driving lesson package.
        //
        // The theory package does NOT use lesson credits.
        // =========================================================

        public bool IsLearnerTheoryPackage { get; set; }

        public virtual ICollection<StudentPackage> StudentPackages { get; set; }
    }
}

