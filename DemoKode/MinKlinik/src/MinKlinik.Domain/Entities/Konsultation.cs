using MinKlinik.Domain.Enums;
using MinKlinik.Domain.Exceptions;
using MinKlinik.Domain.ValueObjects;

namespace MinKlinik.Domain.Entities;

/// <summary>
/// AGGREGATE ROOT: Konsultation
///
/// Identificeret som Aggregate Root fordi:
///   1. Egen livscyklus — oprettes og afsluttes/aflyses uafhængigt
///   2. Transaktionsgrænse — status, notat og tidspunkt ændres som én enhed
///   3. Eget repository — IKonsultationRepository
///   4. Refereres via ID af eventuelle fremtidige aggregater
///
/// Ejer: Tidsinterval (Value Object — ingen egen identitet)
/// Refererer til: Patient, Behandler, Behandlingstype via Guid — IKKE objektreferencer.
/// Aggregater må ikke navigere direkte til andre aggregater.
///
/// Oprettelse sker via factory-metoden Opret(), der håndhæver overlap-regler.
/// Constructoren er privat — det er umuligt at oprette en ugyldig konsultation.
/// </summary>
public class Konsultation : AggregateRoot
{
    public Tidsinterval Tidspunkt { get; private set; } = null!;

    // Andre Aggregate Roots refereres via ID — IKKE via objektreferencer.
    public Guid BehandlingstypeId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid BehandlerId { get; private set; }

    public string Notat { get; private set; } = string.Empty;
    public KonsultationStatus Status { get; private set; }

    // Privat parameterløs constructor til EF Core
    private Konsultation() { }

    // Privat constructor — tvinger brug af factory-metoden Opret()
    private Konsultation(
        Tidsinterval tidspunkt,
        Guid behandlingstypeId,
        Guid patientId,
        Guid behandlerId)
    {
        if (tidspunkt.Fra < DateTime.UtcNow)
            throw new DomainException("Konsultation kan ikke oprettes i fortiden.");

        Id = Guid.NewGuid();
        Tidspunkt = tidspunkt;
        BehandlingstypeId = behandlingstypeId != Guid.Empty
            ? behandlingstypeId : throw new DomainException("BehandlingstypeId er påkrævet.");
        PatientId = patientId != Guid.Empty
            ? patientId : throw new DomainException("PatientId er påkrævet.");
        BehandlerId = behandlerId != Guid.Empty
            ? behandlerId : throw new DomainException("BehandlerId er påkrævet.");
        Status = KonsultationStatus.Planlagt;
    }

    // ── Factory-metode ──────────────────────────────────────────────────

    /// <summary>
    /// Opretter en ny konsultation og validerer at den ikke overlapper
    /// med eksisterende bookinger for hverken patienten eller behandleren.
    /// </summary>
    public static Konsultation Opret(
        Tidsinterval tidspunkt,
        Guid behandlingstypeId,
        Guid patientId,
        Guid behandlerId,
        IEnumerable<Konsultation> eksisterendeForPatient,
        IEnumerable<Konsultation> eksisterendeForBehandler)
    {
        var konsultation = new Konsultation(tidspunkt, behandlingstypeId, patientId, behandlerId);

        konsultation.ValiderIngenOverlap(eksisterendeForPatient, eksisterendeForBehandler);

        return konsultation;
    }

    // ── Tilstandsændringer ──────────────────────────────────────────────

    public void OpdaterBehandlingstype(Guid nyBehandlingstypeId)
    {
        if (Status == KonsultationStatus.Afsluttet)
            throw new DomainException("Kan ikke ændre type på afsluttet konsultation.");

        BehandlingstypeId = nyBehandlingstypeId != Guid.Empty
            ? nyBehandlingstypeId : throw new DomainException("BehandlingstypeId er påkrævet.");
    }

    public void Aflys()
    {
        if (Status == KonsultationStatus.Afsluttet)
            throw new DomainException("Kan ikke aflyse en afsluttet konsultation.");

        Status = KonsultationStatus.Aflyst;
    }

    public void Afslut(string notat)
    {
        if (string.IsNullOrWhiteSpace(notat))
            throw new DomainException("Afslutning kræver et notat.");

        Notat = notat;
        Status = KonsultationStatus.Afsluttet;
    }

    public bool ErAktiv => Status != KonsultationStatus.Aflyst;

    // ── Privat overlap-validering ───────────────────────────────────────

    private void ValiderIngenOverlap(
        IEnumerable<Konsultation> eksisterendeForPatient,
        IEnumerable<Konsultation> eksisterendeForBehandler)
    {
        var patientOverlap = eksisterendeForPatient
            .Where(k => k.Id != Id)
            .Where(k => k.ErAktiv)
            .FirstOrDefault(k => Tidspunkt.OverlapperMed(k.Tidspunkt));

        if (patientOverlap is not null)
        {
            throw new DomainException(
                $"Patienten har allerede en booking "
                + $"({patientOverlap.Tidspunkt.Fra:HH:mm}-{patientOverlap.Tidspunkt.Til:HH:mm}) "
                + $"der overlapper med det ønskede tidspunkt.");
        }

        var behandlerOverlap = eksisterendeForBehandler
            .Where(k => k.Id != Id)
            .Where(k => k.ErAktiv)
            .FirstOrDefault(k => Tidspunkt.OverlapperMed(k.Tidspunkt));

        if (behandlerOverlap is not null)
        {
            throw new DomainException(
                $"Behandleren har allerede en booking "
                + $"({behandlerOverlap.Tidspunkt.Fra:HH:mm}-{behandlerOverlap.Tidspunkt.Til:HH:mm}) "
                + $"der overlapper med det ønskede tidspunkt.");
        }
    }
}
