namespace CinemaBooking.Api.DomainModel;

// A single seat held by a single customer. The screening used to store only
// seat numbers, which made it impossible to answer "who holds this seat?" -
// and both cancellation and the four-seat rule need that answer.
public class Reservation
{
    public int SeatNumber { get; set; }

    public string CustomerName { get; set; } = "";
}
