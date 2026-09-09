using Microsoft.EntityFrameworkCore;
using SupportTickets.Api.Data;
using SupportTickets.Api.Models;

namespace SupportTickets.Api.Repositories;

public class EfTicketRepository : ITicketRepository
{
    private readonly SupportTicketDbContext _context;

    public EfTicketRepository(SupportTicketDbContext context)
    {
        _context = context;
    }

    // With Dapper this was a SELECT listing all seven columns. EF writes it.
    public async Task<List<Ticket>> GetAllAsync()
    {
        return await _context.Tickets.ToListAsync();
    }

    public async Task<Ticket?> FindAsync(int id)
    {
        return await _context.Tickets.SingleOrDefaultAsync(ticket => ticket.Id == id);
    }

    public async Task<List<Ticket>> SearchAsync(string? status, int? minPriority)
    {
        return await BuildSearch(status, minPriority).ToListAsync();
    }

    public string SearchSql(string? status, int? minPriority)
    {
        return BuildSearch(status, minPriority).ToQueryString();
    }

    public async Task<List<Ticket>> GetHighPriorityAsync()
    {
        return await _context.Tickets
            .Where(ticket => ticket.Priority >= 4)
            .OrderByDescending(ticket => ticket.CreatedUtc)
            .ToListAsync();
    }

    // CountAsync sends SELECT COUNT(*) and gets one number back. Loading the rows
    // and calling Count() on the list would move ten rows across the wire to learn
    // something the database already knew.
    public async Task<int> CountOpenAsync()
    {
        return await _context.Tickets.CountAsync(ticket => ticket.Status == "Open");
    }

    // Include tells EF to fetch the customer in the same round trip — a JOIN,
    // not a second query.
    public async Task<Ticket?> FindWithCustomerAsync(int id)
    {
        return await _context.Tickets
            .Include(ticket => ticket.Customer)
            .SingleOrDefaultAsync(ticket => ticket.Id == id);
    }

    // Variant B from the exercise. ToListAsync runs first, so the SQL has no
    // WHERE and no ORDER BY — the database sends all ten rows and the filtering
    // and sorting happen in memory. With two million rows this is the wrong
    // shape, and the SQL log is where that shows.
    public async Task<List<Ticket>> GetHighPriorityInMemoryAsync()
    {
        var tickets = await _context.Tickets.ToListAsync();

        return tickets
            .Where(ticket => ticket.Priority >= 4)
            .OrderByDescending(ticket => ticket.CreatedUtc)
            .ToList();
    }

    // Status is filtered by the database. AsEnumerable draws the line: the title
    // check uses StringComparison, which has no SQL translation, so it has to
    // run in C#. Narrow the database side first — everything past the line is
    // paid for in rows transferred.
    public List<Ticket> SearchOpenByTitleWord(string word)
    {
        var openTickets = _context.Tickets
            .Where(ticket => ticket.Status == "Open")
            .AsEnumerable();

        return openTickets
            .Where(ticket => ticket.Title.Contains(word, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    // Built but not executed. Each Where adds to the SQL; nothing is sent until
    // the caller asks for a result with ToListAsync or ToQueryString.
    private IQueryable<Ticket> BuildSearch(string? status, int? minPriority)
    {
        var query = _context.Tickets.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(ticket => ticket.Status == status);
        }

        if (minPriority.HasValue)
        {
            query = query.Where(ticket => ticket.Priority >= minPriority.Value);
        }

        return query
            .OrderByDescending(ticket => ticket.Priority)
            .ThenBy(ticket => ticket.CreatedUtc);
    }
}
