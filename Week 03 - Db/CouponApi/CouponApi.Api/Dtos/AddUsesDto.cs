namespace CouponApi.Api.Dtos;

// Challenge 3. A body with one number, so the amount cannot be smuggled in
// through the URL where it would end up in server logs.
public class AddUsesDto
{
    public int Amount { get; set; }
}
