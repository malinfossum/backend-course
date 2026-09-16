using CodeFirstOrders.Api.Infrastructure;
using CodeFirstOrders.Api.Infrastructure.Models;
using CodeFirstOrders.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("OrdersDb");

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<ShippingService>();

var app = builder.Build();

// Monday: two endpoints only. The point of this project is the migrations, not the API;
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

// Wednesday: the transaction exercise. The endpoint validates the request shape; the rules
// (stock, product exists) live in the service, which is also where the transaction boundary
// belongs — the service is the one that knows the two writes are one operation.
app.MapGet("/products", async (OrdersDbContext context) =>
    await context.Products
        .Select(product => new { product.Id, product.Name, product.StockCount })
        .ToListAsync());

app.MapGet("/shipments", async (OrdersDbContext context) =>
    await context.Shipments
        .Select(shipment => new { shipment.Id, shipment.ProductId, shipment.Quantity, shipment.CreatedUtc })
        .ToListAsync());

// ?version=naive|transaction|final (default final) picks the stage of the exercise;
// &crash=true throws between the two writes in the naive and transaction versions.
app.MapPost("/shipments", async (CreateShipmentRequest input, ShippingService shipping, string? version = null, bool crash = false) =>
{
    switch (version)
    {
        case "naive":
            await shipping.ShipNaiveAsync(input.ProductId, input.Quantity, crash);
            return Results.Ok();
        case "transaction":
            await shipping.ShipWithTransactionAsync(input.ProductId, input.Quantity, crash);
            return Results.Ok();
    }

    var result = await shipping.ShipAsync(input.ProductId, input.Quantity);

    if (result.IsSuccess)
    {
        var shipment = result.Value!;
        return Results.Created($"/shipments/{shipment.Id}", new { shipment.Id, shipment.ProductId, shipment.Quantity });
    }

    return result.Error switch
    {
        ErrorKind.NotFound => Results.NotFound(new { error = result.ErrorMessage }),
        ErrorKind.Conflict => Results.Conflict(new { error = result.ErrorMessage }),
        _ => Results.BadRequest(new { error = result.ErrorMessage })
    };
});

// Test helper for the exercise ("set the database back to StockCount = 10"). Would never ship.
app.MapPost("/shipments/reset", async (OrdersDbContext context) =>
{
    await context.Shipments.ExecuteDeleteAsync();
    await context.Products.ExecuteUpdateAsync(setters => setters.SetProperty(product => product.StockCount, 10));
    return Results.NoContent();
});

app.Run();

public record CreateCustomer(string Name, string Email);

public record CreateShipmentRequest(int ProductId, int Quantity);
