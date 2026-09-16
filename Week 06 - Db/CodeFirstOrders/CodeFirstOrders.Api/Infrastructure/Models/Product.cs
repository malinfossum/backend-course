namespace CodeFirstOrders.Api.Infrastructure.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public int StockCount { get; set; }

    public List<Shipment> Shipments { get; set; } = [];
}
