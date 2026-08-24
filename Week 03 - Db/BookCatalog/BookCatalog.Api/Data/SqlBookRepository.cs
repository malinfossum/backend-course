using BookCatalog.Api.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BookCatalog.Api.Data;

// Same contract as FileBookRepository, different mechanism. The filtering that
// FileBookRepository does in C# happens in the WHERE clause here, so SQL Server
// only sends back the rows that matched.
public class SqlBookRepository : IBookRepository
{
    private const string SelectBooks =
        """
        SELECT Id, Title, Author, [Year], IsAvailable
        FROM Books
        """;

    private readonly string _connectionString;

    public SqlBookRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("BookCatalog")
                            ?? throw new InvalidOperationException("Connection string mangler.");
    }

    public async Task<IEnumerable<Book>> SearchAsync(string? author, bool? isAvailable, bool sortByYear)
    {
        // Both filters are optional, and "@X IS NULL OR Column = @X" lets one
        // query handle every combination without pasting values into the SQL
        // string. ORDER BY cannot be a parameter, so the two possible endings
        // are constants — never something the caller can put text into.
        var sql = SelectBooks +
                  """

                  WHERE (@Author IS NULL OR Author = @Author)
                    AND (@IsAvailable IS NULL OR IsAvailable = @IsAvailable)
                  """ +
                  (sortByYear ? "\nORDER BY [Year];" : ";");

        await using var connection = new SqlConnection(_connectionString);

        return await connection.QueryAsync<Book>(
            sql,
            new { Author = author, IsAvailable = isAvailable });
    }

    public async Task<Book?> FindAsync(int id)
    {
        const string sql = SelectBooks +
                           """

                           WHERE Id = @Id;
                           """;

        // A new connection per call. It looks wasteful, but ADO.NET keeps a
        // pool of open connections behind the scenes, so this hands one back
        // to the pool instead of holding it for the lifetime of the app.
        await using var connection = new SqlConnection(_connectionString);

        // The value is sent separately from the SQL text. Interpolating it into
        // the string would be SQL injection waiting to happen, and would also
        // give the server a brand new query to plan on every call.
        return await connection.QuerySingleOrDefaultAsync<Book>(sql, new { Id = id });
    }
}
