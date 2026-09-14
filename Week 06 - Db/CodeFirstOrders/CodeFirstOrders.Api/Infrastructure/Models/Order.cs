namespace CodeFirstOrders.Api.Infrastructure.Models;

public class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = "Created";

    public string? Note { get; set; }

    public Customer Customer { get; set; } = null!;
}
