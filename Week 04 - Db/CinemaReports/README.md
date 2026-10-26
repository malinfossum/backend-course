# CinemaReports

Week 4: reporting: `GROUP BY`, aggregates, `EXISTS`, `CASE`, indexes and execution plans.

SQL only again, and no application on purpose. The task asks for three endpoints, but two of them are
the third with a simpler query, and the session that follows walks one of them from request to JSON in
class. What is left is the part that is actually about SQL: turning four related tables into the
answers a user asks for.

- The report per screening needs both kinds of join at once. `Movies` is an `INNER JOIN`, because the
  foreign key already guarantees a screening has a film. `Reservations` has to be a `LEFT JOIN`,
  because the screening nobody booked is the row the report exists to show.
- `COUNT(r.Id)`, not `COUNT(*)`. It is the same trap as [`CourseDb`](../CourseDb/), met again in a
  different domain.
- `100.0 * COUNT(r.Id) / s.NumberOfSeats`, not `100 *`. Two ints divide as ints, so every occupancy
  would come back zero.
- "Customers who never booked" is written twice, with `NOT EXISTS` and with `LEFT JOIN ... IS NULL`.
  Same answer. `NOT EXISTS` says what is meant; the left join shows why it works.
- The seed writes its ids by hand. `DBCC CHECKIDENT(..., RESEED, 0)` on a table that has never held a
  row makes the *next* id `0` rather than `1`, which shifts every foreign key down one and fails with
  a `547`. `SET IDENTITY_INSERT` says exactly which id each row gets.
- The index is the interesting one. `SELECT *` filtered on the foreign key keeps scanning even at
  20 000 rows: a sixth of the table matches, so seeking and then fetching each row costs more than
  reading the table. The same filter under `COUNT(*)` seeks, because the index answers that question
  by itself. An index is used when it is cheaper than the alternative, and that depends on how much is
  asked for, not only on what is filtered.

## Running it

Needs a local SQL Server. The scripts run in order, and `02` can be run on its own to put the test
data back. From the repository root:

```bash
sqlcmd -S localhost -E -C -b -i "Week 04 - Db/CinemaReports/Database/01-create-database.sql"
sqlcmd -S localhost -E -C -b -W -s"|" -i "Week 04 - Db/CinemaReports/Database/02-seed.sql"
sqlcmd -S localhost -E -C -b -W -s"|" -i "Week 04 - Db/CinemaReports/Database/03-reports.sql"
sqlcmd -S localhost -E -C -b -i "Week 04 - Db/CinemaReports/Database/04-index.sql"
```

To see an execution plan without SSMS, ask `sqlcmd` for one. `SET SHOWPLAN_TEXT ON` returns the plan
instead of running the query, and has to sit in a batch of its own:

```sql
SET SHOWPLAN_TEXT ON;
GO
SELECT COUNT(*) FROM Reservations WHERE ScreeningId = 1;
GO
```
