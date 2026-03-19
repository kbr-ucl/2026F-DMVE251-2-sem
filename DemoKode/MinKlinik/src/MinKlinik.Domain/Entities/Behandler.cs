using MinKlinik.Domain.Exceptions;

namespace MinKlinik.Domain.Entities;

/// <summary>
/// AGGREGATE ROOT: Behandler
///
/// Identificeret som Aggregate Root fordi:
///   1. Egen livscyklus — behandleren eksisterer uafhængigt af sine konsultationer
///   2. Transaktionsgrænse — behandlerdata ændres uafhængigt
///   3. Eget repository — IBehandlerRepository
///   4. Refereres via FK fra Konsultation
/// </summary>
public class Behandler : AggregateRoot
{
    public string Navn { get; private set; } = string.Empty;
    public string Speciale { get; private set; } = string.Empty;

    private Behandler() { }

    public Behandler(string navn, string speciale)
    {
        if (string.IsNullOrWhiteSpace(navn))
            throw new DomainException("Behandler skal have et navn.");

        Id = Guid.NewGuid();
        Navn = navn;
        Speciale = speciale ?? string.Empty;
    }
}
