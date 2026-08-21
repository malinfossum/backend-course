# backend-kurs

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

## Running it

```bash
dotnet run --project "Week 01 - API/Auction/AuctionApi"
dotnet test "Week 02 - API/BookLoan/BookLoan.slnx"
dotnet test "Week 02 - API/CinemaBooking/CinemaBooking.slnx"
```

The auction project also serves a small web front end on the same address.

Stack: .NET 10, ASP.NET Core Minimal API, NUnit, Moq.
