using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Models;

[Index("CourseId", Name = "IX_Enrollments_CourseId")]
[Index("StudentId", "CourseId", "Semester", "AcademicYear", Name = "UQ_Enrollments_Student_Course_Term", IsUnique = true)]
public partial class Enrollment
{
    [Key]
    public int EnrollmentId { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    [StringLength(20)]
    public string Semester { get; set; } = null!;

    [StringLength(9)]
    [RegularExpression(@"^\d{4}-\d{4}$", ErrorMessage = "Academic Year must be in the format YYYY-YYYY (e.g. 2025-2026).")]
    public string AcademicYear { get; set; } = null!;

    public DateTime EnrolledOn { get; set; }

    [InverseProperty("Enrollment")]
    [ValidateNever]
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    [ForeignKey("CourseId")]
    [InverseProperty("Enrollments")]
    [ValidateNever]
    public virtual Course Course { get; set; } = null!;

    [InverseProperty("Enrollment")]
    [ValidateNever]
    public virtual Grade? Grade { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("Enrollments")]
    [ValidateNever]
    public virtual Student Student { get; set; } = null!;
}