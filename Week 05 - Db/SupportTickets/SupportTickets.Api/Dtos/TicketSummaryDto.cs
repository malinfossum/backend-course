namespace SupportTickets.Api.Dtos;

// What a ticket list actually needs: no Description, no Email, no entity.
public record TicketSummaryDto(
    int Id,
    string Title,
    string Status,
    int Priority,
    string CustomerName);
