using System.Text.Json;
using CinemaBooking.Api.DomainModel;
using CinemaBooking.Api.DomainServices;

namespace CinemaBooking.Api.Infrastructure;

// Den eneste klassen som kjenner til File, JSON og screenings.json.
public class FileScreeningRepository : IScreeningRepository
{
    private static readonly string FilePath =
        Path.Combine(AppContext.BaseDirectory, "screenings.json");

    private static readonly JsonSerializerOptions JsonOptions =
        new JsonSerializerOptions
        {
            WriteIndented = true
        };

    public IReadOnlyList<Screening> FindAll()
    {
        return ReadAll();
    }

    public Screening? Find(int id)
    {
        var screenings = ReadAll();

        return screenings.FirstOrDefault(screening => screening.Id == id);
    }

    public void Save(Screening screening)
    {
        var screenings = ReadAll();

        var index = screenings.FindIndex(
            existing => existing.Id == screening.Id);

        if (index == -1)
        {
            throw new InvalidOperationException(
                $"Fant ikke kinovisning {screening.Id}.");
        }

        screenings[index] = screening;

        var json = JsonSerializer.Serialize(screenings, JsonOptions);

        File.WriteAllText(FilePath, json);
    }

    private static List<Screening> ReadAll()
    {
        var json = File.ReadAllText(FilePath);

        return JsonSerializer.Deserialize<List<Screening>>(json)
               ?? new List<Screening>();
    }
}
