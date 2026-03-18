using EfValueObjectsSqlServer2025Demo.Console.ValueObjects;

namespace EfValueObjectsSqlServer2025Demo.Console.Entities;

public class ComplexConsultation
{
    public Guid Id { get; private set; }
    public TimeInterval TimeInterval { get; private set; } = null!;

    // EF Core materialization
    private ComplexConsultation() { }

    public ComplexConsultation(Guid id, TimeInterval timeInterval)
    {
        Id = id;
        TimeInterval = timeInterval;
    }
}

