using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;

public class EnrollmentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public EnrollmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var schoolContext = _context.Enrollments.Include(e => e.Student).Include(e => e.Course);
        return View(await schoolContext.ToListAsync());
    }

    public async Task<IActionResult> Details(int? enrollmentid)
    {
        if (enrollmentid == null) return NotFound();

        var enrollment = await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(m => m.EnrollmentId == enrollmentid);
        if (enrollment == null) return NotFound();

        return View(enrollment);
    }

    public IActionResult Create()
    {
        PopulateDropdowns();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("EnrollmentId,StudentId,CourseId,Semester,AcademicYear")] Enrollment enrollment)
    {
        if (ModelState.IsValid)
        {
            var student = await _context.Students.FindAsync(enrollment.StudentId);
            if (student != null && !student.IsActive)
            {
                ModelState.AddModelError(string.Empty, "This student is not active.");
                PopulateDropdowns(enrollment.StudentId, enrollment.CourseId);
                return View(enrollment);
            }

            enrollment.EnrolledOn = DateTime.UtcNow;
            try
            {
                _context.Add(enrollment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty,
                    "This student is already enrolled in this course for the selected semester/year.");
            }
        }
        PopulateDropdowns(enrollment.StudentId, enrollment.CourseId);
        return View(enrollment);
    }

    public async Task<IActionResult> Edit(int? enrollmentid)
    {
        if (enrollmentid == null) return NotFound();

        var enrollment = await _context.Enrollments.FindAsync(enrollmentid);
        if (enrollment == null) return NotFound();

        PopulateDropdowns(enrollment.StudentId, enrollment.CourseId);
        return View(enrollment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? enrollmentid, [Bind("EnrollmentId,StudentId,CourseId,Semester,AcademicYear,EnrolledOn")] Enrollment enrollment)
    {
        if (enrollmentid != enrollment.EnrollmentId) return NotFound();

        if (ModelState.IsValid)
        {
            var student = await _context.Students.FindAsync(enrollment.StudentId);
            if (student != null && !student.IsActive)
            {
                ModelState.AddModelError(string.Empty, "This student is not active.");
                PopulateDropdowns(enrollment.StudentId, enrollment.CourseId);
                return View(enrollment);
            }

            try
            {
                _context.Update(enrollment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EnrollmentExists(enrollment.EnrollmentId)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty,
                    "This student is already enrolled in this course for the selected semester/year.");
                PopulateDropdowns(enrollment.StudentId, enrollment.CourseId);
                return View(enrollment);
            }
            return RedirectToAction(nameof(Index));
        }
        PopulateDropdowns(enrollment.StudentId, enrollment.CourseId);
        return View(enrollment);
    }

    public async Task<IActionResult> Delete(int? enrollmentid)
    {
        if (enrollmentid == null) return NotFound();

        var enrollment = await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(m => m.EnrollmentId == enrollmentid);
        if (enrollment == null) return NotFound();

        return View(enrollment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? enrollmentid)
    {
        var enrollment = await _context.Enrollments.FindAsync(enrollmentid);
        if (enrollment != null) _context.Enrollments.Remove(enrollment);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool EnrollmentExists(int? enrollmentid)
    {
        return _context.Enrollments.Any(e => e.EnrollmentId == enrollmentid);
    }

    private void PopulateDropdowns(int? selectedStudentId = null, int? selectedCourseId = null)
    {
        var studentList = _context.Students
            .Select(s => new { s.StudentId, DisplayName = s.StudentNumber + " - " + s.LastName + ", " + s.FirstName })
            .ToList();
        ViewData["StudentId"] = new SelectList(studentList, "StudentId", "DisplayName", selectedStudentId);

        ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "Title", selectedCourseId);
    }
}