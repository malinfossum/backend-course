using CinemaBooking.Api;
using CinemaBooking.Api.DTO;
using CinemaBooking.Api.DomainServices;
using CinemaBooking.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// DI-containeren: hvem som skal leveres når noen ber om et
// IScreeningRepository, og at ScreeningService kan opprettes automatisk.
builder.Services.AddScoped<IScreeningRepository, FileScreeningRepository>();
builder.Services.AddScoped<ScreeningService>();

var app = builder.Build();

app.MapGet("/screenings", (ScreeningService service) =>
{
    return Results.Ok(service.GetAll());
});

app.MapGet("/screenings/{id:int}", (int id, ScreeningService service) =>
{
    var result = service.GetById(id);

    if (result.IsSuccess)
    {
        return Results.Ok(result.Value);
    }

    return Results.NotFound(result.ErrorMessage);
});

app.MapPost("/screenings/{id:int}/reservations",
    (int id, ReserveSeatDto request, ScreeningService service) =>
{
    // Servicen svarer med Result<T>. Endepunktet oversetter det til HTTP.
    var result = service.ReserveSeat(
        id,
        request.CustomerName,
        request.SeatNumber);

    if (result.IsSuccess)
    {
        return Results.Ok(result.Value);
    }

    return Results.BadRequest(result.ErrorMessage);
});

app.Run();
