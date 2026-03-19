using MinKlinik.Domain.Exceptions;

namespace MinKlinik.Domain.Entities;

/// <summary>
/// AGGREGATE ROOT: Behandlingstype
///
/// Identificeret som Aggregate Root fordi:
///   1. Egen livscyklus — behandlingstyper administreres uafhængigt (stamdata)
///   2. Transaktionsgrænse — ændres uafhængigt af konsultationer
///   3. Eget repository — IBehandlingstypeRepository
///   4. Refereres via FK fra Konsultation
/// </summary>
public class Behandlingstype : AggregateRoot
{
    public string Navn { get; private set; } = string.Empty;

    private Behandlingstype() { }

    public Behandlingstype(string navn)
    {
        if (string.IsNullOrWhiteSpace(navn))
            throw new DomainException("Behandlingstype skal have et navn.");

        Id = Guid.NewGuid();
        Navn = navn;
    }
}
