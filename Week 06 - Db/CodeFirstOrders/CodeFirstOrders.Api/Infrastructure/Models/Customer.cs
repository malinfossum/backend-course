namespace CodeFirstOrders.Api.Infrastructure.Models;

public class Customer
{
    public int Id { get; set; }

    public string FullName { get; set; } = "";

    public string EmailAddress { get; set; } = "";

    public string? PhoneNumber { get; set; }

    public string Country { get; set; } = "";

    public bool IsActive { get; set; } = true;

    public List<Order> Orders { get; set; } = new();
}
