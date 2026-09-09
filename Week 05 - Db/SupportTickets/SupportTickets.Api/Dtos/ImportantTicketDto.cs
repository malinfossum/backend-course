namespace SupportTickets.Api.Dtos;

public record ImportantTicketDto(
    string Title,
    int Priority,
    string CustomerName,
    DateTime CreatedUtc);
