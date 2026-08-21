using CinemaBooking.Api;
using CinemaBooking.Api.DTO;
using CinemaBooking.Api.DomainServices;
using CinemaBooking.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// The DI container: who to hand out when somebody asks for an
// IScreeningRepository, and that ScreeningService can be built automatically.
builder.Services.AddScoped<IScreeningRepository, FileScreeningRepository>();
builder.Services.AddScoped<ScreeningService>();

var app = builder.Build();

app.MapGet("/screenings", (ScreeningService service) =>
{
    return Results.Ok(service.GetAll());
});

app.MapGet("/screenings/{id:int}", (int id, ScreeningService service) =>
{
    return ToHttp(service.GetById(id));
});

app.MapPost("/screenings/{id:int}/reservations",
    (int id, ReserveSeatDto request, ScreeningService service) =>
{
    return ToHttp(service.ReserveSeat(
        id,
        request.CustomerName,
        request.SeatNumber));
});

app.MapDelete("/screenings/{id:int}/reservations/{seatNumber:int}",
    (int id, int seatNumber, string customerName, ScreeningService service) =>
{
    // customerName comes from the query string:
    // DELETE /screenings/1/reservations/6?customerName=Ada
    return ToHttp(service.CancelReservation(id, seatNumber, customerName));
});

app.Run();

// The single place where the service's world meets HTTP. Everything above
// returns Result<T>; nothing above this line mentions a status code.
static IResult ToHttp<T>(Result<T> result)
{
    if (result.IsSuccess)
    {
        return Results.Ok(result.Value);
    }

    return result.Error switch
    {
        ErrorKind.NotFound => Results.NotFound(result.ErrorMessage),
        ErrorKind.Conflict => Results.Conflict(result.ErrorMessage),
        _ => Results.BadRequest(result.ErrorMessage)
    };
}
