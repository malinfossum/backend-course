namespace CinemaBooking.Api;

public class ReservationReceipt
{
    public int ScreeningId { get; set; }

    public string MovieTitle { get; set; } = "";

    public string CustomerName { get; set; } = "";

    public int SeatNumber { get; set; }
}
