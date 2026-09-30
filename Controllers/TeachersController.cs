using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;

public class TeachersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TeachersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var schoolContext = _context.Teachers.Include(t => t.Department);
        return View(await schoolContext.ToListAsync());
    }

    public async Task<IActionResult> Details(int? teacherid)
    {
        if (teacherid == null) return NotFound();

        var teacher = await _context.Teachers
            .Include(t => t.Department)
            .FirstOrDefaultAsync(m => m.TeacherId == teacherid);
        if (teacher == null) return NotFound();

        return View(teacher);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "Name");
        ViewData["UserId"] = new SelectList(await GetAvailableTeacherUsersAsync(null), "Id", "Email");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TeacherId,UserId,DepartmentId,FirstName,LastName,HireDate,IsActive")] Teacher teacher)
    {
        ModelState.Remove(nameof(Teacher.EmployeeNumber));
        ModelState.Remove(nameof(Teacher.Department));
        ModelState.Remove(nameof(Teacher.User));
        ModelState.Remove(nameof(Teacher.CourseTeachers));

        if (!ModelState.IsValid)
        {
            foreach (var kv in ModelState)
            {
                foreach (var e in kv.Value.Errors)
                {
                    Console.WriteLine($"[MS-ERROR] Key='{kv.Key}' Msg='{e.ErrorMessage}'");
                }
            }
        }

        if (ModelState.IsValid)
        {
            teacher.EmployeeNumber = GenerateEmployeeNumber();
            teacher.CreatedAt = DateTime.UtcNow;
            _context.Add(teacher);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "Name", teacher.DepartmentId);
        ViewData["UserId"] = new SelectList(await GetAvailableTeacherUsersAsync(null), "Id", "Email", teacher.UserId);
        return View(teacher);
    }

    public async Task<IActionResult> Edit(int? teacherid)
    {
        if (teacherid == null) return NotFound();

        var teacher = await _context.Teachers.FindAsync(teacherid);
        if (teacher == null) return NotFound();

        ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "Name", teacher.DepartmentId);
        ViewData["UserId"] = new SelectList(await GetAvailableTeacherUsersAsync(teacher.UserId), "Id", "Email", teacher.UserId);
        return View(teacher);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? teacherid, [Bind("TeacherId,UserId,DepartmentId,EmployeeNumber,FirstName,LastName,HireDate,IsActive,CreatedAt,ModifiedAt")] Teacher teacher)
    {
        if (teacherid != teacher.TeacherId) return NotFound();

        ModelState.Remove(nameof(Teacher.Department));
        ModelState.Remove(nameof(Teacher.User));
        ModelState.Remove(nameof(Teacher.CourseTeachers));

        if (!ModelState.IsValid)
        {
            foreach (var kv in ModelState)
            {
                foreach (var e in kv.Value.Errors)
                {
                    Console.WriteLine($"[MS-ERROR-EDIT] Key='{kv.Key}' Msg='{e.ErrorMessage}'");
                }
            }
        }

        if (ModelState.IsValid)
        {
            try
            {
                teacher.ModifiedAt = DateTime.UtcNow;
                _context.Update(teacher);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeacherExists(teacher.TeacherId)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "Name", teacher.DepartmentId);
        ViewData["UserId"] = new SelectList(await GetAvailableTeacherUsersAsync(teacher.UserId), "Id", "Email", teacher.UserId);
        return View(teacher);
    }

    public async Task<IActionResult> Delete(int? teacherid)
    {
        if (teacherid == null) return NotFound();

        var teacher = await _context.Teachers
            .Include(t => t.Department)
            .FirstOrDefaultAsync(m => m.TeacherId == teacherid);
        if (teacher == null) return NotFound();

        return View(teacher);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? teacherid)
    {
        var teacher = await _context.Teachers.FindAsync(teacherid);
        if (teacher != null) _context.Teachers.Remove(teacher);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool TeacherExists(int? teacherid)
    {
        return _context.Teachers.Any(e => e.TeacherId == teacherid);
    }

    private string GenerateEmployeeNumber()
    {
        var lastNumber = _context.Teachers
            .Select(t => t.EmployeeNumber)
            .AsEnumerable()
            .Where(en => !string.IsNullOrEmpty(en) && en.StartsWith("ep") && int.TryParse(en.Substring(2), out _))
            .Select(en => int.Parse(en.Substring(2)))
            .DefaultIfEmpty(1099)
            .Max();

        return "ep" + (lastNumber + 1);
    }

    private async Task<List<ApplicationUser>> GetAvailableTeacherUsersAsync(string? currentUserId)
    {
        var teacherRoleUsers = await _userManager.GetUsersInRoleAsync("Teacher");

        var linkedUserIds = await _context.Teachers
            .Where(t => t.UserId != null && t.UserId != currentUserId)
            .Select(t => t.UserId)
            .ToListAsync();

        return teacherRoleUsers
            .Where(u => !linkedUserIds.Contains(u.Id))
            .OrderBy(u => u.Email)
            .ToList();
    }
}
