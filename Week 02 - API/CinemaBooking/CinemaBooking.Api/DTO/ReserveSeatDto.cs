namespace CinemaBooking.Api.DTO;

// Det klienten sender i bodyen. screeningId ligger ikke her - den kommer
// fra ruta: POST /screenings/{id}/reservations
public class ReserveSeatDto
{
    public string CustomerName { get; set; } = "";

    public int SeatNumber { get; set; }
}
