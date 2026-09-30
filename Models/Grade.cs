using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Models;

[Index("EnrollmentId", Name = "UQ_Grades_EnrollmentId", IsUnique = true)]
public partial class Grade
{
    [Key]
    public int GradeId { get; set; }

    public int EnrollmentId { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? AssignmentScore { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? MidtermScore { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? FinalScore { get; set; }
        public decimal? QuizScore { get; set; }

    [Column(TypeName = "decimal(7, 2)")]
    public decimal? TotalScore { get; set; }

    [StringLength(2)]
    public string? LetterGrade { get; set; }

    [ForeignKey("EnrollmentId")]
    [InverseProperty("Grade")]
    public virtual Enrollment Enrollment { get; set; } = null!;
}
