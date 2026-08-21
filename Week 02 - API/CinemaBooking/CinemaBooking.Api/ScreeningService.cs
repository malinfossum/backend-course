using CinemaBooking.Api.DomainModel;
using CinemaBooking.Api.DomainServices;

namespace CinemaBooking.Api;

public class ScreeningService
{
    private readonly IScreeningRepository _screeningRepository;

    // The service does not create FileScreeningRepository itself. It is handed
    // an IScreeningRepository, and it does not know which one it got.
    public ScreeningService(IScreeningRepository screeningRepository)
    {
        _screeningRepository = screeningRepository;
    }

    public IReadOnlyList<Screening> GetAll()
    {
        return _screeningRepository.FindAll();
    }

    public Result<Screening> GetById(int screeningId)
    {
        var screening = _screeningRepository.Find(screeningId);

        if (screening == null)
        {
            return Result<Screening>.NotFound(
                $"No screening with id {screeningId}.");
        }

        return Result<Screening>.Success(screening);
    }

    public Result<ReservationReceipt> ReserveSeat(
        int screeningId,
        string customerName,
        int seatNumber)
    {
        if (string.IsNullOrWhiteSpace(customerName))
        {
            return Result<ReservationReceipt>.Validation(
                "The customer must have a name.");
        }

        // Name validation runs before the lookup on purpose: it is pure
        // validation with no I/O, so there is no reason to read storage to
        // reject a request we already know is invalid.
        var screening = _screeningRepository.Find(screeningId);

        if (screening == null)
        {
            return Result<ReservationReceipt>.NotFound(
                $"No screening with id {screeningId}.");
        }

        if (!screening.HasSeat(seatNumber))
        {
            return Result<ReservationReceipt>.Validation(
                $"Seat {seatNumber} does not exist. "
                + $"This screening has seats 1-{screening.NumberOfSeats}.");
        }

        if (screening.ReservationFor(seatNumber) != null)
        {
            return Result<ReservationReceipt>.Conflict(
                $"Seat {seatNumber} is already reserved.");
        }

        if (screening.SeatsHeldBy(customerName) >= Screening.MaxSeatsPerCustomer)
        {
            return Result<ReservationReceipt>.Conflict(
                $"{customerName.Trim()} already holds "
                + $"{Screening.MaxSeatsPerCustomer} seats on this screening, "
                + "which is the maximum.");
        }

        // Only here does anything change. Every rule above returns before this
        // line, so a failed reservation cannot touch the state.
        screening.Reserve(seatNumber, customerName.Trim());

        _screeningRepository.Save(screening);

        var receipt = new ReservationReceipt
        {
            ScreeningId = screening.Id,
            MovieTitle = screening.MovieTitle,
            CustomerName = customerName.Trim(),
            SeatNumber = seatNumber
        };

        return Result<ReservationReceipt>.Success(receipt);
    }

    public Result<Reservation> CancelReservation(
        int screeningId,
        int seatNumber,
        string customerName)
    {
        if (string.IsNullOrWhiteSpace(customerName))
        {
            return Result<Reservation>.Validation(
                "The customer must have a name.");
        }

        var screening = _screeningRepository.Find(screeningId);

        if (screening == null)
        {
            return Result<Reservation>.NotFound(
                $"No screening with id {screeningId}.");
        }

        if (!screening.HasSeat(seatNumber))
        {
            return Result<Reservation>.Validation(
                $"Seat {seatNumber} does not exist. "
                + $"This screening has seats 1-{screening.NumberOfSeats}.");
        }

        var reservation = screening.ReservationFor(seatNumber);

        if (reservation == null)
        {
            return Result<Reservation>.NotFound(
                $"Seat {seatNumber} is not reserved.");
        }

        // Same choice as in the library task: whoever releases the booking has
        // to be the one who made it. Without this check anyone could cancel
        // anyone else's seat with a single request.
        if (!Screening.IsSameCustomer(reservation.CustomerName, customerName))
        {
            return Result<Reservation>.Conflict(
                $"Seat {seatNumber} is reserved by somebody else.");
        }

        screening.Release(reservation);

        _screeningRepository.Save(screening);

        return Result<Reservation>.Success(reservation);
    }
}
