# Guide: Flyt IoC ud i lagene med extension metoder (.NET 10)

> Målgruppe: 1. års datamatikerstuderende
> Projekt: `MinKlinik`
> Forudsætninger: Du har et kørende clean architecture–projekt, hvor al DI-opsætning står i `Program.cs`.

---

## 1. Hvorfor gør vi det her?

Når `Program.cs` bliver en lang liste af `AddScoped<...>`-kald, så bryder vi **Single Responsibility Principle (SRP)**: composition root'en ved pludselig alt om implementeringsdetaljerne i hvert lag. Det giver tre problemer:

1. **Tæt kobling.** `Program.cs` (API-laget) importerer typer fra `Infrastructure.Persistence`, `Infrastructure.Repositories`, `UseCases.Konsultationer` osv. Enhver intern ændring i et lag ryger helt op i composition root.
2. **Genbrug er svært.** Når vi senere tilføjer `MinKlinik.Console`, skal vi kopiere registreringerne. Det strider mod DRY.
3. **Test-setup bliver rodet.** Integrationstests skal også opsætte DI. Hvis logikken kun findes i `Program.cs`, må vi duplikere.

**Løsning:** Hvert lag stiller sin *egen* `IServiceCollection`-extension metode til rådighed. Composition root'en skriver så bare tre linjer:

```csharp
builder.Services.AddDomain();
builder.Services.AddUseCases();
builder.Services.AddInfrastructure(builder.Configuration);
```

Det er **SOLID** og **clean architecture** i praksis: hvert lag ejer sin egen afhængighedsopsætning.

---

## 2. Udgangspunkt (det vi starter med)

I dag ser `src/MinKlinik.Api/Program.cs` således ud (uddrag):

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("MinKlinikDb"));

// Repositories
builder.Services.AddScoped<IKonsultationRepository, KonsultationRepository>();
builder.Services.AddScoped<IBehandlingstypeRepository, BehandlingstypeRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IBehandlerRepository, BehandlerRepository>();

// Use Cases
builder.Services.AddScoped<IOpretKonsultationUseCase, OpretKonsultationUseCase>();
builder.Services.AddScoped<IAfslutKonsultationUseCase, AfslutKonsultationUseCase>();
builder.Services.AddScoped<IAflysKonsultationUseCase, AflysKonsultationUseCase>();

// Queries
builder.Services.AddScoped<IKonsultationQueries, KonsultationQueriesImpl>();
builder.Services.AddScoped<IBehandlingstypeQueries, BehandlingstypeQueriesImpl>();
builder.Services.AddScoped<IPatientQueries, PatientQueriesImpl>();
builder.Services.AddScoped<IBehandlerQueries, BehandlerQueriesImpl>();
```

Læg mærke til at `Program.cs` kender til mindst 11 konkrete typer fra Infrastructure og UseCases. Det vil vi lave om.

---

## 3. Mønstret: `IServiceCollection`-extension per lag

I .NET 10 er den idiomatiske måde at registrere DI på stadig extension metoder på `IServiceCollection`. Mønstret er:

```csharp
namespace Microsoft.Extensions.DependencyInjection; // <-- bevidst valg

public static class <LagNavn>ServiceCollectionExtensions
{
    public static IServiceCollection Add<LagNavn>(
        this IServiceCollection services,
        IConfiguration configuration)   // kun hvis laget behøver config
    {
        // registreringer
        return services; // gør fluent-chaining muligt
    }
}
```

### Hvorfor placerer vi klassen i namespace `Microsoft.Extensions.DependencyInjection`?
Fordi `Program.cs` allerede har `using Microsoft.Extensions.DependencyInjection;` implicit (via `ImplicitUsings`). Extension metoden bliver dermed synlig **uden** at `Program.cs` behøver et ekstra `using`. Det er konventionen Microsoft selv bruger (fx `AddDbContext`, `AddControllers`).

### Nye .NET 10-features vi bruger
- **File-scoped namespaces** (`namespace X;`) — mindre indrykning.
- **Primary constructors** — ikke relevant her, men værd at nævne i undervisningen.
- **`TimeProvider`** kan registreres her, hvis use cases har brug for tid (fx `services.AddSingleton(TimeProvider.System);`).
- **Keyed services** (`AddKeyedScoped`) — brug dem hvis I har flere implementeringer af samme interface.

---

## 4. Trin 1: Infrastructure-laget

Opret en ny fil: `src/MinKlinik.Infrastructure/DependencyInjection.cs`.

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinKlinik.Facade.Queries;
using MinKlinik.Infrastructure.Persistence;
using MinKlinik.Infrastructure.QueryHandlers;
using MinKlinik.Infrastructure.Repositories;
using MinKlinik.UseCases; // IRepositories

namespace Microsoft.Extensions.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("MinKlinikDb");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Udvikling/tests — in-memory
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("MinKlinikDb"));
        }
        else
        {
            // Produktion — SQL Server
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));
        }

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

        return services;
    }
}
```

### Pointer til de studerende
- **Infrastructure-laget kender til Infrastructure-typer.** Det er fint. Composition root'en skal ikke.
- **Konfigurationen ligger hos det lag der har brug for den.** Connection string bliver læst her, ikke i `Program.cs`.
- **`return services;`** giver os *fluent syntax*: `services.AddInfrastructure(cfg).AddUseCases();`

---

## 5. Trin 2: UseCases-laget

Opret en ny fil: `src/MinKlinik.UseCases/DependencyInjection.cs`.

```csharp
using MinKlinik.Facade.UseCases;
using MinKlinik.UseCases.Konsultationer;

namespace Microsoft.Extensions.DependencyInjection;

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
```

Hvis I senere tilføjer `TimeProvider` eller en logger-abstraktion på use case-niveau, er det her det skal registreres — ikke i `Program.cs`.

---

## 6. Trin 3: Domain-laget (valgfrit, men godt at vise)

Domain-laget har som udgangspunkt *ingen* afhængigheder — det er jo pointen med DDD. Men nogle gange vil I registrere **domæne-services** (fx en `IPrisberegner` eller en `IKonsultationPolicy`).

Opret — hvis det bliver aktuelt — `src/MinKlinik.Domain/DependencyInjection.cs`:

```csharp
namespace Microsoft.Extensions.DependencyInjection;

public static class DomainServiceCollectionExtensions
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        // services.AddSingleton<IPrisberegner, Prisberegner>();
        return services;
    }
}
```

Hvis domænet ikke har noget at registrere lige nu — så *lad være med at lave filen*. Tom ceremoni er værre end ingen ceremoni.

---

## 7. Trin 4: Ryd op i `Program.cs`

Efter refaktoreringen ser `src/MinKlinik.Api/Program.cs` sådan her ud:

```csharp
using MinKlinik.Infrastructure;
using MinKlinik.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Infrastruktur (web/API-specifikt)
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Applikationslag — hver extension metode ejer sin egen opsætning
builder.Services
    .AddUseCases()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Seed testdata
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    new SeedData().Initialize(db);
}

app.MapOpenApi();
app.MapScalarApiReference();
app.MapControllers();

app.Run();
```

Sammenlign med før: vi er gået fra **~20 linjers DI-opsætning spredt over alle lag** til **tre linjer** der udtrykker intention ("jeg vil have use cases og infrastruktur"). Læsbarheden og vedligeholdet er markant bedre.

### Vigtige usings der forsvinder
Du kan fjerne disse `using`-statements fra `Program.cs` — de hører ikke hjemme i composition root'en længere:
- `Microsoft.EntityFrameworkCore`
- `MinKlinik.Facade.Queries`
- `MinKlinik.Facade.UseCases`
- `MinKlinik.Infrastructure.QueryHandlers`
- `MinKlinik.Infrastructure.Repositories`
- `MinKlinik.UseCases`
- `MinKlinik.UseCases.Konsultationer`

Kun `MinKlinik.Infrastructure.Persistence` beholdes, fordi `SeedData` stadig kaldes direkte i composition root'en.

---

## 8. Bonus: Tilsvarende refaktorering af `MinKlinik.Console`

Nu kan console-projektet genbruge den samme opsætning:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddUseCases()
    .AddInfrastructure(builder.Configuration);

var host = builder.Build();
// ... brug host.Services.GetRequiredService<IOpretKonsultationUseCase>() osv.
```

**Dette er den store gevinst ved refaktoreringen:** Vi har kun ét sted hvor hvert lags DI er beskrevet. Tilføjer vi en ny repository-implementering, er der præcis ét sted der skal opdateres.

---

## 9. Tjekliste til de studerende

Gennemgå disse punkter efter I har lavet refaktoreringen:

- [ ] Der findes præcis én `DependencyInjection.cs` (eller `<Lag>ServiceCollectionExtensions.cs`) per lag der har noget at registrere.
- [ ] Alle extension metoder ligger i namespace `Microsoft.Extensions.DependencyInjection`.
- [ ] Alle metoder returnerer `IServiceCollection` så de kan chaines.
- [ ] `Program.cs` indeholder ingen `AddScoped<ISomething, SomethingImpl>()`-kald for domæne-/applikationstyper.
- [ ] `Program.cs` har ingen `using`-statements til `Repositories`, `QueryHandlers` eller `Konsultationer`-namespaces.
- [ ] Infrastructure-laget læser selv connection strings fra `IConfiguration`.
- [ ] Løsningen bygger uden advarsler: `dotnet build`.
- [ ] API'et starter og Scalar UI virker på `/scalar/v1`.

---

## 10. Diskussionsspørgsmål (til hold-diskussion)

1. Hvorfor valgte vi namespace `Microsoft.Extensions.DependencyInjection` frem for fx `MinKlinik.Infrastructure`?
2. Hvad er fordelen ved at returnere `IServiceCollection` fra extension metoder? Hvad taber vi hvis vi returnerer `void`?
3. Hvor ville I placere registreringen af `IHttpClientFactory`? Argumentér ud fra clean architecture.
4. Kunne vi have brugt *assembly scanning* (fx Scrutor) i stedet for at liste hver `AddScoped` manuelt? Hvad er trade-off'et for 1.-års kode?
5. Hvorfor brugte vi `AddScoped` og ikke `AddSingleton` for repositories? (Hint: `DbContext`.)

---

## 11. Gåder og faldgruber

**Faldgrube 1 — Cirkulære projekt-referencer.**
`Infrastructure` referer til `UseCases` (for interface'et `IKonsultationRepository`). Det er OK. Men `UseCases` må **aldrig** referere til `Infrastructure` — det vil bryde dependency-reglen i clean architecture. Tjek `.csproj`-filerne.

**Faldgrube 2 — Double-registration.**
Hvis I ved en fejl kalder både `AddUseCases()` i to forskellige filer, bliver den samme registrering kørt to gange. `AddScoped` tillader det (seneste vinder), men det er et tegn på at composition root'en er utydelig. Hold jer til ét sted.

**Faldgrube 3 — Konfiguration i forkert lag.**
Det er fristende at sende `IConfiguration` ned gennem alle lag. Lad være. Kun composition root'en og Infrastructure bør kende til `IConfiguration`. UseCases/Domain skal tage *stærkt typede* options (`IOptions<T>`) eller simple værdier som parametre.

---

## 12. Sammenfatning

| Før | Efter |
|---|---|
| ~20 linjers DI i `Program.cs` | 3 linjer i `Program.cs` |
| API-laget kender til Infrastructure-typer | API-laget kender kun til extension metoden |
| Konfiguration spredt ud | Konfiguration bor hos det lag der bruger den |
| Svær at genbruge i Console/tests | Direkte genbrug: samme `AddInfrastructure` overalt |

Det er **SOLID i praksis**: hvert lag har ét ansvar — også hvad angår sin egen DI-opsætning.
