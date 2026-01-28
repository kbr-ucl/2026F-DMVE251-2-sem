using Domain.Entities;

namespace Application.InfrastructureFacade;

public interface IBogRepository
{
    Bog Hent(string isbn);
    void Gem(Bog bog);
}
