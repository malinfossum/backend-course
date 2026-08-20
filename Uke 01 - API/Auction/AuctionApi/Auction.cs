using AuctionApi;

public class Auction
{
    public int Id { get; set; }
    public string ItemName { get; set; } = "";
    public decimal CurrentBid { get; set; }
    public string? HighestBidder { get; set; }
    public bool IsClosed { get; set; }
    public List<Bid> Bids { get; set; } = new List<Bid>();

    public BidResult PlaceBid(string bidderName, decimal amount)
    {
        if (IsClosed)
        {
            return BidResult.Fail("Auksjonen er avsluttet.");
        }

        if (string.IsNullOrWhiteSpace(bidderName))
        {
            return BidResult.Fail("Budgiver må ha et navn.");
        }

        if (amount <= CurrentBid)
        {
            return BidResult.Fail(
                $"Budet må være høyere enn {CurrentBid} kr.");
        }

        CurrentBid = amount;
        HighestBidder = bidderName.Trim();
        Bids.Add(new Bid
        {
            BidderName = HighestBidder,
            Amount = amount,
            PlacedAt = DateTimeOffset.UtcNow
        });

        return BidResult.Ok();
    }

    public bool Close()
    {
        if (IsClosed)
        {
            return false;
        }

        IsClosed = true;
        return true;
    }
}
