using BookLoan.DI.DomainModel;

namespace BookLoan.DI.DomainServices;

public interface IBookRepository
{
    Book? Get(int id);

    void Update(Book book);
}
