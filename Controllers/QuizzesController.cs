using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;
using System.Text.Json;

namespace SchoolManagementSystem.Controllers
{
    [Authorize]
    public class QuizzesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public QuizzesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? courseId)
        {
            if (courseId == null || courseId == 0)
            {
                if (User.IsInRole("Admin"))
                {
                    var allQuizzes = await _context.Quizzes
                        .Include(q => q.Course)
                        .Include(q => q.Questions)
                        .OrderByDescending(q => q.StartDate)
                        .ToListAsync();

                    ViewBag.CourseId = 0;
                    ViewBag.CourseTitle = "All Quizzes";
                    ViewBag.CanManage = true;
                    return View(allQuizzes);
                }
                return RedirectToAction("Index", "Home");
            }

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            ViewBag.CourseId = courseId;
            ViewBag.CourseTitle = course.Title;
            ViewBag.CanManage = User.IsInRole("Admin") || User.IsInRole("Teacher");

            var quizzes = await _context.Quizzes
                .Where(q => q.CourseId == courseId)
                .Include(q => q.Course)
                .Include(q => q.Questions)
                .OrderByDescending(q => q.StartDate)
                .ToListAsync();

            return View(quizzes);
        }

        [Authorize(Roles = "Admin,Teacher")]
        public IActionResult Create(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Create(int courseId, string title, string? description, DateTime startDate, DateTime endDate, int durationMinutes, string questionsJson)
        {
            if (string.IsNullOrWhiteSpace(questionsJson) || questionsJson == "[]")
            {
                ModelState.AddModelError("", "Please add at least one question.");
                ViewBag.CourseId = courseId;
                return View();
            }

            var quiz = new Quiz
            {
                Title = title,
                Description = description,
                StartDate = startDate,
                EndDate = endDate,
                DurationMinutes = durationMinutes,
                CourseId = courseId,
                TotalMarks = 0
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            var questions = JsonSerializer.Deserialize<List<QuestionInput>>(questionsJson);

            if (questions != null)
            {
                foreach (var q in questions)
                {
                    var question = new Question
                    {
                        QuizId = quiz.QuizId,
                        QuestionText = q.QuestionText,
                        OptionsJson = q.OptionsJson,
                        CorrectAnswer = q.CorrectAnswer,
                        Marks = q.Marks
                    };
                    _context.Questions.Add(question);
                    quiz.TotalMarks += q.Marks;
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { courseId = courseId });
        }

        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Edit(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.QuizId == id);

            if (quiz == null) return NotFound();

            ViewBag.CourseId = quiz.CourseId;
            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Edit(int quizId, string title, string? description, DateTime startDate, DateTime endDate, int durationMinutes, string questionsJson)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.QuizId == quizId);

            if (quiz == null) return NotFound();

            quiz.Title = title;
            quiz.Description = description;
            quiz.StartDate = startDate;
            quiz.EndDate = endDate;
            quiz.DurationMinutes = durationMinutes;

            _context.Questions.RemoveRange(quiz.Questions);

            var questions = JsonSerializer.Deserialize<List<QuestionInput>>(questionsJson);
            quiz.TotalMarks = 0;

            if (questions != null)
            {
                foreach (var q in questions)
                {
                    var question = new Question
                    {
                        QuizId = quiz.QuizId,
                        QuestionText = q.QuestionText,
                        OptionsJson = q.OptionsJson,
                        CorrectAnswer = q.CorrectAnswer,
                        Marks = q.Marks
                    };
                    _context.Questions.Add(question);
                    quiz.TotalMarks += q.Marks;
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { courseId = quiz.CourseId });
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Take(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.QuizId == id);

            if (quiz == null) return NotFound();

            if (DateTime.Now < quiz.StartDate || DateTime.Now > quiz.EndDate)
            {
                TempData["Error"] = "This quiz is not available at the moment.";
                return RedirectToAction("Index", "Dashboard");
            }

            var userId = _userManager.GetUserId(User);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return RedirectToAction("Index", "Dashboard");

            var isEnrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == student.StudentId && e.CourseId == quiz.CourseId);
            if (!isEnrolled) return RedirectToAction("Index", "Dashboard");

            var existing = await _context.StudentQuizzes
                .FirstOrDefaultAsync(sq => sq.QuizId == id && sq.StudentId == student.StudentId);

            if (existing != null)
            {
                if (existing.SubmittedAt == default)
                {
                    existing.SubmittedAt = DateTime.Now;
                    existing.Score = existing.Score; // يحتفظ بالدرجة الحالية (0)
                    await _context.SaveChangesAsync();
                }

                TempData["Error"] = "You have already taken this quiz. Your submission was recorded.";
                return RedirectToAction("Index", "Dashboard");
            }

            var studentQuiz = new StudentQuiz
            {
                QuizId = id,
                StudentId = student.StudentId,
                StartedAt = DateTime.Now,
                SubmittedAt = default,
                Score = 0,
                TotalMarks = quiz.TotalMarks
            };
            _context.StudentQuizzes.Add(studentQuiz);
            await _context.SaveChangesAsync();

            ViewBag.RemainingSeconds = quiz.DurationMinutes * 60;
            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Submit(int quizId, IFormCollection form)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.QuizId == quizId);

            if (quiz == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null) return RedirectToAction("Index", "Dashboard");

            var studentQuiz = await _context.StudentQuizzes
                .FirstOrDefaultAsync(sq => sq.QuizId == quizId && sq.StudentId == student.StudentId);

            if (studentQuiz == null) return RedirectToAction("Index", "Dashboard");

            if (studentQuiz.SubmittedAt != default)
            {
                TempData["Error"] = "You have already submitted this quiz.";
                return RedirectToAction("Index", "Dashboard");
            }

            int score = 0;
            var answers = new Dictionary<string, string>();

            foreach (var question in quiz.Questions)
            {
                var selected = form[$"q_{question.QuestionId}"].ToString();
                answers[question.QuestionId.ToString()] = selected;

                if (!string.IsNullOrEmpty(selected) && selected == question.CorrectAnswer)
                {
                    score += question.Marks;
                }
            }

            studentQuiz.Score = score;
            studentQuiz.SubmittedAt = DateTime.Now;
            studentQuiz.AnswersJson = JsonSerializer.Serialize(answers);

            await _context.SaveChangesAsync();

            TempData["Message"] = $"Quiz submitted! Your score: {score} / {quiz.TotalMarks}";
            return RedirectToAction("Index", "Dashboard");
        }

        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Results(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.QuizId == id);

            if (quiz == null) return NotFound();

            var results = await _context.StudentQuizzes
                .Where(sq => sq.QuizId == id)
                .Include(sq => sq.Student)
                    .ThenInclude(s => s.User)
                .OrderByDescending(sq => sq.Score)
                .ToListAsync();

            ViewBag.QuizTitle = quiz.Title;
            ViewBag.TotalMarks = quiz.TotalMarks;
            ViewBag.DurationMinutes = quiz.DurationMinutes;

            return View(results);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Delete(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null) return NotFound();

            var questions = await _context.Questions.Where(q => q.QuizId == id).ToListAsync();
            _context.Questions.RemoveRange(questions);

            var studentQuizzes = await _context.StudentQuizzes.Where(sq => sq.QuizId == id).ToListAsync();
            _context.StudentQuizzes.RemoveRange(studentQuizzes);

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { courseId = quiz.CourseId });
        }
    }

    public class QuestionInput
    {
        public string QuestionText { get; set; } = string.Empty;
        public string? OptionsJson { get; set; }
        public string CorrectAnswer { get; set; } = string.Empty;
        public int Marks { get; set; } = 1;
    }
}
