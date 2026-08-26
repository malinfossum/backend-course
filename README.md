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

The auction project also serves a small web front end on the same address.

Stack: .NET 10, ASP.NET Core Minimal API, NUnit, Moq, Dapper, SQL Server.
