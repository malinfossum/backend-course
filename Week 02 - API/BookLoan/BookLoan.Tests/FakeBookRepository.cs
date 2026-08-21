using BookLoan.API.DomainModel;
using BookLoan.API.DomainServices;

namespace BookLoan.Tests;

// Test double. Keeps books in memory so BookLoanService can be tested
// without a file, without JSON and without HTTP.
//
// It hands out copies, and stores copies, because that is what the real
// FileBookRepository does: it deserialises fresh JSON on every read, so the
// caller never holds a live reference into storage.
//
// While this fake returned the same object it stored, the service and the
// test were mutating the same instance. A test could assert on state and stay
// green even when the service forgot to call Update - only the counter below
// noticed. Copying closes that hole, so the state assertions carry their own
// weight again.
public class FakeBookRepository : IBookRepository
{
    private readonly Dictionary<int, Book> _books = new();

    // Kept for the cases where the number of writes is the actual claim,
    // for example "saved once, not twice". It is no longer the only thing
    // standing between a forgotten Update and a green test run.
    public int UpdateCount { get; private set; }

    public FakeBookRepository(params Book[] books)
    {
        foreach (var book in books)
        {
            _books[book.Id] = CopyOf(book);
        }
    }

    public Book? Get(int id)
    {
        return _books.TryGetValue(id, out var book) ? CopyOf(book) : null;
    }

    public void Update(Book book)
    {
        UpdateCount++;

        _books[book.Id] = CopyOf(book);
    }

    private static Book CopyOf(Book book)
    {
        return new Book
        {
            Id = book.Id,
            Title = book.Title,
            BorrowedBy = book.BorrowedBy
        };
    }
}
