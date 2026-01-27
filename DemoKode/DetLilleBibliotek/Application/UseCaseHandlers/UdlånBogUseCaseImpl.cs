using Facade.UseCases;
using Application.InfrastructureFacade;

namespace Application.UseCaseHandlers;

public class UdlånBogUseCaseImpl : IUdlånBogUseCase
{
    // Vi bruger Repositories (abstraktioner) til at hente data
    private readonly IBogRepository _bogRepository;
    private readonly IMedlemsRepository _medlemsRepository;

    public UdlånBogUseCaseImpl(IBogRepository bogRepo, IMedlemsRepository medlRepo)
    {
        _bogRepository = bogRepo;
        _medlemsRepository = medlRepo;
    }

    // Denne metode kaldes, når brugeren klikker på "LÅN" knappen
    void IUdlånBogUseCase.LånAfBogTilMedlem(UdlånBogCommmandDto commmandDto)
    {
        // 1. Hent data (Rehydrering)
        var medlem = _medlemsRepository.HentPåId(commmandDto.MedlemsId);
        var bog = _bogRepository.HentPåId(commmandDto.BogId);

        if (medlem == null || bog == null) throw new Exception("Medlem eller bog findes ikke.");

        // 2. Aktiver Adfærd (Domain Logic)
        try
        {
            // Vi beder medlemmet om at udføre handlingen.
            // Hvis bogen er udlånt, eller medlemmet har for mange bøger,
            // vil koden her kaste en fejl og stoppe.
            medlem.LånBog(bog);
        }
        catch (InvalidOperationException ex)
        {
            // Send fejlbesked tilbage til brugeren
            Console.WriteLine($"Kunne ikke låne bog: {ex.Message}");
            return;
        }

        // 3. Gem tilstand (Persistering)
        _medlemsRepository.Gem(medlem);

        Console.WriteLine("Succes: Bogen er nu registreret som udlånt.");
    }
}
