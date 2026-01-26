using Domain.Shared;

namespace Domain.Entities;


public class Medlem : Entity
{
    public string Navn { get; private set; }

    // Vi skjuler listen, så ingen kan bruge .Add() udefra og omgå vores regler
    private List<Bog> _lånteBøger;
    public IReadOnlyCollection<Bog> LånteBogIds => _lånteBøger.AsReadOnly();

    public Medlem(string navn)
    {
        if (string.IsNullOrWhiteSpace(navn)) throw new ArgumentException("Navn mangler");

        Id = Guid.NewGuid();
        Navn = navn;
        _lånteBøger = [];
    }

    // Adfærd: Her samles logikken
    public void LånBog(Bog bog)
    {
        ArgumentNullException.ThrowIfNull(bog);

        // Regel: Maks 3 bøger
        if (_lånteBøger.Count >= 3)
            throw new InvalidOperationException("Du må højst låne 3 bøger.");

        // Handlingen udføres (Koordinering mellem objekter)
        bog.MarkerSomUdlånt();
        _lånteBøger.Add(bog);
    }

    public void AfleverBog(Bog bog)
    {
        if (!_lånteBøger.Contains(bog))
            throw new InvalidOperationException("Medlemmet har ikke lånt denne bog.");

        bog.MarkerSomAfleveret();
        _lånteBøger.Remove(bog);
    }
}
