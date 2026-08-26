namespace CouponApi.Api.Models;

public class Coupon
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Description { get; set; } = "";
    public int RemainingUses { get; set; }
    public bool IsActive { get; set; }
}
