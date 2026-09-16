namespace CodeFirstOrders.Api.Infrastructure.Models;

public class Shipment
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public DateTime CreatedUtc { get; set; }

    public Product Product { get; set; } = null!;
}
