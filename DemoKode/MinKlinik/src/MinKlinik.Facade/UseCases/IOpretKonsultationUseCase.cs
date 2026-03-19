using MinKlinik.Facade.DTOs;

namespace MinKlinik.Facade.UseCases;

public interface IOpretKonsultationUseCase
{
    Task Udfør(OpretKonsultationRequest request);
}
