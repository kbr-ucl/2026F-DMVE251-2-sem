using MinKlinik.Facade.DTOs;
using MinKlinik.Facade.Queries;
using MinKlinik.Facade.UseCases;

namespace MinKlinik.Blazor.Services;

public class KlinikUiService : IKlinikUiService
{
    private readonly IBehandlingstypeQueries _behandlingstypeQueries;
    private readonly IPatientQueries _patientQueries;
    private readonly IBehandlerQueries _behandlerQueries;
    private readonly IKonsultationQueries _konsultationQueries;
    private readonly IOpretKonsultationUseCase _opretKonsultationUseCase;
    private readonly IAfslutKonsultationUseCase _afslutKonsultationUseCase;
    private readonly IAflysKonsultationUseCase _aflysKonsultationUseCase;

    public KlinikUiService(
        IBehandlingstypeQueries behandlingstypeQueries,
        IPatientQueries patientQueries,
        IBehandlerQueries behandlerQueries,
        IKonsultationQueries konsultationQueries,
        IOpretKonsultationUseCase opretKonsultationUseCase,
        IAfslutKonsultationUseCase afslutKonsultationUseCase,
        IAflysKonsultationUseCase aflysKonsultationUseCase)
    {
        _behandlingstypeQueries = behandlingstypeQueries;
        _patientQueries = patientQueries;
        _behandlerQueries = behandlerQueries;
        _konsultationQueries = konsultationQueries;
        _opretKonsultationUseCase = opretKonsultationUseCase;
        _afslutKonsultationUseCase = afslutKonsultationUseCase;
        _aflysKonsultationUseCase = aflysKonsultationUseCase;
    }

    public Task<IReadOnlyList<BehandlingstypeDto>> HentBehandlingstyperAsync()
        => _behandlingstypeQueries.HentAlleAsync();

    public Task<IReadOnlyList<PatientDto>> HentPatienterAsync()
        => _patientQueries.HentAlleAsync();

    public Task<IReadOnlyList<BehandlerDto>> HentBehandlereAsync()
        => _behandlerQueries.HentAlleAsync();

    public Task<IReadOnlyList<KonsultationDto>> HentKonsultationerAsync()
        => _konsultationQueries.HentAlleAsync();

    public async Task<IReadOnlyList<KonsultationDto>> HentAktiveKonsultationerAsync()
    {
        var alle = await _konsultationQueries.HentAlleAsync();
        return alle.Where(x => x.Status != "Aflyst").ToList();
    }

    public Task OpretKonsultationAsync(OpretKonsultationRequest request)
        => _opretKonsultationUseCase.Udfør(request);

    public Task AfslutKonsultationAsync(AfslutKonsultationRequest request)
        => _afslutKonsultationUseCase.Udfør(request);

    public Task AflysKonsultationAsync(AflysKonsultationRequest request)
        => _aflysKonsultationUseCase.Udfør(request);
}
