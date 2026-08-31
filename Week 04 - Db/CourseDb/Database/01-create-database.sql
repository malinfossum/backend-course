-- Week 4: the course administration database.
-- Run once against a local SQL Server instance:
--   sqlcmd -S localhost -E -C -b -i "Database/01-create-database.sql"
--
-- Students and Courses are many-to-many: a student takes several courses, and a
-- course holds several students. Neither table can hold the other side, so the
-- relationship gets a table of its own - Enrollments.

IF DB_ID('CourseDb') IS NULL
BEGIN
    CREATE DATABASE CourseDb;
END
GO

USE CourseDb;
GO

-- Dropped child first: Enrollments points at both parents, and a foreign key
-- refuses to be left hanging.
IF OBJECT_ID('dbo.Enrollments', 'U') IS NOT NULL DROP TABLE dbo.Enrollments;
IF OBJECT_ID('dbo.Students', 'U')    IS NOT NULL DROP TABLE dbo.Students;
IF OBJECT_ID('dbo.Courses', 'U')     IS NOT NULL DROP TABLE dbo.Courses;
GO

CREATE TABLE Students
(
    Id    INT           IDENTITY(1,1) PRIMARY KEY,
    Name  NVARCHAR(150) NOT NULL,
    Email NVARCHAR(200) NOT NULL,

    -- One person, one address. Without this the same student can be entered
    -- twice, and then "which row is the real Ada" has no answer.
    CONSTRAINT UQ_Students_Email UNIQUE (Email)
);
GO

CREATE TABLE Courses
(
    Id         INT           IDENTITY(1,1) PRIMARY KEY,
    Name       NVARCHAR(150) NOT NULL,
    CourseCode NVARCHAR(20)  NOT NULL,
    StartDate  DATE          NOT NULL,

    -- The code is what people type and what other systems refer to. If two
    -- courses can share it, it identifies nothing.
    CONSTRAINT UQ_Courses_CourseCode UNIQUE (CourseCode)
);
GO

CREATE TABLE Enrollments
(
    Id          INT      IDENTITY(1,1) PRIMARY KEY,
    StudentId   INT      NOT NULL,
    CourseId    INT      NOT NULL,

    -- Belongs here and nowhere else: the date describes this student on this
    -- course, not the student and not the course.
    EnrolledUtc DATETIME2 NOT NULL,

    CONSTRAINT FK_Enrollments_Students FOREIGN KEY (StudentId) REFERENCES Students(Id),
    CONSTRAINT FK_Enrollments_Courses  FOREIGN KEY (CourseId)  REFERENCES Courses(Id)
);
GO

INSERT INTO Students (Name, Email)
VALUES
    ('Ada Lovelace',      'ada@example.com'),
    ('Grace Hopper',      'grace@example.com'),
    ('Alan Turing',       'alan@example.com'),
    ('Margaret Hamilton', 'margaret@example.com');
GO

INSERT INTO Courses (Name, CourseCode, StartDate)
VALUES
    ('Backend Development', 'BACKEND',  '2026-08-10'),
    ('Software Testing',    'TESTING',  '2026-09-14'),
    ('Databases',           'DATABASE', '2026-10-05');
GO

-- Looked up by email and course code rather than by hard coded 1, 2, 3: IDENTITY
-- decides the numbers, and the seed should not have to guess them.
--
-- The data is shaped for the JOIN tasks that follow:
--   Ada, Grace and Margaret take two courses each  -> a student with many courses
--   Backend Development and Databases hold three   -> a course with many students
--   Alan Turing is enrolled in nothing             -> LEFT JOIN gives him NULL
--   Software Testing has nobody                    -> LEFT JOIN the other way
INSERT INTO Enrollments (StudentId, CourseId, EnrolledUtc)
VALUES
    ((SELECT Id FROM Students WHERE Email = 'ada@example.com'),      (SELECT Id FROM Courses WHERE CourseCode = 'BACKEND'),  '2026-07-01T09:00:00'),
    ((SELECT Id FROM Students WHERE Email = 'ada@example.com'),      (SELECT Id FROM Courses WHERE CourseCode = 'DATABASE'), '2026-07-01T09:05:00'),
    ((SELECT Id FROM Students WHERE Email = 'grace@example.com'),    (SELECT Id FROM Courses WHERE CourseCode = 'BACKEND'),  '2026-07-02T11:30:00'),
    ((SELECT Id FROM Students WHERE Email = 'grace@example.com'),    (SELECT Id FROM Courses WHERE CourseCode = 'DATABASE'), '2026-07-02T11:31:00'),
    ((SELECT Id FROM Students WHERE Email = 'margaret@example.com'), (SELECT Id FROM Courses WHERE CourseCode = 'BACKEND'),  '2026-07-03T08:15:00'),
    ((SELECT Id FROM Students WHERE Email = 'margaret@example.com'), (SELECT Id FROM Courses WHERE CourseCode = 'DATABASE'), '2026-08-20T14:45:00');
GO

SELECT * FROM Students;
SELECT * FROM Courses;
SELECT * FROM Enrollments;
GO
