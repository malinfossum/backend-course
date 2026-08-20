using CinemaBooking.Api;
using CinemaBooking.Api.DomainModel;

namespace CinemaBooking.Tests;

public class ScreeningServiceTests
{
    private static Screening InterstellarWithSeatsTaken(params int[] taken)
    {
        return new Screening
        {
            Id = 1,
            MovieTitle = "Interstellar",
            NumberOfSeats = 10,
            ReservedSeats = taken.ToList()
        };
    }

    [Test]
    public void ReserveSeat_WhenSeatIsFree_Succeeds()
    {
        var screening = InterstellarWithSeatsTaken(2, 5);

        var repository = new FakeScreeningRepository(screening);

        var service = new ScreeningService(repository);


        var result = service.ReserveSeat(1, "Ada", 7);


        Assert.That(result.IsSuccess, Is.True);

        Assert.That(screening.ReservedSeats, Does.Contain(7));

        Assert.That(result.Value!.MovieTitle, Is.EqualTo("Interstellar"));

        Assert.That(result.Value!.CustomerName, Is.EqualTo("Ada"));

        Assert.That(result.Value!.SeatNumber, Is.EqualTo(7));
    }

    [Test]
    public void ReserveSeat_WhenSeatIsFree_SavesThroughRepository()
    {
        var repository = new FakeScreeningRepository(
            InterstellarWithSeatsTaken(2, 5));

        var service = new ScreeningService(repository);


        service.ReserveSeat(1, "Ada", 7);


        Assert.That(repository.SaveCount, Is.EqualTo(1));
    }

    [Test]
    public void ReserveSeat_WhenSeatIsAlreadyTaken_FailsAndLeavesStateAlone()
    {
        var screening = InterstellarWithSeatsTaken(2, 5);

        var repository = new FakeScreeningRepository(screening);

        var service = new ScreeningService(repository);


        var result = service.ReserveSeat(1, "Ada", 5);


        Assert.That(result.IsSuccess, Is.False);

        Assert.That(result.ErrorMessage,
            Is.EqualTo("Sete 5 er allerede reservert."));

        Assert.That(screening.ReservedSeats, Is.EqualTo(new[] { 2, 5 }));

        Assert.That(repository.SaveCount, Is.EqualTo(0));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(11)]
    [TestCase(15)]
    public void ReserveSeat_WhenSeatNumberIsInvalid_Fails(int seatNumber)
    {
        var screening = InterstellarWithSeatsTaken(2, 5);

        var repository = new FakeScreeningRepository(screening);

        var service = new ScreeningService(repository);


        var result = service.ReserveSeat(1, "Ada", seatNumber);


        Assert.That(result.IsSuccess, Is.False);

        Assert.That(screening.ReservedSeats, Is.EqualTo(new[] { 2, 5 }));

        Assert.That(repository.SaveCount, Is.EqualTo(0));
    }

    [Test]
    public void ReserveSeat_WhenScreeningDoesNotExist_Fails()
    {
        var repository = new FakeScreeningRepository(
            InterstellarWithSeatsTaken(2, 5));

        var service = new ScreeningService(repository);


        var result = service.ReserveSeat(999, "Ada", 7);


        Assert.That(result.IsSuccess, Is.False);

        Assert.That(result.ErrorMessage,
            Is.EqualTo("Fant ikke kinovisning 999."));

        Assert.That(repository.SaveCount, Is.EqualTo(0));
    }

    [Test]
    public void ReserveSeat_WhenCustomerNameIsMissing_Fails()
    {
        var screening = InterstellarWithSeatsTaken(2, 5);

        var repository = new FakeScreeningRepository(screening);

        var service = new ScreeningService(repository);


        var result = service.ReserveSeat(1, "", 7);


        Assert.That(result.IsSuccess, Is.False);

        Assert.That(result.ErrorMessage, Is.EqualTo("Kunden må ha et navn."));

        Assert.That(screening.ReservedSeats, Is.EqualTo(new[] { 2, 5 }));

        Assert.That(repository.SaveCount, Is.EqualTo(0));
    }
}
