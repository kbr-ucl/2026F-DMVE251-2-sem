# Kode-plan for MinKlinik baseret på lærebogen *Software der holder*

> **Formål:** Bringe MinKlinik-kodebasen i overensstemmelse med lærebogens kapitel-referencer og git-tag-strategi (`kap-01` … `kap-25`). Denne fil er en arbejds-roadmap — den lister konkrete kode-opgaver pr. kapitel med klare acceptkriterier.
>
> **Bog-kontekst:** *Software der holder — Professionel C#-udvikling med Clean Architecture, DDD og parallelisme* (Bromose Publishing, 2026, ISBN 978-87-976951-1-1). Bogens manuskript ligger i et separat repository.
>
> **Status pr. 2026-05-06:** Alle 25 kapitler skrevet i førsteudkast. Bog-deadline: **1. juni 2026**. Tag-arbejdet sker baglæns fra `main` (= `kap-25`) — se §2.

---

## 1. Strategiske beslutninger der skal afspejles i koden

| Beslutning | Konsekvens for MinKlinik |
|------------|--------------------------|
| **Blazor render-mode = Interactive Server** (ikke SSR) | `MinKlinik.Blazor` skal bruge `@rendermode InteractiveServer`. `Components/_Imports.razor` eller per-komponent. |
| **Kap. 14 = ægte HTML5 + CSS-intro** | Der skal ligge en simpel statisk HTML-side et sted (kan være i `docs/eksempler/kap-14-static.html`) som læseren kan åbne uden Blazor. |
| **Kap. 19 deles i to** (samtidighed + synkronisering) | Påvirker ikke kode-strukturen, kun bog-tag-strategien — `kap-19` og `kap-20` får begge race-conditions/lock-eksempler. |
| **Kap. 12 dækker `OwnsOne`/`ComplexProperty`/`ToJson`** | `Tidsinterval` skal mappes med `ComplexProperty` i `MinKlinikDbContext.OnModelCreating` (eller en `KonsultationConfiguration`-klasse). Reference-implementation findes i søsterprojektet `EfValueObjectsSqlServer2025Demo`. |

---

## 2. Tag-strategi — *bag-tag baglæns*

I stedet for at *bygge MinKlinik op trinvist* fra kap. 1, opbygges tags *baglæns* fra den færdige løsning:

1. Tag den nuværende færdige `main`-branch som `kap-25` (eller `kap-final`).
2. Lav `kap-24` ved at fjerne kapitel 25's stof (helhedsbillede-tilføjelser).
3. Lav `kap-23` ved at fjerne den parallelle Strategy-rabat.
4. Fortsæt baglæns til `kap-01` der kun har en simpel `Konsultation`-klasse.

**Fordele:** Hver tag er garanteret en valid delmængde — koden kompilerer og tests passer på enhver tag. Ændringer i den endelige løsning forplanter sig naturligt baglæns.

**Konkret arbejdsproces pr. tag:**

```bash
# Stå på den nyeste tag du har bygget
git checkout kap-N

# Lav en ny branch til fjernelses-arbejdet
git checkout -b prep-kap-(N-1)

# Fjern det stof kapitel N introducerer (ifølge planen nedenfor)
# Verificér: dotnet build && dotnet test
git commit -am "Forberedelse: trin baglæns mod kap-(N-1)"

# Tag den
git tag kap-(N-1)
```

---

## 3. Per-kapitel kode-opgaver

For hver kapitel angives:
- **Tag-navn**: git-tag der skal eksistere.
- **Kode-tilstand**: hvad koden skal indeholde (eller mangle) på denne tag.
- **Acceptkriterier**: konkrete, verificerbare betingelser.
- **Filer**: hvor det realiseres.

### kap-01 — Indkapsling, kohæsion og kobling

**Kode-tilstand:** Simpel `Konsultation`-klasse med `private set`, guard clauses i konstruktøren, og metoderne `Aflys()` + `Afslut()`. Plus de nye §1.3.9-10-eksempler om interface og polymorfi.

**Acceptkriterier:**
- [ ] `src/MinKlinik.Domain/Entities/Konsultation.cs` eksisterer med `private set`-properties for `Tidspunkt`, `PatientId`, `BehandlerId`, `Status`, `Notat`.
- [ ] Konstruktøren validerer `Tidspunkt >= DateTime.UtcNow`, `PatientId != Guid.Empty`, `BehandlerId != Guid.Empty`.
- [ ] `Aflys()` kaster `DomainException` hvis `Status == Afsluttet`.
- [ ] `Afslut(string notat)` kaster `DomainException` hvis status ikke er `Planlagt` eller notat er tom.
- [ ] **NYT (jf. kap. 1 §1.3.9-10):** `src/MinKlinik.Domain/Notifikation/INotifikation.cs` eksisterer med `Task SendBekræftelseAsync(Konsultation k)`-metode.
- [ ] **NYT:** `src/MinKlinik.Infrastructure/Notifikation/EmailNotifikation.cs` implementerer `INotifikation`.
- [ ] `dotnet build` succesful, `dotnet test` grøn.

**Filer:**
- `src/MinKlinik.Domain/Entities/Konsultation.cs`
- `src/MinKlinik.Domain/Enums/KonsultationStatus.cs`
- `src/MinKlinik.Domain/Exceptions/DomainException.cs`
- *(nye)* `src/MinKlinik.Domain/Notifikation/INotifikation.cs`
- *(nye)* `src/MinKlinik.Infrastructure/Notifikation/EmailNotifikation.cs`

### kap-02 — SRP, OCP, LSP og ISP

**Kode-tilstand:** Tilføjer `SmsNotifikation` som anden `INotifikation`-implementation (OCP-eksempel). Splittet repository-interfaces i `IKonsultationReader` og `IKonsultationWriter` (ISP-eksempel). `KonsultationsNotifier` der tager `IEnumerable<INotifikation>` (polymorfi).

**Acceptkriterier:**
- [ ] `src/MinKlinik.Infrastructure/Notifikation/SmsNotifikation.cs` eksisterer.
- [ ] `src/MinKlinik.UseCases/Notifikation/KonsultationsNotifier.cs` (eller tilsvarende) tager `IEnumerable<INotifikation>` i konstruktøren.
- [ ] `src/MinKlinik.UseCases/Repositories/IKonsultationReader.cs` indeholder kun query-metoder.
- [ ] `src/MinKlinik.UseCases/Repositories/IKonsultationWriter.cs` indeholder kun command-metoder.
- [ ] `src/MinKlinik.UseCases/Repositories/IKonsultationRepository.cs` *eksisterer ikke længere som combined* — eller den er en marker `: IKonsultationReader, IKonsultationWriter`.
- [ ] `KonsultationRepository` implementerer både `IKonsultationReader` og `IKonsultationWriter`.
- [ ] `dotnet build` succesful, `dotnet test` grøn.

### kap-03 — Dependency Inversion Principle og IoC-containeren

**Kode-tilstand:** `MinKlinik.Console/Program.cs` bruger `Host.CreateApplicationBuilder` med `services.AddScoped<...>` for repositories, notifier, og use cases. Manuel `using var scope = ...` for at åbne scope.

**Acceptkriterier:**
- [ ] `src/MinKlinik.Console/Program.cs` bruger `Host.CreateApplicationBuilder(args)`.
- [ ] `KonsultationRepository` registreres mod *både* `IKonsultationReader` og `IKonsultationWriter`.
- [ ] Use cases registreres som Scoped.
- [ ] Konsolappen åbner et scope manuelt med `host.Services.CreateScope()` og henter use casen via `GetRequiredService<IOpretKonsultationUseCase>()`.
- [ ] `dotnet run --project src/MinKlinik.Console` kører uden fejl.

### kap-04 — Pre- og post-conditions og guard clauses

**Kode-tilstand:** Som kap. 03 — den ufuldstændige nuværende `Konsultation` har nu XML-doc med Pre/Post/Inv-kommentarer. Ingen ny kode, kun dokumentations-tilføjelser.

**Acceptkriterier:**
- [ ] `Konsultation.Aflys`, `Konsultation.Afslut`, `Konsultation.Opret` har XML-doc-kommentarer der beskriver Pre/Post/Inv.
- [ ] Ingen ny funktionalitet — testene fra kap-03 er stadig grønne.

### kap-05 — Unit testing med xUnit og Moq

**Kode-tilstand:** Test-projekter med navngivnings-konvention `Metode_Tilstand_ForventetResultat` på dansk. Mindst tre tests pr. domæne-klasse og pr. use case. **NYT:** Test-projekterne refererer NuGet-pakken `FixtureBuilder` og bruger den i mindst én test for at klargøre et always-valid aggregate til en specifik tilstand uden at gå gennem factory.

**Acceptkriterier:**
- [ ] `tests/MinKlinik.Domain.Tests/MinKlinik.Domain.Tests.csproj` har `<PackageReference Include="xunit.v3" />` og `<PackageReference Include="FixtureBuilder" />`.
- [ ] `tests/MinKlinik.UseCases.Tests/MinKlinik.UseCases.Tests.csproj` har samme plus `<PackageReference Include="Moq" />`.
- [ ] `tests/MinKlinik.Domain.Tests/KonsultationTests.cs` har minimum:
  - `Aflys_NårStatusErPlanlagt_SætterStatusTilAflyst` — bygger via factory.
  - `Aflys_NårStatusErAfsluttet_KasterDomainException` — bygger via FixtureBuilder (`new Fixture<Konsultation>().CreateUninitialized().With(k => k.Status, KonsultationStatus.Afsluttet).Build()`).
  - `Afslut_MedTomNotat_KasterDomainException` — bygger via factory.
  - Plus en separat `KonsultationOpretTests`-fil eller -test-class der eksplicit verificerer factory'ens guard clauses (så FixtureBuilder-baserede tests ikke skjuler regressioner i factory-validering — jf. §5.3.5's "Stop og tænk").
- [ ] `tests/MinKlinik.UseCases.Tests/OpretKonsultationUseCaseTests.cs` bruger Moq for `IKonsultationRepository` og `INotifikation`.
- [ ] Mindst én `[Theory]` med `[InlineData]` (fx `Tidsinterval_AccepterePositiveVarigheder`).
- [ ] Mindst én test bruger FixtureBuilder's `.With(...)` til at sætte en private-set-property uden at gå gennem konstruktør.
- [ ] `dotnet test` grøn.

### kap-06 — Hvorfor DDD?

**Kode-tilstand:** Domæne-klasser bruger danske navne (`Konsultation`, `Patient`, `Behandler`, `Behandlingstype`, `Tidsinterval`). `MinKlinik.Domain` har ingen NuGet-references til EF Core. Rig model — adfærd bor sammen med data.

**Acceptkriterier:**
- [ ] `src/MinKlinik.Domain/MinKlinik.Domain.csproj` har ingen `<PackageReference>` til `Microsoft.EntityFrameworkCore.*`.
- [ ] `src/MinKlinik.Domain/MinKlinik.Domain.csproj` har ingen `<ProjectReference>`.
- [ ] Domæneklasse-navne er dansk: `Konsultation`, `Patient`, `Behandler`, `Behandlingstype`.

### kap-07 — Entity, Value Object og Aggregate Root

**Kode-tilstand:** Formaliseret `Entity`/`AggregateRoot` base-klasser. `Tidsinterval` som `record`. `KonsultationStatus` som enum.

**Acceptkriterier:**
- [ ] `src/MinKlinik.Domain/Entity.cs` med `Guid Id` + value-equality på `Id`.
- [ ] `src/MinKlinik.Domain/AggregateRoot.cs` arver `Entity`.
- [ ] Alle aggregate roots (`Konsultation`, `Patient`, `Behandler`, `Behandlingstype`) arver `AggregateRoot`.
- [ ] `src/MinKlinik.Domain/ValueObjects/Tidsinterval.cs` er en `sealed record(DateTime Fra, DateTime Til)` med `OverlapperMed`.
- [ ] Indre samlinger (fx eventuelle `KonsultationsBesked`) er `private readonly` med `IReadOnlyList<>`-eksponering.

### kap-08 — Domænereglen i koden — invarianter, factory og guard

**Kode-tilstand:** Privat konstruktør + statisk `Opret`-factory på alle aggregate roots. `Konsultation.Opret` modtager eksisterende konsultationer og kalder `ValidérIngenOverlap`.

**Acceptkriterier:**
- [ ] `Konsultation` har `private` konstruktør plus `private Konsultation()` til EF Core.
- [ ] `Konsultation.Opret(Tidsinterval, Guid patient, Guid behandler, Guid behandlingstype, IEnumerable<Konsultation> eksisterendeForPatient, IEnumerable<Konsultation> eksisterendeForBehandler)` er den eneste lovlige indgang.
- [ ] `ValidérIngenOverlap` kaster `DomainException` ved overlap.
- [ ] Tilsvarende factory-mønster på `Patient`, `Behandler`, `Behandlingstype`.

### kap-09 — Lagdeling og Dependency Rule

**Kode-tilstand:** Solution-strukturen er på plads (Domain, UseCases, Facade, Infrastructure, Console, Api, Blazor). ProjectReferences peger korrekt indad.

**Acceptkriterier:**
- [ ] `MinKlinik.Domain.csproj`: ingen ProjectReferences.
- [ ] `MinKlinik.UseCases.csproj`: kun → Domain.
- [ ] `MinKlinik.Facade.csproj`: → Domain, → UseCases.
- [ ] `MinKlinik.Infrastructure.csproj`: → Domain, → UseCases.
- [ ] `MinKlinik.Console.csproj`: → Facade, → Infrastructure, → UseCases.
- [ ] `MinKlinik.Api.csproj`: → Facade, → Infrastructure, → UseCases.
- [ ] `MinKlinik.Blazor.csproj`: → Facade, → Infrastructure, → UseCases.
- [ ] `dotnet build MinKlinik.slnx` succesful.

### kap-10 — Use Case-laget og CQS

**Kode-tilstand:** Use cases organiseret pr. aggregate. Hver use case har én `Udfør`-metode. Adskillelse af commands og queries.

**Acceptkriterier:**
- [ ] `src/MinKlinik.UseCases/Konsultation/` indeholder: `OpretKonsultationUseCase.cs`, `AflysKonsultationUseCase.cs`, `AfslutKonsultationUseCase.cs`, `HentKonsultationQuery.cs`, `HentDagensKonsultationerQuery.cs`.
- [ ] Hver use case har én public `Udfør`-metode.
- [ ] Commands returnerer `Task<Guid>` eller `Task`.
- [ ] Queries returnerer `Task<DTO>` eller `Task<List<DTO>>` — *ikke* domain-objekter.

### kap-11 — Facade-laget — kontrakten

**Kode-tilstand:** Use case-interfaces og DTOs i `MinKlinik.Facade`. Klienter (Console, Api, Blazor) afhænger kun af Facade for use case-kald.

**Acceptkriterier:**
- [ ] `src/MinKlinik.Facade/UseCases/Konsultation/IOpretKonsultationUseCase.cs` og tilsvarende interfaces eksisterer.
- [ ] `src/MinKlinik.Facade/Dtos/Konsultation/OpretKonsultationRequest.cs` og `KonsultationDto.cs` eksisterer som `record`s.
- [ ] DTOs lækker ikke domain-typer (`KonsultationStatus` enum konverteres til `string`).
- [ ] `MinKlinik.Blazor` referer kun `MinKlinik.Facade` for use case-typer (plus `MinKlinik.Domain` evt. for grundlæggende typer som `Guid`-id'er — men ikke for `Konsultation`-aggregatet).

### kap-12 — Infrastructure med EF Core 10

**Kode-tilstand:** `MinKlinikDbContext`, `IEntityTypeConfiguration`-klasser, repository-implementationer, og særligt: `Tidsinterval` mappet med `ComplexProperty` (ikke `OwnsOne`).

**Acceptkriterier:**
- [ ] `src/MinKlinik.Infrastructure/MinKlinikDbContext.cs` har `DbSet`-properties for hvert aggregate root.
- [ ] `src/MinKlinik.Infrastructure/Configurations/KonsultationConfiguration.cs` bruger `ComplexProperty(k => k.Tidspunkt, ...)` for `Tidsinterval`.
- [ ] **NYT:** Sammenligningsdokumentation eller test der viser de tre strategier (`OwnsOne`, `ComplexProperty`, `ComplexProperty().ToJson()`) — eller en henvisning til søsterprojektet `EfValueObjectsSqlServer2025Demo`.
- [ ] `KonsultationRepository` implementerer `IKonsultationReader` *og* `IKonsultationWriter`.
- [ ] Migrations-mappe eksisterer med en initial migration.
- [ ] `dotnet ef migrations list` viser den initiale migration.

### kap-13 — LINQ og deferred execution

**Kode-tilstand:** Repository-metoder bruger LINQ to Entities med `Include` for at undgå N+1. Use cases bruger LINQ to Objects på materialiserede data.

**Acceptkriterier:**
- [ ] Mindst én repository-metode bruger `Include(...)` for navigation properties (fx `HentForBehandlerOgDatoAsync` med `Include(k => k.Patient)`).
- [ ] Mindst én query-use case bruger `GroupBy` + projektion til DTO (fx `HentMånedsstatistikQuery`).
- [ ] Ingen `ToListAsync()` efterfulgt af `Where(...)` i samme metode (klassisk anti-pattern).

### kap-14 — Semantisk HTML5 og CSS

**Kode-tilstand:** En statisk HTML+CSS-eksempel-fil der viser semantisk markup uden Blazor. Læseren skal kunne åbne den i en browser uden at starte .NET.

**Acceptkriterier:**
- [ ] `docs/eksempler/kap-14-statisk.html` eksisterer med korrekt HTML5-skelet, semantiske elementer (`<header>`, `<nav>`, `<main>`, `<article>`, `<aside>`, `<footer>`), og overskrifts-hierarki.
- [ ] `docs/eksempler/site.css` eksisterer og styler den statiske side.
- [ ] Filen kan åbnes med `start docs\eksempler\kap-14-statisk.html` (Windows) og rendres læsbart uden internet.

### kap-15 — Blazor Interactive Server — komponenter, parametre, lifecycle

**Kode-tilstand:** Blazor-komponenter bruger Interactive Server render-mode. `@inject` for use case-interfaces. `OnInitializedAsync` lifecycle-metode demonstreret.

**Acceptkriterier:**
- [ ] `src/MinKlinik.Blazor/Components/_Imports.razor` eller per-komponent har `@rendermode InteractiveServer`.
- [ ] Mindst én komponent bruger `@inject IOpretKonsultationUseCase UseCase` (eller tilsvarende).
- [ ] Mindst én komponent overrider `OnInitializedAsync` til at hente data via en query.
- [ ] `Program.cs` registrerer komponenterne med `AddRazorComponents().AddInteractiveServerComponents()`.

### kap-16 — Forms, validering og EditForm

**Kode-tilstand:** Mindst én `<EditForm>`-komponent med `DataAnnotations`-validering der binder til en request-DTO fra Facade.

**Acceptkriterier:**
- [ ] `src/MinKlinik.Blazor/Components/Konsultation/OpretKonsultationForm.razor` bruger `<EditForm Model="@request" OnValidSubmit="@SubmitAsync">`.
- [ ] `OpretKonsultationRequest` (Facade) eller en separat form-DTO har `[Required]`/`[Range]`-attributter.
- [ ] Form viser valideringsfejl med `<ValidationSummary />` eller `<ValidationMessage For="..." />`.

### kap-17 — CRUD via Facade-laget

**Kode-tilstand:** Komplet CRUD-flow for konsultationer i Blazor: opret, hent liste, opdater, aflys. Alle kald går gennem Facade-interfaces.

**Acceptkriterier:**
- [ ] Komponenter for: liste-visning, detalje-visning, opret-form, redigér-form. Aflys håndteres som knap på detalje- eller liste-visning.
- [ ] Alle `@inject` peger på Facade-interfaces — *ingen* `@inject KonsultationRepository` eller `@inject MinKlinikDbContext`.
- [ ] `try/catch (DomainException)` viser fejlbesked til brugeren via en `ErrorMessage`-property eller en toast/banner.

### kap-18 — Strategy-pattern i praksis (rabatberegning)

**Kode-tilstand:** `IRabatStrategi`-interface med flere implementationer (`StandardRabat`, `SeniorRabat`, `BlackFridayRabat`). Registreres i IoC-container. Bruges af en use case eller komponent.

**Acceptkriterier:**
- [ ] `src/MinKlinik.Domain/Rabat/IRabatStrategi.cs` eksisterer.
- [ ] Mindst tre implementationer.
- [ ] Strategierne registreres i composition root.
- [ ] En use case eller komponent vælger strategi baseret på input (fx patientens type).

### kap-19 + kap-20 — Tråde, samtidighedsproblemer og synkroniseringsmekanismer

**Kode-tilstand:** Demonstrations-eksempler — ikke nødvendigvis i MinKlinik's main-kode, men i en `samples/`-mappe. Race condition-eksempel og lock-løsning. **Bogen bruger `System.Threading.Lock`** (.NET 9+ / C# 13) som default låse-type — kode-eksemplerne skal følge samme konvention.

**Acceptkriterier:**
- [ ] `samples/Samtidighed/RaceConditionDemo.cs` viser en counter med race condition.
- [ ] `samples/Samtidighed/LockDemo.cs` viser samme counter med `private readonly Lock _lås = new();` (ikke `object`) eller `Interlocked.Increment`.
- [ ] (Valgfrit) `samples/Samtidighed/SemaphoreDemo.cs` viser `SemaphoreSlim`.
- [ ] (Valgfrit) `samples/Samtidighed/MonitorWaitPulseDemo.cs` — det ene legitime sted hvor `object`-baseret låse stadig er nødvendig (Monitor.Wait/Pulse understøtter ikke `Lock`-typen).

### kap-21 — Async/await for I/O-bound

**Kode-tilstand:** Alle repository-metoder er `Async`. Use cases bruger `await`. Mindst ét eksempel på `Task.WhenAll` for parallel I/O.

**Acceptkriterier:**
- [ ] Alle repository-metoder har `Async`-suffix og returnerer `Task<T>`.
- [ ] Mindst ét sted i koden bruges `await Task.WhenAll(...)` til at vente på flere I/O-operationer parallelt.
- [ ] CancellationToken accepteres i mindst de offentlige use case-metoder.

### kap-22 — Tasks og Parallel.For for CPU-bound

**Kode-tilstand:** Et CPU-tungt eksempel der bruger `Parallel.ForEachAsync`. Kan være rabatberegning over en stor liste, eller en statistisk analyse.

**Acceptkriterier:**
- [ ] `samples/Parallelisme/ParallelRabatBeregning.cs` (eller tilsvarende) bruger `Parallel.ForEachAsync` over en samling.
- [ ] Sammenligning med sekventiel version (Stopwatch).

### kap-23 — Parallel Strategy-rabat — integration i MinKlinik

**Kode-tilstand:** Strategy-mønstret fra kap. 18 anvendt parallelt på mange konsultationer.

**Acceptkriterier:**
- [ ] En `BeregnRabatterParallelt`-use case der bruger `Parallel.ForEachAsync` og `IRabatStrategi`-implementationer.
- [ ] Tests der verificerer konsistens på tværs af parallel kørsel (fx ved hjælp af `ConcurrentBag` eller lignende).

### kap-24 — Big O og algoritmisk analyse

**Kode-tilstand:** Sammenligning af to algoritmer på samme problem (fx O(n²) vs O(n log n) sortering). Kan være i `samples/Algoritmer/`.

**Acceptkriterier:**
- [ ] `samples/Algoritmer/BigO-Sammenligning.cs` med Stopwatch-måling.
- [ ] (Valgfrit) BenchmarkDotNet-projekt for præcis måling.

### kap-25 — Helhedsbillede

**Kode-tilstand:** Den fulde `main`-branch — alle features, alle lag, alle kapitler' stof. Det er det sted MinKlinik er færdig.

**Acceptkriterier:**
- [ ] `dotnet build MinKlinik.slnx` succesful uden warnings.
- [ ] `dotnet test` 100 % grøn.
- [ ] README.md er opdateret med arkitektur-diagram og kørselsvejledning for alle host-projekter (Console, Api, Blazor).

---

## 4. Tværgående opgaver

### 4.1 Render-mode skift fra SSR til Interactive Server

**Hvor:** `src/MinKlinik.Blazor/`

**Konkret:**
- [ ] Tilføj `@rendermode InteractiveServer` i `_Imports.razor` eller på root-komponenter.
- [ ] Verificér at `Program.cs` har `services.AddRazorComponents().AddInteractiveServerComponents()` (ikke kun `AddRazorComponents()`).
- [ ] Tjek at SignalR-circuit fungerer: åbn en side, lav en interaktion, verificér at side ikke laver full reload.

### 4.2 NuGet-pakke-tjek

**Konkret:**
- [ ] Alle projekter target `net10.0`.
- [ ] EF Core-pakker er version 10.0.x.
- [ ] xUnit er v3 (`xunit.v3`).
- [ ] Moq for use case-tests.
- [ ] **NYT:** `FixtureBuilder` (Dennis Johnsen) i begge test-projekter (`Domain.Tests` og `UseCases.Tests`) — bruges til klargøring af always-valid aggregates uden factory-rejse.

### 4.3 Verifikations-script

**Konkret:**
- [ ] Tilføj `scripts/verify-tag.ps1` (eller `.sh`) der for et givet tag-navn:
  1. Checker ud det specifikke tag.
  2. Kører `dotnet build MinKlinik.slnx`.
  3. Kører `dotnet test`.
  4. Returnerer success/failure.
- [ ] Kør scriptet for hver tag i `kap-01..kap-25` rækkefølge for at validere bag-tag-strategien.

### 4.4 Opdatering af README

**Konkret:**
- [ ] Tilføj sektion til `README.md` der forklarer tag-strategien.
- [ ] Tilføj liste over tags og hvad hver dækker.
- [ ] Krydshenvis til lærebogens `Planlægning/Inventory-opdatering 2026-05-05.md` for skrive-status.

---

## 5. Arbejdsrækkefølge

Anbefalet rækkefølge for implementering (fra mest pressende til mindst):

1. **Tværgående 4.1** (Blazor render-mode) — påvirker Del 4 (kap. 15-18) og bør være på plads før kap. 15-tags laves.
2. **kap-25 → kap-01 baglæns** — tag-by-tag bag-strategi.
3. **Tværgående 4.3** (verifikations-script) — efter første tag er på plads, så efterfølgende kan automatisk valideres.
4. **kap-12 ComplexProperty** (særligt vigtigt — det er det største DDD-relaterede skift i Infrastructure).
5. **kap-19/20 + kap-22 samples** — kan være de sidste, da de er separate eksempler.

---

## 6. Acceptkriterium for hele planen

Når denne plan er fuldt implementeret:

- [ ] Alle 25 git-tags eksisterer i MinKlinik-repositoriet.
- [ ] Hver tag bygger og passerer tests.
- [ ] Hver tag matcher det stof bogen henviser til.
- [ ] README.md krydshenviser til lærebogen.
- [ ] Lærebogens `Inventory-opdatering` markerer alle kapitlers MinKlinik-anvendelse som verificeret.

---

## 7. Påmindelse til Claude Code (eller dig selv)

- Hvert tag-skift skal verificeres med `dotnet build && dotnet test` før commit.
- Brug `git diff kap-N..kap-(N-1)` til at se hvad der ændres mellem to tags — det er en sundhedstjek for om bog-fortællingen matcher kode-fortællingen.
- Hvis et acceptkriterium ikke kan opfyldes, så *opdatér bogen* i stedet for at presse koden — bog og kode skal stemme overens, og bogen er det forklarende lag.
- Ved tvivl om et kapitels intention: åbn `..\Lærebog\Kapitler\Kapitel NN - ....md` og §N.4 ("Anvendelse i MinKlinik").
