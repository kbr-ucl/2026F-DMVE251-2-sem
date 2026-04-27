using Microsoft.EntityFrameworkCore;
using MinKlinik.Domain.Entities;
using MinKlinik.Infrastructure.Persistence;

namespace MinKlinik.Infrastructure;

/// <summary>
/// Seed-data til test og demonstration.
/// </summary>
public class SeedData
{
    public void Initialize(AppDbContext db)
    {
        db.Database.EnsureCreated();

        if (db.Behandlingstyper.Any()) return;

        var undersøgelse = new Behandlingstype("Undersøgelse");
        var vaccination = new Behandlingstype("Vaccination");
        var kontrol = new Behandlingstype("Kontrol");

        db.Behandlingstyper.AddRange(undersøgelse, vaccination, kontrol);

        var patient1 = new Patient("Jens Hansen", "010190-1234");
        var patient2 = new Patient("Maria Nielsen", "150285-5678");

        db.Patienter.AddRange(patient1, patient2);

        var behandler1 = new Behandler("Dr. Pia Jensen", "Almen medicin");
        var behandler2 = new Behandler("Dr. Lars Pedersen", "Ortopædi");

        db.Behandlere.AddRange(behandler1, behandler2);

        db.SaveChanges();
    }
}
