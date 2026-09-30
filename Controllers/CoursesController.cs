using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Models;
using SchoolManagementSystem.Data;

public class CoursesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CoursesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var schoolContext = _context.Courses.Include(c => c.Department);
        return View(await schoolContext.ToListAsync());
    }

    public async Task<IActionResult> Details(int? courseid)
    {
        if (courseid == null) return NotFound();

        var course = await _context.Courses
            .Include(c => c.Department)
            .FirstOrDefaultAsync(m => m.CourseId == courseid);
        if (course == null) return NotFound();

        return View(course);
    }

    public IActionResult Create()
    {
        ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CourseId,DepartmentId,CourseCode,Title,Credits,MaxStudents,IsActive,IsEnrollmentOpen")] Course course)
    {
        if (ModelState.IsValid)
        {
            _context.Add(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "Name", course.DepartmentId);
        return View(course);
    }

    public async Task<IActionResult> Edit(int? courseid)
    {
        if (courseid == null) return NotFound();

        var course = await _context.Courses.FindAsync(courseid);
        if (course == null) return NotFound();

        ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "Name", course.DepartmentId);
        return View(course);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? courseid, [Bind("CourseId,DepartmentId,CourseCode,Title,Credits,MaxStudents,IsActive,IsEnrollmentOpen")] Course course)
    {
        if (courseid != course.CourseId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(course);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(course.CourseId)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "Name", course.DepartmentId);
        return View(course);
    }

    public async Task<IActionResult> Delete(int? courseid)
    {
        if (courseid == null) return NotFound();

        var course = await _context.Courses
            .Include(c => c.Department)
            .FirstOrDefaultAsync(m => m.CourseId == courseid);
        if (course == null) return NotFound();

        return View(course);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? courseid)
    {
        var course = await _context.Courses.FindAsync(courseid);
        if (course != null) _context.Courses.Remove(course);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CourseExists(int? courseid)
    {
        return _context.Courses.Any(e => e.CourseId == courseid);
    }
}