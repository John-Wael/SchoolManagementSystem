using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models.ViewModels;

namespace SchoolManagementSystem.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        if (User.IsInRole("Admin")) return RedirectToAction(nameof(Admin));
        if (User.IsInRole("Teacher")) return RedirectToAction(nameof(Teacher));
        if (User.IsInRole("Student")) return RedirectToAction(nameof(Student));
        return RedirectToAction("Index", "Home");
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Admin()
    {
        var vm = new AdminDashboardViewModel
        {
            DepartmentCount = await _context.Departments.CountAsync(),
            StudentCount = await _context.Students.CountAsync(),
            ActiveStudentCount = await _context.Students.CountAsync(s => s.IsActive),
            TeacherCount = await _context.Teachers.CountAsync(),
            ActiveTeacherCount = await _context.Teachers.CountAsync(t => t.IsActive),
            CourseCount = await _context.Courses.CountAsync(),
            ActiveCourseCount = await _context.Courses.CountAsync(c => c.IsActive),
            EnrollmentCount = await _context.Enrollments.CountAsync()
        };
        return View(vm);
    }

    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Teacher()
    {
        var userId = _userManager.GetUserId(User);

        var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);

        if (teacher == null)
        {
            return View(new TeacherDashboardViewModel { IsLinked = false });
        }

        var courses = await _context.CourseTeachers
            .Where(ct => ct.TeacherId == teacher.TeacherId)
            .Include(ct => ct.Course)
            .Select(ct => new TeacherCourseRow
            {
                CourseId = ct.Course.CourseId,
                CourseCode = ct.Course.CourseCode,
                Title = ct.Course.Title,
                IsPrimary = ct.IsPrimary,
                MaxStudents = ct.Course.MaxStudents,
                EnrolledCount = _context.Enrollments.Count(e => e.CourseId == ct.Course.CourseId)
            })
            .ToListAsync();

        var vm = new TeacherDashboardViewModel
        {
            IsLinked = true,
            TeacherName = $"{teacher.FirstName} {teacher.LastName}",
            Courses = courses
        };
        return View(vm);
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Student()
    {
        var userId = _userManager.GetUserId(User);

        var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);

        if (student == null)
        {
            return View(new StudentDashboardViewModel { IsLinked = false });
        }

        var enrollments = await _context.Enrollments
            .Where(e => e.StudentId == student.StudentId)
            .Include(e => e.Course)
            .Include(e => e.Grade)
            .Include(e => e.Attendances)
            .Select(e => new StudentEnrollmentRow
            {
                EnrollmentId = e.EnrollmentId,
                CourseId = e.CourseId,
                CourseTitle = e.Course.Title,
                Semester = e.Semester,
                AcademicYear = e.AcademicYear,
                TotalScore = e.Grade != null ? e.Grade.TotalScore : null,
                LetterGrade = e.Grade != null ? e.Grade.LetterGrade : null,
                PresentCount = e.Attendances.Count(a => a.Status == "Present"),
                AbsentCount = e.Attendances.Count(a => a.Status == "Absent"),
                TotalAttendanceRecords = e.Attendances.Count(),
                HasGrade = e.Grade != null,
                HasTakenAnyQuiz = _context.StudentQuizzes.Any(sq => sq.StudentId == student.StudentId
                    && _context.Quizzes.Any(q => q.QuizId == sq.QuizId && q.CourseId == e.CourseId))
            })
            .ToListAsync();

        var vm = new StudentDashboardViewModel
        {
            IsLinked = true,
            StudentName = $"{student.FirstName} {student.LastName}",
            StudentNumber = student.StudentNumber,
            Enrollments = enrollments
        };
        return View(vm);
    }

    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> CourseDetails(int courseId)
    {
        var course = await _context.Courses
            .Include(c => c.Department)
            .FirstOrDefaultAsync(c => c.CourseId == courseId);

        if (course == null) return NotFound();

        var students = await _context.Enrollments
            .Where(e => e.CourseId == courseId)
            .Include(e => e.Student)
                .ThenInclude(s => s.User)
            .Select(e => new
            {
                e.Student.StudentId,
                e.Student.StudentNumber,
                FullName = e.Student.FirstName + " " + e.Student.LastName,
                Email = e.Student.User != null ? e.Student.User.Email : "N/A",
                e.Semester,
                e.AcademicYear,
                e.EnrolledOn
            })
            .ToListAsync();

        ViewBag.CourseCode = course.CourseCode;
        ViewBag.CourseTitle = course.Title;
        ViewBag.Department = course.Department != null ? course.Department.Name : "N/A";
        ViewBag.EnrolledCount = students.Count;
        ViewBag.MaxStudents = course.MaxStudents;

        return View(students);
    }
}
