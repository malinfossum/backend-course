using System.Text.Json;
using BookLoan.API.DomainModel;
using BookLoan.API.DomainServices;

namespace BookLoan.API.Infrastructure;

public class FileBookRepository : IBookRepository
{
    private static readonly string FilePath =
        Path.Combine(AppContext.BaseDirectory, "books.json");

    private static readonly JsonSerializerOptions JsonOptions =
        new JsonSerializerOptions
        {
            WriteIndented = true
        };

    public Book? Get(int id)
    {
        var books = ReadAll();

        return books.FirstOrDefault(book => book.Id == id);
    }

    public void Update(Book book)
    {
        var books = ReadAll();

        var index = books.FindIndex(existing => existing.Id == book.Id);

        if (index == -1)
        {
            throw new InvalidOperationException(
                $"Fant ikke boka med id {book.Id}.");
        }

        books[index] = book;

        var json = JsonSerializer.Serialize(books, JsonOptions);

        File.WriteAllText(FilePath, json);
    }

    private static List<Book> ReadAll()
    {
        var json = File.ReadAllText(FilePath);

        return JsonSerializer.Deserialize<List<Book>>(json)
               ?? new List<Book>();
    }
}
