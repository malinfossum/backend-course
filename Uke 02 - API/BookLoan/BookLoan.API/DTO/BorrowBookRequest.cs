namespace BookLoan.API.DTO;

public class BorrowBookRequest
{
    public int BookId { get; set; }
    public string UserName { get; set; } = "";
}
