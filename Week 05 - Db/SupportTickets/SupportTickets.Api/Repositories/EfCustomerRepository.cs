using Microsoft.EntityFrameworkCore;
using SupportTickets.Api.Data;
using SupportTickets.Api.Dtos;
using SupportTickets.Api.Models;

namespace SupportTickets.Api.Repositories;

public class EfCustomerRepository : ICustomerRepository
{
    private readonly SupportTicketDbContext _context;

    public EfCustomerRepository(SupportTicketDbContext context)
    {
        _context = context;
    }

    // Tickets.Count inside the projection becomes a counting subquery, so the
    // count is done by SQL Server and one number per customer comes back.
    public async Task<List<CustomerTicketCountDto>> GetTicketCountsAsync()
    {
        return await _context.Customers
            .OrderBy(customer => customer.Name)
            .Select(customer => new CustomerTicketCountDto(
                customer.Id,
                customer.Name,
                customer.Tickets.Count))
            .ToListAsync();
    }

    // N+1 on purpose: 5 customers cost 6 queries, 10 000 customers cost 10 001.
    // Kept next to GetTicketCountsAsync so the two SQL logs can be compared.
    public async Task<List<CustomerTicketCountDto>> GetTicketCountsNPlusOneAsync()
    {
        var customers = await _context.Customers.ToListAsync();

        var counts = new List<CustomerTicketCountDto>();

        foreach (var customer in customers)
        {
            var tickets = await _context.Tickets
                .Where(ticket => ticket.CustomerId == customer.Id)
                .ToListAsync();

            counts.Add(new CustomerTicketCountDto(customer.Id, customer.Name, tickets.Count));
        }

        return counts;
    }

    public async Task<List<Customer>> GetWithOpenTicketsAsync()
    {
        return await _context.Customers
            .Where(customer => customer.Tickets.Any(ticket => ticket.Status == "Open"))
            .OrderBy(customer => customer.Name)
            .ToListAsync();
    }

    public async Task<List<CustomerStatusDto>> GetStatusesAsync()
    {
        return await _context.Customers
            .OrderBy(customer => customer.Name)
            .Select(customer => new CustomerStatusDto(
                customer.Id,
                customer.Name,
                customer.Tickets.Any(ticket => ticket.Status == "Open")))
            .ToListAsync();
    }

    // The nested Select is part of the same query — EF fetches the customer row
    // and the ticket rows together rather than asking twice.
    public async Task<CustomerTicketsDto?> FindWithTicketsAsync(int id)
    {
        return await _context.Customers
            .Where(customer => customer.Id == id)
            .Select(customer => new CustomerTicketsDto(
                customer.Name,
                customer.Email,
                customer.Tickets
                    .OrderByDescending(ticket => ticket.Priority)
                    .Select(ticket => new CustomerTicketDto(
                        ticket.Id,
                        ticket.Title,
                        ticket.Status,
                        ticket.Priority,
                        ticket.CreatedUtc))
                    .ToList()))
            .SingleOrDefaultAsync();
    }

    // Ordered before the projection so the sort is expressed in SQL.
    public async Task<List<CustomerTicketCountDto>> GetBusyAsync(int minTickets)
    {
        return await _context.Customers
            .Where(customer => customer.Tickets.Count >= minTickets)
            .OrderByDescending(customer => customer.Tickets.Count)
            .Select(customer => new CustomerTicketCountDto(
                customer.Id,
                customer.Name,
                customer.Tickets.Count))
            .ToListAsync();
    }
}
