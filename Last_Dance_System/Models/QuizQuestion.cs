using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Last_Dance_System.Models
{
    public class QuizQuestion
    {
        [Key]
        public int QuizQuestionId { get; set; }

        [Required]
        public int LearnerLessonId { get; set; }

        [ForeignKey("LearnerLessonId")]
        public virtual LearnerLesson LearnerLesson { get; set; }

        [Required]
        [Display(Name = "Question")]
        public string Question { get; set; }

        [Required]
        [Display(Name = "Option A")]
        public string OptionA { get; set; }

        [Required]
        [Display(Name = "Option B")]
        public string OptionB { get; set; }

        [Required]
        [Display(Name = "Option C")]
        public string OptionC { get; set; }

        [Required]
        [Display(Name = "Option D")]
        public string OptionD { get; set; }

        [Required]
        [Display(Name = "Correct Answer")]
        public string CorrectAnswer { get; set; }

        [Display(Name = "Question Order")]
        public int QuestionOrder { get; set; }

        [Display(Name = "Active Question")]
        public bool IsActive { get; set; }
    }
}