using CinemaBooking.Api;
using CinemaBooking.Api.DomainModel;
using CinemaBooking.Api.DomainServices;
using Moq;

namespace CinemaBooking.Tests;

// Challenge 3 - the same rules, tested with Moq instead of the hand written
// fake. Kept as a parallel class so both versions can be compared side by
// side; the fake based tests in ScreeningServiceTests are untouched.
//
// What Moq changes: Setup(...).Returns(...) replaces the dictionary, .Object
// is the instance the service actually receives, and Verify replaces the
// SaveCount property.
//
// What Moq does NOT change: Returns hands back exactly the object it was
// given, so the mock shares a reference with the test the same way the fake
// originally did. An assertion on that local object stays green even if the
// service never calls Save. Only Verify catches it. That is the honest
// comparison between the two files.
public class ScreeningServiceTestsMoq
{
    private static Screening Interstellar(
        params (int Seat, string Customer)[] reservations)
    {
        return new Screening
        {
            Id = 1,
            MovieTitle = "Interstellar",
            NumberOfSeats = 10,
            Reservations = reservations
                .Select(reservation => new Reservation
                {
                    SeatNumber = reservation.Seat,
                    CustomerName = reservation.Customer
                })
                .ToList()
        };
    }

    [Test]
    public void ReserveSeat_WhenSeatIsFree_Succeeds()
    {
        var screening = Interstellar((2, "Grace"), (5, "Alan"));

        var repository = new Mock<IScreeningRepository>();

        repository
            .Setup(instance => instance.Find(1))
            .Returns(screening);

        var service = new ScreeningService(repository.Object);


        var result = service.ReserveSeat(1, "Ada", 7);


        Assert.That(result.IsSuccess, Is.True);

        Assert.That(result.Value!.SeatNumber, Is.EqualTo(7));

        // The claim that the reservation was written back. Without this line
        // the test would pass with Save deleted from the service.
        repository.Verify(
            instance => instance.Save(screening),
            Times.Once);
    }

    [Test]
    public void ReserveSeat_WhenSeatIsAlreadyTaken_DoesNotSave()
    {
        var screening = Interstellar((2, "Grace"), (5, "Alan"));

        var repository = new Mock<IScreeningRepository>();

        repository
            .Setup(instance => instance.Find(1))
            .Returns(screening);

        var service = new ScreeningService(repository.Object);


        var result = service.ReserveSeat(1, "Ada", 5);


        Assert.That(result.Error, Is.EqualTo(ErrorKind.Conflict));

        repository.Verify(
            instance => instance.Save(It.IsAny<Screening>()),
            Times.Never);
    }

    [Test]
    public void ReserveSeat_WhenScreeningDoesNotExist_FailsWithNotFound()
    {
        var repository = new Mock<IScreeningRepository>();

        // Nothing is configured for id 999. A loose mock answers null, which
        // is precisely the case being tested here - but it is worth knowing
        // that a forgotten Setup fails the same silent way.
        var service = new ScreeningService(repository.Object);


        var result = service.ReserveSeat(999, "Ada", 7);


        Assert.That(result.Error, Is.EqualTo(ErrorKind.NotFound));

        repository.Verify(
            instance => instance.Save(It.IsAny<Screening>()),
            Times.Never);
    }

    [Test]
    public void CancelReservation_WhenOwnerCancels_SavesTheChange()
    {
        var screening = Interstellar((2, "Grace"), (5, "Ada"));

        var repository = new Mock<IScreeningRepository>();

        repository
            .Setup(instance => instance.Find(1))
            .Returns(screening);

        var service = new ScreeningService(repository.Object);


        var result = service.CancelReservation(1, 5, "Ada");


        Assert.That(result.IsSuccess, Is.True);

        repository.Verify(
            instance => instance.Save(screening),
            Times.Once);
    }
}
