# Backend-kurs

Egne oppgaveløsninger fra GET Prepared sitt backend-kurs. Én mappe per uke, som i kursets fagstoff-repo.

## Innhold

| Uke | Prosjekt | Tema |
|-----|----------|------|
| Uke 01 – API | `Auction` | Minimal API, DTO, `.http`-filer, JSON-fil-persistens, async |
| Uke 02 – API | `BookLoan` | Serviceklasser, dependency injection, lifetimes |
| Uke 02 – API | `CinemaBooking` | Unit testing med NUnit, fakes, `Result<T>` |

## Kjøre

```
dotnet run --project "Uke 01 - API/Auction/AuctionApi"
dotnet test "Uke 02 - API/BookLoan/BookLoan.slnx"
dotnet test "Uke 02 - API/CinemaBooking/CinemaBooking.slnx"
```

## Merk

Koden er på norsk. Referanseløsninger fra GetAcademy ligger ikke her — de holdes lokalt i en gitignorert `Løsningsforslag/`-mappe.

Stack: .NET 10, ASP.NET Core Minimal API, NUnit.
