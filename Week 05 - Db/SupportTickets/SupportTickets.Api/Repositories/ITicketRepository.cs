using SupportTickets.Api.Dtos;
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

    // Open tickets with a title longer than fifteen characters. The length test is
    // a plain C# method, so it runs after AsEnumerable.
    List<Ticket> SearchOpenWithLongTitle();

    // Every ticket with its customer loaded in the same round trip.
    Task<List<Ticket>> GetAllWithCustomerAsync();

    // Filters applied only when given, then sorted and projected — all before
    // the query is sent.
    Task<List<TicketSummaryDto>> GetSummaryAsync(string? status, int? minPriority);

    // The SQL the same call would send. Useful for comparing filter combinations.
    string GetSummarySql(string? status, int? minPriority);

    // The same summaries built the other way: Include the entities, then map in
    // C#. Kept for the SQL comparison in the exercise.
    Task<List<TicketSummaryDto>> GetSummaryViaIncludeAsync();

    // Open tickets only: highest priority first, oldest first within a priority.
    Task<List<TicketSummaryDto>> GetOpenSummaryAsync();

    // Open tickets at priority 4 and up, oldest first.
    Task<List<ImportantTicketDto>> GetImportantAsync();

    // The highest-priority open tickets, limited by the database with TOP.
    Task<List<Ticket>> GetTopOpenAsync(int count);

    // A read-only query that opts out of change tracking.
    Task<List<Ticket>> GetOpenNoTrackingAsync();
}
