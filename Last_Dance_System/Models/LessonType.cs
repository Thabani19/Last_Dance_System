using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Last_Dance_System.Models
{
    public class LessonType
    {
        [Key]
        public int LessonTypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string LessonTypeName { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(1, 480)]
        public int DurationMinutes { get; set; }

        [Required]
        [Range(0, 100000)]
        public decimal Price { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<LessonSchedule> LessonSchedules { get; set; }
    }
}