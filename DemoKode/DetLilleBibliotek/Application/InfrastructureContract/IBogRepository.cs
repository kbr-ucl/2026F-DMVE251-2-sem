using Domain.Entities;

namespace Application.InfrastructureContract;

public interface IBogRepository
{
    Bog HentPåId(Guid id);
    void Gem(Bog bog);
}
