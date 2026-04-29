# ADR 0001 — Aggregater refereres via Guid, ikke via objektreferencer

| | |
|---|---|
| **Status** | Accepted |
| **Dato** | 2026-04-29 |
| **Beslutter** | Arkitekturteamet (eksempel) |
| **Berørt kode** | `MinKlinik.Domain.Entities.Konsultation`, alle Aggregate Roots, `KonsultationRepository`, `AppDbContext` |

## 1. Kontekst

I MinKlinik har vi flere domæneobjekter, der står i relation til hinanden:

- En **Konsultation** har en patient, en behandler og en behandlingstype.
- **Patient**, **Behandler** og **Behandlingstype** har hver især selvstændig livscyklus, eget repository og er identificeret som **Aggregate Roots** efter de fire kriterier i `AggregateRoot.cs` (egen livscyklus, transaktionsgrænse, eget repository, refereres via ID).

Vi skal beslutte, hvordan en `Konsultation` skal referere til disse andre aggregater i selve domænekoden. Der er to gængse muligheder:

**A. Objektreferencer (navigation properties).** `Konsultation` har felter som `public Patient Patient { get; }`, `public Behandler Behandler { get; }` osv. Det er den klassiske EF Core-tilgang og det de studerende først møder i de fleste lærebogseksempler.

**B. ID-referencer.** `Konsultation` har felter som `public Guid PatientId { get; }`, `public Guid BehandlerId { get; }` osv. Når et use case har brug for navnet på patienten, henter det Patient-aggregatet eksplicit via `IPatientRepository`.

Beslutningen påvirker domænemodellens form, hvordan use cases skrives, hvordan EF Core mappes, og hvordan vores read-side (queries) ser ud. Den er derfor værd at dokumentere.

### 1.1 Drivers

Vi vægter — i prioriteret rækkefølge:

1. **At håndhæve aggregat-grænser.** En forretningsregel skal ikke kunne omgås ved at navigere fra ét aggregat ind i et andet og kalde noget på det.
2. **Tydelig transaktionsgrænse.** Ét aggregat = én transaktion. Hvis vi kan røre et andet aggregat fra inden i et aggregat, så bliver det uklart, hvilke ændringer der gemmes som ét.
3. **Genbrug i ikke-EF-kontekster.** Domænet skal kunne testes uden EF Core og potentielt bruges med andre persistens-mekanismer.
4. **Pædagogisk klarhed.** Studerende skal kunne læse domænekoden og forstå reglerne uden at slå op i en database eller en lazy-load.

## 2. Beslutning

**Andre Aggregate Roots refereres altid via `Guid`-properties — aldrig via objektreferencer.**

Konkret betyder det:

```csharp
public class Konsultation : AggregateRoot
{
    public Guid BehandlingstypeId { get; private set; }   // ✅
    public Guid PatientId        { get; private set; }    // ✅
    public Guid BehandlerId      { get; private set; }    // ✅

    // public Patient Patient { get; private set; }       // ❌ ikke tilladt
}
```

Konsekvenser i de øvrige lag:

- **Domain.** Aggregate Roots indeholder kun `Guid`-FK'er til andre aggregater. Value Objects (fx `Tidsinterval`) ejes derimod direkte af aggregatet og er stadig "rigtige" objekter.
- **UseCases.** Når et use case har brug for at validere, at en refereret entitet eksisterer, gør det det eksplicit via det relevante repository — fx `_patientRepo.HentAsync(request.PatientId)` i `OpretKonsultationUseCase`.
- **Infrastructure.** `AppDbContext.OnModelCreating` mapper kun `Guid`-FK'er som almindelige kolonner. Der er ingen `HasOne(...).WithMany(...)`-konfigurationer mellem aggregater.
- **Queries.** `KonsultationQueriesImpl` joiner manuelt mellem tabellerne via subquery på `Guid` for at hente fx `PatientNavn` ind i `KonsultationDto`. Dette er bevidst — read-siden er fri til at flade data ud (CQS).

## 3. Konsekvenser

### 3.1 Positive

- **Aggregat-grænser kan ikke omgås.** Det er bogstaveligt talt umuligt at skrive `konsultation.Patient.Suspendér()` — der er ingen `Patient`-property at navigere ind på. Forretningsreglerne på `Patient` skal gå gennem dets eget use case og repository, hvor invarianter og rettigheder kan håndhæves samlet.
- **Transaktioner er tydelige.** En `Konsultation`-transaktion kan kun ændre `Konsultation`-data. Vil man også opdatere `Patient`, kræver det en separat use case og bevidst koordinering.
- **Domænet er testbart uden EF Core.** Enhedstestene i `MinKlinik.Domain.Tests` kan instantiere `Konsultation` direkte uden DbContext, fordi der ikke er navigation properties, der skal bygges op.
- **Ingen N+1-overraskelser fra lazy loading.** Vi kan ikke ved et uheld trigge en database-roundtrip ved at læse en property på et domæneobjekt.
- **EF mapping bliver enkel.** `AppDbContext.OnModelCreating` skal ikke konfigurere komplicerede relationer mellem aggregater. Hvert aggregat står på egne ben.

### 3.2 Negative

- **Mere boilerplate i use cases.** Use cases skal eksplicit hente refererede aggregater for at validere eksistens, sådan som `OpretKonsultationUseCase.Udfør` gør det med tre separate repository-kald i starten af metoden. Dette er bevidst, men det er flere linjer kode end et navigation property-baseret design.
- **Read-siden bliver "manuel".** `KonsultationQueriesImpl` skal selv opbygge JOIN-lignende subqueries i LINQ for at flade `PatientNavn`, `BehandlerNavn` osv. ind i sin DTO. Med navigation properties ville vi kunne bruge `Include` direkte.
- **Ingen referentiel integritet på databaseniveau by default.** Da der ikke er navigation properties, opsætter EF ikke automatisk FK-constraints. Vi skal selv konfigurere dem via `HasOne().HasForeignKey()` hvis vi vil have referentiel integritet på databasen — ellers kan en `Konsultation` peget på en slettet `Patient` ende som "dangling pointer".
- **Højere kognitiv tærskel for begyndere.** Studerende, der har lært EF Core via standard-tutorials, skal vænne sig til, at relationer ikke materialiseres automatisk.

### 3.3 Neutrale

- Mønsteret kræver, at man har **et repository pr. aggregat**. Det har vi allerede besluttet uafhængigt, så det er ikke en ekstra omkostning.
- Mønsteret er konsistent med, hvordan distribuerede systemer (microservices) refererer på tværs af services — der har man under alle omstændigheder kun ID'er, fordi der ikke er en fælles database. Dette ADR gør altså domænekoden klar til en evt. fremtidig opdeling i flere services.

## 4. Alternativer overvejet

### Alternativ 1: Objektreferencer med EF navigation properties

**Beskrivelse.** `Konsultation` har `public Patient Patient { get; private set; }`. EF mapper det automatisk via FK `PatientId` (shadow property eller eksplicit). I koden kan man skrive `konsultation.Patient.Navn`.

**Hvorfor ikke valgt:**

- Den grundlæggende DDD-regel om aggregat-grænser er svær at håndhæve. Selvom man markerer property'en som `private set`, kan en udvikler stadig læse `konsultation.Patient` og kalde metoder på den. Dermed lækker invarianter mellem aggregater.
- Lazy loading kan resultere i utilsigtede database-kald langt fra det sted, koden ser ud til at læse data — dårlig pædagogik for studerende, der lige er ved at lære om performance.
- Tester man et use case, skal man instantiere et fuldt graf-træ af relaterede objekter, hvilket gør tests skøre og bundne til EF-implementationen.

### Alternativ 2: Hybrid — objektreferencer kun inden for samme aggregat

**Beskrivelse.** Vi tillader objektreferencer inden for ét aggregat (en aggregate root og dens *interne* entiteter), men forbyder dem på tværs af aggregat-grænser.

**Hvorfor ikke valgt:**

- I MinKlinik har vi pt. ingen aggregater med flere interne entiteter — `Konsultation` har kun et `Tidsinterval` (Value Object). Reglen ville være ren teori uden praktisk anvendelse i denne kodebase.
- Hvis vi senere får et aggregat med interne entiteter (fx en `Behandlingsplan` med `BehandlingsTrin`), kan vi udvide ADR'en på det tidspunkt. Vi behøver ikke at åbne for hybriden nu.

### Alternativ 3: Domænemodel uden aggregat-koncept (anæmisk model)

**Beskrivelse.** Drop hele DDD-tankegangen og brug entiteter med `public` setters og services, der manipulerer dem.

**Hvorfor ikke valgt:**

- Det er præcis det anti-mønster (anemic domain model), som projektets pædagogiske formål er at vise et alternativ til. Forretningsreglerne ville sprede sig ud over use cases, og overlap-tjekket ville ikke være beskyttet af en factory-metode.

## 5. Eksempel — sådan bruges beslutningen i praksis

### 5.1 Konsultation refererer kun via ID

```csharp
public class Konsultation : AggregateRoot
{
    public Tidsinterval Tidspunkt        { get; private set; }   // Value Object — direkte ejerskab
    public Guid         BehandlingstypeId { get; private set; }   // andet aggregat — kun ID
    public Guid         PatientId         { get; private set; }   // andet aggregat — kun ID
    public Guid         BehandlerId       { get; private set; }   // andet aggregat — kun ID
    // …
}
```

### 5.2 Use case verificerer eksistens eksplicit

```csharp
public async Task Udfør(OpretKonsultationRequest request)
{
    _ = await _behandlingstypeRepo.HentAsync(request.BehandlingstypeId)
        ?? throw new NotFoundException("Behandlingstype ikke fundet.");
    _ = await _patientRepo.HentAsync(request.PatientId)
        ?? throw new NotFoundException("Patient ikke fundet.");
    _ = await _behandlerRepo.HentAsync(request.BehandlerId)
        ?? throw new NotFoundException("Behandler ikke fundet.");

    // … og videre med Konsultation.Opret(...) som vanligt
}
```

### 5.3 Read-siden joiner selv

```csharp
return await _db.Konsultationer
    .AsNoTracking()
    .Select(k => new KonsultationDto(
        k.Id,
        k.Tidspunkt.Fra,
        k.Tidspunkt.Til,
        k.PatientId,
        _db.Patienter.Where(p => p.Id == k.PatientId)
                     .Select(p => p.Navn).FirstOrDefault() ?? "",
        // … osv.
    ))
    .ToListAsync();
```

## 6. Opfølgning og åbne spørgsmål

- **Referentiel integritet.** Når vi går væk fra in-memory SQLite og over på SQL Server i produktion, bør vi tilføje eksplicitte FK-constraints til kolonnerne `BehandlingstypeId`, `PatientId`, `BehandlerId`, så databasen forhindrer dangling references. Dette er ikke afhængigt af denne beslutning, blot en konsekvens, der skal håndteres.
- **Sletning af refererede aggregater.** Vi har endnu ikke besluttet, hvad der sker, hvis en patient slettes, mens der findes konsultationer, der refererer til vedkommende. Et nyt ADR bør behandle dette, når sletning bliver et reelt scenarie.
- **Performance på read-siden.** De manuelle subqueries i `KonsultationQueriesImpl` er fine ved få konsultationer. Hvis listen vokser, bør vi måle og overveje materialiserede read-modeller (denormaliserede tabeller eller projection cache). Det vil være indholdet af et fremtidigt ADR.

## 7. Referencer

- Eric Evans: *Domain-Driven Design* — kapitlet om Aggregates.
- Vaughn Vernon: *Implementing Domain-Driven Design* — kapitel 10, "Aggregates", og hans tre tommelfingerregler ("Reference other aggregates by identity").
- `MinKlinik.Domain.AggregateRoot` — projektets egen definition af de fire Aggregate Root-kriterier.
- `MinKlinik.Domain.Entities.Konsultation` — den kanoniske implementation af denne beslutning i kodebasen.
