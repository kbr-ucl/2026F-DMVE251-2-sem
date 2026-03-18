using EfValueObjectsSqlServer2025Demo.Console.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfValueObjectsSqlServer2025Demo.Console.Persistence;

public class ComplexPropertyConsultationDbContext : DbContext
{
    public DbSet<ComplexConsultation> Consultations => Set<ComplexConsultation>();

    public ComplexPropertyConsultationDbContext(DbContextOptions<ComplexPropertyConsultationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ComplexConsultation>(b =>
        {
            b.ToTable("ComplexConsultations");
            b.HasKey(e => e.Id);

            // Complex type value object persisted as additional columns (table splitting).
            b.ComplexProperty(e => e.TimeInterval);
        });
    }
}

