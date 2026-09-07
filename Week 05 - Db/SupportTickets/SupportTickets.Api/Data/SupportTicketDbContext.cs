using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SupportTickets.Api.Models;

namespace SupportTickets.Api.Data;

public partial class SupportTicketDbContext : DbContext
{
    public SupportTicketDbContext(DbContextOptions<SupportTicketDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC07F88DE33C");

            entity.HasIndex(e => e.Email, "UQ__Customer__A9D105340A5ED797").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tickets__3214EC072ED2C24F");

            entity.Property(e => e.Status).HasMaxLength(30);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Customer).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tickets_Customers");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
