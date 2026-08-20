using BookLoan.DI.DomainServices;

namespace BookLoan.DI;

public class BookLoanService
{
    private readonly IBookRepository _bookRepository;

    public BookLoanService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public void BorrowBook(int bookId, string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("Låntaker må ha et navn.");
        }

        var book = _bookRepository.Get(bookId);

        if (book == null)
        {
            throw new InvalidOperationException("Boka finnes ikke.");
        }

        if (book.BorrowedBy != null)
        {
            throw new InvalidOperationException("Boka er allerede utlånt.");
        }

        book.BorrowedBy = userName.Trim();

        _bookRepository.Update(book);
    }

    public void ReturnBook(int bookId, string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("Låntaker må ha et navn.");
        }

        var book = _bookRepository.Get(bookId);

        if (book == null)
        {
            throw new InvalidOperationException("Boka finnes ikke.");
        }

        if (book.BorrowedBy == null)
        {
            throw new InvalidOperationException("Boka er ikke utlånt.");
        }

        if (book.BorrowedBy != userName.Trim())
        {
            throw new InvalidOperationException(
                "Boka er lånt ut til noen andre.");
        }

        book.BorrowedBy = null;

        _bookRepository.Update(book);
    }
}
