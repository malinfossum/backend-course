namespace SupportTickets.Api.Dtos;

public record CustomerStatusDto(
    int Id,
    string Name,
    bool HasOpenTickets);
