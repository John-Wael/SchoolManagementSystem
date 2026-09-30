using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;

namespace SchoolManagementSystem.Models;

[Index("DepartmentId", Name = "IX_Students_DepartmentId")]
[Index("LastName", Name = "IX_Students_LastName")]
[Index("StudentNumber", Name = "UQ_Students_StudentNumber", IsUnique = true)]
public partial class Student
{
    [Key]
    public int StudentId { get; set; }

    public string? UserId { get; set; }

    public int DepartmentId { get; set; }

    [StringLength(20)]
    public string StudentNumber { get; set; } = null!;

    [StringLength(100)]
    public string FirstName { get; set; } = null!;

    [StringLength(100)]
    public string LastName { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    [ForeignKey("DepartmentId")]
    [InverseProperty("Students")]
    public virtual Department? Department { get; set; }

    [InverseProperty("Student")]
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    [ForeignKey("UserId")]
    public virtual ApplicationUser? User { get; set; }
}