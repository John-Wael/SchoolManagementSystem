using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            await SeedStudentsAsync(serviceProvider);
        }

        public static async Task SeedStudentsAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var department = await context.Departments.FirstOrDefaultAsync();
            if (department == null)
            {
                department = new Department { Name = "Data Science", Code = "DS" };
                context.Departments.Add(department);
                await context.SaveChangesAsync();
            }

            var firstCourse = await context.Courses.FirstOrDefaultAsync();

            for (int i = 1; i <= 95; i++)
            {
                string email = $"stud{i}@fcai.cu";
                string password = "Stud@12345";

                var existingUser = await userManager.FindByEmailAsync(email);
                if (existingUser != null) continue;

                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded) continue;

                await userManager.AddToRoleAsync(user, "Student");

                var student = new Student
                {
                    UserId = user.Id,
                    FirstName = $"Student{i}",
                    LastName = "FCAI",
                    StudentNumber = $"STU{i:D4}",
                    DateOfBirth = new DateOnly(2000, 1, 1),
                    DepartmentId = department.DepartmentId,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                context.Students.Add(student);
                await context.SaveChangesAsync();

                if (firstCourse != null)
                {
                    var enrollment = new Enrollment
                    {
                        StudentId = student.StudentId,
                        CourseId = firstCourse.CourseId,
                        Semester = "Fall",
                        AcademicYear = "2026",
                        EnrolledOn = DateTime.Now
                    };
                    context.Enrollments.Add(enrollment);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
