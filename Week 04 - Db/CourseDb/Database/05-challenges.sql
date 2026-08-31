-- Week 4, the four challenges.
--   sqlcmd -S localhost -E -C -b -W -s"|" -i "Database/05-challenges.sql"
--
-- Challenge 4 has no SQL: it is the argument for why Enrollments exists at all,
-- and it is written out in NOTES.md instead.

USE CourseDb;
GO

-- Challenge 1. Nothing so far stops the same enrollment being entered twice.
-- The rule is not "StudentId is unique" or "CourseId is unique" - both repeat
-- on purpose - it is the pair that may only occur once. UNIQUE takes a column
-- list, so the pair is what gets the constraint.
IF OBJECT_ID('dbo.UQ_Enrollments_StudentCourse', 'UQ') IS NULL
BEGIN
    ALTER TABLE Enrollments
        ADD CONSTRAINT UQ_Enrollments_StudentCourse UNIQUE (StudentId, CourseId);
END
GO

PRINT '--- Challenge 1: enrolling Ada on Databases a second time ---';
BEGIN TRY
    INSERT INTO Enrollments (StudentId, CourseId, EnrolledUtc)
    VALUES ((SELECT Id FROM Students WHERE Email = 'ada@example.com'),
            (SELECT Id FROM Courses  WHERE CourseCode = 'DATABASE'),
            SYSUTCDATETIME());

    PRINT 'The duplicate went in, which would mean the constraint is missing.';
END TRY
BEGIN CATCH
    PRINT CONCAT('Msg ', ERROR_NUMBER(), ': ', ERROR_MESSAGE());
END CATCH
GO

-- Challenge 2. Students per course, empty courses included.
--
-- Two details decide the answer: the join must be LEFT, or Software Testing
-- disappears; and it must be COUNT(e.Id), not COUNT(*). COUNT(*) counts rows,
-- and the LEFT JOIN hands it one row of NULLs for the empty course - so it
-- would report 1. COUNT of a column skips NULL and reports 0.
SELECT c.Name AS CourseName,
       COUNT(e.Id) AS StudentCount
FROM Courses c
LEFT JOIN Enrollments e ON c.Id = e.CourseId
GROUP BY c.Name
ORDER BY StudentCount DESC, c.Name;
GO

-- Challenge 3. Students enrolled in nothing. The LEFT JOIN already produces
-- their NULL row; the only new part is filtering on it. A NULL on the right
-- side is precisely the evidence that no match existed.
SELECT s.Name AS StudentName, s.Email
FROM Students s
LEFT JOIN Enrollments e ON s.Id = e.StudentId
WHERE e.Id IS NULL
ORDER BY s.Name;
GO

-- The same question said differently: not "join and throw away the matches" but
-- "keep the students for whom no enrollment exists". Same rows, and this one
-- says out loud what is being asked.
SELECT s.Name AS StudentName, s.Email
FROM Students s
WHERE NOT EXISTS (SELECT 1 FROM Enrollments e WHERE e.StudentId = s.Id)
ORDER BY s.Name;
GO
