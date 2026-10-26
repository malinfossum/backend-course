# BookLoan

Week 2: service classes, dependency injection, lifetimes.

The same loan service in three steps:

- `BookLoan.Basic.Console`: the service reads and writes `books.json` itself.
- `BookLoan.DI.Console`: the service gets an `IBookRepository` through its constructor, and the same
  scenario runs against `FileBookRepository` and `InMemoryBookRepository`.
- `BookLoan.API`: the container builds the service. `IBookRepository` and `BookLoanService` are
  registered with `AddScoped` in `Program.cs`, behind `POST /loans` and `POST /returns`.

`BookLoan.Tests` tests the service with NUnit against a hand written `FakeBookRepository`.

## Running it

From the repository root:

```bash
dotnet test "Week 02 - API/BookLoan/BookLoan.slnx"
dotnet run --project "Week 02 - API/BookLoan/BookLoan.API"
```
