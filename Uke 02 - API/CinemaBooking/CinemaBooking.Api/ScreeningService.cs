using CinemaBooking.Api.DomainModel;
using CinemaBooking.Api.DomainServices;

namespace CinemaBooking.Api;

public class ScreeningService
{
    private readonly IScreeningRepository _screeningRepository;

    // Servicen oppretter ikke FileScreeningRepository selv. Den får et
    // IScreeningRepository inn, og vet ikke hvilken som kommer.
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
            return Result<Screening>.Failure(
                $"Fant ikke kinovisning {screeningId}.");
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
            return Result<ReservationReceipt>.Failure(
                "Kunden må ha et navn.");
        }

        var screening = _screeningRepository.Find(screeningId);

        if (screening == null)
        {
            return Result<ReservationReceipt>.Failure(
                $"Fant ikke kinovisning {screeningId}.");
        }

        if (seatNumber < 1 || seatNumber > screening.NumberOfSeats)
        {
            return Result<ReservationReceipt>.Failure(
                $"Sete {seatNumber} finnes ikke. "
                + $"Visningen har setene 1-{screening.NumberOfSeats}.");
        }

        if (screening.ReservedSeats.Contains(seatNumber))
        {
            return Result<ReservationReceipt>.Failure(
                $"Sete {seatNumber} er allerede reservert.");
        }

        // Først her endrer vi noe. Alle feilene over returnerer før dette,
        // slik at en mislykket reservasjon ikke rører tilstanden.
        screening.ReservedSeats.Add(seatNumber);

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
}
