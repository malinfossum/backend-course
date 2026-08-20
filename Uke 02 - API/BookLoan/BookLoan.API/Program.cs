using BookLoan.API;
using BookLoan.API.DTO;
using BookLoan.API.DomainServices;
using BookLoan.API.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IBookRepository, FileBookRepository>();
builder.Services.AddScoped<BookLoanService>();

var app = builder.Build();

app.MapPost("/loans", (BorrowBookRequest request, BookLoanService service) =>
{
    try
    {
        service.BorrowBook(request.BookId, request.UserName);

        return Results.Ok();
    }
    catch (Exception exception)
    {
        return Results.BadRequest(exception.Message);
    }
});

app.MapPost("/returns", (ReturnBookRequest request, BookLoanService service) =>
{
    try
    {
        service.ReturnBook(request.BookId, request.UserName);

        return Results.Ok();
    }
    catch (Exception exception)
    {
        return Results.BadRequest(exception.Message);
    }
});

app.Run();
