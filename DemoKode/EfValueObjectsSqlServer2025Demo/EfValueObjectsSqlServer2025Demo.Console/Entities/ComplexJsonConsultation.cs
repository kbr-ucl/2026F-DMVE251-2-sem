using EfValueObjectsSqlServer2025Demo.Console.ValueObjects;

namespace EfValueObjectsSqlServer2025Demo.Console.Entities;

public class ComplexJsonConsultation
{
    public Guid Id { get; private set; }
    public TimeInterval TimeInterval { get; private set; } = null!;

    // EF Core materialization
    private ComplexJsonConsultation() { }

    public ComplexJsonConsultation(Guid id, TimeInterval timeInterval)
    {
        Id = id;
        TimeInterval = timeInterval;
    }
}

