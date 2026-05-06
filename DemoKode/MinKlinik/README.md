# MinKlinik — reference-implementation til *Software der holder*

Et klinik-bookingsystem bygget med Clean Architecture, DDD, Facade-lag og CQS i C# 13 / .NET 10. **MinKlinik er det gennemgående eksempel i lærebogen *Software der holder — Professionel C#-udvikling med Clean Architecture, DDD og parallelisme*** (Bromose Publishing, 2026, ISBN 978-87-976951-1-1).

Bogen forklarer *hvorfor* — denne kode viser *hvordan*. Repo'et er versionsstyret med ét tag pr. kapitel (`kap-01` … `kap-25`), så du kan checke kodebasens tilstand ud som den ser ud ved hvert kapitels afslutning.

## Hvordan bruger du repo'et?

```bash
git clone https://github.com/bromose/minklinik.git
cd minklinik

# Se den færdige kodebase (alle 25 kapitler implementeret)
git checkout main

# Eller hop til en bestemt kapitel-tilstand
git checkout kap-08    # som koden ser ud efter kapitel 8 (DDD-pakke afsluttet)
git checkout kap-12    # efter kapitel 12 (EF Core 10 introduceret)
git checkout kap-25    # færdig version (samme som main)

# Kør build og tests
dotnet build
dotnet test

# Kør én af host-applikationerne
dotnet run --project src/MinKlinik.Console
dotnet run --project src/MinKlinik.Api
dotnet run --project src/MinKlinik.Blazor
```

> **Læs bogen først.** Repo'et er en *reference-implementation*, ikke en tutorial. Bogens kapitler introducerer koncepterne trin for trin; her ser du resultatet samlet. Hvis du ikke har bogen, så start på `leanpub.com/software-der-holder`.

## Tag-konvention

| Tag | Tilstand | Bog-kapitel |
|-----|----------|-------------|
| `kap-01` | Indkapsling, kohæsion, kobling — Konsultation med `private set` + guard clauses | Kapitel 1 |
| `kap-02` | + SRP, OCP, ISP refaktorering | Kapitel 2 |
| `kap-03` | + DIP og IoC-container | Kapitel 3 |
| ... | ... | ... |
| `kap-25` | Færdig version — samme som `main` | Kapitel 25 |

Detaljeret tag-strategi og acceptkriterier pr. kapitel ligger i `docs/BOG-KODE-PLAN.md`.

## Licens

Koden i dette repository er udgivet under **MIT License** (se `LICENSE`). Det betyder at du frit kan klone, modificere, bruge og distribuere koden — også i kommercielle produkter — så længe copyright-noten bevares.

> **Bemærk:** MIT-licensen gælder *kun koden i dette repository*. Lærebogen *Software der holder* (manuskript, kapitler, øvelser, bilag) er separat copyright © 2026 Kaj Bromose, alle rettigheder forbeholdes, og udgives via Bromose Publishing på LeanPub. Køb af bogen er ikke en forudsætning for at bruge koden.

## Arkitektur

```
MinKlinik/
├── src/
│   ├── MinKlinik.Domain/            # Entities, Value Objects — INGEN afhængigheder
│   │   ├── AggregateRoot.cs         #   Base classes: Entity → AggregateRoot
│   │   ├── Entities/
│   │   │   ├── Konsultation.cs      #   Aggregate Root — factory-metode Opret()
│   │   │   ├── Patient.cs           #   Aggregate Root
│   │   │   ├── Behandler.cs         #   Aggregate Root
│   │   │   └── Behandlingstype.cs   #   Aggregate Root
│   │   ├── ValueObjects/
│   │   │   └── Tidsinterval.cs      #   Value Object (record) — IKKE aggregate root
│   │   ├── Enums/
│   │   │   └── KonsultationStatus.cs  # Enum — IKKE aggregate root
│   │   └── Exceptions/
│   │       ├── DomainException.cs
│   │       └── NotFoundException.cs
│   │
│   ├── MinKlinik.Facade/            # Kun interfaces + DTO'er — INGEN implementering
│   ├── MinKlinik.UseCases/          # Use Case-klasser + Repository-interfaces
│   ├── MinKlinik.Infrastructure/    # EF Core, Repositories, Query Handlers, SeedData
│   ├── MinKlinik.Api/               # ASP.NET Web API + Scalar
│   ├── MinKlinik.Blazor/            # Blazor frontend (Interactive Server; Weather bruger StreamRendering)
│   └── MinKlinik.Console/           # Konsol-app (menu-drevet) — alternativ til API
│
└── tests/
    ├── MinKlinik.Domain.Tests/      # Unit tests uden mocks
    └── MinKlinik.UseCases.Tests/    # Unit tests med Moq
```

## Aggregate Roots — Sådan identificerer du dem

En Aggregate Root er en entity der opfylder alle fire kriterier:

| Kriterium | Spørgsmål at stille | Eksempel: Patient |
|-----------|---------------------|-------------------|
| **1. Egen livscyklus** | Kan den oprettes og slettes uafhængigt af andre? | Ja — patienten eksisterer uafhængigt af konsultationer |
| **2. Transaktionsgrænse** | Ændres dens data som én samlet enhed? | Ja — navn og CPR ændres uafhængigt af alt andet |
| **3. Eget repository** | Hentes den direkte fra databasen via sit eget repository? | Ja — `IPatientRepository` |
| **4. Refereres via ID** | Holder andre aggregater kun en Guid-reference, IKKE en objektreference? | Ja — Konsultation har `Guid PatientId`, ikke `Patient Patient` |

### Aggregate Roots i MinKlinik

| Aggregate Root | Ejer (Value Objects) | Refererer til (andre AR via FK) |
|----------------|----------------------|---------------------------------|
| **Konsultation** | Tidsinterval | Patient, Behandler, Behandlingstype (via `Guid`, ikke objektref) |
| **Patient** | — | — |
| **Behandler** | — | — |
| **Behandlingstype** | — | — |

### Hvad er IKKE en Aggregate Root?

| Type | Hvorfor ikke? |
|------|---------------|
| `Tidsinterval` | Value Object — ingen egen identitet, eksisterer kun som del af Konsultation |
| `KonsultationStatus` | Enum — en simpel værdi, ikke en entity |
| `DomainException` | Exception — ikke en entity; ligger i `MinKlinik.Domain/Exceptions` |

### Klassehierarki i koden

```
Entity (abstract)              ← Har Id, Equals/GetHashCode baseret på identitet
  └── AggregateRoot (abstract) ← Markør: "dette er en transaktionsgrænse"
        ├── Konsultation
        ├── Patient
        ├── Behandler
        └── Behandlingstype
```

`Entity` og `AggregateRoot` er defineret i `MinKlinik.Domain/AggregateRoot.cs`.

### Synlighedsregel

| Type | Synlighed | Begrundelse |
|------|-----------|-------------|
| Aggregate Root | `public` | Indgangen til aggregatet — skal tilgås af Use Cases og Infrastructure |
| Non-root Entity | `internal` | Lever inden i et aggregat — tilgås kun via Aggregate Root'en |
| Value Object | `public` | Immutable værdi — kan trygt deles (f.eks. Tidsinterval i DTO-mapping) |

MinKlinik har ingen non-root entities, men reglen er vigtig i større domæner. Eksempel: Hvis `Konsultation` havde en `KonsultationsLinje`-entity, ville den være `internal` — omverdenen ville aldrig oprette eller hente den direkte, kun via `Konsultation`.

## Dependency Rule

| Lag             | Må referere til            | Må IKKE referere til   |
|-----------------|---------------------------|------------------------|
| Domain          | Intet                     | Alt andet              |
| Facade          | Intet                     | Alt andet              |
| Use Case        | Domain, Facade            | Infrastructure         |
| Infrastructure  | Facade, Use Case, Domain  | –                      |
| Api             | Facade, Infrastructure    | Domain (kun via DI)    |

## Designbeslutning: Factory-metode for overlap-validering

Booking-overlap er en **generel forretningsregel** — den gælder uanset hvilken use case
der opretter en konsultation. Derfor håndhæves den i domænet, ikke i Use Case-laget.

`Konsultation.Opret()` er en **static factory-metode** der er den eneste måde at
oprette en konsultation på (constructoren er privat):

```csharp
var konsultation = Konsultation.Opret(
    tidspunkt, behandlingstype, patient, behandler,
    eksisterendeForPatient, eksisterendeForBehandler);
```

## Forudsætninger

- .NET 10 SDK
- (Valgfrit) SQL Server 2025 / Azure SQL for `ToJson()` med `json` datatype

## Kør projektet

**Blazor (default startup i `MinKlinik.slnx`):**

```bash
dotnet run --project src/MinKlinik.Blazor/MinKlinik.Blazor.csproj
```

Frontend routes: `/`, `/stamdata`, `/konsultationer`, `/opret-konsultation`, `/afslut-konsultation`, `/aflys-konsultation`, `/counter`, `/weather`, `/not-found`, `/Error` (fejlside).

### Blazor Interactive Server (.NET 10)

**Stamdata, konsultationer og konsultation-handlinger** (`Stamdata`, `Konsultationer`, `OpretKonsultation`, `AfslutKonsultation`, `AflysKonsultation`) bruger **Interactive Server uden prerender**:

```razor
@rendermode @(new InteractiveServerRenderMode(prerender: false))
```

Det undgår typiske problemer med scoped services og datahentning under prerender.

**`Home`** og **`Counter`** bruger den korte form `@rendermode InteractiveServer` (samme render mode-familie, men uden eksplicit `prerender: false` i koden).

**`Weather`** (`/weather`) bruger **`[StreamRendering]`** — ikke Interactive Server.

Lister der hentes i `OnInitializedAsync` på data-siderne ovenfor er markeret med **`[PersistentState]`** og tildeles med null-coalescing assignment (`??=`), så navngivning matcher domænet og hydration kan genbruge persisted state hvor det understøttes:

```razor
[PersistentState]
public IReadOnlyList<PatientDto>? Patienter { get; set; }

protected override async Task OnInitializedAsync()
{
    Patienter ??= await PatientQueries.HentAlleAsync();
    // ...
}
```

**Navngivning:** Brug PascalCase på properties (som Stamdata). Siden `Konsultationer.razor` kan ikke have en property der hedder `Konsultationer` (samme navn som den genererede komponentklasse → CS0542); der bruges f.eks. `Konsultationerne`.

### Infrastruktur-note

`AddInfrastructure` findes i to varianter i `DependencyInjection.cs`. Blazor- og Api-værter kalder typisk `AddInfrastructure(builder.Configuration)` i `Program.cs`.

1. **`AddInfrastructure(IConfiguration)`** — læser `ConnectionStrings:MinKlinikDb`. Er strengen sat, delegeres til variant (2) med `UseSqlServer` (så EF debug-logging som nedenfor gælder). Er strengen tom, registreres SQLite in-memory med en åben `SqliteConnection` som singleton og `AddDbContext<AppDbContext>((serviceProvider, options) => options.UseSqlite(...))` — **uden** `LogTo` / `EnableSensitiveDataLogging` / `EnableDetailedErrors`.

2. **`AddInfrastructure(Action<DbContextOptionsBuilder> configureDb)`** — `AddDbContext<AppDbContext>(options => { ... })` hvor der **først** sættes `LogTo(Console.WriteLine)`, `EnableSensitiveDataLogging()` og `EnableDetailedErrors()`, og **derefter** kaldes `configureDb(options)` (fx `UseSqlServer` fra variant 1 eller `UseSqlServer` fra Console-appens `Program.cs`).

### Database: SQLite in-memory (test) og SQL Server (drift)

**Test og hurtig demo (Blazor og Api uden konfigureret connection string)**  
Standard-`appsettings` for Blazor og Api indeholder **ingen** `ConnectionStrings:MinKlinikDb`. Så vælger `AddInfrastructure(IConfiguration)` automatisk **SQLite in-memory** (`DataSource=:memory:`). En åben `SqliteConnection` registreres som **singleton**, så alle `AppDbContext`-instanser (på tværs af scopes) deler **samme** in-memory database — ellers ville hver scope få sin egen tomme database.

Ved opstart kalder både Blazor- og Api-`Program.cs` `SeedData.Initialize(db)`, som først kalder `Database.EnsureCreated()` og derefter fylder stamdata, **kun hvis** der ikke allerede findes rækker. Data **overlever ikke** procesgenstart; hvert app-kørsel starter med et nyt in-memory skema (og seed ved tom database). Denne gren har **ikke** den udvidede EF Core console-logging (se infrastruktur-note punkt 1).

**Drift og lokal udvikling mod SQL Server**  
Sæt `ConnectionStrings:MinKlinikDb` til en gyldig SQL Server-connection string, for eksempel i `appsettings.Development.json` / `appsettings.Production.json`, via **dotnet user-secrets**, eller med miljøvariablen `ConnectionStrings__MinKlinikDb` (Azure App Service og lignende bruger ofte sidstnævnte). Derefter bruges `UseSqlServer` gennem variant (2), inklusive `LogTo` og detaljeret EF-logging som i koden i dag — praktisk under udvikling, men i **produktion** bør man normalt styre logning og undgå sensitive SQL-detaljer i logs (et drift- og sikkerhedsspørgsmål frem for noget demoen retter ind for dig).

**Console-appen** følger et andet mønster: den kalder direkte `AddInfrastructure(options => options.UseSqlServer(...))` med en connection string i `Program.cs` — velegnet som reference for »altid SQL Server«, men i rigtig drift bør connection strings komme fra konfiguration eller et hemmelighedslager (fx Key Vault, user secrets), ikke fra kildekode.

Se også forudsætningerne om valgfri SQL Server for `ToJson()` med `json`-datatype i domænemodellen.

**API:**

```bash
dotnet run --project src/MinKlinik.Api/MinKlinik.Api.csproj
```

Åbn Scalar UI: `https://localhost:7001/scalar/v1`

**Console (menu-drevet):**

```bash
dotnet run --project src/MinKlinik.Console/MinKlinik.Console.csproj
```

Menu (numre i konsollen): 1 Vis stamdata, 2 Vis konsultationer, 3 Opret konsultation, 4 Afslut konsultation, 5 Aflys konsultation, 0 Afslut.

**Byg hele løsningen:**

```bash
dotnet build MinKlinik.slnx
```

## Test-workflow i Scalar

1. `GET /api/stamdata/patienter` → noter et patient-ID
2. `GET /api/stamdata/behandlere` → noter et behandler-ID
3. `GET /api/stamdata/behandlingstyper` → noter et behandlingstype-ID
4. `POST /api/konsultationer` → opret booking med ID'erne
5. `GET /api/konsultationer` → se bookingen
6. Prøv at oprette en overlappende booking → se DomainException

## Kør tests

```bash
dotnet test
```

## Nøgleprincipper demonstreret

- **Tre hosts**: Blazor (UI), API (HTTP) og Console (menu) deler samme DI, Use Cases og Infrastructure
- **Blazor**: Stamdata/konsultation-sider med `InteractiveServerRenderMode(prerender: false)`; `Home`/`Counter` med `InteractiveServer`; liste-DTO'er med `[PersistentState]` og `??=` hvor data hentes i `OnInitializedAsync`
- **Entity / AggregateRoot base classes**: Eksplicit markering af domæneroller
- **4 identificerede Aggregate Roots**: Konsultation, Patient, Behandler, Behandlingstype
- **Static factory-metode**: `Konsultation.Opret()` håndhæver overlap som generel forretningsregel
- **Value Objects**: `Tidsinterval` som `record` — ejet af Konsultation, ingen egen livscyklus
- **CQS**: Commands (Use Cases) returnerer `Task`, Queries returnerer DTO'er
- **Facade-lag**: Kun interfaces og DTO'er — ingen implementering
- **EF Core 10**: `ComplexProperty` + `ToJson()` til Value Objects
- **Repository per Aggregate Root**: `GemAsync()` = `SaveChangesAsync()` (ingen `Update()`!)
- **Query Handlers**: `AsNoTracking()` + `.Select()` for performance
- **Testbarhed**: Domain testes uden mocks, Use Cases testes med Moq
