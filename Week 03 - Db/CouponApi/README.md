# CouponApi

Week 3: writing to the database: `IDENTITY`, constraints, rows affected, `Result<T>`.

Discount codes, and the week where the application starts changing the database rather than reading
it. One table, eight endpoints.

- The rule for using a coupon lives in the `WHERE` clause: `SET RemainingUses = RemainingUses - 1
  WHERE Id = @Id AND IsActive = 1 AND RemainingUses > 0`. Read and write are one statement, so two
  clients cannot both spend the last use. Four concurrent requests against a coupon with one use
  left give one 200 and three 409s.
- `rows affected` is honest but silent. Zero can mean the coupon does not exist, is deactivated or
  is used up, and the repository cannot tell which. So the service asks afterwards and turns the
  number into a sentence.
- Every rule is stated twice on purpose. The service explains it to the caller; `UNIQUE` and
  `CHECK (RemainingUses >= 0)` enforce it against writes that never pass through the service at all.
- `IsActive` and `RemainingUses` are separate rules: a reactivated coupon that is used up is active
  and still unusable.

## Running it

Needs a local SQL Server. From the repository root, create the database first:

```bash
sqlcmd -S localhost -E -C -b -i "Week 03 - Db/CouponApi/Database/01-create-database.sql"
dotnet run --project "Week 03 - Db/CouponApi/CouponApi.Api"
```

The script drops and reseeds its table, so the `.http` file always starts from the same three
coupons.
