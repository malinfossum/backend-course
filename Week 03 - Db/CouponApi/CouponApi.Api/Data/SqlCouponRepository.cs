using CouponApi.Api.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CouponApi.Api.Data;

// Every method here is one round trip: no coupon is read into C#, changed and
// written back. That matters most in TryUseAsync - see the comment there.
public class SqlCouponRepository : ICouponRepository
{
    private const string SelectCoupons =
        """
        SELECT Id, Code, Description, RemainingUses, IsActive
        FROM Coupons
        """;

    private readonly string _connectionString;

    public SqlCouponRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("CouponDb")
                            ?? throw new InvalidOperationException("Connection string mangler.");
    }

    public async Task<IEnumerable<Coupon>> GetAllAsync()
    {
        const string sql = SelectCoupons + ";";

        // A new connection per call. It looks wasteful, but ADO.NET keeps a
        // pool of open connections behind the scenes, so this hands one back
        // to the pool instead of holding it for the lifetime of the app.
        await using var connection = new SqlConnection(_connectionString);

        return await connection.QueryAsync<Coupon>(sql);
    }

    public async Task<Coupon?> FindAsync(int id)
    {
        const string sql = SelectCoupons +
                           """

                           WHERE Id = @Id;
                           """;

        await using var connection = new SqlConnection(_connectionString);

        // The value travels separately from the SQL text. Interpolating it into
        // the string would be SQL injection waiting to happen, and would also
        // give the server a brand new query to plan on every call.
        return await connection.QuerySingleOrDefaultAsync<Coupon>(sql, new { Id = id });
    }

    public async Task<Coupon?> FindByCodeAsync(string code)
    {
        const string sql = SelectCoupons +
                           """

                           WHERE Code = @Code;
                           """;

        await using var connection = new SqlConnection(_connectionString);

        return await connection.QuerySingleOrDefaultAsync<Coupon>(sql, new { Code = code });
    }

    public async Task<int> CreateAsync(Coupon coupon)
    {
        // OUTPUT INSERTED.Id makes the INSERT hand back the id IDENTITY just
        // generated, in the same round trip. Reading MAX(Id) afterwards would
        // be a second query and a lie the moment two clients insert at once.
        const string sql =
            """
            INSERT INTO Coupons (Code, Description, RemainingUses, IsActive)
            OUTPUT INSERTED.Id
            VALUES (@Code, @Description, @RemainingUses, @IsActive);
            """;

        await using var connection = new SqlConnection(_connectionString);

        return await connection.QuerySingleAsync<int>(sql, coupon);
    }

    public async Task<bool> TryUseAsync(int id)
    {
        // The whole rule sits in the WHERE clause, so the read and the write
        // are one statement. Doing SELECT -> RemainingUses-- -> UPDATE in C#
        // would leave a gap where another request could spend the last use
        // between the two, and both would succeed on the same one.
        const string sql =
            """
            UPDATE Coupons
            SET RemainingUses = RemainingUses - 1
            WHERE Id = @Id
              AND IsActive = 1
              AND RemainingUses > 0;
            """;

        await using var connection = new SqlConnection(_connectionString);

        // ExecuteAsync returns rows affected: 1 means the coupon was used,
        // 0 means nothing changed. Why nothing changed is not this layer's
        // business - it only saw that the UPDATE matched no row.
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

        return rowsAffected == 1;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        const string sql =
            """
            UPDATE Coupons
            SET IsActive = 0
            WHERE Id = @Id;
            """;

        await using var connection = new SqlConnection(_connectionString);

        // No condition beyond the id, so 0 rows can only mean "no such coupon".
        // SQL Server still counts a row it wrote the same value back to.
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

        return rowsAffected == 1;
    }

    public async Task<bool> ActivateAsync(int id)
    {
        const string sql =
            """
            UPDATE Coupons
            SET IsActive = 1
            WHERE Id = @Id;
            """;

        await using var connection = new SqlConnection(_connectionString);

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

        return rowsAffected == 1;
    }

    public async Task<bool> AddUsesAsync(int id, int amount)
    {
        // Same reasoning as TryUseAsync: the database adds to whatever the
        // current value is. Reading it into C# first would overwrite anything
        // that changed in between with a stale number.
        const string sql =
            """
            UPDATE Coupons
            SET RemainingUses = RemainingUses + @Amount
            WHERE Id = @Id;
            """;

        await using var connection = new SqlConnection(_connectionString);

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, Amount = amount });

        return rowsAffected == 1;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        // The WHERE is the difference between deleting one coupon and emptying
        // the table. Nothing in SQL Server warns about the version without it.
        const string sql =
            """
            DELETE FROM Coupons
            WHERE Id = @Id;
            """;

        await using var connection = new SqlConnection(_connectionString);

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

        return rowsAffected == 1;
    }
}
