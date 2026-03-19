using MinKlinik.Domain.Entities;

namespace MinKlinik.UseCases;

// Repository-interfaces: kontrakt for datadgang til domæneobjekter.
// Placeres i Use Case-laget — implementeres i Infrastructure.

public interface IKonsultationRepository
{
    Task<Konsultation?> HentAsync(Guid id);
    Task<IReadOnlyList<Konsultation>> HentForPatientAsync(Guid patientId);
    Task<IReadOnlyList<Konsultation>> HentForBehandlerAsync(Guid behandlerId);
    Task TilføjAsync(Konsultation konsultation);
    Task GemAsync();
}

public interface IBehandlingstypeRepository
{
    Task<Behandlingstype?> HentAsync(Guid id);
}

public interface IPatientRepository
{
    Task<Patient?> HentAsync(Guid id);
}

public interface IBehandlerRepository
{
    Task<Behandler?> HentAsync(Guid id);
}
