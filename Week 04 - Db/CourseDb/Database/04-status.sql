-- Week 4, part 4: a value that belongs on the join table.
--   sqlcmd -S localhost -E -C -b -W -s"|" -i "Database/04-status.sql"
--
-- Status is not a property of the student - she can be active on one course and
-- have dropped another - and not a property of the course either. It describes
-- this student on this course, which is exactly what an Enrollment row is.

USE CourseDb;
GO

IF COL_LENGTH('dbo.Enrollments', 'Status') IS NULL
BEGIN
    -- The default is what fills the six rows that already exist; without it the
    -- column cannot be NOT NULL on a table that is not empty.
    ALTER TABLE Enrollments
        ADD Status NVARCHAR(20) NOT NULL
            CONSTRAINT DF_Enrollments_Status DEFAULT 'Active';
END
GO

-- Free text would let 'active', 'Aktiv' and 'compleeted' all in, and then no
-- query can be trusted. Three spellings, checked by the database.
IF OBJECT_ID('dbo.CK_Enrollments_Status', 'C') IS NULL
BEGIN
    ALTER TABLE Enrollments
        ADD CONSTRAINT CK_Enrollments_Status
            CHECK (Status IN ('Active', 'Completed', 'Dropped'));
END
GO

-- Ada finished Databases, Grace dropped it, everyone else is still going.
UPDATE e
SET Status = 'Completed'
FROM Enrollments e
INNER JOIN Students s ON s.Id = e.StudentId
INNER JOIN Courses c  ON c.Id = e.CourseId
WHERE s.Email = 'ada@example.com' AND c.CourseCode = 'DATABASE';

UPDATE e
SET Status = 'Dropped'
FROM Enrollments e
INNER JOIN Students s ON s.Id = e.StudentId
INNER JOIN Courses c  ON c.Id = e.CourseId
WHERE s.Email = 'grace@example.com' AND c.CourseCode = 'DATABASE';
GO

-- 27. Student, course and the status of that pairing.
SELECT s.Name AS StudentName,
       c.Name AS CourseName,
       e.Status
FROM Enrollments e
INNER JOIN Students s ON s.Id = e.StudentId
INNER JOIN Courses c  ON c.Id = e.CourseId
ORDER BY s.Name, c.Name;
GO

-- The status the CHECK constraint will not accept.
PRINT '--- A status outside the three allowed ones ---';
BEGIN TRY
    UPDATE Enrollments SET Status = 'Paused' WHERE Id = 1;
    PRINT 'The update went through, which would mean the CHECK is missing.';
END TRY
BEGIN CATCH
    PRINT CONCAT('Msg ', ERROR_NUMBER(), ': ', ERROR_MESSAGE());
END CATCH
GO
