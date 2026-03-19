namespace MinKlinik.Domain;

/// <summary>
/// Base class for alle Entities i domænet.
/// En Entity har en unik identitet (Id) der følger den gennem hele dens livscyklus.
/// To entities er ens hvis de har samme Id — uanset om deres øvrige data er ændret.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; }

    public override bool Equals(object? obj)
        => obj is Entity other && Id == other.Id;

    public override int GetHashCode()
        => Id.GetHashCode();
}

/// <summary>
/// Base class for Aggregate Roots.
///
/// Sådan identificerer man en Aggregate Root:
///
///   1. HAR EGEN LIVSCYKLUS — kan oprettes og slettes uafhængigt af andre.
///      → Patient eksisterer uafhængigt af sine konsultationer.
///      → Konsultation eksisterer uafhængigt af andre konsultationer.
///
///   2. ER TRANSAKTIONSGRÆNSE — ændringer inden for aggregatet gemmes som én enhed.
///      → Når en Konsultation afsluttes, gemmes status + notat i én SaveChanges().
///
///   3. HAR ET EGET REPOSITORY — omverdenen henter aggregatet via et repository.
///      → IKonsultationRepository, IPatientRepository, etc.
///
///   4. REFERERES VIA ID — andre aggregater holder kun en Guid-reference,
///      IKKE en objektreference. Domænet må ikke navigere direkte til
///      andre aggregater.
///
/// Aggregate Roots der IKKE opfylder disse kriterier er sandsynligvis
/// bare Entities der bør leve INDEN I et andet aggregat.
/// Eksempel: Tidsinterval er IKKE en Aggregate Root — det er et Value Object
/// der kun eksisterer som en del af Konsultation.
/// </summary>
public abstract class AggregateRoot : Entity
{
}
