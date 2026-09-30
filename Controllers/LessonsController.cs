using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Controllers
{
    [Authorize]
    public class LessonsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public LessonsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index(int? courseId)
        {
            if (courseId == null || courseId == 0)
            {
                if (User.IsInRole("Admin"))
                {
                    var allLessons = await _context.Lessons
                        .Include(l => l.Course)
                        .OrderByDescending(l => l.UploadDate)
                        .ToListAsync();

                    ViewBag.CourseId = 0;
                    ViewBag.CourseTitle = "All Lessons";
                    ViewBag.CanManage = true;
                    return View(allLessons);
                }
                return RedirectToAction("Index", "Home");
            }

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            ViewBag.CourseId = courseId;
            ViewBag.CourseTitle = course.Title;

            bool canManage = User.IsInRole("Admin") || User.IsInRole("Teacher");
            ViewBag.CanManage = canManage;

            var lessons = await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .Include(l => l.Course)
                .OrderByDescending(l => l.UploadDate)
                .ToListAsync();

            return View(lessons);
        }

        [Authorize(Roles = "Admin,Teacher")]
        public IActionResult Create(int courseId)
        {
            ViewBag.CourseId = courseId;

            if (courseId == 0)
            {
                ViewBag.Courses = new SelectList(_context.Courses.ToList(), "CourseId", "Title");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Create(int courseId, string title, string? description, string videoType, IFormFile? videoFile, string? youtubeUrl)
        {
            if (videoType == "Upload")
            {
                if (videoFile == null || videoFile.Length == 0)
                {
                    ModelState.AddModelError("", "Please select a video file.");
                    ViewBag.CourseId = courseId;
                    if (courseId == 0)
                        ViewBag.Courses = new SelectList(_context.Courses.ToList(), "CourseId", "Title");
                    return View();
                }
            }
            else if (videoType == "YouTube")
            {
                if (string.IsNullOrWhiteSpace(youtubeUrl))
                {
                    ModelState.AddModelError("", "Please enter a YouTube URL.");
                    ViewBag.CourseId = courseId;
                    if (courseId == 0)
                        ViewBag.Courses = new SelectList(_context.Courses.ToList(), "CourseId", "Title");
                    return View();
                }
            }

            var lesson = new Lesson
            {
                Title = title,
                Description = description,
                VideoType = videoType,
                CourseId = courseId,
                UploadDate = DateTime.Now
            };

            if (videoType == "Upload" && videoFile != null)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "videos");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(videoFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await videoFile.CopyToAsync(fileStream);
                }

                lesson.VideoPath = "/videos/" + uniqueFileName;
            }
else if (videoType == "YouTube")
{
    var embedUrl = ConvertToYouTubeEmbed(youtubeUrl!);
    if (embedUrl == null)
    {
        ModelState.AddModelError("", "Invalid YouTube URL. Please enter a valid link.");
        ViewBag.CourseId = courseId;
        if (courseId == 0)
            ViewBag.Courses = new SelectList(_context.Courses.ToList(), "CourseId", "Title");
        return View();
    }
    lesson.YouTubeUrl = embedUrl;
}
            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { courseId = courseId });
        }

        private string? ConvertToYouTubeEmbed(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;

            if (url.Contains("/embed/"))
            {
                var id = url.Split("/embed/")[1].Split("?")[0];
                if (id.Length != 11) return null;
                return url;
            }

            string videoId = "";

            try
            {
                if (url.Contains("youtu.be/"))
                {
                    videoId = url.Split("youtu.be/")[1].Split("?")[0].Split("&")[0];
                }
                else if (url.Contains("watch?v="))
                {
                    videoId = url.Split("watch?v=")[1].Split("&")[0];
                }
                else if (url.Contains("youtube.com/shorts/"))
                {
                    videoId = url.Split("shorts/")[1].Split("?")[0];
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }

            if (videoId.Length != 11) return null;

            return $"https://www.youtube.com/embed/{videoId}";
        }

        // مسح درس
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Delete(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null) return NotFound();

            if (!string.IsNullOrEmpty(lesson.VideoPath))
            {
                string fullPath = Path.Combine(_environment.WebRootPath, lesson.VideoPath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
            }

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { courseId = lesson.CourseId });
        }
    }
}
