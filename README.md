# Backend-course

My own solutions to the tasks from the backend course in GET Prepared. One folder per week, mirroring
the structure of the course material.

Everything here is written from scratch against the task descriptions — the reference solutions handed
out by the school are deliberately not in this repository.

## Contents

| Week | Project | Topic |
|------|---------|-------|
| Week 01 – API | `Auction` | Minimal API, DTOs, `.http` files, JSON file persistence, async |
| Week 02 – API | `BookLoan` | Service classes, dependency injection, lifetimes |
| Week 02 – API | `CinemaBooking` | Unit testing with NUnit, fakes and mocks, `Result<T>` |
| Week 03 – Db | `BookCatalog` | SQL Server, raw SQL, Dapper, swapping file persistence for a database |
| Week 03 – Db | `CouponApi` | Writing to the database: `IDENTITY`, constraints, rows affected, `Result<T>` |
| Week 04 – Db | `CourseDb` | Relations: primary and foreign keys, many-to-many, `INNER JOIN`, `LEFT JOIN` |
| Week 04 – Db | `CinemaReports` | Reporting: `GROUP BY`, aggregates, `EXISTS`, `CASE`, indexes and execution plans |

### CinemaBooking

The most complete of the three, and the one worth reading first.

- `Result<T>` carries an `ErrorKind`, so the service can say *what kind* of failure it was without
  knowing anything about HTTP. The endpoint is the only place that turns that into 404, 409 or 400.
- A screening tracks reservations with an owner, which is what both cancellation and the
  "at most four seats per customer" rule need.
- The service is covered twice: once with a hand written fake, once with Moq, kept side by side so
  the two styles can be compared.
- The fake hands out and stores copies, the way the file repository does. While it returned the same
  instance it stored, a test could assert on state and stay green even when the service forgot to
  save — the service and the test were mutating the same object.

### BookCatalog

The same API served two ways. `IBookRepository` never changes; only the class registered in
`Program.cs` does.

- `FileBookRepository` reads the whole JSON file and filters in C#. `SqlBookRepository` sends a
  `WHERE` clause and lets SQL Server return only the matching rows — the difference that matters
  once the table is large.
- Values always travel as parameters, separate from the SQL text. `ORDER BY` cannot be a parameter,
  so the two possible endings are compile time constants instead.
- One `SearchAsync` covers `?author=`, `?available=` and `?sort=year`, because "all books" is just a
  search with nothing filtered out.
- A new `SqlConnection` per call. The pool keeps the underlying connections open, so this returns one
  to the pool rather than holding it for the lifetime of the app.

### CouponApi

Discount codes, and the week where the application starts changing the database rather than reading
it. One table, eight endpoints.

- The rule for using a coupon lives in the `WHERE` clause: `SET RemainingUses = RemainingUses - 1
  WHERE Id = @Id AND IsActive = 1 AND RemainingUses > 0`. Read and write are one statement, so two
  clients cannot both spend the last use. Four concurrent requests against a coupon with one use
  left give one 200 and three 409s.
- `rows affected` is honest but silent. Zero can mean the coupon does not exist, is deactivated or
  is used up, and the repository cannot tell which — so the service asks afterwards and turns the
  number into a sentence.
- Every rule is stated twice on purpose. The service explains it to the caller; `UNIQUE` and
  `CHECK (RemainingUses >= 0)` enforce it against writes that never pass through the service at all.
- `IsActive` and `RemainingUses` are separate rules: a reactivated coupon that is used up is active
  and still unusable.

### CourseDb

No application this week — a course administration database, and five SQL scripts that build it and
then ask it questions. Students and courses are many-to-many, so the relationship gets a table:
`Enrollments`.

- `Enrollments` is not just two foreign keys. `EnrolledUtc` and `Status` describe *this student on
  this course* — neither value has anywhere else to live, which is what makes the join table a real
  table rather than plumbing.
- `INNER JOIN` drops the student who is enrolled in nothing; `LEFT JOIN` keeps him with `NULL` on the
  other side. Adding `Courses` has to be a `LEFT JOIN` too, or he is thrown out again on the second
  join.
- Counting students per course needs `COUNT(e.Id)`, not `COUNT(*)`. The left join hands `COUNT(*)` one
  row of `NULL`s for the empty course, so it reports 1 where the answer is 0.
- Duplicate enrollments are stopped by `UNIQUE (StudentId, CourseId)`. Neither column is unique alone
  — both are supposed to repeat — it is the pair that may occur only once.
- `547` is not one error. Foreign key on insert, foreign key on delete and `CHECK` all report it;
  only `UNIQUE` gets its own number, `2627`. The constraint name in the message is what tells them
  apart, which is the argument for naming them.

### CinemaReports

SQL only again, and no application on purpose. The task asks for three endpoints, but two of them are
the third with a simpler query, and the session that follows walks one of them from request to JSON in
class. What is left is the part that is actually about SQL: turning four related tables into the
answers a user asks for.

- The report per screening needs both kinds of join at once. `Movies` is an `INNER JOIN`, because the
  foreign key already guarantees a screening has a film. `Reservations` has to be a `LEFT JOIN`,
  because the screening nobody booked is the row the report exists to show.
- `COUNT(r.Id)`, not `COUNT(*)` — the same trap as `CourseDb`, met again in a different domain.
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
  asked for — not only on what is filtered.

## Running it

```bash
dotnet run --project "Week 01 - API/Auction/AuctionApi"
dotnet test "Week 02 - API/BookLoan/BookLoan.slnx"
dotnet test "Week 02 - API/CinemaBooking/CinemaBooking.slnx"
dotnet run --project "Week 03 - Db/BookCatalog/BookCatalog.Api"
dotnet run --project "Week 03 - Db/CouponApi/CouponApi.Api"
```

`BookCatalog` and `CouponApi` need a local SQL Server. Create the databases first:

```bash
sqlcmd -S localhost -E -C -b -i "Week 03 - Db/BookCatalog/Database/01-create-database.sql"
sqlcmd -S localhost -E -C -b -i "Week 03 - Db/CouponApi/Database/01-create-database.sql"
```

`CouponApi`'s script drops and reseeds its table, so the `.http` file always starts from the same
three coupons.

Week 4 is SQL only. The scripts run in order, and `01` drops and reseeds, so `04` and `05` have to be
run again after it:

```bash
sqlcmd -S localhost -E -C -b -i "Week 04 - Db/CourseDb/Database/01-create-database.sql"
sqlcmd -S localhost -E -C -b -W -s"|" -i "Week 04 - Db/CourseDb/Database/02-joins.sql"
sqlcmd -S localhost -E -C -b -W -s"|" -i "Week 04 - Db/CourseDb/Database/03-constraints.sql"
sqlcmd -S localhost -E -C -b -W -s"|" -i "Week 04 - Db/CourseDb/Database/04-status.sql"
sqlcmd -S localhost -E -C -b -W -s"|" -i "Week 04 - Db/CourseDb/Database/05-challenges.sql"
```

`03` is meant to fail: three statements the foreign keys refuse, caught and printed so the whole
script still runs.

`CinemaReports` runs the same way, and `02` can be run on its own to put the test data back:

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

The auction project also serves a small web front end on the same address.

Stack: .NET 10, ASP.NET Core Minimal API, NUnit, Moq, Dapper, SQL Server.
