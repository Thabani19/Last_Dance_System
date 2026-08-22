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

        public virtual ICollection<StudentPackage> StudentPackages { get; set; }
    }
}