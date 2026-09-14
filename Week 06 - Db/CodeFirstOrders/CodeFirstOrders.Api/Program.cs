using CodeFirstOrders.Api.Infrastructure;
using CodeFirstOrders.Api.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("OrdersDb");

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// Two endpoints only. The point of this project is the migrations, not the API;
// these exist to prove that a Code First database behaves like any other EF database.
app.MapGet("/customers", async (OrdersDbContext context) =>
    await context.Customers
        .Select(customer => new { customer.Id, customer.FullName, customer.EmailAddress })
        .ToListAsync());

app.MapPost("/customers", async (CreateCustomer input, OrdersDbContext context) =>
{
    if (string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.Email))
    {
        return Results.BadRequest("Name and email are required.");
    }

    var customer = new Customer { FullName = input.Name.Trim(), EmailAddress = input.Email.Trim() };

    context.Customers.Add(customer);
    await context.SaveChangesAsync();

    return Results.Created($"/customers/{customer.Id}", new { customer.Id, customer.FullName, customer.EmailAddress });
});

app.Run();

public record CreateCustomer(string Name, string Email);
