using CinemaBooking.Api.DomainModel;

namespace CinemaBooking.Api.DomainServices;

// Beskriver hva servicen trenger, ikke hvordan det lagres. Ordene "fil" og
// "JSON" finnes ikke her - det er hele poenget med interfacet.
public interface IScreeningRepository
{
    IReadOnlyList<Screening> FindAll();

    Screening? Find(int id);

    void Save(Screening screening);
}
