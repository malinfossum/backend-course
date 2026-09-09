using SupportTickets.Api.Models;

namespace SupportTickets.Api.Repositories;

// The same interface would work over Dapper, EF Core, JSON files or MongoDB.
// Nothing here mentions Entity Framework — that is the whole point of keeping it.
public interface ITicketRepository
{
    Task<List<Ticket>> GetAllAsync();

    Task<Ticket?> FindAsync(int id);

    // Open tickets, high priority first — but "Open" is a parameter, not a constant,
    // so the same method answers /tickets?status=Open&minPriority=3 as well.
    Task<List<Ticket>> SearchAsync(string? status, int? minPriority);

    // The SQL that SearchAsync would send, without sending it.
    string SearchSql(string? status, int? minPriority);

    Task<List<Ticket>> GetHighPriorityAsync();

    // Counted by the database, not by C#.
    Task<int> CountOpenAsync();

    Task<Ticket?> FindWithCustomerAsync(int id);

    // The same result as GetHighPriorityAsync, built the slow way on purpose:
    // every row crosses the wire and C# throws most of them away. Kept so the
    // two SQL statements can be compared.
    Task<List<Ticket>> GetHighPriorityInMemoryAsync();

    // Open tickets whose title contains a word, compared the way C# compares
    // strings. Not async: after AsEnumerable the rest runs on this side.
    List<Ticket> SearchOpenByTitleWord(string word);
}
