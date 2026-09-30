using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Models;

[Index("Code", Name = "UQ_Departments_Code", IsUnique = true)]
public partial class Department
{
    [Key]
    public int DepartmentId { get; set; }

    [StringLength(150)]
    public string Name { get; set; } = null!;

    [StringLength(20)]
    public string Code { get; set; } = null!;

    [InverseProperty("Department")]
    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    [InverseProperty("Department")]
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    [InverseProperty("Department")]
    public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
}
