using System.Text.Json;
using BookCatalog.Api.Models;

namespace BookCatalog.Api.Data;

// Reads the whole file on every call, turns it into Book objects, and then
// filters in C#. That last part is the one to compare against SqlBookRepository:
// six books are read from disk even when the answer is a single book.
public class FileBookRepository : IBookRepository
{
    private readonly string _filePath;

    public FileBookRepository(IWebHostEnvironment environment)
    {
        // The task description says "data/books.json". On Windows that folder
        // and the "Data" folder holding the repositories are the same folder,
        // so the file lives in "Data" and the path spells it that way.
        _filePath = Path.Combine(environment.ContentRootPath, "Data", "books.json");
    }

    public async Task<IEnumerable<Book>> SearchAsync(string? author, bool? isAvailable, bool sortByYear)
    {
        IEnumerable<Book> books = await LoadBooksAsync();

        if (!string.IsNullOrWhiteSpace(author))
        {
            books = books.Where(book =>
                book.Author.Equals(author, StringComparison.OrdinalIgnoreCase));
        }

        if (isAvailable.HasValue)
        {
            books = books.Where(book => book.IsAvailable == isAvailable.Value);
        }

        if (sortByYear)
        {
            books = books.OrderBy(book => book.Year);
        }

        return books.ToList();
    }

    public async Task<Book?> FindAsync(int id)
    {
        var books = await LoadBooksAsync();

        return books.FirstOrDefault(book => book.Id == id);
    }

    private async Task<List<Book>> LoadBooksAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new List<Book>();
        }

        var json = await File.ReadAllTextAsync(_filePath);

        return JsonSerializer.Deserialize<List<Book>>(json) ?? new List<Book>();
    }
}
