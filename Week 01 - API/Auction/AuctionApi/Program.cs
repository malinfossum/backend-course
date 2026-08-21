using System.Text.Json;
using System.Text.Encodings.Web;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var filename = "auctions.json";

var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
};

var auctions = await LoadAuctionsAsync(filename, jsonOptions);
var nextId = auctions.Count == 0 ? 1 : auctions.Max(a => a.Id) + 1;

app.MapGet("/auctions", (bool? active) =>
{
    if (active == true)
    {
        return Results.Ok(auctions.Where(a => !a.IsClosed));
    }

    return Results.Ok(auctions);
});

app.MapGet("/auctions/{id}", (int id) =>
{
    var auction = auctions.FirstOrDefault(a => a.Id == id);

    return auction is null
        ? Results.NotFound()
        : Results.Ok(auction);
});

app.MapGet("/auctions/{id}/bids", (int id) =>
{
    var auction = auctions.FirstOrDefault(a => a.Id == id);

    return auction is null
        ? Results.NotFound()
        : Results.Ok(auction.Bids);
});

app.MapPost("/auctions", async (CreateAuctionDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.ItemName))
    {
        return Results.BadRequest("The auction must have a name.");
    }

    if (dto.StartingPrice < 0)
    {
        return Results.BadRequest("The starting price cannot be negative.");
    }

    var auction = new Auction
    {
        Id = nextId++,
        ItemName = dto.ItemName.Trim(),
        CurrentBid = dto.StartingPrice
    };

    auctions.Add(auction);
    await SaveAuctionsAsync(filename, auctions, jsonOptions);

    return Results.Created($"/auctions/{auction.Id}", auction);
});

app.MapPost("/auctions/{id}/bids", async (int id, PlaceBidDto dto) =>
{
    var auction = auctions.FirstOrDefault(a => a.Id == id);

    if (auction is null)
    {
        return Results.NotFound();
    }

    var result = auction.PlaceBid(dto.BidderName, dto.Amount);

    if (!result.Success)
    {
        return Results.BadRequest(result.ErrorMessage);
    }

    await SaveAuctionsAsync(filename, auctions, jsonOptions);

    return Results.Ok(auction);
});

app.MapPatch("/auctions/{id}", async (int id, UpdateAuctionDto dto) =>
{
    var auction = auctions.FirstOrDefault(a => a.Id == id);

    if (auction is null)
    {
        return Results.NotFound();
    }

    if (dto.IsClosed is null)
    {
        return Results.BadRequest("Nothing to update.");
    }

    if (dto.IsClosed == false)
    {
        return Results.BadRequest("A closed auction cannot be reopened.");
    }

    var wasClosedNow = auction.Close();

    if (!wasClosedNow)
    {
        return Results.BadRequest("Auksjonen er allerede avsluttet.");
    }

    await SaveAuctionsAsync(filename, auctions, jsonOptions);

    return Results.Ok(auction);
});

app.MapDelete("/auctions/{id}", async (int id) =>
{
    var auction = auctions.FirstOrDefault(a => a.Id == id);

    if (auction is null)
    {
        return Results.NotFound();
    }

    auctions.Remove(auction);
    await SaveAuctionsAsync(filename, auctions, jsonOptions);

    return Results.NoContent();
});

app.Run();

static async Task<List<Auction>> LoadAuctionsAsync(
    string path,
    JsonSerializerOptions options)
{
    if (!File.Exists(path))
    {
        return new List<Auction>
        {
            new Auction
            {
                Id = 1,
                ItemName = "Vintage Nintendo Game Boy",
                CurrentBid = 500
            },

            new Auction
            {
                Id = 2,
                ItemName = "LEGO Millennium Falcon",
                CurrentBid = 1000
            },

            new Auction
            {
                Id = 3,
                ItemName = "Old Commodore 64",
                CurrentBid = 750
            }
        };
    }

    var json = await File.ReadAllTextAsync(path);

    return JsonSerializer.Deserialize<List<Auction>>(json, options)
           ?? new List<Auction>();
}

static async Task SaveAuctionsAsync(
    string path,
    List<Auction> auctions,
    JsonSerializerOptions options)
{
    var json = JsonSerializer.Serialize(auctions, options);
    await File.WriteAllTextAsync(path, json);
}
