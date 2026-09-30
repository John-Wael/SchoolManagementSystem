using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;

namespace SchoolManagementSystem.Models;

[Index("DepartmentId", Name = "IX_Teachers_DepartmentId")]
[Index("LastName", Name = "IX_Teachers_LastName")]
[Index("EmployeeNumber", Name = "UQ_Teachers_EmployeeNumber", IsUnique = true)]
public partial class Teacher
{
    [Key]
    public int TeacherId { get; set; }

    public string? UserId { get; set; }

    public int DepartmentId { get; set; }

    [StringLength(20)]
    public string EmployeeNumber { get; set; } = null!;

    [StringLength(100)]
    public string FirstName { get; set; } = null!;

    [StringLength(100)]
    public string LastName { get; set; } = null!;

    public DateOnly? HireDate { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    [InverseProperty("Teacher")]
    public virtual ICollection<CourseTeacher> CourseTeachers { get; set; } = new List<CourseTeacher>();

    [ForeignKey("DepartmentId")]
    [InverseProperty("Teachers")]
    public virtual Department? Department { get; set; }

    [ForeignKey("UserId")]
    public virtual ApplicationUser? User { get; set; }
}