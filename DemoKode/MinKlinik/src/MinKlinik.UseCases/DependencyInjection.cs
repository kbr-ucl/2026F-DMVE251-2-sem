using MinKlinik.Facade.UseCases;
using MinKlinik.UseCases.Konsultationer;

// Bevidst placeret i Microsoft-namespace så composition root får extension metoden
// ind uden at skulle tilføje et ekstra using. Samme konvention som AddControllers,
// AddDbContext osv.
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registrerer alle use case-implementeringer (application layer).
/// </summary>
public static class UseCasesServiceCollectionExtensions
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<IOpretKonsultationUseCase, OpretKonsultationUseCase>();
        services.AddScoped<IAfslutKonsultationUseCase, AfslutKonsultationUseCase>();
        services.AddScoped<IAflysKonsultationUseCase, AflysKonsultationUseCase>();

        return services;
    }
}
