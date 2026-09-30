using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Models
{
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        public string QuestionText { get; set; } = string.Empty;

        // الاختيارات مخزنة كـ JSON array (مثلاً: ["Option1","Option2","Option3"])
        public string? OptionsJson { get; set; }

        // الإجابة الصح (رقم الاختيار الصح بدءاً من 0)
        [Required]
        public string CorrectAnswer { get; set; } = string.Empty;

        public int Marks { get; set; } = 1;

        [Required]
        public int QuizId { get; set; }

        [ForeignKey("QuizId")]
        public virtual Quiz? Quiz { get; set; }
    }
}
