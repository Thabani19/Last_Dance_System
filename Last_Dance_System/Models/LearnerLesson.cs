using System.ComponentModel.DataAnnotations;

namespace Last_Dance_System.Models
{
    public class LearnerLesson
    {
        [Key]
        public int LearnerLessonId { get; set; }

        [Required]
        [Display(Name = "Lesson Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Lesson Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Learning Content")]
        public string Content { get; set; }

        [Display(Name = "Lesson Order")]
        public int LessonOrder { get; set; }

        [Display(Name = "Active Lesson")]
        public bool IsActive { get; set; }
    }
}