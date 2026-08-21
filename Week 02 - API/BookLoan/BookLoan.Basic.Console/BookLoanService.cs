using System.Text.Json;
using BookLoan.Basic.Model;

namespace BookLoan.Basic;

public class BookLoanService
{
    private const string FilePath = "books.json";

    public void BorrowBook(int bookId, string userName)
    {
        var json = File.ReadAllText(FilePath);

        var books =
            JsonSerializer.Deserialize<List<Book>>(json)
            ?? new List<Book>();

        var book = books.FirstOrDefault(book => book.Id == bookId);

        if (book == null)
        {
            throw new Exception("The book does not exist.");
        }

        if (book.BorrowedBy != null)
        {
            throw new Exception("The book is already on loan.");
        }

        book.BorrowedBy = userName;

        json = JsonSerializer.Serialize(
            books,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(FilePath, json);
    }
}
