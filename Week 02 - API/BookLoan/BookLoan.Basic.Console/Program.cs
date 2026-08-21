using BookLoan.Basic;

try
{
    var service = new BookLoanService();
    service.BorrowBook(bookId: 1, userName: "Grace");

    Console.WriteLine("The book was loaned out.");
}
catch (Exception exception)
{
    Console.WriteLine($"The loan failed: {exception.Message}");
}
