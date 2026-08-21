using BookLoan.DI.DomainModel;
using BookLoan.DI.DomainServices;

namespace BookLoan.DI.Infrastructure;

public class InMemoryBookRepository : IBookRepository
{
    private readonly List<Book> _books = new List<Book>
    {
        new Book { Id = 1, Title = "The Hobbit" },
        new Book { Id = 2, Title = "Clean Code" },
        new Book { Id = 3, Title = "The Pragmatic Programmer", BorrowedBy = "Ada" }
    };

    public Book? Get(int id)
    {
        return _books.FirstOrDefault(book => book.Id == id);
    }

    public void Update(Book book)
    {
        var index = _books.FindIndex(existing => existing.Id == book.Id);

        if (index == -1)
        {
            throw new InvalidOperationException(
                $"Fant ikke boka med id {book.Id}.");
        }

        _books[index] = book;
    }
}
