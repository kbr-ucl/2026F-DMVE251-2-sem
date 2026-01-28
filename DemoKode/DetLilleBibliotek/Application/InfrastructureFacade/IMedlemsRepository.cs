using Domain.Entities;

namespace Application.InfrastructureFacade;

public interface IMedlemsRepository
{
    Medlem Hent(int medlemsNummer);
    void Opret(Medlem medlem);
    void Opdater(Medlem medlem);
}
