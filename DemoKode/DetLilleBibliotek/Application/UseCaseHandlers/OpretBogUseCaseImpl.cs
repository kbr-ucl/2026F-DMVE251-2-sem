using Application.InfrastructureFacade;
using Domain.Entities;
using Facade.UseCases;

namespace Application.UseCaseHandlers;

public class OpretBogUseCaseImpl : IOpretBogUseCase
{
    // Vi bruger Repositories (abstraktioner) til at hente data
    private readonly IBogRepository _bogRepo;

    public OpretBogUseCaseImpl(IBogRepository bogRepo)
    {
        _bogRepo = bogRepo;
    }

    // Denne metode kaldes, når brugeren klikker på "OPRET BOG" knappen
    void IOpretBogUseCase.OpretBog(OpretBogCommandDto commandDto)
    {
        // Pre-conditions
        // Tjek om bog allerede findes
        var eksisterendeBog = _bogRepo.Hent(commandDto.Isbn);
        if (eksisterendeBog != null) throw new Exception("Bog med dette ISBN nummer findes allerede.");

        // Opret nyt medlem
        var nyBog = new Bog(commandDto.Isbn, commandDto.Forfatter, commandDto.Titel);

        // Gem nyt medlem
        _bogRepo.Opret(nyBog);
    }
}