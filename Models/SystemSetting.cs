using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Models;

public partial class SystemSetting
{
    [Key]
    public int SettingId { get; set; }

    [StringLength(20)]
    public string CurrentSemester { get; set; } = null!;

    [StringLength(9)]
    public string CurrentAcademicYear { get; set; } = null!;
}
