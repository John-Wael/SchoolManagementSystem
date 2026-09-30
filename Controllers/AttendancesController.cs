using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;

public class AttendanceController : Controller
{
    private readonly ApplicationDbContext _context;

    public AttendanceController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var schoolContext = _context.Attendances
            .Include(a => a.Enrollment).ThenInclude(e => e.Student)
            .Include(a => a.Enrollment).ThenInclude(e => e.Course);
        return View(await schoolContext.ToListAsync());
    }

    public async Task<IActionResult> Details(int? attendanceid)
    {
        if (attendanceid == null) return NotFound();

        var attendance = await _context.Attendances
            .Include(a => a.Enrollment).ThenInclude(e => e.Student)
            .Include(a => a.Enrollment).ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(m => m.AttendanceId == attendanceid);
        if (attendance == null) return NotFound();

        return View(attendance);
    }

    public IActionResult Create()
    {
        PopulateDropdowns();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("AttendanceId,EnrollmentId,AttendanceDate,Status")] Attendance attendance)
    {
        ModelState.Remove(nameof(Attendance.Enrollment));

        if (ModelState.IsValid)
        {
            try
            {
                _context.Add(attendance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty,
                    "Attendance for this enrollment on this date has already been recorded.");
            }
        }
        PopulateDropdowns(attendance.EnrollmentId);
        return View(attendance);
    }

    public async Task<IActionResult> Edit(int? attendanceid)
    {
        if (attendanceid == null) return NotFound();

        var attendance = await _context.Attendances.FindAsync(attendanceid);
        if (attendance == null) return NotFound();

        PopulateDropdowns(attendance.EnrollmentId);
        return View(attendance);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? attendanceid, [Bind("AttendanceId,EnrollmentId,AttendanceDate,Status")] Attendance attendance)
    {
        if (attendanceid != attendance.AttendanceId) return NotFound();

        ModelState.Remove(nameof(Attendance.Enrollment));

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(attendance);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AttendanceExists(attendance.AttendanceId)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty,
                    "Attendance for this enrollment on this date has already been recorded.");
                PopulateDropdowns(attendance.EnrollmentId);
                return View(attendance);
            }
            return RedirectToAction(nameof(Index));
        }
        PopulateDropdowns(attendance.EnrollmentId);
        return View(attendance);
    }

    public async Task<IActionResult> Delete(int? attendanceid)
    {
        if (attendanceid == null) return NotFound();

        var attendance = await _context.Attendances
            .Include(a => a.Enrollment).ThenInclude(e => e.Student)
            .Include(a => a.Enrollment).ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(m => m.AttendanceId == attendanceid);
        if (attendance == null) return NotFound();

        return View(attendance);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? attendanceid)
    {
        var attendance = await _context.Attendances.FindAsync(attendanceid);
        if (attendance != null) _context.Attendances.Remove(attendance);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool AttendanceExists(int? attendanceid)
    {
        return _context.Attendances.Any(e => e.AttendanceId == attendanceid);
    }

    private void PopulateDropdowns(int? selectedEnrollmentId = null)
    {
        var enrollmentList = _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .AsEnumerable()
            .Select(e => new
            {
                e.EnrollmentId,
                DisplayName = e.Student.StudentNumber + " - " + e.Student.LastName + ", " + e.Student.FirstName
                              + " | " + e.Course.Title + " (" + e.Semester + " " + e.AcademicYear + ")"
            })
            .ToList();

        ViewData["EnrollmentId"] = new SelectList(enrollmentList, "EnrollmentId", "DisplayName", selectedEnrollmentId);
    }
}
