using BookLoan.API.DomainModel;
using BookLoan.API.DomainServices;

namespace BookLoan.Tests;

// Test double. Lagrer bøker i minnet, slik at BookLoanService kan testes
// uten fil, uten JSON og uten HTTP.
public class FakeBookRepository : IBookRepository
{
    private readonly Dictionary<int, Book> _books = new();

    // Teller hvor mange ganger Update faktisk ble kalt. Uten den kan en test
    // bestå selv om servicen glemmer å lagre, fordi Get gir tilbake det samme
    // objektet som ligger i dictionaryet.
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
