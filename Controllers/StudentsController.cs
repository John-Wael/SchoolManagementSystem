using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;

[Authorize(Roles = "Admin")]
public class StudentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var students = await _context.Students
            .Include(s => s.Department)
            .Include(s => s.User)
            .ToListAsync();
        return View(students);
    }

    public async Task<IActionResult> Details(int? studentid)
    {
        if (studentid == null) return NotFound();

        var student = await _context.Students
            .Include(s => s.Department)
            .Include(s => s.User)
            .FirstOrDefaultAsync(m => m.StudentId == studentid);

        if (student == null) return NotFound();

        return View(student);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "Name");
        await PopulateAvailableUsers();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [Bind("StudentId,UserId,DepartmentId,StudentNumber,FirstName,LastName,DateOfBirth,IsActive,CreatedAt")] Student student)
    {
        Console.WriteLine("=== CREATE POST CALLED ===");
        ModelState.Remove(nameof(Student.Department));
        ModelState.Remove(nameof(Student.StudentNumber));
        ModelState.Remove(nameof(Student.User));
foreach (var kv in ModelState)
{
    foreach (var err in kv.Value.Errors)
    {
        Console.WriteLine($"[MS-ERROR] Key='{kv.Key}' Msg='{err.ErrorMessage}' Exception='{err.Exception?.Message}'");
    }
}

        if (string.IsNullOrWhiteSpace(student.UserId))
            ModelState.AddModelError("UserId", "Please select a user account.");

        if (student.DateOfBirth.HasValue && student.DateOfBirth.Value > DateOnly.FromDateTime(DateTime.Now))
        {
            ModelState.AddModelError("DateOfBirth", "Date of birth cannot be in the future.");
        }

        if (ModelState.IsValid)
        {
            var lastStudent = await _context.Students
                .OrderByDescending(s => s.StudentNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastStudent != null && lastStudent.StudentNumber.StartsWith("STU"))
            {
                if (int.TryParse(lastStudent.StudentNumber.Substring(3), out int lastNum))
                {
                    nextNumber = lastNum + 1;
                }
            }

            student.StudentNumber = "STU" + nextNumber.ToString("D4");
            student.CreatedAt = DateTime.Now;

            _context.Add(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "Name", student.DepartmentId);
        await PopulateAvailableUsers(student.UserId);
        return View(student);
    }

    public async Task<IActionResult> Edit(int? studentid)
    {
        if (studentid == null) return NotFound();

        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StudentId == studentid);

        if (student == null) return NotFound();

        ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "Name", student.DepartmentId);
        return View(student);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? studentid,
        [Bind("StudentId,UserId,DepartmentId,StudentNumber,FirstName,LastName,DateOfBirth,IsActive,CreatedAt")] Student student)
    {
        if (studentid != student.StudentId) return NotFound();

        ModelState.Remove(nameof(Student.Department));
        ModelState.Remove(nameof(Student.User));

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(student);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(student.StudentId)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "Name", student.DepartmentId);
        return View(student);
    }

    public async Task<IActionResult> Delete(int? studentid)
    {
        if (studentid == null) return NotFound();

        var student = await _context.Students
            .Include(s => s.Department)
            .Include(s => s.User)
            .FirstOrDefaultAsync(m => m.StudentId == studentid);

        if (student == null) return NotFound();

        return View(student);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? studentid)
    {
        var student = await _context.Students.FindAsync(studentid);
        if (student != null) _context.Students.Remove(student);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool StudentExists(int? studentid)
    {
        return _context.Students.Any(e => e.StudentId == studentid);
    }

    private async Task PopulateAvailableUsers(string? selectedUserId = null)
    {
        var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Student");
        if (studentRole == null)
        {
            ViewData["UserId"] = new SelectList(new List<object>(), "Id", "DisplayName");
            return;
        }

        var studentUserIds = await _context.UserRoles
            .Where(ur => ur.RoleId == studentRole.Id)
            .Select(ur => ur.UserId)
            .ToListAsync();

        var linkedUserIds = await _context.Students
            .Where(s => s.UserId != null)
            .Select(s => s.UserId)
            .ToListAsync();

        var availableUsers = await _context.Users
            .Where(u => studentUserIds.Contains(u.Id) && !linkedUserIds.Contains(u.Id))
            .OrderBy(u => u.Email)
            .Select(u => new
            {
                u.Id,
                DisplayName = u.Email
            })
            .ToListAsync();

        ViewData["UserId"] = new SelectList(availableUsers, "Id", "DisplayName", selectedUserId);
    }
}
