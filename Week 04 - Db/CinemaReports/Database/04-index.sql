-- Part 17 and 18: an index on the foreign key, and what it does to the plan.
--   sqlcmd -S localhost -E -C -b -i "Database/04-index.sql"
--
-- The API asks this constantly - every time someone opens a screening:
--
--   SELECT * FROM Reservations WHERE ScreeningId = @ScreeningId;
--
-- PRIMARY KEY gets an index for free. FOREIGN KEY does not. Without one, SQL
-- Server has no shortcut to "the reservations for screening 3" and has to walk
-- the table. ScreeningId is a good candidate precisely because the query above
-- runs on every page load.
--
-- The cost sits on the other side: every INSERT of a reservation - every ticket
-- sold - now has to write the index too. That is the trade, and it is why we do
-- not index every column.

USE CinemaReportsDb;
GO

IF INDEXPROPERTY(OBJECT_ID('dbo.Reservations'), 'IX_Reservations_ScreeningId', 'IndexID') IS NULL
BEGIN
    CREATE INDEX IX_Reservations_ScreeningId
        ON Reservations(ScreeningId);
END
GO

-- To see the plan without SSMS, ask sqlcmd for it. SET SHOWPLAN_TEXT returns the
-- plan instead of running the query, and has to sit in a batch of its own:
--
--   SET SHOWPLAN_TEXT ON;
--   GO
--   SELECT * FROM Reservations WHERE ScreeningId = 1;
--   GO
--
-- With sixteen rows SQL Server may well keep scanning anyway - reading the whole
-- table is cheaper than consulting an index and then fetching the rows. That is
-- not a fault. It is the optimizer costing the two plans and picking one.
