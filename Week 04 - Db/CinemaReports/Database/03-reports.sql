-- Report queries.
--   sqlcmd -S localhost -E -C -b -W -s"|" -i "Database/03-reports.sql"

USE CinemaReportsDb;
GO

-- Part 5, 6, 7, 12 and 15: one report per screening - how many booked, how full,
-- what it earned, and which band that puts it in.
--
-- Two joins doing two different jobs. Movies is an INNER JOIN because a screening
-- without a movie cannot exist - the foreign key says so. Reservations has to be a
-- LEFT JOIN, because the screening nobody booked is exactly the row the report is
-- about, and an INNER JOIN would drop it.
--
-- COUNT(r.Id) rather than COUNT(*): the LEFT JOIN hands the empty screening one row
-- with every Reservations column set to NULL. COUNT(*) counts that row and reports 1.
-- COUNT of a column skips NULLs, so it reports 0 - which is the true answer.
--
-- 100.0 rather than 100: two INTs divide as INTs, so 4/20 would be 0 and every
-- occupancy would come back zero. One decimal literal makes the whole thing decimal.
SELECT
    m.Title         AS MovieTitle,
    s.StartsAt,
    s.NumberOfSeats,
    s.TicketPrice,

    COUNT(r.Id)     AS Reservations,

    CAST(100.0 * COUNT(r.Id) / s.NumberOfSeats AS DECIMAL(5,1))
                    AS OccupancyPercent,

    COUNT(r.Id) * s.TicketPrice
                    AS Revenue,

    CASE
        WHEN 100.0 * COUNT(r.Id) / s.NumberOfSeats <= 25 THEN 'Low'
        WHEN 100.0 * COUNT(r.Id) / s.NumberOfSeats <= 75 THEN 'Medium'
        ELSE 'High'
    END             AS OccupancyBand

FROM Screenings s

JOIN Movies m
    ON m.Id = s.MovieId

LEFT JOIN Reservations r
    ON r.ScreeningId = s.Id

-- Grouped by the screening itself, not by the title: Interstellar has three
-- screenings, and each one is its own line. Everything selected that is not
-- inside an aggregate has to be listed here - the group has to be able to
-- answer with one value.
GROUP BY s.Id, m.Title, s.StartsAt, s.NumberOfSeats, s.TicketPrice

ORDER BY OccupancyPercent DESC, s.StartsAt;
GO

-- Part 8, 9, 10 and 11: registered customers who have never made a reservation.
-- The same question asked two ways, on purpose.

-- (a) NOT EXISTS - "is there any reservation belonging to this customer?"
--     SELECT 1 because the subquery's columns are never read. The question is
--     whether a row comes back at all, not what is in it.
SELECT
    c.Id,
    c.Name,
    c.Email

FROM Customers c

WHERE NOT EXISTS
(
    SELECT 1
    FROM Reservations r
    WHERE r.CustomerId = c.Id
)

ORDER BY c.Name;
GO

-- (b) LEFT JOIN + IS NULL - join everything, then keep only the rows where the
--     join found nothing. A LEFT JOIN with no match still returns the left row,
--     with the right side filled with NULL. r.Id IS NULL is therefore the same
--     sentence as "this customer has no reservations".
--
--     Same answer, different route. Worth knowing both: NOT EXISTS says what you
--     mean, the LEFT JOIN shows you why it works.
SELECT
    c.Id,
    c.Name,
    c.Email

FROM Customers c

LEFT JOIN Reservations r
    ON r.CustomerId = c.Id

WHERE r.Id IS NULL

ORDER BY c.Name;
GO
