using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;

public class GradesController : Controller
{
    private readonly ApplicationDbContext _context;

    public GradesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // نجيب كل الـ Enrollments
        var enrollments = await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Include(e => e.Grade)
            .ToListAsync();

        // نبني قائمة Grades لكل الـ Enrollments
        var gradesList = new List<Grade>();

        foreach (var enrollment in enrollments)
        {
            var quizScore = await ComputeQuizScore(enrollment.EnrollmentId);
            var grade = enrollment.Grade;

            if (grade == null)
            {
                // لو مفيش grade مسجل، نعمل واحد افتراضي
                grade = new Grade
                {
                    EnrollmentId = enrollment.EnrollmentId,
                    Enrollment = enrollment,
                    AssignmentScore = 0,
                    MidtermScore = 0,
                    FinalScore = 0,
                    QuizScore = quizScore,
                    TotalScore = quizScore,
                    LetterGrade = ComputeLetterGrade(quizScore)
                };
            }
            else
            {
                // لو موجود، نحدث الـ Quiz Score تلقائياً
                grade.Enrollment = enrollment;
                grade.QuizScore = quizScore;
                grade.TotalScore = (grade.AssignmentScore ?? 0) + (grade.MidtermScore ?? 0) 
                                   + (grade.FinalScore ?? 0) + quizScore;
                grade.LetterGrade = ComputeLetterGrade(grade.TotalScore.Value);

                // نحفظ التحديث لو اختلف
                if (grade.GradeId > 0)
                {
                    _context.Update(grade);
                }
            }

            gradesList.Add(grade);
        }

        // نحفظ أي تحديثات في قاعدة البيانات
        await _context.SaveChangesAsync();

        return View(gradesList);
    }

    public async Task<IActionResult> Details(int? gradeid)
    {
        if (gradeid == null) return NotFound();

        var grade = await _context.Grades
            .Include(g => g.Enrollment).ThenInclude(e => e.Student)
            .Include(g => g.Enrollment).ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(m => m.GradeId == gradeid);
        if (grade == null) return NotFound();

        return View(grade);
    }

    public IActionResult Create()
    {
        PopulateDropdowns();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("GradeId,EnrollmentId,AssignmentScore,MidtermScore,FinalScore")] Grade grade)
    {
        ModelState.Remove(nameof(Grade.Enrollment));

        if (grade.AssignmentScore.HasValue && (grade.AssignmentScore < 0 || grade.AssignmentScore > 10))
            ModelState.AddModelError(nameof(Grade.AssignmentScore), "Assignment Score must be between 0 and 10.");

        if (grade.MidtermScore.HasValue && (grade.MidtermScore < 0 || grade.MidtermScore > 20))
            ModelState.AddModelError(nameof(Grade.MidtermScore), "Midterm Score must be between 0 and 20.");

        if (grade.FinalScore.HasValue && (grade.FinalScore < 0 || grade.FinalScore > 60))
            ModelState.AddModelError(nameof(Grade.FinalScore), "Final Score must be between 0 and 60.");

        if (ModelState.IsValid)
        {
            grade.QuizScore = await ComputeQuizScore(grade.EnrollmentId);
            grade.TotalScore = (grade.AssignmentScore ?? 0) + (grade.MidtermScore ?? 0) 
                               + (grade.FinalScore ?? 0) + (grade.QuizScore ?? 0);
            grade.LetterGrade = ComputeLetterGrade(grade.TotalScore.Value);

            try
            {
                _context.Add(grade);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty,
                    "This enrollment already has a grade recorded. Edit the existing grade instead.");
            }
        }
        PopulateDropdowns(grade.EnrollmentId);
        return View(grade);
    }

    public async Task<IActionResult> Edit(int? gradeid)
    {
        if (gradeid == null) return NotFound();

        var grade = await _context.Grades.FindAsync(gradeid);
        if (grade == null) return NotFound();

        PopulateDropdowns(grade.EnrollmentId);
        return View(grade);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? gradeid, [Bind("GradeId,EnrollmentId,AssignmentScore,MidtermScore,FinalScore")] Grade grade)
    {
        if (gradeid != grade.GradeId) return NotFound();

        ModelState.Remove(nameof(Grade.Enrollment));

        if (grade.AssignmentScore.HasValue && (grade.AssignmentScore < 0 || grade.AssignmentScore > 10))
            ModelState.AddModelError(nameof(Grade.AssignmentScore), "Assignment Score must be between 0 and 10.");

        if (grade.MidtermScore.HasValue && (grade.MidtermScore < 0 || grade.MidtermScore > 20))
            ModelState.AddModelError(nameof(Grade.MidtermScore), "Midterm Score must be between 0 and 20.");

        if (grade.FinalScore.HasValue && (grade.FinalScore < 0 || grade.FinalScore > 60))
            ModelState.AddModelError(nameof(Grade.FinalScore), "Final Score must be between 0 and 60.");

        if (ModelState.IsValid)
        {
            grade.QuizScore = await ComputeQuizScore(grade.EnrollmentId);
            grade.TotalScore = (grade.AssignmentScore ?? 0) + (grade.MidtermScore ?? 0) 
                               + (grade.FinalScore ?? 0) + (grade.QuizScore ?? 0);
            grade.LetterGrade = ComputeLetterGrade(grade.TotalScore.Value);

            try
            {
                _context.Update(grade);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GradeExists(grade.GradeId)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "This enrollment already has a grade recorded.");
                PopulateDropdowns(grade.EnrollmentId);
                return View(grade);
            }
            return RedirectToAction(nameof(Index));
        }
        PopulateDropdowns(grade.EnrollmentId);
        return View(grade);
    }

    public async Task<IActionResult> Delete(int? gradeid)
    {
        if (gradeid == null) return NotFound();

        var grade = await _context.Grades
            .Include(g => g.Enrollment).ThenInclude(e => e.Student)
            .Include(g => g.Enrollment).ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(m => m.GradeId == gradeid);
        if (grade == null) return NotFound();

        return View(grade);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? gradeid)
    {
        var grade = await _context.Grades.FindAsync(gradeid);
        if (grade != null) _context.Grades.Remove(grade);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool GradeExists(int? gradeid)
    {
        return _context.Grades.Any(e => e.GradeId == gradeid);
    }

    private void PopulateDropdowns(int? selectedEnrollmentId = null)
    {
        var enrollmentQuery = _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Where(e => e.Grade == null || e.EnrollmentId == selectedEnrollmentId);

        var enrollmentList = enrollmentQuery
            .Select(e => new
            {
                e.EnrollmentId,
                DisplayName = e.Student.StudentNumber + " - " + e.Student.LastName + ", " + e.Student.FirstName
                              + " | " + e.Course.Title + " (" + e.Semester + " " + e.AcademicYear + ")"
            })
            .ToList();

        ViewData["EnrollmentId"] = new SelectList(enrollmentList, "EnrollmentId", "DisplayName", selectedEnrollmentId);
    }

    private async Task<decimal> ComputeQuizScore(int enrollmentId)
    {
        var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
        if (enrollment == null) return 0;

        var studentId = enrollment.StudentId;
        var courseId = enrollment.CourseId;

        var quizIds = await _context.Quizzes
            .Where(q => q.CourseId == courseId)
            .Select(q => q.QuizId)
            .ToListAsync();

        if (!quizIds.Any()) return 0;

        var studentQuizzes = await _context.StudentQuizzes
            .Where(sq => sq.StudentId == studentId && quizIds.Contains(sq.QuizId) && sq.SubmittedAt != default)
            .ToListAsync();

        if (!studentQuizzes.Any()) return 0;

        var highestPercentage = studentQuizzes.Max(sq => 
            sq.TotalMarks > 0 ? (decimal)sq.Score / sq.TotalMarks : 0
        );

        return Math.Round(highestPercentage * 10, 2);
    }

    private static string ComputeLetterGrade(decimal totalScore)
    {
        if (totalScore >= 90) return "A";
        if (totalScore >= 80) return "B";
        if (totalScore >= 70) return "C";
        if (totalScore >= 60) return "D";
        return "F";
    }
}
