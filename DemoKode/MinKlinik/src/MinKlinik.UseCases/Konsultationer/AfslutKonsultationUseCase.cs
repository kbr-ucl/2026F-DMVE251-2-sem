using MinKlinik.Domain.Exceptions;
using MinKlinik.Facade.DTOs;
using MinKlinik.Facade.UseCases;

namespace MinKlinik.UseCases.Konsultationer;

public class AfslutKonsultationUseCase : IAfslutKonsultationUseCase
{
    private readonly IKonsultationRepository _repo;

    public AfslutKonsultationUseCase(IKonsultationRepository repo)
    {
        _repo = repo;
    }

    public async Task Udfør(AfslutKonsultationRequest request)
    {
        var konsultation = await _repo.HentAsync(request.KonsultationId)
            ?? throw new NotFoundException("Konsultation ikke fundet.");

        konsultation.Afslut(request.Notat);

        await _repo.GemAsync();
    }
}
