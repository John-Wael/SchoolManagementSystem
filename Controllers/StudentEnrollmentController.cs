using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;
using SchoolManagementSystem.Models.ViewModels;

namespace SchoolManagementSystem.Controllers;

[Authorize(Roles = "Student")]
public class StudentEnrollmentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentEnrollmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var student = await GetCurrentStudentAsync();
        if (student == null)
        {
            TempData["Error"] = "Your account isn't linked to a Student record yet. Contact an Admin.";
            return RedirectToAction("Student", "Dashboard");
        }

        if (!student.IsActive)
        {
            TempData["Error"] = "Your account is not active. Contact an Admin.";
            return RedirectToAction("Student", "Dashboard");
        }

        var settings = await _context.SystemSettings.FindAsync(1);
        if (settings == null)
        {
            TempData["Error"] = "Enrollment term hasn't been configured yet. Contact an Admin.";
            return RedirectToAction("Student", "Dashboard");
        }

        var alreadyEnrolledCourseIds = await _context.Enrollments
            .Where(e => e.StudentId == student.StudentId
                        && e.Semester == settings.CurrentSemester
                        && e.AcademicYear == settings.CurrentAcademicYear)
            .Select(e => e.CourseId)
            .ToListAsync();

        var openCourses = await _context.Courses
            .Where(c => c.IsEnrollmentOpen && c.IsActive && !alreadyEnrolledCourseIds.Contains(c.CourseId))
            .Include(c => c.Department)
            .Select(c => new OpenCourseRow
            {
                CourseId = c.CourseId,
                CourseCode = c.CourseCode,
                Title = c.Title,
                DepartmentName = c.Department.Name,
                Credits = c.Credits,
                MaxStudents = c.MaxStudents,
                CurrentEnrolledCount = _context.Enrollments.Count(e => e.CourseId == c.CourseId
                    && e.Semester == settings.CurrentSemester
                    && e.AcademicYear == settings.CurrentAcademicYear)
            })
            .ToListAsync();

        ViewBag.CurrentSemester = settings.CurrentSemester;
        ViewBag.CurrentAcademicYear = settings.CurrentAcademicYear;
        return View(openCourses);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int courseId)
    {
        var student = await GetCurrentStudentAsync();
        if (student == null || !student.IsActive)
        {
            TempData["Error"] = "You are not eligible to enroll.";
            return RedirectToAction(nameof(Index));
        }

        var course = await _context.Courses.FindAsync(courseId);
        if (course == null || !course.IsEnrollmentOpen || !course.IsActive)
        {
            TempData["Error"] = "This course is not open for enrollment.";
            return RedirectToAction(nameof(Index));
        }

        var settings = await _context.SystemSettings.FindAsync(1);
        if (settings == null)
        {
            TempData["Error"] = "Enrollment term hasn't been configured yet.";
            return RedirectToAction(nameof(Index));
        }

        if (course.MaxStudents.HasValue)
        {
            var currentCount = await _context.Enrollments.CountAsync(e => e.CourseId == courseId
                && e.Semester == settings.CurrentSemester
                && e.AcademicYear == settings.CurrentAcademicYear);

            if (currentCount >= course.MaxStudents.Value)
            {
                TempData["Error"] = "This course is full.";
                return RedirectToAction(nameof(Index));
            }
        }

        var enrollment = new Enrollment
        {
            StudentId = student.StudentId,
            CourseId = courseId,
            Semester = settings.CurrentSemester,
            AcademicYear = settings.CurrentAcademicYear,
            EnrolledOn = DateTime.UtcNow
        };

        try
        {
            _context.Add(enrollment);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Enrolled successfully.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "You're already enrolled in this course this term.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(int enrollmentId)
    {
        var student = await GetCurrentStudentAsync();
        if (student == null)
        {
            TempData["Error"] = "Your account isn't linked to a Student record.";
            return RedirectToAction("Student", "Dashboard");
        }

        var enrollment = await _context.Enrollments
            .Include(e => e.Grade)
            .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);

        if (enrollment == null || enrollment.StudentId != student.StudentId)
        {
            TempData["Error"] = "Enrollment not found.";
            return RedirectToAction("Student", "Dashboard");
        }

        if (enrollment.Grade != null)
        {
            TempData["Error"] = "You can't withdraw from a course that already has a grade recorded.";
            return RedirectToAction("Student", "Dashboard");
        }

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();

        TempData["Message"] = "Withdrawn successfully.";
        return RedirectToAction("Student", "Dashboard");
    }

    private async Task<Student?> GetCurrentStudentAsync()
    {
        var userId = _userManager.GetUserId(User);
        return await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
    }
}