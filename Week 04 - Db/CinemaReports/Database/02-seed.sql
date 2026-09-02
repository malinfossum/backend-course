-- Test data for the cinema reports.
--   sqlcmd -S localhost -E -C -b -W -s"|" -i "Database/02-seed.sql"
--
-- The set is small on purpose, but every case the report queries need is in it:
--
--   Arrival        - a movie with no screenings at all      (LEFT JOIN / NOT EXISTS)
--   Interstellar   - one movie across three screenings      (GROUP BY)
--   screening 5    - a screening nobody reserved            (COUNT(*) counts the NULL row as 1)
--   Ada Lovelace   - five reservations, two on one screening
--   Barbara Liskov - a registered customer who never booked (NOT EXISTS)
--   five guests    - CustomerId IS NULL, name on the row    (COALESCE)
--   occupancy      - 0 %, 20 %, 25 %, 50 % and 83 %, so CASE hits Low, Medium and High
--   Interstellar vs The Matrix - Ada and Alan saw both, Grace and Katherine only
--                                Interstellar, which is what INTERSECT and EXCEPT need
--
-- The ids are written out by hand because the reservations below refer to them.
-- Letting IDENTITY hand them out looks like it works and then does not: on a table
-- that has never held a row, DBCC CHECKIDENT(..., RESEED, 0) makes the *next* id 0
-- rather than 1, so everything shifts down one and the foreign keys break with a
-- 547. IDENTITY_INSERT says exactly which id each row gets, every time.

USE CinemaReportsDb;
GO

-- Children first, same reason the tables are dropped in that order.
DELETE FROM Reservations;
DELETE FROM Screenings;
DELETE FROM Customers;
DELETE FROM Movies;
GO

SET IDENTITY_INSERT Movies ON;
INSERT INTO Movies (Id, Title, DurationMinutes) VALUES
    (1, N'Interstellar', 169),
    (2, N'The Matrix',   136),
    (3, N'Alien',        117),
    (4, N'Arrival',      116);   -- never scheduled
SET IDENTITY_INSERT Movies OFF;
GO

SET IDENTITY_INSERT Screenings ON;
INSERT INTO Screenings (Id, MovieId, StartsAt, Auditorium, NumberOfSeats, TicketPrice) VALUES
    (1, 1, '2026-09-10T19:00:00', N'Sal 1', 20, 149.00),
    (2, 1, '2026-09-11T19:00:00', N'Sal 4',  6, 179.00),   -- minisal
    (3, 1, '2026-09-12T21:30:00', N'Sal 3',  8, 159.00),   -- loungesal
    (4, 2, '2026-09-11T20:00:00', N'Sal 4',  6, 129.00),
    (5, 3, '2026-09-12T18:00:00', N'Sal 2', 40,  99.00),   -- no reservations
    (6, 2, '2026-09-13T17:00:00', N'Sal 3',  8, 199.00);
SET IDENTITY_INSERT Screenings OFF;
GO

SET IDENTITY_INSERT Customers ON;
INSERT INTO Customers (Id, Name, Email) VALUES
    (1, N'Ada Lovelace',      N'ada@example.com'),
    (2, N'Grace Hopper',      N'grace@example.com'),
    (3, N'Alan Turing',       N'alan@example.com'),
    (4, N'Katherine Johnson', N'katherine@example.com'),
    (5, N'Barbara Liskov',    N'barbara@example.com');   -- never booked
SET IDENTITY_INSERT Customers OFF;
GO

INSERT INTO Reservations (ScreeningId, CustomerId, CustomerName, SeatNumber, ReservedUtc) VALUES
    -- Screening 1, Interstellar in Sal 1: 4 of 20 seats
    (1, 1,    NULL,                 4,  '2026-09-01T09:14:00'),
    (1, 2,    NULL,                 7,  '2026-09-01T10:02:00'),
    (1, NULL, N'Donald Knuth',      12, '2026-09-02T18:41:00'),
    (1, 3,    NULL,                 15, '2026-09-03T07:55:00'),

    -- Screening 2, Interstellar in Sal 4: 3 of 6 seats
    (2, 1,    NULL,                 1,  '2026-09-01T09:15:00'),
    (2, NULL, N'Edsger Dijkstra',   2,  '2026-09-04T20:10:00'),
    (2, 4,    NULL,                 3,  '2026-09-05T11:30:00'),

    -- Screening 3, Interstellar in Sal 3: 2 of 8 seats
    (3, 1,    NULL,                 1,  '2026-09-01T09:16:00'),
    (3, 2,    NULL,                 2,  '2026-09-06T13:20:00'),

    -- Screening 4, The Matrix in Sal 4: 5 of 6 seats. Ada takes two of them,
    -- which is why a count of reservations is not a count of people.
    (4, 1,    NULL,                 1,  '2026-09-02T08:00:00'),
    (4, 1,    NULL,                 5,  '2026-09-02T08:00:00'),
    (4, 3,    NULL,                 2,  '2026-09-02T19:45:00'),
    (4, NULL, N'Margaret Hamilton', 3,  '2026-09-03T12:12:00'),
    (4, NULL, N'Linus Torvalds',    4,  '2026-09-07T16:03:00'),

    -- Screening 5, Alien in Sal 2: nobody. Deliberately empty.

    -- Screening 6, The Matrix in Sal 3: 2 of 8 seats
    (6, 3,    NULL,                 1,  '2026-09-08T09:30:00'),
    (6, NULL, N'Ken Thompson',      2,  '2026-09-08T21:17:00');
GO

SELECT 'Movies' AS TableName, COUNT(*) AS [Rows] FROM Movies
UNION ALL SELECT 'Screenings',   COUNT(*) FROM Screenings
UNION ALL SELECT 'Customers',    COUNT(*) FROM Customers
UNION ALL SELECT 'Reservations', COUNT(*) FROM Reservations;
GO
