using CinemaBooking.Api.DomainModel;
using CinemaBooking.Api.DomainServices;

namespace CinemaBooking.Tests;

// Test double. Lagrer visningene i minnet, slik at ScreeningService kan testes
// uten ASP.NET, uten HTTP, uten JSON og uten fil.
public class FakeScreeningRepository : IScreeningRepository
{
    private readonly Dictionary<int, Screening> _screenings = new();

    // Teller hvor mange ganger Save faktisk ble kalt. Uten den kan en test
    // bestå selv om servicen glemmer å lagre, fordi Find gir tilbake det
    // samme objektet som allerede ligger i dictionaryet.
    public int SaveCount { get; private set; }

    public FakeScreeningRepository(params Screening[] screenings)
    {
        foreach (var screening in screenings)
        {
            _screenings[screening.Id] = screening;
        }
    }

    public IReadOnlyList<Screening> FindAll()
    {
        return _screenings.Values.ToList();
    }

    public Screening? Find(int id)
    {
        return _screenings.GetValueOrDefault(id);
    }

    public void Save(Screening screening)
    {
        SaveCount++;

        _screenings[screening.Id] = screening;
    }
}
