using CinemaBooking.Api;
using CinemaBooking.Api.DomainModel;

namespace CinemaBooking.Tests;

public class ScreeningServiceTests
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

    private static int[] ReservedSeats(FakeScreeningRepository repository)
    {
        return repository.Find(1)!
            .Reservations
            .Select(reservation => reservation.SeatNumber)
            .OrderBy(seatNumber => seatNumber)
            .ToArray();
    }

    [Test]
    public void ReserveSeat_WhenSeatIsFree_Succeeds()
    {
        var repository = new FakeScreeningRepository(
            Interstellar((2, "Grace"), (5, "Alan")));

        var service = new ScreeningService(repository);


        var result = service.ReserveSeat(1, "Ada", 7);


        Assert.That(result.IsSuccess, Is.True);

        Assert.That(result.Value!.MovieTitle, Is.EqualTo("Interstellar"));

        Assert.That(result.Value!.CustomerName, Is.EqualTo("Ada"));

        Assert.That(result.Value!.SeatNumber, Is.EqualTo(7));
    }

    [Test]
    public void ReserveSeat_WhenSeatIsFree_PersistsThroughRepository()
    {
        var repository = new FakeScreeningRepository(
            Interstellar((2, "Grace"), (5, "Alan")));

        var service = new ScreeningService(repository);


        service.ReserveSeat(1, "Ada", 7);


        // Read back through the repository, not from a local variable. If the
        // service forgot to call Save, storage would still hold the old copy
        // and this assertion would fail on its own.
        Assert.That(ReservedSeats(repository), Is.EqualTo(new[] { 2, 5, 7 }));

        Assert.That(repository.Find(1)!.ReservationFor(7)!.CustomerName,
            Is.EqualTo("Ada"));

        Assert.That(repository.SaveCount, Is.EqualTo(1));
    }

    [Test]
    public void ReserveSeat_WhenSeatIsAlreadyTaken_FailsWithConflict()
    {
        var repository = new FakeScreeningRepository(
            Interstellar((2, "Grace"), (5, "Alan")));

        var service = new ScreeningService(repository);


        var result = service.ReserveSeat(1, "Ada", 5);


        Assert.That(result.IsSuccess, Is.False);

        Assert.That(result.Error, Is.EqualTo(ErrorKind.Conflict));

        Assert.That(result.ErrorMessage,
            Is.EqualTo("Seat 5 is already reserved."));

        Assert.That(ReservedSeats(repository), Is.EqualTo(new[] { 2, 5 }));

        Assert.That(repository.SaveCount, Is.EqualTo(0));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(11)]
    [TestCase(15)]
    public void ReserveSeat_WhenSeatNumberIsInvalid_FailsWithValidation(
        int seatNumber)
    {
        var repository = new FakeScreeningRepository(
            Interstellar((2, "Grace"), (5, "Alan")));

        var service = new ScreeningService(repository);


        var result = service.ReserveSeat(1, "Ada", seatNumber);


        Assert.That(result.IsSuccess, Is.False);

        Assert.That(result.Error, Is.EqualTo(ErrorKind.Validation));

        Assert.That(ReservedSeats(repository), Is.EqualTo(new[] { 2, 5 }));

        Assert.That(repository.SaveCount, Is.EqualTo(0));
    }

    [Test]
    public void ReserveSeat_WhenScreeningDoesNotExist_FailsWithNotFound()
    {
        var repository = new FakeScreeningRepository(
            Interstellar((2, "Grace"), (5, "Alan")));

        var service = new ScreeningService(repository);


        var result = service.ReserveSeat(999, "Ada", 7);


        Assert.That(result.IsSuccess, Is.False);

        Assert.That(result.Error, Is.EqualTo(ErrorKind.NotFound));

        Assert.That(result.ErrorMessage,
            Is.EqualTo("No screening with id 999."));

        Assert.That(repository.SaveCount, Is.EqualTo(0));
    }

    [Test]
    public void ReserveSeat_WhenCustomerNameIsMissing_FailsWithValidation()
    {
        var repository = new FakeScreeningRepository(
            Interstellar((2, "Grace"), (5, "Alan")));

        var service = new ScreeningService(repository);


        var result = service.ReserveSeat(1, "", 7);


        Assert.That(result.IsSuccess, Is.False);

        Assert.That(result.Error, Is.EqualTo(ErrorKind.Validation));

        Assert.That(result.ErrorMessage,
            Is.EqualTo("The customer must have a name."));

        Assert.That(ReservedSeats(repository), Is.EqualTo(new[] { 2, 5 }));

        Assert.That(repository.SaveCount, Is.EqualTo(0));
    }

    // Challenge 4 - the rule was written as a failing test first, then the
    // check in ScreeningService was added until it went green.
    [Test]
    public void ReserveSeat_WhenCustomerAlreadyHasFourSeats_FailsWithConflict()
    {
        var repository = new FakeScreeningRepository(
            Interstellar((1, "Ada"), (2, "Ada"), (3, "Ada"), (4, "Ada")));

        var service = new ScreeningService(repository);


        var result = service.ReserveSeat(1, "Ada", 7);


        Assert.That(result.IsSuccess, Is.False);

        Assert.That(result.Error, Is.EqualTo(ErrorKind.Conflict));

        Assert.That(result.ErrorMessage, Does.Contain("maximum"));

        Assert.That(ReservedSeats(repository), Is.EqualTo(new[] { 1, 2, 3, 4 }));

        Assert.That(repository.SaveCount, Is.EqualTo(0));
    }

    [Test]
    public void ReserveSeat_WhenCustomerHasThreeSeats_StillAllowsAFourth()
    {
        var repository = new FakeScreeningRepository(
            Interstellar((1, "Ada"), (2, "Ada"), (3, "Ada")));

        var service = new ScreeningService(repository);


        var result = service.ReserveSeat(1, "Ada", 7);


        Assert.That(result.IsSuccess, Is.True);

        Assert.That(ReservedSeats(repository), Is.EqualTo(new[] { 1, 2, 3, 7 }));
    }

    [Test]
    public void ReserveSeat_CountsSeatsPerCustomer_NotPerScreening()
    {
        var repository = new FakeScreeningRepository(
            Interstellar((1, "Ada"), (2, "Ada"), (3, "Ada"), (4, "Ada")));

        var service = new ScreeningService(repository);


        var result = service.ReserveSeat(1, "Grace", 7);


        Assert.That(result.IsSuccess, Is.True);
    }

    // Challenge 2 - cancellation.

    [Test]
    public void CancelReservation_WhenOwnerCancels_ReleasesTheSeat()
    {
        var repository = new FakeScreeningRepository(
            Interstellar((2, "Grace"), (5, "Ada")));

        var service = new ScreeningService(repository);


        var result = service.CancelReservation(1, 5, "Ada");


        Assert.That(result.IsSuccess, Is.True);

        Assert.That(result.Value!.SeatNumber, Is.EqualTo(5));

        Assert.That(ReservedSeats(repository), Is.EqualTo(new[] { 2 }));

        Assert.That(repository.SaveCount, Is.EqualTo(1));
    }

    [Test]
    public void CancelReservation_MatchesTheNameLoosely()
    {
        var repository = new FakeScreeningRepository(
            Interstellar((5, "Ada")));

        var service = new ScreeningService(repository);


        var result = service.CancelReservation(1, 5, "  ada ");


        Assert.That(result.IsSuccess, Is.True);

        Assert.That(ReservedSeats(repository), Is.Empty);
    }

    [Test]
    public void CancelReservation_WhenSeatIsNotReserved_FailsWithNotFound()
    {
        var repository = new FakeScreeningRepository(
            Interstellar((2, "Grace")));

        var service = new ScreeningService(repository);


        var result = service.CancelReservation(1, 7, "Ada");


        Assert.That(result.IsSuccess, Is.False);

        Assert.That(result.Error, Is.EqualTo(ErrorKind.NotFound));

        Assert.That(result.ErrorMessage,
            Is.EqualTo("Seat 7 is not reserved."));

        Assert.That(repository.SaveCount, Is.EqualTo(0));
    }

    [Test]
    public void CancelReservation_WhenSomebodyElseHoldsTheSeat_FailsWithConflict()
    {
        var repository = new FakeScreeningRepository(
            Interstellar((5, "Grace")));

        var service = new ScreeningService(repository);


        var result = service.CancelReservation(1, 5, "Ada");


        Assert.That(result.IsSuccess, Is.False);

        Assert.That(result.Error, Is.EqualTo(ErrorKind.Conflict));

        Assert.That(ReservedSeats(repository), Is.EqualTo(new[] { 5 }));

        Assert.That(repository.SaveCount, Is.EqualTo(0));
    }

    [Test]
    public void CancelReservation_WhenScreeningDoesNotExist_FailsWithNotFound()
    {
        var repository = new FakeScreeningRepository(Interstellar((5, "Ada")));

        var service = new ScreeningService(repository);


        var result = service.CancelReservation(999, 5, "Ada");


        Assert.That(result.IsSuccess, Is.False);

        Assert.That(result.Error, Is.EqualTo(ErrorKind.NotFound));

        Assert.That(repository.SaveCount, Is.EqualTo(0));
    }

    [TestCase(0)]
    [TestCase(11)]
    public void CancelReservation_WhenSeatNumberIsInvalid_FailsWithValidation(
        int seatNumber)
    {
        var repository = new FakeScreeningRepository(Interstellar((5, "Ada")));

        var service = new ScreeningService(repository);


        var result = service.CancelReservation(1, seatNumber, "Ada");


        Assert.That(result.IsSuccess, Is.False);

        Assert.That(result.Error, Is.EqualTo(ErrorKind.Validation));

        Assert.That(repository.SaveCount, Is.EqualTo(0));
    }
}
