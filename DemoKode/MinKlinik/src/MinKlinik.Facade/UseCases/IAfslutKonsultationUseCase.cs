using MinKlinik.Facade.DTOs;

namespace MinKlinik.Facade.UseCases;

public interface IAfslutKonsultationUseCase
{
    Task Udfør(AfslutKonsultationRequest request);
}
