namespace CinemaBooking.Api.DomainModel;

public class Screening
{
    // A customer may hold at most this many seats on one screening.
    public const int MaxSeatsPerCustomer = 4;

    public int Id { get; set; }

    public string MovieTitle { get; set; } = "";

    public int NumberOfSeats { get; set; }

    public List<Reservation> Reservations { get; set; } = new();

    // The questions below belong to the screening itself, not to the service.
    // Same move as in the teaching example, where Withdraw and Deposit were
    // pushed into Account instead of living as "Balance -= amount" outside it.

    public bool HasSeat(int seatNumber)
    {
        return seatNumber >= 1 && seatNumber <= NumberOfSeats;
    }

    public Reservation? ReservationFor(int seatNumber)
    {
        return Reservations.FirstOrDefault(
            reservation => reservation.SeatNumber == seatNumber);
    }

    public int SeatsHeldBy(string customerName)
    {
        return Reservations.Count(
            reservation => IsSameCustomer(reservation.CustomerName, customerName));
    }

    public void Reserve(int seatNumber, string customerName)
    {
        Reservations.Add(new Reservation
        {
            SeatNumber = seatNumber,
            CustomerName = customerName
        });
    }

    public void Release(Reservation reservation)
    {
        Reservations.Remove(reservation);
    }

    // Names are compared trimmed and case-insensitively. "ada" and "Ada  "
    // are the same person as far as this API is concerned.
    public static bool IsSameCustomer(string left, string right)
    {
        return string.Equals(
            left.Trim(),
            right.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }
}
