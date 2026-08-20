namespace BookLoan.DI.DomainModel;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? BorrowedBy { get; set; }
}
