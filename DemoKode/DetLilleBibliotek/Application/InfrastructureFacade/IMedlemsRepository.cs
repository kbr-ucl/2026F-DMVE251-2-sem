using Domain.Entities;

namespace Application.InfrastructureFacade;

public interface IMedlemsRepository
{
    Medlem HentPåId(Guid id);
    void Gem(Medlem medlem);
}
