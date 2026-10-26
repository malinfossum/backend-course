# CodeFirstOrders

Week 6: Entity Framework Code First: Fluent API, migrations, schema changes on a database that already
has data; transactions and ACID.

The direction reversed: the classes come first and the migrations build the database. The project is
the nine migrations under `Infrastructure/Migrations`, and what each one was before it was run; the
Wednesday half adds `ShippingService`, one business operation that is two writes.

## Migrations

- EF Core 10 recognises a rename. `Name` → `FullName` came out as `RenameColumn`, not the
  `DropColumn` + `AddColumn` the trap is usually described as. But it is matching old and new columns
  by type, name and position, and that is a guess: two same-typed renames with no name overlap came
  out crossed (`FullName` → `Beta`, `Country` → `Alpha`) with no warning at all. Reading `Up()` is
  not about catching data loss; it is about catching a wrong guess.
- The data-loss warning is only raised for `DropColumn`. A crossed rename is silent.
- `= "Created"` on the property is not a database default. EF put `DEFAULT N''` on `Status` and
  `DEFAULT 0` on `IsActive`, so a schema change would have deactivated every existing customer. The
  migration was edited to `defaultValue: true`.
- A required column on rows that already exist is three steps, not one: add it nullable, `UPDATE` the
  rows, then `AlterColumn` to `NOT NULL`. EF's one-liner leaves an empty-string default that lets
  future inserts skip the column too.
- `dotnet ef migrations script <from> <to>` prints the SQL a migration will run. That is the
  `ToQueryString()` of migrations, and it is more honest than the `migrationBuilder` calls.
- `1505`, a new number: `CREATE UNIQUE INDEX` over existing duplicates. `2627`/`2601` are an insert
  breaking a constraint; `1505` is creating the constraint when the data already breaks it.
- Conventions decide things that were never asked for: `Email` became `nvarchar(450)` because a unique
  index key is capped at 900 bytes, and the foreign key got `ON DELETE CASCADE`.

## Transactions

`Services/ShippingService.cs` holds three versions side by side. `?version=` and `&crash=true` on
`POST /shipments` replace pasting a `throw` between runs.

- Two `SaveChanges` and a crash between them: stock 8, no shipment, two keyboards gone. The same two
  inside `BeginTransactionAsync` with the crash: stock back at 10. The first `UPDATE` was sent but
  never committed.
- EF's log shows what "one `SaveChanges` is already a transaction" actually means: a `SaveChanges`
  with one statement gets no transaction at all (a single statement is atomic on its own); one with
  two statements gets `Began transaction` … `Committed transaction` around both. And inside an
  explicit transaction each `SaveChanges` creates a *savepoint* first.
- `CHECK (StockCount >= 0)` on the final one-`SaveChanges` version, with the stock validation
  bypassed: error 547, and the shipment `INSERT` in the same batch was rolled back with the failed
  `UPDATE`. Same number as an FK violation: 547 is "a constraint said no", not "which one".
- Reading a row another connection has updated but not committed: on `EfOrdersDb` (created by a
  script) the reader blocks and times out (1222) until the writer commits or rolls back. On this
  database it reads the last committed value at once. That is because EF Core turned on
  `READ_COMMITTED_SNAPSHOT` when it *created* the database, and none of the script-created ones have
  it. `WITH (NOLOCK)` shows the uncommitted value on both. Friday's topic, seen a day early.
- A failed statement does not end the transaction: after the 547 the transaction was still open and
  still holding the row lock. That is what the `catch → RollbackAsync` is for.
- EF picked `Cascade` for the new FK too; `Shipment` is history, so it was set to `Restrict` in the
  model and the migration regenerated rather than hand-edited (the snapshot has to agree).

## Running it

Needs a local SQL Server. The database is built from the migrations; the script only adds test data.
From the repository root:

```bash
dotnet ef database update --project "Week 06 - Db/CodeFirstOrders/CodeFirstOrders.Api"
sqlcmd -S localhost -E -C -b -i "Week 06 - Db/CodeFirstOrders/Database/seed.sql"
dotnet run --project "Week 06 - Db/CodeFirstOrders/CodeFirstOrders.Api"
```
