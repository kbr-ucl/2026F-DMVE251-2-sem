using MinKlinik.Facade.DTOs;

namespace MinKlinik.Blazor.Services;

public interface IKlinikUiService
{
    Task<IReadOnlyList<BehandlingstypeDto>> HentBehandlingstyperAsync();
    Task<IReadOnlyList<PatientDto>> HentPatienterAsync();
    Task<IReadOnlyList<BehandlerDto>> HentBehandlereAsync();
    Task<IReadOnlyList<KonsultationDto>> HentKonsultationerAsync();
    Task<IReadOnlyList<KonsultationDto>> HentAktiveKonsultationerAsync();
    Task OpretKonsultationAsync(OpretKonsultationRequest request);
    Task AfslutKonsultationAsync(AfslutKonsultationRequest request);
    Task AflysKonsultationAsync(AflysKonsultationRequest request);
}
