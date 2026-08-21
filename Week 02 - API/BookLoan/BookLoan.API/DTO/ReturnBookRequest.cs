namespace BookLoan.API.DTO;

public class ReturnBookRequest
{
    public int BookId { get; set; }
    public string UserName { get; set; } = "";
}
