using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Models;

[Index("CourseId", "TeacherId", Name = "UQ_CourseTeachers_Course_Teacher", IsUnique = true)]
public partial class CourseTeacher
{
    [Key]
    public int CourseTeacherId { get; set; }

    public int CourseId { get; set; }

    public int TeacherId { get; set; }

    public bool IsPrimary { get; set; }

    [ForeignKey("CourseId")]
    [InverseProperty("CourseTeacher")]
    public virtual Course Course { get; set; } = null!;

    [ForeignKey("TeacherId")]
    [InverseProperty("CourseTeachers")]
    public virtual Teacher Teacher { get; set; } = null!;
}
