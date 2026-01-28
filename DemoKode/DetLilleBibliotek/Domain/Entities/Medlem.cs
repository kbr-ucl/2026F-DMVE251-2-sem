using System.Diagnostics;

namespace Domain.Entities;

public class Medlem
{
    public int Medlemsnummer { get; init; }
    public string Navn { get; init; }

    // Vi bruger en liste til INTERNT at holde styr på lånte bøger
    private List<Bog> LånteBøger { get; }

    // Vi skjuler listen, så ingen kan bruge .Add() udefra og omgå vores regler
    public IReadOnlyCollection<Bog> AktueltLånteBøger => LånteBøger.AsReadOnly();

    public Medlem(int medlemsnummer, string navn)
    {
        // PRE-CONDITIONS (Førbetingelser)
        // Vi tjekker kravene FØR vi gør noget.
        // Hvis disse ikke er opfyldt, kaster vi en fejl (Exception).
        if (string.IsNullOrWhiteSpace(navn)) throw new ArgumentNullException(nameof(navn));
        if (string.IsNullOrWhiteSpace(navn)) throw new ArgumentNullException(nameof(navn));

        // SELVE HANDLINGEN
        Medlemsnummer = medlemsnummer;
        Navn = navn;
        LånteBøger = [];

        // POST-CONDITIONS (Efterbetingelser) ---
        // Vi tjekker, om resultatet er som forventet.
        // I C# bruger man ofte 'Debug.Assert' til dette under udvikling.
        Debug.Assert(Medlemsnummer == medlemsnummer, "Medlemsnummer blev ikke sat");
        Debug.Assert(Navn == navn, "Navn blev ikke sat");
        Debug.Assert(LånteBøger.Count == 0, "LånteBøger blev ikke initialiseret korrekt");
    }

    // Adfærd: Her samles logikken
    public void LånBog(Bog bog)
    {
        // Gward Clauses (pre-conditions)
        ArgumentNullException.ThrowIfNull(bog);

        // Regel: Maks 3 bøger
        if (LånteBøger.Count >= 3)
            throw new InvalidOperationException("Du må højst låne 3 bøger.");

        
        // Handlingen udføres (Koordinering mellem objekter)
        bog.Udlån();
        LånteBøger.Add(bog);

        // Post-condition
        Debug.Assert(LånteBøger.Contains(bog), "Bogen blev ikke tilføjet til lånte bøger");
        // Bemærk at vi IKKE tjekker bog.ErUdlånt her, da det er et andet objekt, hvorfor vi stoler på at bog.Udlån() virker korrekt.
    }

    public void AfleverBog(Bog bog)
    {
        // Gward Clauses (pre-conditions)
        ArgumentNullException.ThrowIfNull(bog);

        if (!LånteBøger.Contains(bog))
            throw new InvalidOperationException("Medlemmet har ikke lånt denne bog.");

        // Handlingen udføres (Koordinering mellem objekter)
        bog.Aflever();
        LånteBøger.Remove(bog);

        // Post-condition
        Debug.Assert(!LånteBøger.Contains(bog), "Bogen blev ikke fjernet fra lånte bøger");
        // Igen stoler vi på at bog.Aflever() virker korrekt.
    }
}