using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Models;
using SchoolManagementSystem.Data;

public class DepartmentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public DepartmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Departments.ToListAsync());
    }

    public async Task<IActionResult> Details(int? departmentid)
    {
        if (departmentid == null) return NotFound();

        var department = await _context.Departments.FirstOrDefaultAsync(m => m.DepartmentId == departmentid);
        if (department == null) return NotFound();

        return View(department);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("DepartmentId,Name,Code,Courses,Students,Teachers")] Department department)
    {
        if (ModelState.IsValid)
        {
            _context.Add(department);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(department);
    }

    public async Task<IActionResult> Edit(int? departmentid)
    {
        if (departmentid == null) return NotFound();

        var department = await _context.Departments.FindAsync(departmentid);
        if (department == null) return NotFound();

        return View(department);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? departmentid, [Bind("DepartmentId,Name,Code,Courses,Students,Teachers")] Department department)
    {
        if (departmentid != department.DepartmentId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(department);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepartmentExists(department.DepartmentId)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(department);
    }

    public async Task<IActionResult> Delete(int? departmentid)
    {
        if (departmentid == null) return NotFound();

        var department = await _context.Departments.FirstOrDefaultAsync(m => m.DepartmentId == departmentid);
        if (department == null) return NotFound();

        return View(department);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? departmentid)
    {
        var department = await _context.Departments.FindAsync(departmentid);
        if (department != null) _context.Departments.Remove(department);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool DepartmentExists(int? departmentid)
    {
        return _context.Departments.Any(e => e.DepartmentId == departmentid);
    }
}