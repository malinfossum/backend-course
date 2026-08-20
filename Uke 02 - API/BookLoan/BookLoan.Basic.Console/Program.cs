using BookLoan.Basic;

try
{
    var service = new BookLoanService();
    service.BorrowBook(bookId: 1, userName: "Grace");

    Console.WriteLine("Boka ble lånt ut.");
}
catch (Exception exception)
{
    Console.WriteLine($"Utlånet feilet: {exception.Message}");
}
