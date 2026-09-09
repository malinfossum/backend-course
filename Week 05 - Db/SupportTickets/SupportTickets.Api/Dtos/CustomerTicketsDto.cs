namespace SupportTickets.Api.Dtos;

public record CustomerTicketsDto(
    string Name,
    string Email,
    List<CustomerTicketDto> Tickets);
