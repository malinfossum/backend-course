using Microsoft.EntityFrameworkCore;
using SupportTickets.Api.Data;
using SupportTickets.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SupportTickets");

builder.Services.AddDbContext<SupportTicketDbContext>(options =>
    options
        .UseSqlServer(connectionString)

        // Every command EF sends is printed to the console. Development only:
        // parameter values can hold personal data, so this does not belong in
        // a deployed application.
        .LogTo(
            Console.WriteLine,
            [DbLoggerCategory.Database.Command.Name],
            LogLevel.Information));

// The only line that knows Entity Framework is behind the interface.
builder.Services.AddScoped<ITicketRepository, EfTicketRepository>();

var app = builder.Build();

// GET /tickets
// GET /tickets?status=Open
// GET /tickets?status=Open&minPriority=3
app.MapGet("/tickets", async (
    string? status,
    int? minPriority,
    ITicketRepository repository) =>
{
    var tickets = status == null && minPriority == null
        ? await repository.GetAllAsync()
        : await repository.SearchAsync(status, minPriority);

    return Results.Ok(tickets);
});

// The SQL EF would send for the same query, without sending it.
// GET /tickets/sql?status=Open&minPriority=3
app.MapGet("/tickets/sql", (
    string? status,
    int? minPriority,
    ITicketRepository repository) =>
    Results.Text(repository.SearchSql(status, minPriority)));

// Open tickets, highest priority first, oldest first within a priority.
app.MapGet("/tickets/open", async (ITicketRepository repository) =>
    Results.Ok(await repository.SearchAsync("Open", null)));

// Answered by SELECT COUNT(*), not by counting a list in C#.
app.MapGet("/tickets/open/count", async (ITicketRepository repository) =>
    Results.Ok(new { openTickets = await repository.CountOpenAsync() }));

// Priority 4 and up, newest first.
app.MapGet("/tickets/high-priority", async (ITicketRepository repository) =>
    Results.Ok(await repository.GetHighPriorityAsync()));

app.MapGet("/tickets/{id:int}", async (int id, ITicketRepository repository) =>
{
    var ticket = await repository.FindAsync(id);

    return ticket == null ? Results.NotFound() : Results.Ok(ticket);
});

// Ticket plus the customer name, fetched in one round trip with Include.
// The result is projected rather than returned as-is: Ticket points at Customer
// and Customer points back at its Tickets, so serialising the entity directly
// would run in a circle.
app.MapGet("/tickets/{id:int}/with-customer", async (int id, ITicketRepository repository) =>
{
    var ticket = await repository.FindWithCustomerAsync(id);

    if (ticket == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new
    {
        ticket.Id,
        ticket.Title,
        ticket.Status,
        ticket.Priority,
        ticket.CreatedUtc,
        CustomerName = ticket.Customer.Name,
        CustomerEmail = ticket.Customer.Email
    });
});

app.Run();
