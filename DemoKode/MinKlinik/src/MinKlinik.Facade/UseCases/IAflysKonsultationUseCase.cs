using MinKlinik.Facade.DTOs;

namespace MinKlinik.Facade.UseCases;

public interface IAflysKonsultationUseCase
{
    Task Udfør(AflysKonsultationRequest request);
}
