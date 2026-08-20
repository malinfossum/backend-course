namespace AuctionApi;

public class Bid
{
    public string BidderName { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTimeOffset PlacedAt { get; set; }
}
