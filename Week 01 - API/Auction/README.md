# Auction

Week 1: Minimal API, DTOs, `.http` files, JSON file persistence, async.

An auction API with seven endpoints: list, read, create, update and delete auctions, and list and
place bids. Data is stored in `auctions.json`, read and written asynchronously. The API also serves a
small web front end from `wwwroot/` on the same address.

## Running it

From the repository root:

```bash
dotnet run --project "Week 01 - API/Auction/AuctionApi"
```

`AuctionApi/AuctionApi.http` has a request for every endpoint.
