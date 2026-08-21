using BookLoan.API.DomainServices;

namespace BookLoan.API;

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
            throw new ArgumentException("The borrower must have a name.");
        }

        var book = _bookRepository.Get(bookId);

        if (book == null)
        {
            throw new InvalidOperationException("The book does not exist.");
        }

        if (book.BorrowedBy != null)
        {
            throw new InvalidOperationException("The book is already on loan.");
        }

        book.BorrowedBy = userName.Trim();

        _bookRepository.Update(book);
    }

    public void ReturnBook(int bookId, string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("The borrower must have a name.");
        }

        var book = _bookRepository.Get(bookId);

        if (book == null)
        {
            throw new InvalidOperationException("The book does not exist.");
        }

        if (book.BorrowedBy == null)
        {
            throw new InvalidOperationException("The book is not on loan.");
        }

        if (book.BorrowedBy != userName.Trim())
        {
            throw new InvalidOperationException(
                "The book is on loan to somebody else.");
        }

        book.BorrowedBy = null;

        _bookRepository.Update(book);
    }
}
