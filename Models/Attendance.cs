using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Models;

[Table("Attendance")]
[Index("EnrollmentId", Name = "IX_Attendance_EnrollmentId")]
[Index("EnrollmentId", "AttendanceDate", Name = "UQ_Attendance_Enrollment_Date", IsUnique = true)]
public partial class Attendance
{
    [Key]
    public int AttendanceId { get; set; }

    public int EnrollmentId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    [StringLength(10)]
    public string Status { get; set; } = null!;

    [ForeignKey("EnrollmentId")]
    [InverseProperty("Attendances")]
    public virtual Enrollment Enrollment { get; set; } = null!;
}
