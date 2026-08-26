using CouponApi.Api;
using CouponApi.Api.Data;
using CouponApi.Api.Dtos;
using CouponApi.Api.Models;
using CouponApi.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// The only line that knows the coupons live in SQL Server. Everything above
// this file talks to ICouponRepository.
builder.Services.AddScoped<ICouponRepository, SqlCouponRepository>();

// Registered as the concrete class, the way Terje does it - the repository has
// an interface, the service does not. That asymmetry is still the open question
// for Friday, so the code keeps it rather than quietly picking a side.
builder.Services.AddScoped<CouponService>();

var app = builder.Build();

app.MapGet("/coupons", async (CouponService service) =>
{
    var coupons = await service.GetAllAsync();

    return Results.Ok(coupons);
});

app.MapGet("/coupons/{id:int}", async (int id, CouponService service) =>
{
    var result = await service.GetByIdAsync(id);

    return ToHttpResult(result);
});

app.MapPost("/coupons", async (CreateCouponDto dto, CouponService service) =>
{
    var result = await service.CreateCouponAsync(dto);

    if (!result.IsSuccess)
    {
        return ToHttpResult(result);
    }

    // 201 with the address of the thing that was created. The id in that URL
    // came from the database, not from the request.
    return Results.Created($"/coupons/{result.Value!.Id}", result.Value);
});

app.MapPost("/coupons/{id:int}/use", async (int id, CouponService service) =>
{
    var result = await service.UseCouponAsync(id);

    return ToHttpResult(result);
});

app.MapPatch("/coupons/{id:int}/deactivate", async (int id, CouponService service) =>
{
    var result = await service.DeactivateCouponAsync(id);

    return ToHttpResult(result);
});

// Challenge 2
app.MapPatch("/coupons/{id:int}/activate", async (int id, CouponService service) =>
{
    var result = await service.ActivateCouponAsync(id);

    return ToHttpResult(result);
});

// Challenge 3
app.MapPatch("/coupons/{id:int}/add-uses", async (int id, AddUsesDto dto, CouponService service) =>
{
    var result = await service.AddUsesAsync(id, dto.Amount);

    return ToHttpResult(result);
});

app.MapDelete("/coupons/{id:int}", async (int id, CouponService service) =>
{
    var result = await service.DeleteCouponAsync(id);

    return ToHttpResult(result);
});

app.Run();

// The one place where the application's own vocabulary becomes HTTP. The
// service never says 404 - it says NotFound, and this decides what that is
// worth on the wire.
static IResult ToHttpResult(Result<Coupon> result)
{
    if (result.IsSuccess)
    {
        return Results.Ok(result.Value);
    }

    return result.Error switch
    {
        ErrorKind.NotFound => Results.NotFound(new { error = result.ErrorMessage }),
        ErrorKind.Conflict => Results.Conflict(new { error = result.ErrorMessage }),
        _ => Results.BadRequest(new { error = result.ErrorMessage })
    };
}
