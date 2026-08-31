-- Week 4, part 3: what the foreign keys actually refuse.
--   sqlcmd -S localhost -E -C -b -W -s"|" -i "Database/03-constraints.sql"
--
-- Every statement below is meant to fail. TRY/CATCH prints the error instead of
-- stopping the script, so all three attempts can be seen in one run - and the
-- row count afterwards shows the table is untouched.

USE CourseDb;
GO

PRINT '--- 20. Enrollment pointing at a student who does not exist ---';
BEGIN TRY
    INSERT INTO Enrollments (StudentId, CourseId, EnrolledUtc)
    VALUES (99999, 1, SYSUTCDATETIME());

    PRINT 'The insert went through. That would mean the foreign key is missing.';
END TRY
BEGIN CATCH
    PRINT CONCAT('Msg ', ERROR_NUMBER(), ': ', ERROR_MESSAGE());
END CATCH
GO

PRINT '--- 21. Enrollment pointing at a course that does not exist ---';
BEGIN TRY
    INSERT INTO Enrollments (StudentId, CourseId, EnrolledUtc)
    VALUES ((SELECT Id FROM Students WHERE Email = 'ada@example.com'), 99999, SYSUTCDATETIME());

    PRINT 'The insert went through. That would mean the foreign key is missing.';
END TRY
BEGIN CATCH
    PRINT CONCAT('Msg ', ERROR_NUMBER(), ': ', ERROR_MESSAGE());
END CATCH
GO

PRINT '--- 22. Deleting a student who still has enrollments ---';
BEGIN TRY
    DELETE FROM Students
    WHERE Email = 'ada@example.com';

    PRINT 'The delete went through, and the enrollments now point at nobody.';
END TRY
BEGIN CATCH
    PRINT CONCAT('Msg ', ERROR_NUMBER(), ': ', ERROR_MESSAGE());
END CATCH
GO

-- The same delete succeeds for the student nobody references. Not because the
-- rule is weaker for him, but because there is nothing left pointing at his row.
-- Rolled back, since the later tasks still need four students.
PRINT '--- 22b. Deleting the student with no enrollments (rolled back) ---';
BEGIN TRANSACTION;
    DELETE FROM Students WHERE Email = 'alan@example.com';
    PRINT CONCAT('Rows deleted: ', @@ROWCOUNT);
ROLLBACK TRANSACTION;
GO

SELECT (SELECT COUNT(*) FROM Students)    AS Students,
       (SELECT COUNT(*) FROM Courses)     AS Courses,
       (SELECT COUNT(*) FROM Enrollments) AS Enrollments;
GO
