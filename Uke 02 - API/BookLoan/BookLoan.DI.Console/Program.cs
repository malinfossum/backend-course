using BookLoan.DI;
using BookLoan.DI.DomainServices;
using BookLoan.DI.Infrastructure;

RunScenario(new FileBookRepository(), "FileBookRepository");
RunScenario(new InMemoryBookRepository(), "InMemoryBookRepository");

static void RunScenario(IBookRepository repository, string repositoryName)
{
    Console.WriteLine($"--- {repositoryName} ---");

    var service = new BookLoanService(repository);

    TryBorrow(service, bookId: 1, userName: "Grace");
    TryBorrow(service, bookId: 1, userName: "Linus");
    TryReturn(service, bookId: 1, userName: "Grace");

    Console.WriteLine();
}

static void TryBorrow(BookLoanService service, int bookId, string userName)
{
    try
    {
        service.BorrowBook(bookId, userName);

        Console.WriteLine($"{userName} lånte bok {bookId}.");
    }
    catch (Exception exception)
    {
        Console.WriteLine($"Utlånet feilet: {exception.Message}");
    }
}

static void TryReturn(BookLoanService service, int bookId, string userName)
{
    try
    {
        service.ReturnBook(bookId, userName);

        Console.WriteLine($"{userName} leverte bok {bookId}.");
    }
    catch (Exception exception)
    {
        Console.WriteLine($"Innleveringen feilet: {exception.Message}");
    }
}
