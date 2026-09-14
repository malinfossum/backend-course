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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
