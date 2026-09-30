CREATE TABLE dbo.AspNetUsers
(
    Id            NVARCHAR(450)   NOT NULL,
    UserName      NVARCHAR(256)   NULL,
    NormalizedUserName NVARCHAR(256) NULL,
    Email         NVARCHAR(256)   NULL,
    NormalizedEmail NVARCHAR(256) NULL,
    EmailConfirmed BIT            NOT NULL DEFAULT (0),
    PasswordHash  NVARCHAR(MAX)   NULL,
    SecurityStamp NVARCHAR(MAX)   NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL,
    PhoneNumber   NVARCHAR(50)    NULL,
    PhoneNumberConfirmed BIT      NOT NULL DEFAULT (0),
    TwoFactorEnabled BIT          NOT NULL DEFAULT (0),
    LockoutEnd    DATETIMEOFFSET  NULL,
    LockoutEnabled BIT            NOT NULL DEFAULT (1),
    AccessFailedCount INT         NOT NULL DEFAULT (0),
    CONSTRAINT PK_AspNetUsers PRIMARY KEY (Id),
    CONSTRAINT UQ_AspNetUsers_NormalizedUserName UNIQUE (NormalizedUserName)
);
GO

CREATE TABLE dbo.AspNetRoles
(
    Id   NVARCHAR(450) NOT NULL,
    Name NVARCHAR(256) NULL,
    NormalizedName NVARCHAR(256) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL,
    CONSTRAINT PK_AspNetRoles PRIMARY KEY (Id),
    CONSTRAINT UQ_AspNetRoles_NormalizedName UNIQUE (NormalizedName)
);
GO


CREATE TABLE dbo.Departments
(
    DepartmentId INT IDENTITY(1,1) NOT NULL,
    Name         NVARCHAR(150)     NOT NULL,
    Code         NVARCHAR(20)      NOT NULL,
    CONSTRAINT PK_Departments PRIMARY KEY (DepartmentId),
    CONSTRAINT UQ_Departments_Code UNIQUE (Code)
);
GO


CREATE TABLE dbo.Students
(
    StudentId     INT IDENTITY(1,1) NOT NULL,
    UserId        NVARCHAR(450)     NULL,
    DepartmentId  INT               NOT NULL,
    StudentNumber NVARCHAR(20)      NOT NULL,
    FirstName     NVARCHAR(100)     NOT NULL,
    LastName      NVARCHAR(100)     NOT NULL,
    DateOfBirth   DATE              NULL,
    IsActive      BIT               NOT NULL DEFAULT (1),
    CreatedAt     DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
    ModifiedAt    DATETIME2         NULL,
    CONSTRAINT PK_Students PRIMARY KEY (StudentId),
    CONSTRAINT UQ_Students_StudentNumber UNIQUE (StudentNumber),
    CONSTRAINT UQ_Students_UserId UNIQUE (UserId),
    CONSTRAINT FK_Students_Users FOREIGN KEY (UserId)
        REFERENCES dbo.AspNetUsers (Id) ON DELETE SET NULL,
    CONSTRAINT FK_Students_Departments FOREIGN KEY (DepartmentId)
        REFERENCES dbo.Departments (DepartmentId) ON DELETE NO ACTION
);
GO

CREATE TABLE dbo.Teachers
(
    TeacherId      INT IDENTITY(1,1) NOT NULL,
    UserId         NVARCHAR(450)     NULL,
    DepartmentId   INT               NOT NULL,
    EmployeeNumber NVARCHAR(20)      NOT NULL,
    FirstName      NVARCHAR(100)     NOT NULL,
    LastName       NVARCHAR(100)     NOT NULL,
    HireDate       DATE              NULL,
    IsActive       BIT               NOT NULL DEFAULT (1),
    CreatedAt      DATETIME2         NOT NULL DEFAULT (SYSUTCDATETIME()),
    ModifiedAt     DATETIME2         NULL,
    CONSTRAINT PK_Teachers PRIMARY KEY (TeacherId),
    CONSTRAINT UQ_Teachers_EmployeeNumber UNIQUE (EmployeeNumber),
    CONSTRAINT UQ_Teachers_UserId UNIQUE (UserId),
    CONSTRAINT FK_Teachers_Users FOREIGN KEY (UserId)
        REFERENCES dbo.AspNetUsers (Id) ON DELETE SET NULL,
    CONSTRAINT FK_Teachers_Departments FOREIGN KEY (DepartmentId)
        REFERENCES dbo.Departments (DepartmentId) ON DELETE NO ACTION
);
GO


CREATE TABLE dbo.Courses
(
    CourseId     INT IDENTITY(1,1) NOT NULL,
    DepartmentId INT                NOT NULL,
    CourseCode   NVARCHAR(20)       NOT NULL,
    Title        NVARCHAR(200)      NOT NULL,
    Credits      INT                NOT NULL DEFAULT (3),
    MaxStudents  INT                NULL,
    IsActive     BIT                NOT NULL DEFAULT (1),
    CONSTRAINT PK_Courses PRIMARY KEY (CourseId),
    CONSTRAINT UQ_Courses_CourseCode UNIQUE (CourseCode),
    CONSTRAINT FK_Courses_Departments FOREIGN KEY (DepartmentId)
        REFERENCES dbo.Departments (DepartmentId) ON DELETE NO ACTION,
    CONSTRAINT CK_Courses_Credits CHECK (Credits > 0),
    CONSTRAINT CK_Courses_MaxStudents CHECK (MaxStudents IS NULL OR MaxStudents > 0)
);
GO


CREATE TABLE dbo.CourseTeachers
(
    CourseTeacherId INT IDENTITY(1,1) NOT NULL,
    CourseId        INT               NOT NULL,
    TeacherId       INT               NOT NULL,
    IsPrimary       BIT               NOT NULL DEFAULT (0),
    CONSTRAINT PK_CourseTeachers PRIMARY KEY (CourseTeacherId),
    CONSTRAINT UQ_CourseTeachers_Course_Teacher UNIQUE (CourseId, TeacherId),
    CONSTRAINT FK_CourseTeachers_Courses FOREIGN KEY (CourseId)
        REFERENCES dbo.Courses (CourseId) ON DELETE CASCADE,
    CONSTRAINT FK_CourseTeachers_Teachers FOREIGN KEY (TeacherId)
        REFERENCES dbo.Teachers (TeacherId) ON DELETE NO ACTION
);
GO

CREATE UNIQUE INDEX UX_CourseTeachers_OnePrimaryPerCourse
    ON dbo.CourseTeachers (CourseId)
    WHERE IsPrimary = 1;
GO


CREATE TABLE dbo.Enrollments
(
    EnrollmentId INT IDENTITY(1,1) NOT NULL,
    StudentId    INT                NOT NULL,
    CourseId     INT                NOT NULL,
    Semester     NVARCHAR(20)       NOT NULL,
    AcademicYear NVARCHAR(9)        NOT NULL,
    EnrolledOn   DATETIME2          NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Enrollments PRIMARY KEY (EnrollmentId),
    CONSTRAINT UQ_Enrollments_Student_Course_Term
        UNIQUE (StudentId, CourseId, Semester, AcademicYear),
    CONSTRAINT FK_Enrollments_Students FOREIGN KEY (StudentId)
        REFERENCES dbo.Students (StudentId) ON DELETE CASCADE,
    CONSTRAINT FK_Enrollments_Courses FOREIGN KEY (CourseId)
        REFERENCES dbo.Courses (CourseId) ON DELETE NO ACTION
);
GO


CREATE TABLE dbo.Grades
(
    GradeId          INT IDENTITY(1,1) NOT NULL,
    EnrollmentId     INT                NOT NULL,
    AssignmentScore  DECIMAL(5,2)       NULL,
    MidtermScore     DECIMAL(5,2)       NULL,
    FinalScore       DECIMAL(5,2)       NULL,
    TotalScore  AS (ISNULL(AssignmentScore, 0) + ISNULL(MidtermScore, 0) + ISNULL(FinalScore, 0)) PERSISTED,
    LetterGrade      NVARCHAR(2)        NULL,
    CONSTRAINT PK_Grades PRIMARY KEY (GradeId),
    CONSTRAINT UQ_Grades_EnrollmentId UNIQUE (EnrollmentId),
    CONSTRAINT FK_Grades_Enrollments FOREIGN KEY (EnrollmentId)
        REFERENCES dbo.Enrollments (EnrollmentId) ON DELETE CASCADE,
    CONSTRAINT CK_Grades_AssignmentScore CHECK (AssignmentScore IS NULL OR AssignmentScore BETWEEN 0 AND 20),
    CONSTRAINT CK_Grades_MidtermScore    CHECK (MidtermScore    IS NULL OR MidtermScore    BETWEEN 0 AND 30),
    CONSTRAINT CK_Grades_FinalScore      CHECK (FinalScore      IS NULL OR FinalScore      BETWEEN 0 AND 50)
);
GO


CREATE TABLE dbo.Attendance
(
    AttendanceId   INT IDENTITY(1,1) NOT NULL,
    EnrollmentId   INT                NOT NULL,
    AttendanceDate DATE               NOT NULL,
    Status         NVARCHAR(10)       NOT NULL,
    CONSTRAINT PK_Attendance PRIMARY KEY (AttendanceId),
    CONSTRAINT UQ_Attendance_Enrollment_Date UNIQUE (EnrollmentId, AttendanceDate),
    CONSTRAINT FK_Attendance_Enrollments FOREIGN KEY (EnrollmentId)
        REFERENCES dbo.Enrollments (EnrollmentId) ON DELETE CASCADE,
    CONSTRAINT CK_Attendance_Status
        CHECK (Status IN (N'Present', N'Absent', N'Late', N'Excused'))
);
GO

CREATE INDEX IX_Students_DepartmentId  ON dbo.Students (DepartmentId);
CREATE INDEX IX_Students_LastName      ON dbo.Students (LastName);
CREATE INDEX IX_Teachers_DepartmentId  ON dbo.Teachers (DepartmentId);
CREATE INDEX IX_Teachers_LastName      ON dbo.Teachers (LastName);
CREATE INDEX IX_Courses_DepartmentId   ON dbo.Courses (DepartmentId);
CREATE INDEX IX_Enrollments_CourseId   ON dbo.Enrollments (CourseId);
CREATE INDEX IX_Attendance_EnrollmentId ON dbo.Attendance (EnrollmentId);
GO

INSERT INTO dbo.Departments (Name, Code) VALUES
    (N'Computer Science', N'CS'),
    (N'Mathematics',      N'MATH'),
    (N'Business Administration', N'BUS');
GO

INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName) VALUES
    (NEWID(), N'Admin',   N'ADMIN'),
    (NEWID(), N'Teacher', N'TEACHER'),
    (NEWID(), N'Student', N'STUDENT');
GO

PRINT N'School Management System schema created successfully.';
GO