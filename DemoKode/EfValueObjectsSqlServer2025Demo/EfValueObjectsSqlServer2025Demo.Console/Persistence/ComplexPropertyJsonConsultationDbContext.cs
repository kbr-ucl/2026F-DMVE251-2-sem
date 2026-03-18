using EfValueObjectsSqlServer2025Demo.Console.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfValueObjectsSqlServer2025Demo.Console.Persistence;

public class ComplexPropertyJsonConsultationDbContext : DbContext
{
    public DbSet<ComplexJsonConsultation> Consultations => Set<ComplexJsonConsultation>();

    public ComplexPropertyJsonConsultationDbContext(DbContextOptions<ComplexPropertyJsonConsultationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ComplexJsonConsultation>(b =>
        {
            b.ToTable("ComplexJsonConsultations");
            b.HasKey(e => e.Id);

            // Complex type value object persisted as a single JSON column.
            b.ComplexProperty(e => e.TimeInterval, t => t.ToJson());
        });
    }
}

