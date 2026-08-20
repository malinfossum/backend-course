namespace CinemaBooking.Api.DomainModel;

public class Screening
{
    public int Id { get; set; }

    public string MovieTitle { get; set; } = "";

    public int NumberOfSeats { get; set; }

    public List<int> ReservedSeats { get; set; } = new();
}
