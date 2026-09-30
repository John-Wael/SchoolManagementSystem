using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Models
{
    public class Lesson
    {
        [Key]
        public int LessonId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        // نوع الفيديو: "Upload" أو "YouTube"
        public string VideoType { get; set; } = "Upload";

        // مسار الفيديو المحلي (لو VideoType = "Upload")
        public string? VideoPath { get; set; }

        // لينك يوتيوب (لو VideoType = "YouTube")
        public string? YouTubeUrl { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }
    }
}