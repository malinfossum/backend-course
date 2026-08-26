namespace CouponApi.Api.Dtos;

// What the client is allowed to decide. Id is missing because the database
// hands it out, and IsActive is missing because a brand new coupon is always
// active - letting the client send either one would be letting it lie.
public class CreateCouponDto
{
    public string Code { get; set; } = "";
    public string Description { get; set; } = "";
    public int RemainingUses { get; set; }
}
