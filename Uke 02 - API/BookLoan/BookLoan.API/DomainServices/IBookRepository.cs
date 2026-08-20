using BookLoan.API.DomainModel;

namespace BookLoan.API.DomainServices;

public interface IBookRepository
{
    Book? Get(int id);

    void Update(Book book);
}
