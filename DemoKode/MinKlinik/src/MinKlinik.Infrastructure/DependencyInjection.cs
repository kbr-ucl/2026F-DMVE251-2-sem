using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using MinKlinik.Facade.Queries;
using MinKlinik.Infrastructure.Persistence;
using MinKlinik.Infrastructure.QueryHandlers;
using MinKlinik.Infrastructure.Repositories;
using MinKlinik.UseCases;

// Bevidst placeret i Microsoft-namespace så composition root får extension metoden
// ind uden at skulle tilføje et ekstra using.
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Registrerer alle infrastruktur-implementeringer: DbContext, repositories og query handlers.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    // Overload 1: Læs connection string fra IConfiguration.
    // Hvis ingen connection string er konfigureret, falder vi tilbage til in-memory —
    // det er praktisk for udvikling og integrationstests.
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MinKlinikDb");

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return services.AddInfrastructure(options => options.UseSqlServer(connectionString));
        }

        // SQLite in-memory kræver en åben forbindelse for at databasen lever på tværs af scopes.
        var sqliteConnection = new SqliteConnection("DataSource=:memory:");
        sqliteConnection.Open();
        services.AddSingleton(sqliteConnection);

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            options.UseSqlite(serviceProvider.GetRequiredService<SqliteConnection>());
        });

        RegisterRepositoriesAndQueries(services);
        return services;
    }

    // Overload 2: Kalderen bestemmer selv hvordan DbContext'en opsættes.
    // Bruges fx fra Console, tests eller andre scenarier uden IConfiguration.
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDb)
    {
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            options.LogTo(Console.WriteLine);
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            configureDb(options);
        });

        RegisterRepositoriesAndQueries(services);
        return services;
    }

    private static void RegisterRepositoriesAndQueries(IServiceCollection services)
    {
        // Repositories (Use Case-interfaces → Infrastructure-implementeringer)
        services.AddScoped<IKonsultationRepository, KonsultationRepository>();
        services.AddScoped<IBehandlingstypeRepository, BehandlingstypeRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IBehandlerRepository, BehandlerRepository>();

        // Queries (Facade-interfaces → Infrastructure-implementeringer)
        services.AddScoped<IKonsultationQueries, KonsultationQueriesImpl>();
        services.AddScoped<IBehandlingstypeQueries, BehandlingstypeQueriesImpl>();
        services.AddScoped<IPatientQueries, PatientQueriesImpl>();
        services.AddScoped<IBehandlerQueries, BehandlerQueriesImpl>();
    }
}