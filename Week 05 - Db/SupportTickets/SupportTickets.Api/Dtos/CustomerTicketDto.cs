namespace SupportTickets.Api.Dtos;

// One ticket as it appears inside a customer response — the customer is already
// named by the parent, so it is not repeated here.
public record CustomerTicketDto(
    int Id,
    string Title,
    string Status,
    int Priority,
    DateTime CreatedUtc);
