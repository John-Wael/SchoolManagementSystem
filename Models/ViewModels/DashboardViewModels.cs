namespace SchoolManagementSystem.Models.ViewModels;

public class AdminDashboardViewModel
{
    public int DepartmentCount { get; set; }
    public int StudentCount { get; set; }
    public int ActiveStudentCount { get; set; }
    public int TeacherCount { get; set; }
    public int ActiveTeacherCount { get; set; }
    public int CourseCount { get; set; }
    public int ActiveCourseCount { get; set; }
    public int EnrollmentCount { get; set; }
}

public class TeacherDashboardViewModel
{
    public bool IsLinked { get; set; }
    public string? TeacherName { get; set; }
    public List<TeacherCourseRow> Courses { get; set; } = new();
}

public class TeacherCourseRow
{
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = null!;
    public string Title { get; set; } = null!;
    public bool IsPrimary { get; set; }
    public int EnrolledCount { get; set; }
    public int? MaxStudents { get; set; }
}

public class StudentDashboardViewModel
{
    public bool IsLinked { get; set; }
    public string? StudentName { get; set; }
    public string? StudentNumber { get; set; }
    public List<StudentEnrollmentRow> Enrollments { get; set; } = new();
}

public class OpenCourseRow
{
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string DepartmentName { get; set; } = null!;
    public int Credits { get; set; }
    public int? MaxStudents { get; set; }
    public int CurrentEnrolledCount { get; set; }
}

public class StudentEnrollmentRow
{
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }   // ← ضيف السطر ده
    public string CourseTitle { get; set; } = null!;
    public string Semester { get; set; } = null!;
    public string AcademicYear { get; set; } = null!;
    public decimal? TotalScore { get; set; }
    public string? LetterGrade { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int TotalAttendanceRecords { get; set; }
    public bool HasGrade { get; set; }
    public bool HasTakenAnyQuiz { get; set; }
}