using Domain.Entities;

namespace Application.InfrastructureContract;

public interface IMedlemsRepository
{
    Medlem HentPåId(Guid id);
    void Gem(Medlem medlem);
}
