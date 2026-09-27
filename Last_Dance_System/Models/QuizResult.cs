using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Last_Dance_System.Models
{
    public class QuizResult
    {
        [Key]
        public int QuizResultId { get; set; }

        [Required]
        [Display(Name = "Student")]
        public string RegistrationId { get; set; }

        [ForeignKey("RegistrationId")]
        public virtual Registration Registration { get; set; }

        [Required]
        public int LearnerLessonId { get; set; }

        [ForeignKey("LearnerLessonId")]
        public virtual LearnerLesson LearnerLesson { get; set; }

        [Required]
        [Display(Name = "Score")]
        public int Score { get; set; }

        [Required]
        [Display(Name = "Total Questions")]
        public int TotalQuestions { get; set; }

        [Required]
        [Display(Name = "Percentage")]
        public int Percentage { get; set; }

        [Required]
        [Display(Name = "Date Taken")]
        public DateTime DateTaken { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Attempt ID")]
        public string AttemptId { get; set; }
    }
}