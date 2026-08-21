using CinemaBooking.Api.DomainModel;
using CinemaBooking.Api.DomainServices;

namespace CinemaBooking.Tests;

// Test double. Keeps screenings in memory so ScreeningService can be tested
// without ASP.NET, without HTTP, without JSON and without a file.
//
// It hands out copies, and stores copies, because that is what the real
// FileScreeningRepository does: it deserialises fresh JSON on every read, so
// the caller never holds a live reference into storage.
//
// That detail matters. While this fake returned the same object it stored, a
// test could assert on state and stay green even when the service forgot to
// call Save - the service and the test were mutating the very same instance.
// Copying closes that hole, and the test below no longer needs a counter to
// notice a missing Save.
public class FakeScreeningRepository : IScreeningRepository
{
    private readonly Dictionary<int, Screening> _screenings = new();

    // Kept for the cases where the number of writes is the actual claim,
    // for example "saved once, not twice". It is no longer what stops a
    // forgotten Save from slipping through.
    public int SaveCount { get; private set; }

    public FakeScreeningRepository(params Screening[] screenings)
    {
        foreach (var screening in screenings)
        {
            _screenings[screening.Id] = CopyOf(screening);
        }
    }

    public IReadOnlyList<Screening> FindAll()
    {
        return _screenings.Values.Select(CopyOf).ToList();
    }

    public Screening? Find(int id)
    {
        return _screenings.TryGetValue(id, out var screening)
            ? CopyOf(screening)
            : null;
    }

    public void Save(Screening screening)
    {
        SaveCount++;

        _screenings[screening.Id] = CopyOf(screening);
    }

    // A shallow copy would still share the Reservations list, and then the
    // whole point is lost - so the list and its items are rebuilt too.
    private static Screening CopyOf(Screening screening)
    {
        return new Screening
        {
            Id = screening.Id,
            MovieTitle = screening.MovieTitle,
            NumberOfSeats = screening.NumberOfSeats,
            Reservations = screening.Reservations
                .Select(reservation => new Reservation
                {
                    SeatNumber = reservation.SeatNumber,
                    CustomerName = reservation.CustomerName
                })
                .ToList()
        };
    }
}
