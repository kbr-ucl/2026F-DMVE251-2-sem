using Domain.Shared;

namespace Domain.Entities;

public class Bog : Entity
{
    public string Titel { get; private set; }
    public string Forfatter { get; private set; }
    public string Isbn { get; private set; }
    public bool ErUdlånt { get; private set; }

    // Konstruktør: Sikrer at vi aldrig har en "tom" bog i systemet
    public Bog(string titel, string forfatter, string isbn, bool erUdlånt = false)
    {
        if (string.IsNullOrWhiteSpace(titel)) throw new ArgumentException("Titel mangler");

        Id = Guid.NewGuid();
        Titel = titel;
        Forfatter = forfatter;
        Isbn = isbn;
        ErUdlånt = erUdlånt;
    }

    // Adfærd: Metoder der håndterer statusændringer sikkert
    public void MarkerSomUdlånt()
    {
        if (ErUdlånt)
            throw new InvalidOperationException("Fejl: Bogen er allerede udlånt.");

        ErUdlånt = true;
    }

    public void MarkerSomAfleveret()
    {
        if (!ErUdlånt)
            throw new InvalidOperationException("Fejl: Bogen er allerede på biblioteket.");

        ErUdlånt = false;
    }
}
