# MinKlinik — Clean Architecture Demo

Et klinik-bookingsystem bygget med Clean Architecture, DDD, Facade-lag og CQS.  
Undervisningsmateriale til 2026F-DMVE251-2-sem, uge 9–12.

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
│   ├── MinKlinik.Blazor/            # Blazor frontend (InteractiveServer pages)
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
| `DomainException` | Infrastruktur — ikke et domænekoncept |

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

Frontend routes: `/`, `/stamdata`, `/konsultationer`, `/opret-konsultation`, `/afslut-konsultation`, `/aflys-konsultation`.

### Blazor prerender (.NET 10)

MinKlinik.Blazor bruger `InteractiveServer` med prerender aktiveret, og cachelagrer initial data med `[PersistentState]`:

```razor
[PersistentState]
public IReadOnlyList<KonsultationDto>? _konsultationer { get; set; }
```

Det undgår dobbelt datahentning mellem prerender og interaktiv render, uden at slå prerender fra.

### Infrastruktur-note

`AddInfrastructure(Action<DbContextOptionsBuilder>)` kalder nu altid `configureDb(options)` først. Det sikrer korrekt provider-konfiguration (`UseSqlServer`/`UseInMemoryDatabase`) i alle hosts, hvorefter man kan tilføje eventuel debug-logging.

**API:**

```bash
dotnet run --project src/MinKlinik.Api/MinKlinik.Api.csproj
```

Åbn Scalar UI: `https://localhost:7001/scalar/v1`

**Console (menu-drevet):**

```bash
dotnet run --project src/MinKlinik.Console/MinKlinik.Console.csproj
```

Menu: Vis stamdata, Vis konsultationer, Opret konsultation, Afslut, Aflys.

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
