using Microsoft.EntityFrameworkCore;
using MinKlinik.Domain.Entities;

namespace MinKlinik.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<Konsultation> Konsultationer => Set<Konsultation>();
    public DbSet<Behandlingstype> Behandlingstyper => Set<Behandlingstype>();
    public DbSet<Patient> Patienter => Set<Patient>();
    public DbSet<Behandler> Behandlere => Set<Behandler>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Konsultation>(b =>
        {
            b.HasKey(k => k.Id);

            // Tidsinterval er et Value Object → map som ComplexProperty med ToJson()
            b.ComplexProperty(k => k.Tidspunkt, t => t.ToJson());

            // Status som string i databasen
            b.Property(k => k.Status).HasConversion<string>();

            // Andre Aggregate Roots refereres via Guid-properties.
            // Ingen navigation properties — kun FK-kolonner.
            // EF mapper automatisk BehandlingstypeId, PatientId, BehandlerId.
        });

        modelBuilder.Entity<Behandlingstype>(b => b.HasKey(bt => bt.Id));
        modelBuilder.Entity<Patient>(b => b.HasKey(p => p.Id));
        modelBuilder.Entity<Behandler>(b => b.HasKey(bh => bh.Id));
    }
}
