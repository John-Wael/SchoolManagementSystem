ALTER TABLE [dbo].[Courses]
ADD [IsEnrollmentOpen] BIT NOT NULL DEFAULT (0);

CREATE TABLE [dbo].[SystemSettings] (
    [SettingId] INT NOT NULL PRIMARY KEY,
    [CurrentSemester] NVARCHAR(20) NOT NULL,
    [CurrentAcademicYear] NVARCHAR(9) NOT NULL
);

INSERT INTO [dbo].[SystemSettings] ([SettingId], [CurrentSemester], [CurrentAcademicYear])
VALUES (1, 'Fall', '2026-2027');