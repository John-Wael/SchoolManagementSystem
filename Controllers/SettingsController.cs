using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class SettingsController : Controller
{
    private readonly ApplicationDbContext _context;

    public SettingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var settings = await _context.SystemSettings.FindAsync(1);
        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SystemSetting model)
    {
        var settings = await _context.SystemSettings.FindAsync(1);
        if (settings == null) return NotFound();

        settings.CurrentSemester = model.CurrentSemester;
        settings.CurrentAcademicYear = model.CurrentAcademicYear;
        await _context.SaveChangesAsync();

        TempData["Message"] = "Current term updated.";
        return RedirectToAction(nameof(Index));
    }
}