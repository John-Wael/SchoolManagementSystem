using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Attendance> Attendances { get; set; }
        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<CourseTeacher> CourseTeachers { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Enrollment> Enrollments { get; set; }
        public virtual DbSet<Grade> Grades { get; set; }
        public virtual DbSet<Lesson> Lessons { get; set; }
        public virtual DbSet<Quiz> Quizzes { get; set; }
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<StudentQuiz> StudentQuizzes { get; set; }
        public virtual DbSet<Student> Students { get; set; }
        public virtual DbSet<SystemSetting> SystemSettings { get; set; }
        public virtual DbSet<Teacher> Teachers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasOne(d => d.Enrollment)
                    .WithMany(p => p.Attendances)
                    .HasConstraintName("FK_Attendance_Enrollments");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.Property(e => e.Credits).HasDefaultValue(3);
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasOne(d => d.Department)
                    .WithMany(p => p.Courses)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Courses_Departments");
            });

            modelBuilder.Entity<CourseTeacher>(entity =>
            {
                entity.HasIndex(e => e.CourseId, "UX_CourseTeachers_OnePrimaryPerCourse")
                    .IsUnique();

                entity.HasOne(d => d.Course)
                    .WithOne(p => p.CourseTeacher)
                    .HasConstraintName("FK_CourseTeachers_Courses");

                entity.HasOne(d => d.Teacher)
                    .WithMany(p => p.CourseTeachers)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CourseTeachers_Teachers");
            });

            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.Property(e => e.EnrolledOn).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Enrollments)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Enrollments_Courses");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.Enrollments)
                    .HasConstraintName("FK_Enrollments_Students");
            });

            modelBuilder.Entity<Grade>(entity =>
            {
                entity.HasOne(d => d.Enrollment)
                    .WithOne(p => p.Grade)
                    .HasConstraintName("FK_Grades_Enrollments");
            });

            modelBuilder.Entity<Lesson>(entity =>
{
    entity.HasOne(d => d.Course)
        .WithMany()
        .OnDelete(DeleteBehavior.Cascade)
        .HasConstraintName("FK_Lessons_Courses");
});

            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasIndex(e => e.UserId, "UX_Students_UserId").IsUnique();

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasOne(d => d.Department)
                    .WithMany(p => p.Students)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Students_Departments");

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Students_Users");
            });

            modelBuilder.Entity<SystemSetting>(entity =>
            {
                entity.HasKey(e => e.SettingId)
                    .HasName("PK__SystemSe__54372B1D55F175DB");

                entity.Property(e => e.SettingId).ValueGeneratedNever();
            });

            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.HasIndex(e => e.UserId, "UX_Teachers_UserId").IsUnique();

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasOne(d => d.Department)
                    .WithMany(p => p.Teachers)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Teachers_Departments");

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Teachers_Users");
            });
        }
    }
}
