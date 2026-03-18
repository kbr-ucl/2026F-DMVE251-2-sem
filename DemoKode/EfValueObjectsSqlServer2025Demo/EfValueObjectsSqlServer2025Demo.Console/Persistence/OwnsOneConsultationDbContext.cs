using EfValueObjectsSqlServer2025Demo.Console.Entities;
using EfValueObjectsSqlServer2025Demo.Console.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EfValueObjectsSqlServer2025Demo.Console.Persistence;

public class OwnsOneConsultationDbContext : DbContext
{
    public DbSet<OwnsConsultation> Consultations => Set<OwnsConsultation>();

    public OwnsOneConsultationDbContext(DbContextOptions<OwnsOneConsultationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OwnsConsultation>(b =>
        {
            b.ToTable("OwnsConsultations");
            b.HasKey(e => e.Id);

            b.OwnsOne(e => e.TimeInterval, ti =>
            {
                // Explicit column names to make the mapping differences obvious.
                ti.Property(t => t.From).HasColumnName("TimeInterval_From");
                ti.Property(t => t.To).HasColumnName("TimeInterval_To");
            });
        });
    }
}

