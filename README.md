# 🎓 School Management System

![.NET](https://img.shields.io/badge/.NET-ASP.NET_Core_MVC-512BD4?logo=dotnet&logoColor=white)
![EF Core](https://img.shields.io/badge/EF_Core-ORM-68217A)
![SQLite](https://img.shields.io/badge/SQLite-Database-003B57?logo=sqlite&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5-7952B3?logo=bootstrap&logoColor=white)
![License](https://img.shields.io/badge/license-educational-green)

A full-featured web application for managing the academic life of a school: users, courses, enrollments, grades, attendance, video lessons, and quizzes, with a dedicated dashboard for each role.

Built with **ASP.NET Core MVC**, **Entity Framework Core**, and **SQLite**.

---

## ✨ Features

**Role-based access** for three user types: Admin, Teacher, and Student.

| Area | What it does |
|------|--------------|
| 👥 **Users** | Manage students, teachers, and admins; link records to Identity accounts |
| 📚 **Courses** | Create courses and departments, assign teachers to courses |
| 📝 **Enrollments** | Students enroll in open courses |
| 📊 **Grades** | Automatic grade calculation from assignments, midterms, finals, and quizzes |
| 📅 **Attendance** | Record and track attendance per course |
| 📹 **Lessons** | Upload video lessons from a device or add a YouTube link |
| ❓ **Quizzes** | MCQ quizzes with a countdown timer and automatic grading |
| 📈 **Dashboards** | Separate views for Admin, Teacher, and Student |

---

## 🛠️ Tech Stack

- **Framework:** ASP.NET Core MVC
- **ORM:** Entity Framework Core (Code First + Migrations)
- **Database:** SQLite
- **Auth:** ASP.NET Core Identity (role-based authorization)
- **Frontend:** Razor Views, Bootstrap 5

---

## 🚀 Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (the version your project targets; check `TargetFramework` in the `.csproj` file)
- [Git](https://git-scm.com/downloads)
- EF Core CLI tools:
```bash
  dotnet tool install --global dotnet-ef
```

> SQLite needs no separate installation. It ships with the EF Core provider.

### Installation

```bash
# 1. Clone the repository
git clone https://github.com/John-Wael/SchoolManagementSystem.git
cd SchoolManagementSystem

# 2. Restore dependencies
dotnet restore

# 3. Create the database from the migrations
dotnet ef database update

# 4. Run the app
dotnet run
```

Then open **http://localhost:5121** in your browser (the exact port is printed in the terminal and set in `Properties/launchSettings.json`).

---

## 🔐 Demo Accounts

The database is seeded with sample data on first run.

| Role | Email | Password |
|------|-------|----------|
| Admin | `admin@school.local` | _see `Data/` seed file_ |
| Teacher | `teacher@school.local` | _see `Data/` seed file_ |
| Student | `stud1@fcai.cu` | _see `Data/` seed file_ |

> ⚠️ These accounts are for local development only. Change or remove them before deploying anywhere public.

---

## 📁 Project Structure

```
SchoolManagementSystem/
├── Areas/Identity/     # Authentication pages
├── Controllers/        # MVC controllers
├── Data/               # DbContext and seed data
├── Migrations/         # EF Core migrations
├── Models/             # Domain models and view models
├── Views/              # Razor views
├── wwwroot/            # Static files (CSS, JS, uploads)
├── appsettings.json    # Configuration
└── Program.cs          # Application entry point
```

---

## 🤝 Contributing

Contributions and suggestions are welcome.

1. Fork the repository
2. Create a branch: `git checkout -b feature/your-feature`
3. Commit your changes: `git commit -m "Add your feature"`
4. Push and open a Pull Request

---

## 📝 License

This project is for educational purposes. Feel free to use and modify it.

## 👤 Author

**John Wael**
GitHub: [@John-Wael](https://github.com/John-Wael)
