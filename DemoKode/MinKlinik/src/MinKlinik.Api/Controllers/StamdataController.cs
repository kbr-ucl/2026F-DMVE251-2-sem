using Microsoft.AspNetCore.Mvc;
using MinKlinik.Facade.Queries;

namespace MinKlinik.Api.Controllers;

/// <summary>
/// Stamdata: behandlingstyper, patienter, behandlere.
/// Bruges til at slå ID'er op til brug i OpretKonsultation.
/// </summary>
[ApiController]
[Route("api/stamdata")]
public class StamdataController : ControllerBase
{
    private readonly IBehandlingstypeQueries _behandlingstyper;
    private readonly IPatientQueries _patienter;
    private readonly IBehandlerQueries _behandlere;

    public StamdataController(
        IBehandlingstypeQueries behandlingstyper,
        IPatientQueries patienter,
        IBehandlerQueries behandlere)
    {
        _behandlingstyper = behandlingstyper;
        _patienter = patienter;
        _behandlere = behandlere;
    }

    [HttpGet("behandlingstyper")]
    public async Task<IActionResult> HentBehandlingstyper()
        => Ok(await _behandlingstyper.HentAlleAsync());

    [HttpGet("patienter")]
    public async Task<IActionResult> HentPatienter()
        => Ok(await _patienter.HentAlleAsync());

    [HttpGet("behandlere")]
    public async Task<IActionResult> HentBehandlere()
        => Ok(await _behandlere.HentAlleAsync());
}
