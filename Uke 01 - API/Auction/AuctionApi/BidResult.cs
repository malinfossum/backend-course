public class BidResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public static BidResult Ok()
    {
        return new BidResult
        {
            Success = true
        };
    }

    public static BidResult Fail(string message)
    {
        return new BidResult
        {
            Success = false,
            ErrorMessage = message
        };
    }
}
