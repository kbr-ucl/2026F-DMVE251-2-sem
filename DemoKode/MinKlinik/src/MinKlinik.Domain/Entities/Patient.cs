using MinKlinik.Domain.Exceptions;

namespace MinKlinik.Domain.Entities;

/// <summary>
/// AGGREGATE ROOT: Patient
///
/// Identificeret som Aggregate Root fordi:
///   1. Egen livscyklus — patienten eksisterer uafhængigt af sine konsultationer
///   2. Transaktionsgrænse — patientdata ændres uafhængigt af konsultationer
///   3. Eget repository — IPatientRepository
///   4. Refereres via FK fra Konsultation (Konsultation ejer IKKE patienten)
/// </summary>
public class Patient : AggregateRoot
{
    public string Navn { get; private set; } = string.Empty;
    public string CprNummer { get; private set; } = string.Empty;

    private Patient() { }

    public Patient(string navn, string cprNummer)
    {
        if (string.IsNullOrWhiteSpace(navn))
            throw new DomainException("Patient skal have et navn.");
        if (string.IsNullOrWhiteSpace(cprNummer))
            throw new DomainException("CPR-nummer er påkrævet.");

        Id = Guid.NewGuid();
        Navn = navn;
        CprNummer = cprNummer;
    }
}
