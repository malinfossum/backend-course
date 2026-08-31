-- Week 4, part 2: putting the tables back together.
--   sqlcmd -S localhost -E -C -b -W -s"|" -i "Database/02-joins.sql"
--
-- Splitting the data into three tables was step one. A JOIN is how the answer is
-- assembled again on the way out. The repetition in the result - Ada twice, one
-- row per course - is produced by the query, not stored anywhere.

USE CourseDb;
GO

-- 12. Student + Enrollment. Enrollments only holds numbers; the join is what
-- turns StudentId = 1 back into a name.
SELECT s.Name AS StudentName,
       e.CourseId,
       e.EnrolledUtc
FROM Students s
INNER JOIN Enrollments e ON s.Id = e.StudentId
ORDER BY s.Name, e.EnrolledUtc;
GO

-- 13. Student + Enrollment + Course. Three tables, one operation - the database
-- picks the execution order, this is not "one join and then another".
SELECT s.Name AS StudentName,
       c.Name AS CourseName,
       e.EnrolledUtc
FROM Students s
INNER JOIN Enrollments e ON s.Id = e.StudentId
INNER JOIN Courses c     ON c.Id = e.CourseId
ORDER BY s.Name, c.Name;
GO

-- 14a. The courses one student takes, found by name.
SELECT c.Name AS CourseName, e.EnrolledUtc
FROM Students s
INNER JOIN Enrollments e ON s.Id = e.StudentId
INNER JOIN Courses c     ON c.Id = e.CourseId
WHERE s.Name = 'Ada Lovelace'
ORDER BY c.Name;
GO

-- 14b. The same question by Id. The name can be misspelled, shared by two
-- people or changed after a marriage; the Id is handed out by the database and
-- never means anyone else. Id is the robust identifier - the name is for humans.
DECLARE @adaId INT = (SELECT Id FROM Students WHERE Email = 'ada@example.com');

SELECT c.Name AS CourseName, e.EnrolledUtc
FROM Enrollments e
INNER JOIN Courses c ON c.Id = e.CourseId
WHERE e.StudentId = @adaId
ORDER BY c.Name;
GO

-- 15. The other direction: the students on one course.
SELECT s.Name AS StudentName, s.Email, e.EnrolledUtc
FROM Courses c
INNER JOIN Enrollments e ON c.Id = e.CourseId
INNER JOIN Students s    ON s.Id = e.StudentId
WHERE c.CourseCode = 'BACKEND'
ORDER BY s.Name;
GO

-- 16. INNER JOIN over Students and Enrollments. Alan Turing is not in this
-- result: an inner join keeps a row only where the other side has a match, and
-- he has no enrollment to match against. Three names, six rows.
SELECT s.Name AS StudentName, e.Id AS EnrollmentId
FROM Students s
INNER JOIN Enrollments e ON s.Id = e.StudentId
ORDER BY s.Name;
GO

-- 17. LEFT JOIN keeps every row from the left table whether or not the right
-- side matched. Alan appears, and the Enrollments columns are NULL - the join
-- found nothing to put there, which is different from an empty string or a zero.
SELECT s.Name AS StudentName, e.Id AS EnrollmentId, e.CourseId, e.EnrolledUtc
FROM Students s
LEFT JOIN Enrollments e ON s.Id = e.StudentId
ORDER BY s.Name;
GO

-- 18. Same query with the course name added. Courses has to be joined with LEFT
-- JOIN as well: an INNER JOIN here would throw Alan out again, because his NULL
-- CourseId matches no course.
SELECT s.Name AS StudentName, c.Name AS CourseName
FROM Students s
LEFT JOIN Enrollments e ON s.Id = e.StudentId
LEFT JOIN Courses c     ON c.Id = e.CourseId
ORDER BY s.Name, c.Name;
GO

-- 19. Mirror image. The table that must survive the join goes on the left, so
-- listing every course - including the empty one - starts from Courses.
SELECT c.Name AS CourseName, s.Name AS StudentName
FROM Courses c
LEFT JOIN Enrollments e ON c.Id = e.CourseId
LEFT JOIN Students s    ON s.Id = e.StudentId
ORDER BY c.Name, s.Name;
GO
