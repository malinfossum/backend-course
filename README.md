# Backend Course

My own solutions to the tasks in Backend Light, the ten-week backend course in GET Prepared (August to October 2026).

Everything here is written from scratch against the task descriptions. The reference solutions handed out by the school are deliberately not in this repository.

## Stack

.NET 10, ASP.NET Core Minimal API, NUnit, Moq, Dapper, Entity Framework Core, SQL Server.

## Contents

One folder per week, mirroring the course material. Each project has its own README with what I found and how to run it.

| Week | Project | Topic |
|------|---------|-------|
| 1 | [Auction](Week%2001%20-%20API/Auction/) | Minimal API, DTOs, `.http` files, JSON file persistence, async |
| 2 | [BookLoan](Week%2002%20-%20API/BookLoan/) | Service classes, dependency injection, lifetimes |
| 2 | [CinemaBooking](Week%2002%20-%20API/CinemaBooking/) | Unit testing with NUnit, fakes and mocks, `Result<T>` |
| 3 | [BookCatalog](Week%2003%20-%20Db/BookCatalog/) | SQL Server, raw SQL, Dapper, swapping file persistence for a database |
| 3 | [CouponApi](Week%2003%20-%20Db/CouponApi/) | Writing to the database: `IDENTITY`, constraints, rows affected |
| 4 | [CourseDb](Week%2004%20-%20Db/CourseDb/) | Relations: keys, many-to-many, `INNER JOIN`, `LEFT JOIN` (SQL only) |
| 4 | [CinemaReports](Week%2004%20-%20Db/CinemaReports/) | Reporting: `GROUP BY`, aggregates, `EXISTS`, `CASE`, indexes (SQL only) |
| 5 | [SupportTickets](Week%2005%20-%20Db/SupportTickets/) | Entity Framework Database First, LINQ translated to SQL, `Include` |
| 6 | [CodeFirstOrders](Week%2006%20-%20Db/CodeFirstOrders/) | Entity Framework Code First, migrations on live data, transactions and ACID |
| 7 and 8 | [devops-course](https://github.com/malinfossum/devops-course) | Containers, Compose, CI with GitHub Actions, GHCR, deploy and rollback. Taught as a separate mini course, so it has its own repository |
| 9 | [Hugin #61](https://github.com/malinfossum/hugin/pull/61) | Architecture: Rich Domain Model, Value Objects, Domain Events. No task to hand in, so I applied it to my own project |
| 10 | None | Functional Core / Imperative Shell, pure functions, `Result` composition. No task code in this repository |

## Running it

Every C# project has a `.slnx` and builds with `dotnet build`. The two test suites:

```bash
dotnet test "Week 02 - API/BookLoan/BookLoan.slnx"
dotnet test "Week 02 - API/CinemaBooking/CinemaBooking.slnx"
```

Weeks 3 to 6 need a local SQL Server. Each project README has the script that creates its database.
