namespace SupportTickets.Api.Dtos;

public record CustomerTicketCountDto(
    int CustomerId,
    string CustomerName,
    int NumberOfTickets);
