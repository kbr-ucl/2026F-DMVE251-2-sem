using MinKlinik.Domain.Exceptions;
using MinKlinik.Facade.DTOs;
using MinKlinik.Facade.UseCases;

namespace MinKlinik.UseCases.Konsultationer;

public class AflysKonsultationUseCase : IAflysKonsultationUseCase
{
    private readonly IKonsultationRepository _repo;

    public AflysKonsultationUseCase(IKonsultationRepository repo)
    {
        _repo = repo;
    }

    public async Task Udfør(AflysKonsultationRequest request)
    {
        var konsultation = await _repo.HentAsync(request.KonsultationId)
            ?? throw new NotFoundException("Konsultation ikke fundet.");

        konsultation.Aflys();

        await _repo.GemAsync();
    }
}
