using System.Diagnostics;

namespace Domain.Entities;

public class Bog
{
    public string Isbn { get; init; }
    public string Titel { get; init; }
    public string Forfatter { get; init; }
    public bool ErUdlånt { get; private set; }

    // Konstruktør: Sikrer at vi aldrig har en "tom" bog i systemet
    public Bog(string isbn, string forfatter, string titel)
    {
        // PRE-CONDITIONS (Førbetingelser)
        // Vi tjekker kravene FØR vi gør noget.
        // Hvis disse ikke er opfyldt, kaster vi en fejl (Exception).
        if (string.IsNullOrWhiteSpace(isbn)) throw new ArgumentNullException(nameof(isbn));
        if (string.IsNullOrWhiteSpace(forfatter)) throw new ArgumentNullException(nameof(forfatter));
        if (string.IsNullOrWhiteSpace(titel)) throw new ArgumentNullException(nameof(titel));

        // SELVE HANDLINGEN
        Titel = titel;
        Forfatter = forfatter;
        Isbn = isbn;
        ErUdlånt = false;

        // POST-CONDITIONS (Efterbetingelser) ---
        // Vi tjekker, om resultatet er som forventet.
        // I C# bruger man ofte 'Debug.Assert' til dette under udvikling.
        Debug.Assert(Isbn == isbn, "Isbn blev ikke sat");
        Debug.Assert(Forfatter == forfatter, "Forfatter blev ikke sat");
        Debug.Assert(Titel == titel, "Titel blev ikke sat");
        Debug.Assert(!ErUdlånt, "ErUdlånt blev ikke sat til falsk");
    }

    // Adfærd: Metoder der håndterer statusændringer sikkert
    public void Udlån()
    {
        // Gward Clauses (pre-conditions)
        if (ErUdlånt)
            throw new InvalidOperationException("Fejl: Bogen er allerede udlånt.");

        // Handling
        ErUdlånt = true;

        // Post-condition
        Debug.Assert(ErUdlånt, "ErUdlånt blev ikke sat til sandt");
    }

    public void Aflever()
    {
        // Gward Clauses (pre-conditions)
        if (!ErUdlånt)
            throw new InvalidOperationException("Fejl: Bogen er allerede på biblioteket.");

        // Handling
        ErUdlånt = false;

        // post-conditions
        Debug.Assert(!ErUdlånt, "ErUdlånt blev ikke sat til falsk");
    }
}