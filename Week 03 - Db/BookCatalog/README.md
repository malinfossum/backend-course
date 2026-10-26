# BookCatalog

Week 3: SQL Server, raw SQL, Dapper, swapping file persistence for a database.

The same API served two ways. `IBookRepository` never changes; only the class registered in
`Program.cs` does.

- `FileBookRepository` reads the whole JSON file and filters in C#. `SqlBookRepository` sends a
  `WHERE` clause and lets SQL Server return only the matching rows. That is the difference that matters
  once the table is large.
- Values always travel as parameters, separate from the SQL text. `ORDER BY` cannot be a parameter,
  so the two possible endings are compile time constants instead.
- One `SearchAsync` covers `?author=`, `?available=` and `?sort=year`, because "all books" is just a
  search with nothing filtered out.
- A new `SqlConnection` per call. The pool keeps the underlying connections open, so this returns one
  to the pool rather than holding it for the lifetime of the app.

## Running it

Needs a local SQL Server. From the repository root, create the database first:

```bash
sqlcmd -S localhost -E -C -b -i "Week 03 - Db/BookCatalog/Database/01-create-database.sql"
dotnet run --project "Week 03 - Db/BookCatalog/BookCatalog.Api"
```
