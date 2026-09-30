using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Models
{
    public class StudentQuiz
    {
        [Key]
        public int StudentQuizId { get; set; }

        [Required]
        public int QuizId { get; set; }

        [ForeignKey("QuizId")]
        public virtual Quiz? Quiz { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student? Student { get; set; }

        public int Score { get; set; }

        public int TotalMarks { get; set; }

        public DateTime? StartedAt { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        // لحفظ إجابات الطالب (JSON)
        public string? AnswersJson { get; set; }
    }
}
