namespace ParallelEntityFrameworkConsoleApp.Dtos;

public sealed record CustomerStatsDto(
    string CustomerId,
    string CompanyName,
    int OrderCount,
    decimal TotalOrderSum);
