using BookLoan.API.DomainModel;
using BookLoan.API.DomainServices;

namespace BookLoan.Tests;

// Test double. Keeps books in memory so BookLoanService can be tested
// without a file, without JSON and without HTTP.
public class FakeBookRepository : IBookRepository
{
    private readonly Dictionary<int, Book> _books = new();

    // Counts how many times Update was actually called. Without it a test
    // can pass even when the service forgets to save, because Get hands back
    // the very same object that sits in the dictionary.
    public int UpdateCount { get; private set; }

    public FakeBookRepository(params Book[] books)
    {
        foreach (var book in books)
        {
            _books[book.Id] = book;
        }
    }

    public Book? Get(int id)
    {
        return _books.GetValueOrDefault(id);
    }

    public void Update(Book book)
    {
        UpdateCount++;

        _books[book.Id] = book;
    }
}
