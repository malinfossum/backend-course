using BookCatalog.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// The only line that knows which storage mechanism is in use. Swapping
// FileBookRepository for SqlBookRepository here is the whole point of this
// week's task — nothing below this line had to change.
builder.Services.AddScoped<IBookRepository, SqlBookRepository>();

var app = builder.Build();

// GET /books
// GET /books?author=Ada
// GET /books?available=true
// GET /books?author=Ada&available=true
// GET /books?sort=year
app.MapGet("/books", async (
    string? author,
    bool? available,
    string? sort,
    IBookRepository repository) =>
{
    var books = await repository.SearchAsync(
        author,
        available,
        sortByYear: string.Equals(sort, "year", StringComparison.OrdinalIgnoreCase));

    return Results.Ok(books);
});

app.MapGet("/books/{id:int}", async (int id, IBookRepository repository) =>
{
    var book = await repository.FindAsync(id);

    if (book == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(book);
});

app.Run();
