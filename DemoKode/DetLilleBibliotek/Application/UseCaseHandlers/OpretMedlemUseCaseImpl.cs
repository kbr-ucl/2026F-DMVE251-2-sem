using Application.InfrastructureFacade;
using Domain.Entities;
using Facade.UseCases;

namespace Application.UseCaseHandlers;

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