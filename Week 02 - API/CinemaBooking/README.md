# CinemaBooking

Week 2: unit testing with NUnit, fakes and mocks, `Result<T>`.

The most complete of the week 1 and 2 projects, and the one worth reading first.

- `Result<T>` carries an `ErrorKind`, so the service can say *what kind* of failure it was without
  knowing anything about HTTP. The endpoint is the only place that turns that into 404, 409 or 400.
- A screening tracks reservations with an owner, which is what both cancellation and the
  "at most four seats per customer" rule need.
- The service is covered twice: once with a hand written fake, once with Moq, kept side by side so
  the two styles can be compared.
- The fake hands out and stores copies, the way the file repository does. While it returned the same
  instance it stored, a test could assert on state and stay green even when the service forgot to
  save. The service and the test were mutating the same object.

## Running it

From the repository root:

```bash
dotnet test "Week 02 - API/CinemaBooking/CinemaBooking.slnx"
dotnet run --project "Week 02 - API/CinemaBooking/CinemaBooking.Api"
```
