using System;

namespace EfValueObjectsSqlServer2025Demo.Console.ValueObjects;

/// <summary>
/// Simple immutable value object (DDD style) representing a time interval.
/// </summary>
public record TimeInterval
{
    public DateTime From { get; init; }
    public DateTime To { get; init; }

    // EF Core materialization constructor
    private TimeInterval() { }

    public TimeInterval(DateTime from, DateTime to)
    {
        if (to <= from)
            throw new ArgumentException("To must be after From.", nameof(to));

        From = from;
        To = to;
    }

    public override string ToString() => $"{From:o} -> {To:o}";
}

