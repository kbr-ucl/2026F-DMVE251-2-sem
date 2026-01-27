using Domain.Shared;

namespace Domain.Entities;

public class Medlem : Entity
{
    public string Navn { get; private set; }
        
    private List<Bog> LånteBøger { get; set; }

    // Vi skjuler listen, så ingen kan bruge .Add() udefra og omgå vores regler
    public IReadOnlyCollection<Bog> AktuelleLånteBøger => LånteBøger.AsReadOnly();

    public Medlem(string navn)
    {
        if (string.IsNullOrWhiteSpace(navn)) throw new ArgumentException("Navn mangler");

        Id = Guid.NewGuid();
        Navn = navn;
        LånteBøger = [];
    }

    // Adfærd: Her samles logikken
    public void LånBog(Bog bog)
    {
        ArgumentNullException.ThrowIfNull(bog);

        // Regel: Maks 3 bøger
        if (LånteBøger.Count >= 3)
            throw new InvalidOperationException("Du må højst låne 3 bøger.");

        // Handlingen udføres (Koordinering mellem objekter)
        bog.Udlån();
        LånteBøger.Add(bog);
    }

    public void AfleverBog(Bog bog)
    {
        if (!LånteBøger.Contains(bog))
            throw new InvalidOperationException("Medlemmet har ikke lånt denne bog.");

        bog.Aflever();
        LånteBøger.Remove(bog);
    }
}
