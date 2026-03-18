using EfValueObjectsSqlServer2025Demo.Console.ValueObjects;

namespace EfValueObjectsSqlServer2025Demo.Console.Entities;

public class OwnsConsultation
{
    public Guid Id { get; private set; }
    public TimeInterval TimeInterval { get; private set; } = null!;

    // EF Core materialization
    private OwnsConsultation() { }

    public OwnsConsultation(Guid id, TimeInterval timeInterval)
    {
        Id = id;
        TimeInterval = timeInterval;
    }
}

