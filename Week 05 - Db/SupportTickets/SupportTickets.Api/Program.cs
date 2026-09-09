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
builder.Services.AddScoped<ICustomerRepository, EfCustomerRepository>();

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

// The same rows as /tickets/high-priority, fetched the slow way. Run both and
// compare the logged SQL: only one of them filters in the database.
app.MapGet("/tickets/high-priority/in-memory", async (ITicketRepository repository) =>
    Results.Ok(await repository.GetHighPriorityInMemoryAsync()));

// Database filter, then AsEnumerable, then a C# filter EF cannot translate.
// GET /tickets/open/title?word=log
app.MapGet("/tickets/open/title", (string? word, ITicketRepository repository) =>
    string.IsNullOrWhiteSpace(word)
        ? Results.BadRequest(new { error = "Query parameter 'word' is required." })
        : Results.Ok(repository.SearchOpenByTitleWord(word)));

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

// Open tickets whose title is longer than fifteen characters. The length test is
// an ordinary C# method, so it cannot be part of the SQL.
app.MapGet("/tickets/open/long-title", (ITicketRepository repository) =>
    Results.Ok(repository.SearchOpenWithLongTitle()));

// Every ticket with its customer, one round trip. Projected on the way out, since
// the entities point at each other.
app.MapGet("/tickets/with-customer", async (ITicketRepository repository) =>
{
    var tickets = await repository.GetAllWithCustomerAsync();

    return Results.Ok(tickets.Select(ticket => new
    {
        ticket.Id,
        ticket.Title,
        ticket.Status,
        ticket.Priority,
        CustomerName = ticket.Customer.Name
    }));
});

// GET /tickets/summary
// GET /tickets/summary?status=Open
// GET /tickets/summary?status=Open&minPriority=3
app.MapGet("/tickets/summary", async (
    string? status,
    int? minPriority,
    ITicketRepository repository) =>
    Results.Ok(await repository.GetSummaryAsync(status, minPriority)));

// The SQL behind the line above. Change the filters and watch it change.
app.MapGet("/tickets/summary/sql", (
    string? status,
    int? minPriority,
    ITicketRepository repository) =>
    Results.Text(repository.GetSummarySql(status, minPriority)));

// Same summaries, built with Include and mapped in C#. Compare the two SQL
// statements: this one selects every column of both tables.
app.MapGet("/tickets/summary/via-include", async (ITicketRepository repository) =>
    Results.Ok(await repository.GetSummaryViaIncludeAsync()));

// Open tickets only: priority down, then oldest first.
app.MapGet("/tickets/summary/open", async (ITicketRepository repository) =>
    Results.Ok(await repository.GetOpenSummaryAsync()));

// Open and priority 4 or higher, oldest first.
app.MapGet("/tickets/important", async (ITicketRepository repository) =>
    Results.Ok(await repository.GetImportantAsync()));

// The n highest-priority open tickets, cut down by the database with TOP.
app.MapGet("/tickets/top-open", async (int? count, ITicketRepository repository) =>
    Results.Ok(await repository.GetTopOpenAsync(count ?? 5)));

// A read-only read with change tracking switched off.
app.MapGet("/tickets/open/no-tracking", async (ITicketRepository repository) =>
    Results.Ok(await repository.GetOpenNoTrackingAsync()));

// One query: every customer and their ticket count, zero included.
app.MapGet("/customers/ticket-counts", async (ICustomerRepository repository) =>
    Results.Ok(await repository.GetTicketCountsAsync()));

// The same answer as N+1 queries. Count the SELECT statements in the log.
app.MapGet("/customers/ticket-counts/n-plus-one", async (ICustomerRepository repository) =>
    Results.Ok(await repository.GetTicketCountsNPlusOneAsync()));

// Customers with at least one open ticket — Any, which EF writes as EXISTS.
app.MapGet("/customers/with-open-tickets", async (ICustomerRepository repository) =>
{
    var customers = await repository.GetWithOpenTicketsAsync();

    return Results.Ok(customers.Select(customer => new { customer.Id, customer.Name }));
});

// Every customer, with the open-ticket question answered by the database.
app.MapGet("/customers/statuses", async (ICustomerRepository repository) =>
    Results.Ok(await repository.GetStatusesAsync()));

// A customer and their tickets, projected in one query.
app.MapGet("/customers/{id:int}/tickets", async (int id, ICustomerRepository repository) =>
{
    var customer = await repository.FindWithTicketsAsync(id);

    return customer == null ? Results.NotFound() : Results.Ok(customer);
});

// GET /customers/busy?minTickets=2 — busiest customers first.
app.MapGet("/customers/busy", async (int? minTickets, ICustomerRepository repository) =>
    Results.Ok(await repository.GetBusyAsync(minTickets ?? 2)));

app.Run();
