using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;

public class CourseTeachersController : Controller
{
    private readonly ApplicationDbContext _context;

    public CourseTeachersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var schoolContext = _context.CourseTeachers.Include(ct => ct.Course).Include(ct => ct.Teacher);
        return View(await schoolContext.ToListAsync());
    }

    public async Task<IActionResult> Details(int? courseteacherid)
    {
        if (courseteacherid == null) return NotFound();

        var courseTeacher = await _context.CourseTeachers
            .Include(ct => ct.Course)
            .Include(ct => ct.Teacher)
            .FirstOrDefaultAsync(m => m.CourseTeacherId == courseteacherid);
        if (courseTeacher == null) return NotFound();

        return View(courseTeacher);
    }

    public IActionResult Create()
    {
        PopulateDropdowns();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CourseTeacherId,CourseId,TeacherId,IsPrimary")] CourseTeacher courseTeacher)
    {
        ModelState.Remove(nameof(CourseTeacher.Course));
        ModelState.Remove(nameof(CourseTeacher.Teacher));

        if (ModelState.IsValid)
        {
            var teacher = await _context.Teachers.FindAsync(courseTeacher.TeacherId);
            if (teacher != null && !teacher.IsActive)
            {
                ModelState.AddModelError(string.Empty, "This teacher is not active.");
                PopulateDropdowns(courseTeacher.CourseId, courseTeacher.TeacherId);
                return View(courseTeacher);
            }

            _context.Add(courseTeacher);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        PopulateDropdowns(courseTeacher.CourseId, courseTeacher.TeacherId);
        return View(courseTeacher);
    }

    public async Task<IActionResult> Edit(int? courseteacherid)
    {
        if (courseteacherid == null) return NotFound();

        var courseTeacher = await _context.CourseTeachers.FindAsync(courseteacherid);
        if (courseTeacher == null) return NotFound();

        PopulateDropdowns(courseTeacher.CourseId, courseTeacher.TeacherId);
        return View(courseTeacher);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? courseteacherid, [Bind("CourseTeacherId,CourseId,TeacherId,IsPrimary")] CourseTeacher courseTeacher)
    {
        if (courseteacherid != courseTeacher.CourseTeacherId) return NotFound();

        ModelState.Remove(nameof(CourseTeacher.Course));
        ModelState.Remove(nameof(CourseTeacher.Teacher));

        if (ModelState.IsValid)
        {
            var teacher = await _context.Teachers.FindAsync(courseTeacher.TeacherId);
            if (teacher != null && !teacher.IsActive)
            {
                ModelState.AddModelError(string.Empty, "This teacher is not active.");
                PopulateDropdowns(courseTeacher.CourseId, courseTeacher.TeacherId);
                return View(courseTeacher);
            }

            try
            {
                _context.Update(courseTeacher);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseTeacherExists(courseTeacher.CourseTeacherId)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        PopulateDropdowns(courseTeacher.CourseId, courseTeacher.TeacherId);
        return View(courseTeacher);
    }

    public async Task<IActionResult> Delete(int? courseteacherid)
    {
        if (courseteacherid == null) return NotFound();

        var courseTeacher = await _context.CourseTeachers
            .Include(ct => ct.Course)
            .Include(ct => ct.Teacher)
            .FirstOrDefaultAsync(m => m.CourseTeacherId == courseteacherid);
        if (courseTeacher == null) return NotFound();

        return View(courseTeacher);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? courseteacherid)
    {
        var courseTeacher = await _context.CourseTeachers.FindAsync(courseteacherid);
        if (courseTeacher != null) _context.CourseTeachers.Remove(courseTeacher);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CourseTeacherExists(int? courseteacherid)
    {
        return _context.CourseTeachers.Any(e => e.CourseTeacherId == courseteacherid);
    }

    private void PopulateDropdowns(int? selectedCourseId = null, int? selectedTeacherId = null)
    {
        ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "Title", selectedCourseId);

        var teacherList = _context.Teachers
            .Select(t => new { t.TeacherId, DisplayName = t.EmployeeNumber + " - " + t.LastName + ", " + t.FirstName })
            .ToList();
        ViewData["TeacherId"] = new SelectList(teacherList, "TeacherId", "DisplayName", selectedTeacherId);
    }
}
