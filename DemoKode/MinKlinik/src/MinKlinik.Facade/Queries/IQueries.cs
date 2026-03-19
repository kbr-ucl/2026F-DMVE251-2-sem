using MinKlinik.Facade.DTOs;

namespace MinKlinik.Facade.Queries;

// Query-interfaces: returnerer DTO'er, aldrig domain entities

public interface IKonsultationQueries
{
    Task<KonsultationDto?> HentAsync(Guid id);
    Task<IReadOnlyList<KonsultationDto>> HentAlleAsync();
}

public interface IBehandlingstypeQueries
{
    Task<IReadOnlyList<BehandlingstypeDto>> HentAlleAsync();
}

public interface IPatientQueries
{
    Task<IReadOnlyList<PatientDto>> HentAlleAsync();
}

public interface IBehandlerQueries
{
    Task<IReadOnlyList<BehandlerDto>> HentAlleAsync();
}
