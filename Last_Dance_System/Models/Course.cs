using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Last_Dance_System.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        [StringLength(100)]
        public string CourseName { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(0, 100000)]
        public decimal Price { get; set; }

        [Required]
        public int Duration { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual ICollection<StudentCourse> StudentCourses { get; set; }
    }
}