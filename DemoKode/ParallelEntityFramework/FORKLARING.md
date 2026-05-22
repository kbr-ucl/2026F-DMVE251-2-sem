# Forklaring af Parallel Entity Framework-demo

Denne fil beskriver **hvad koden gør**, **hvordan den er bygget op**, og **hvorfor** de to varianter opfører sig forskelligt under parallel kørsel.

## Overblik

Projektet er en konsol-app, der kører **100 parallelle database-læsninger** mod en SQLite-kopi af Northwind. Formålet er at vise forskellen mellem:

| Variant | Fil | Strategi | Typisk resultat |
|---------|-----|----------|-----------------|
| **A** | `CustomerStatsTask.cs` | Én delt `DbContext` (singleton) | Mange fejl — context er ikke trådsikker |
| **B** | `CustomerStatsFactoryTask.cs` | `IDbContextFactory` — ny context per kald | 100/100 OK på EF-niveau |

Begge varianter bruger **samme LINQ-query** (`CustomerStatsQuery`) med **`AsNoTracking()`**. Det viser, at read-only queries alene ikke gør en delt context sikker til parallel brug.

```
Program.cs
    ├── Variant A → CustomerStatsTask → CustomerStatsQuery
    └── Variant B → CustomerStatsFactoryTask → (ny context) → CustomerStatsQuery
```

## Kørsel og afhængigheder

- **Framework:** .NET 10 (`net10.0`)
- **Pakker:** EF Core 10, EF SQLite, `Microsoft.Extensions.DependencyInjection`
- **Database:** `Northwind_large.sqlite` kopieres til `bin/` ved build (se `.csproj`)

Appen finder databasen i output-mappen (`AppContext.BaseDirectory`), tjekker at filen findes, og bygger connection string: `Data Source={sti}`.

## Datamodel (`Models/`)

Tre entiteter matcher Northwind-tabellerne:

- **`Customer`** — `Id`, `CompanyName`, navigation `Orders`
- **`Order`** — `Id`, `CustomerId`, navigation til `Customer` og `OrderDetails`
- **`OrderDetail`** — linjeposter med `UnitPrice`, `Quantity`, `Discount`, knyttet til `Order`

Klasserne er simple POCO’er uden EF-attributter; mapping sker i `NorthwindDbContext`.

## Entity Framework (`Data/NorthwindDbContext.cs`)

`NorthwindDbContext` arver `DbContext` og eksponerer tre `DbSet`:

- `Customers`, `Orders`, `OrderDetails`

I `OnModelCreating` konfigureres:

- Tabelnavne: `Customer`, `Order`, `OrderDetail`
- Primærnøgler (`HasKey`)
- Relationer: Customer ↔ Order ↔ OrderDetail (`HasOne` / `WithMany` / `HasForeignKey`)

Contexten oprettes altid via DI med `DbContextOptions<NorthwindDbContext>` — aldrig `new NorthwindDbContext()` direkte i applikationskoden.

## DTO (`Dtos/CustomerStatsDto.cs`)

Et **record** med de fire værdier, som query’en returnerer:

- `CustomerId`, `CompanyName`, `OrderCount`, `TotalOrderSum`

DTO’en holder resultatet adskilt fra EF-entiteter og gør det nemt at logge i konsollen.

## Fælles query (`Queries/CustomerStatsQuery.cs`)

`GetRandomCustomerStatsAsync` er den **fælles forretningslogik** begge varianter kalder. Den tager en `NorthwindDbContext` som parameter (variant A sender den delte; variant B sender en ny fra factory).

Trin for trin:

1. **Tæl kunder** — `Customers.AsNoTracking().CountAsync()`
2. **Vælg tilfældig kunde** — `Random.Shared.Next(customerCount)` og `Skip(skip)` efter `OrderBy(c => c.Id)` (stabil rækkefølge)
3. **Hent kunde + antal ordrer** — projektion med `Orders.Count` i én query
4. **Summér linjebeløb** — over `OrderDetails` for kundens ordrer:

   `UnitPrice * Quantity * (1 - Discount)`

Alle læsninger bruger **`AsNoTracking()`**, så EF ikke bygger change tracker eller holder entiteter i hukommelsen. Det er korrekt for read-only parallel læsning, men **løser ikke** at samme `DbContext`-instans bruges fra flere tråde samtidigt.

## Variant A — delt context (`Tasks/CustomerStatsTask.cs`)

```csharp
public sealed class CustomerStatsTask(NorthwindDbContext db)
{
    public Task<CustomerStatsDto> ExecuteAsync(...)
        => CustomerStatsQuery.GetRandomCustomerStatsAsync(db, cancellationToken);
}
```

`Program.cs` registrerer:

- `AddDbContext<NorthwindDbContext>(..., ServiceLifetime.Singleton)` — **én** context til hele app-livscyklussen
- `CustomerStatsTask` som singleton

Når `Parallel.For` kører 100 gange med `MaxDegreeOfParallelism = ProcessorCount`, deler alle tråde **samme** `DbContext`. EF Core er **ikke thread-safe** på en instans; concurrent `SaveChanges`, tracking og connection-brug kan give `InvalidOperationException` (“A second operation was started on this context…”).

## Variant B — context factory (`Tasks/CustomerStatsFactoryTask.cs`)

```csharp
public async Task<CustomerStatsDto> ExecuteAsync(...)
{
    await using var db = await factory.CreateDbContextAsync(cancellationToken);
    return await CustomerStatsQuery.GetRandomCustomerStatsAsync(db, cancellationToken);
}
```

`Program.cs` registrerer:

- `AddDbContextFactory<NorthwindDbContext>(...)` — factory kan oprette nye contexts
- `CustomerStatsFactoryTask` som singleton (factory’en er thread-safe; contexts er det ikke)

Hver parallel iteration får **sin egen** context, bruger den til query’en, og **`await using`** disposer contexten bagefter (connection frigives). Det er den anbefalede mønster for parallel/arbejde i baggrunden i ASP.NET og worker-services.

**Bemærk:** Under meget høj parallelitet kan **SQLite** stadig kaste `SQLite Error 5: database is locked`. Det er en database-motor-begrænsning (én writer ad gangen), ikke et EF-tracking-problem.

## Programflow (`Program.cs`)

### Konstanter og opsætning

- `TaskCount = 100` — antal parallelle iterationer per variant
- Database-sti og fejl, hvis fil mangler
- Exit code: `0` hvis variant B lykkes fuldt ud, ellers `1` (variant A forventes ofte delvist fejlet)

### To DI-containere

Hver variant får sin egen `ServiceCollection` og `BuildServiceProvider()`, så livstid og registrering ikke blandes:

1. **Variant A** — `RunSharedContextVariantAsync`
2. **Variant B** — `RunFactoryVariantAsync`

### `RunParallelDemoAsync` — test-harness

Fælles metode der:

1. **Warmup** — ét kald på hovedtråden (verificerer DB og query uden parallelisme)
2. **`Parallel.For`** — 0..99 med `MaxDegreeOfParallelism = Environment.ProcessorCount`
3. Samler resultater i `ConcurrentBag<CustomerStatsDto>` og fejl i `ConcurrentBag<Exception>`
4. Bruger `execute().GetAwaiter().GetResult()` inde i `Parallel.For` (synkron blokering per tråd — acceptabelt i denne demo, men i produktion foretrækkes ofte `Parallel.ForEachAsync` + `await`)

Output:

- Tid i ms, antal success/fejl
- De første 5 resultater
- Fejl grupperet efter undtagelsestype med besked og inner exceptions

Returnerer `true` kun hvis `errors.Count == 0`.

## Hvad du bør tage med fra demoen

1. **`AsNoTracking()`** — godt til read-only og mindre hukommelse; **ikke** en erstatning for thread-safe context-brug.
2. **Én `DbContext` per enhed af parallel arbejde** (tråd, request, baggrundsjob) — typisk via `IDbContextFactory<T>`.
3. **Singleton `DbContext`** — passende i nogle single-threaded scenarier; **undgå** ved `Parallel.For`, `Task.WhenAll` på samme instans, osv.
4. **SQLite under load** — kan låse selv med korrekt EF-brug; løsninger kan være WAL, connection pooling, eller en server-database.

## Filstruktur (reference)

| Mappe/fil | Rolle |
|-----------|--------|
| `Program.cs` | Entry, DI, parallel test, konsol-output |
| `Models/` | Entiteter Customer, Order, OrderDetail |
| `Data/NorthwindDbContext.cs` | EF mapping og DbSets |
| `Queries/CustomerStatsQuery.cs` | Fælles tilfældig kunde-statistik-query |
| `Tasks/CustomerStatsTask.cs` | Variant A — delt context |
| `Tasks/CustomerStatsFactoryTask.cs` | Variant B — factory per iteration |
| `Dtos/CustomerStatsDto.cs` | Resultat-type |
| `ParallelEntityFrameworkConsoleApp.csproj` | Pakker + kopier SQLite til output |

Kort opsummering af kørsel: se [README.md](README.md).

---

## For begyndere: `Parallel.For` og `execute`

Denne sektion forklarer den del af `Program.cs`, der ofte føles mest uklar.

### Hvad er `execute`?

`execute` er **ikke** en magisk variabel — det er bare **navnet på en parameter** i metoden `RunParallelDemoAsync`:

```csharp
static async Task<bool> RunParallelDemoAsync(
    string title,
    Func<Task<CustomerStatsDto>> execute)   // ← parameter
```

Typen `Func<Task<CustomerStatsDto>>` betyder: *"en funktion du kan kalde, som returnerer en `Task` med et `CustomerStatsDto`"*.

Når variant A kører, **sender** kaldet en konkret funktion ind som andet argument:

```csharp
return await RunParallelDemoAsync(
    "Variant A: Shared DbContext (singleton)",
    () => task.ExecuteAsync());   // ← dette BLIVER parameteren execute
```

På dansk: *"Når du skal køre testen, så kald `task.ExecuteAsync()`."* Den lille lambda `() => task.ExecuteAsync()` gemmes i `execute`, så `RunParallelDemoAsync` ikke behøver at vide om det er variant A eller B — den kalder bare `execute()`.

| Sted | Hvad sker der |
|------|----------------|
| `RunSharedContextVariantAsync` | Sender `() => task.ExecuteAsync()` ind |
| `RunParallelDemoAsync` | Modtager den som `execute` |
| Warmup | `await execute()` — ét kald på hovedtråden |
| `Parallel.For` | `execute()` kaldes 100 gange (fra flere tråde) |

### Hvad gør `Parallel.For`?

Forestil dig en **kø af 100 numre** (0, 1, 2, … 99). `Parallel.For` siger til .NET: *"Udfør dette stykke kode for hvert nummer — og du må bruge flere tråde samtidigt."*

```csharp
Parallel.For(
    0,              // start (inklusive)
    TaskCount,      // slut (eksklusiv) → 0..99 = 100 gange
    new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
    _ =>            // kroppen: kør denne kode for hvert nummer
    {
        // ...
    });
```

- **`_`** — det aktuelle nummer (0, 1, 2 …). Her bruges det ikke, derfor underscore.
- **`MaxDegreeOfParallelism`** — højst så mange tråde som processoren har kerner (ca. "4 ad gangen" på en 4-kerne CPU).
- Når alle 100 er færdige, **fortsætter** programmet efter `Parallel.For` (linje 102).

### Hvorfor `execute().GetAwaiter().GetResult()`?

`task.ExecuteAsync()` er **async** — den returnerer med det samme en `Task` (et løfte om data senere), ikke selve `CustomerStatsDto`.

I warmup bruges det pænt:

```csharp
var warmup = await execute();   // vent på Task, få DTO
```

Inde i `Parallel.For` er kroppen **ikke** `async`:

```csharp
_ =>   // denne metode må IKKE være async void/async lambda på den måde Parallel.For forventer
{
    var stats = execute().GetAwaiter().GetResult();
}
```

`Parallel.For` forventer en **synkron** action: "kør dette og vær færdig, før næste iteration på den tråd." Derfor:

1. **`execute()`** — start arbejdet (få en `Task<CustomerStatsDto>` tilbage med det samme).
2. **`.GetAwaiter().GetResult()`** — **bloker** den aktuelle tråd indtil databasen er færdig, og hent resultatet (eller kast fejl).

Det er den "gamle" måde at vente på en `Task` uden `await`. Det svarer nogenlunde til:

```csharp
var stats = await execute();
```

…men `await` kan man ikke bruge direkte i denne `Parallel.For`-krop på samme måde uden at skrive koden om (fx med `Parallel.ForEachAsync` i nyere .NET).

**Kort sagt:** `GetAwaiter().GetResult()` = *"Vent her til async-kaldet er færdigt og giv mig resultatet."*

### Hele flowet i ét diagram

```
RunSharedContextVariantAsync
    │
    ├─ opret task (CustomerStatsTask)
    │
    └─ RunParallelDemoAsync(titel, () => task.ExecuteAsync())
              │
              │   execute = den lambda ↑
              │
              ├─ Warmup:  await execute()     (1 tråd)
              │
              └─ Parallel.For 0..99
                    │
                    ├─ tråd 1: execute() → vent → results.Add
                    ├─ tråd 2: execute() → vent → results.Add
                    └─ ... (op til ~CPU-kerner ad gangen)
```

### Analogi

- **`execute`** = en **bestilling** du giver køkkenet: "Lav en kunde-statistik."
- **`ExecuteAsync()`** = køkkenet starter arbejdet (Task).
- **`await` / `GetResult()`** = du **venter ved disken** til tallerkenen er klar.
- **`Parallel.For`** = **100 kunder** bestiller på én gang; flere kokke (tråde) arbejder samtidigt — men i variant A deler de én gryde (DbContext), og det går galt.

### Hvad du kan huske

1. **`execute` kommer fra kaldet** — det er `() => task.ExecuteAsync()` sendt ind som parameter.
2. **`Parallel.For`** = kør den samme kodeblok mange gange, gerne på flere tråde.
3. **`GetAwaiter().GetResult()`** = vent på en `Task` inde i synkron kode; bruges her fordi `Parallel.For` ikke er `async`.
