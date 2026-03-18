using System;
using System.Threading.Tasks;
using EfValueObjectsSqlServer2025Demo.Console.Entities;
using EfValueObjectsSqlServer2025Demo.Console.Persistence;
using EfValueObjectsSqlServer2025Demo.Console.ValueObjects;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

static string GetBaseConnectionString()
{
    var fromEnv = Environment.GetEnvironmentVariable("SQLSERVER_CONNECTION_STRING");
    if (!string.IsNullOrWhiteSpace(fromEnv))
        return fromEnv;

    // Placeholder using Windows auth.
    return "Server=localhost;Database=EfValueObjectsSqlServer2025Demo;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False";
}

static string BuildConnectionStringWithDatabase(string baseConnectionString, string suffix)
{
    var builder = new SqlConnectionStringBuilder(baseConnectionString);
    var db = string.IsNullOrWhiteSpace(builder.InitialCatalog)
        ? "EfValueObjectsSqlServer2025Demo"
        : builder.InitialCatalog;

    builder.InitialCatalog = $"{db}_{suffix}";
    return builder.ConnectionString;
}

static async Task RunOwnsOneAsync(string baseConnectionString, TimeInterval interval)
{
    var cs = BuildConnectionStringWithDatabase(baseConnectionString, "OwnsOne");
    var options = new DbContextOptionsBuilder<OwnsOneConsultationDbContext>().UseSqlServer(cs).Options;

    await using var db = new OwnsOneConsultationDbContext(options);

    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();

    db.Consultations.Add(new OwnsConsultation(Guid.NewGuid(), interval));
    await db.SaveChangesAsync();

    var loaded = await db.Consultations.SingleAsync();
    Console.WriteLine($"OwnsOne -> {loaded.TimeInterval}");
}

static async Task RunComplexPropertyAsync(string baseConnectionString, TimeInterval interval)
{
    var cs = BuildConnectionStringWithDatabase(baseConnectionString, "ComplexProperty");
    var options = new DbContextOptionsBuilder<ComplexPropertyConsultationDbContext>().UseSqlServer(cs).Options;

    await using var db = new ComplexPropertyConsultationDbContext(options);

    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();

    db.Consultations.Add(new ComplexConsultation(Guid.NewGuid(), interval));
    await db.SaveChangesAsync();

    var loaded = await db.Consultations.SingleAsync();
    Console.WriteLine($"ComplexProperty -> {loaded.TimeInterval}");
}

static async Task RunComplexPropertyJsonAsync(string baseConnectionString, TimeInterval interval)
{
    var cs = BuildConnectionStringWithDatabase(baseConnectionString, "ComplexPropertyJson");
    var options = new DbContextOptionsBuilder<ComplexPropertyJsonConsultationDbContext>().UseSqlServer(cs).Options;

    await using var db = new ComplexPropertyJsonConsultationDbContext(options);

    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();

    db.Consultations.Add(new ComplexJsonConsultation(Guid.NewGuid(), interval));
    await db.SaveChangesAsync();

    var loaded = await db.Consultations.SingleAsync();
    Console.WriteLine($"ComplexProperty (ToJson) -> {loaded.TimeInterval}");
}

var baseConnectionString = GetBaseConnectionString();
var now = DateTime.UtcNow;
var interval = new TimeInterval(now.AddMinutes(10), now.AddMinutes(20));

Console.WriteLine("EF Core + SQL Server 2025 Value Object mapping demo");
Console.WriteLine($"From connection string: {baseConnectionString}");
Console.WriteLine($"Value object: {interval}");
Console.WriteLine();

await RunOwnsOneAsync(baseConnectionString, interval);
await RunComplexPropertyAsync(baseConnectionString, interval);
await RunComplexPropertyJsonAsync(baseConnectionString, interval);
