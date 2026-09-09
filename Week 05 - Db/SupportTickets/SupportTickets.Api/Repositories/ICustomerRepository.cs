using SupportTickets.Api.Dtos;
using SupportTickets.Api.Models;

namespace SupportTickets.Api.Repositories;

// Customer-side queries. They live apart from ITicketRepository because they
// answer questions about customers, not about tickets — four of them by now.
public interface ICustomerRepository
{
    // One query: every customer with a count, including the one with no tickets.
    Task<List<CustomerTicketCountDto>> GetTicketCountsAsync();

    // The same answer built the wrong way: one query for the customers, then one
    // more per customer. Kept so the SQL log can show what N+1 looks like.
    Task<List<CustomerTicketCountDto>> GetTicketCountsNPlusOneAsync();

    // Customers with at least one open ticket. Any translates to EXISTS.
    Task<List<Customer>> GetWithOpenTicketsAsync();

    // Every customer, with the open-ticket question answered by the database.
    Task<List<CustomerStatusDto>> GetStatusesAsync();

    // A customer and their tickets, projected in one query — no Include.
    Task<CustomerTicketsDto?> FindWithTicketsAsync(int id);

    // Customers with at least minTickets tickets, busiest first.
    Task<List<CustomerTicketCountDto>> GetBusyAsync(int minTickets);
}
