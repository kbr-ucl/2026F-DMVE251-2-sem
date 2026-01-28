using Application.InfrastructureFacade;
using Domain.Entities;
using Facade.UseCases;

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
        var medlem = _medlemsRepository.Hent(commmandDto.Medlemsnummer);
        var bog = _bogRepository.Hent(commmandDto.Isbn);

        if (medlem == null || bog == null) throw new Exception("Medlem eller bog findes ikke.");

        // 2. Aktiver Adfærd (Domain Logic)

        // Vi beder medlemmet om at udføre handlingen.
        // Hvis bogen er udlånt, eller medlemmet har for mange bøger,
        // vil koden her kaste en fejl og stoppe.
        medlem.LånBog(bog);

        // 3. Gem tilstand (Persistering)
        _medlemsRepository.Opdater(medlem);
    }
}

public class OpretMedlemUseCaseImpl : IOpretMedlemUseCase
{
    // Vi bruger Repositories (abstraktioner) til at hente data
    private readonly IMedlemsRepository _medlemsRepository;

    public OpretMedlemUseCaseImpl(IMedlemsRepository medlRepo)
    {
        _medlemsRepository = medlRepo;
    }

    // Denne metode kaldes, når brugeren klikker på "OPRET MEDLEM" knappen
    void IOpretMedlemUseCase.OpretMedlem(OpretMedlemCommandDto commandDto)
    {
        // Pre-conditions
        // Tjek om medlem allerede findes
        var eksisterendeMedlem = _medlemsRepository.Hent(commandDto.Medlemsnummer);
        if (eksisterendeMedlem != null) throw new Exception("Medlem med dette medlemsnummer findes allerede.");

        // Opret nyt medlem
        var nytMedlem = new Medlem(commandDto.Medlemsnummer, commandDto.Navn);

        // Gem nyt medlem
        _medlemsRepository.Gem(nytMedlem);

    }
}