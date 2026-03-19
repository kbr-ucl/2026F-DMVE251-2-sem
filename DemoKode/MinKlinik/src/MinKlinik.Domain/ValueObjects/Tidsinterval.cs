using MinKlinik.Domain.Exceptions;

namespace MinKlinik.Domain.ValueObjects;

/// <summary>
/// Value Object der repræsenterer et tidsinterval.
/// Immutable og sammenlignes på værdi (record).
/// </summary>
public record Tidsinterval
{
    public DateTime Fra { get; init; }
    public DateTime Til { get; init; }

    // Parameterløs constructor til EF Core
    private Tidsinterval() { }

    public Tidsinterval(DateTime fra, DateTime til)
    {
        if (til <= fra)
            throw new DomainException("Til-dato skal være efter fra-dato.");

        Fra = fra;
        Til = til;
    }

    public TimeSpan Varighed => Til - Fra;

    /// <summary>
    /// To tidsintervaller overlapper hvis det ene starter før det andet slutter,
    /// og omvendt: Fra_A &lt; Til_B og Fra_B &lt; Til_A.
    /// </summary>
    public bool OverlapperMed(Tidsinterval andet)
        => Fra < andet.Til && andet.Fra < Til;
}
