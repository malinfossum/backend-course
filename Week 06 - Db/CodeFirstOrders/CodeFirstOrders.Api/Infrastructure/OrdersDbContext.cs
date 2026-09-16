using CodeFirstOrders.Api.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeFirstOrders.Api.Infrastructure;

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Shipment> Shipments => Set<Shipment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Week 6 Wednesday: the transaction exercise. The FK is found by convention; the CHECK
        // is the database's half of "stock can't go negative" (the service validates the same
        // rule up front, but a constraint holds even for code that forgets to).
        modelBuilder.Entity<Product>()
            .ToTable(table => table.HasCheckConstraint("CK_Products_StockCount", "[StockCount] >= 0"));

        modelBuilder.Entity<Product>()
            .Property(product => product.Name)
            .HasMaxLength(100);

        // Without this EF picks Cascade for a required relationship (same as Orders on Monday).
        // A shipment is history; deleting a product must fail, not erase what was sent.
        modelBuilder.Entity<Shipment>()
            .HasOne(shipment => shipment.Product)
            .WithMany(product => product.Shipments)
            .HasForeignKey(shipment => shipment.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Conventions would find this on their own; written out so the FK is visible here,
        // the same way it was named in the CREATE TABLE scripts of weeks 3–5.
        modelBuilder.Entity<Order>()
            .HasOne(order => order.Customer)
            .WithMany(customer => customer.Orders)
            .HasForeignKey(order => order.CustomerId);

        modelBuilder.Entity<Customer>()
            .HasIndex(customer => customer.EmailAddress)
            .IsUnique();

        modelBuilder.Entity<Order>()
            .Property(order => order.TotalAmount)
            .HasPrecision(18, 2);
    }
}
