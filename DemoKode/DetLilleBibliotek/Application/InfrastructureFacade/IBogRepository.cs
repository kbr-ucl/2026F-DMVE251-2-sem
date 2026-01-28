using Domain.Entities;

namespace Application.InfrastructureFacade;

public interface IBogRepository
{
    Bog Hent(string isbn);
    void Opret(Bog bog);
}
