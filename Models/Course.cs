using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Models;

[Index("DepartmentId", Name = "IX_Courses_DepartmentId")]
[Index("CourseCode", Name = "UQ_Courses_CourseCode", IsUnique = true)]
public partial class Course
{
    [Key]
    public int CourseId { get; set; }

    public int DepartmentId { get; set; }

    [StringLength(20)]
    public string CourseCode { get; set; } = null!;

    [StringLength(200)]
    public string Title { get; set; } = null!;

    public int Credits { get; set; }

    public int? MaxStudents { get; set; }

    public bool IsActive { get; set; }

    public bool IsEnrollmentOpen { get; set; }

    [InverseProperty("Course")]
    [ValidateNever]
    public virtual CourseTeacher? CourseTeacher { get; set; }

    [ForeignKey("DepartmentId")]
    [InverseProperty("Courses")]
    [ValidateNever]
    public virtual Department Department { get; set; } = null!;

    [InverseProperty("Course")]
    [ValidateNever]
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}