using Domain.Entities;

namespace Application.InfrastructureFacade;

public interface IBogRepository
{
    Bog HentPåId(Guid id);
    void Gem(Bog bog);
}
