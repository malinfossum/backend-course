using BookCatalog.Api.Models;

namespace BookCatalog.Api.Data;

// The contract the rest of the application depends on. It says nothing about
// JSON, files, SQL, Dapper or SQL Server — only what the endpoints need.
public interface IBookRepository
{
    // One method covers /books, ?author=, ?available= and ?sort=year, because
    // "all books" is just a search with nothing filtered out. A separate
    // GetAllAsync would be the same query with every argument set to null.
    Task<IEnumerable<Book>> SearchAsync(string? author, bool? isAvailable, bool sortByYear);

    Task<Book?> FindAsync(int id);
}
