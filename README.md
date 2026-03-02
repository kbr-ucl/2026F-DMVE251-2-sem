# 2026F-DMVE251-2-sem

Demo kode, vejledende løsninger og eksempler til kurset DMVE251.

## Projektoversigt

Dette repository indeholder vejledende løsninger og demo-projekter, der demonstrerer vigtige softwarearkitektur- og designkoncepter i C#. Projekterne er designet til at lære og forstå:

- Domain-Driven Design (DDD)
- Clean Architecture
- Object-Oriented Programming principper
- Design patterns og best practices
- Arkitektur tests

### Teknologier

- **C#** - Programmeringssprog
- **.NET 10.0** - Framework version
- **Visual Studio / VS Code** - Udviklingsmiljø

### Krav

For at køre projekterne skal du have installeret:

- .NET 10.0 SDK eller nyere
- Visual Studio 2026, Visual Studio Code, eller JetBrains Rider

## Projektstruktur

```
2026F-DMVE251-2-sem/
├── DemoKode/                    # Alle demo-projekter
│   ├── 01-Lægehuset-dk-opgave-01/    # Konsultationsaftale system
│   ├── DetLilleBibliotek/             # Bibliotekssystem med Clean Architecture
│   ├── Bankkonto/                     # Bankkonto eksempler med arv
│   ├── 01-Refactoring-opgave-OrderProcessor/  # Order processing refactoring
│   └── TestAfPrivateSet/              # Arkitektur tests
├── SolutionTemplate.bat         # Script til at oprette nye Clean Architecture projekter
└── README.md                     # Denne fil
```

Hvert projekt indeholder sin egen solution-fil (`.slnx`), som kan åbnes i Visual Studio eller et kompatibelt IDE.

## SolutionTemplate.bat

**Beskrivelse:** Et Windows batch script, der automatisk opretter et nyt Clean Architecture projekt-skelet med korrekt struktur, projekt-referencer og NuGet-pakker.

**Hvad scriptet opretter:**

**Source projekter (5):**
- `Domain` - Domæneentiteter, value objects og exceptions (ingen afhængigheder)
- `Facade` - Interfaces, DTOs og queries (ingen afhængigheder)
- `UseCases` - Use case implementeringer (refererer Domain + Facade)
- `Infrastructure` - Persistence, repositories og eksterne services (refererer Domain + Facade + UseCases)
- `Api` - Web API med controllers (refererer Facade + Infrastructure + UseCases)

**Test projekter (2):**
- `Domain.Tests` - Unit tests for domain layer (refererer Domain)
- `UseCases.Tests` - Unit tests for use cases (refererer Domain + Facade + UseCases)

**Automatisk konfiguration:**
- ✅ Korrekte projekt-referencer efter Dependency Rule
- ✅ NuGet-pakker installeret:
  - **Infrastructure:** Entity Framework Core 10 (SqlServer + InMemory)
  - **Api:** Microsoft.AspNetCore.OpenApi + Scalar.AspNetCore
  - **Tests:** xunit.v3 + Moq
- ✅ Mappestruktur oprettet i hvert projekt
- ✅ Base classes: `Entity`, `AggregateRoot`
- ✅ Exception klasser: `DomainException`, `NotFoundException`

**Brug:**

1. Åbn PowerShell eller Command Prompt
2. Naviger til den mappe, hvor du vil oprette projektet
3. Kør scriptet:
   ```batch
   SolutionTemplate.bat
   ```
4. Indtast projektnavn når du bliver bedt om det (f.eks. `MinKlinik`)
5. Scriptet opretter projektet og bygger solution'en

**Eksempel:**
```batch
C:\Projects> SolutionTemplate.bat
Indtast projektnavn (f.eks. MinKlinik): MinKlinik

[1/7] Domain (classlib - ingen afhængigheder)
[2/7] Facade (classlib - ingen afhængigheder)
...
Build OK!
```

**Krav:**
- .NET 10.0 SDK skal være installeret
- Windows operativsystem (batch script)

**Næste skridt efter oprettelse:**
1. Åbn `.sln` filen i Visual Studio eller Rider
2. Implementer Value Objects i `Domain\ValueObjects\`
3. Implementer Entities som arver fra `AggregateRoot` i `Domain\Entities\`
4. Implementer Use Cases i `UseCases\`
5. Implementer Repositories i `Infrastructure\Repositories\`

## Demo-projekter

### 01-Lægehuset-dk-opgave-01

**Beskrivelse:** Et konsultationsaftale system, der demonstrerer hvordan man modellerer et domæne med læger, patienter og forskellige typer af konsultationer.

**Arkitektur:**
- `Domain/` - Domæneentiteter og forretningslogik
- `ConsoleUi/` - Konsol-baseret brugergrænseflade

**Vigtige koncepter:**
- **Domain entities:** `Læge`, `Patient`, `Konsultationsaftale`
- **Polymorfi:** Hierarki af `Konsultationstype` (AlmindeligKonsultation, Vaccination, Receptfornyelse, Rådgivning)
- **Validering:** Pre-condition checks (fx. at starttid ikke kan være i fortiden)
- **Encapsulation:** Private setters og init-only properties
- **Varighed:** Hver konsultationstype har sin egen varighed (10-20 minutter)
- **Mutability:** Konsultationstype kan udskiftes efter oprettelse via `UdskiftKonsultationsTypen()` metode

**Kørsel:**
```bash
cd DemoKode/01-Lægehuset-dk-opgave-01/ConsoleUi
dotnet run
```

### DetLilleBibliotek

**Beskrivelse:** Et bibliotekssystem, der demonstrerer Clean Architecture med klare lag og separation of concerns. Systemet håndterer bøger, medlemmer og udlån.

**Arkitektur:**
- `Domain/` - Domæneentiteter (`Bog`, `Medlem`) med forretningslogik
- `Application/` - Use case handlers og applikationslogik
- `Facade/` - Interfaces og DTOs til ekstern kommunikation

**Vigtige koncepter:**
- **Pre/post conditions:** Eksplicitte kontroller før og efter operationer
- **Guard clauses:** Defensive programming med tidlig returnering ved fejl
- **Encapsulation:** Private collections med read-only accessors
- **Use cases:** Adskilte use case handlers for hver operation
  - `IOpretBogUseCase` - Opret nye bøger i systemet
  - `IOpretMedlemUseCase` - Opret nye medlemmer
  - `IUdlånBogUseCase` - Håndter udlån af bøger til medlemmer
- **Repository pattern:** Abstraktioner for dataadgang (`IBogRepository`)
- **DTOs:** Command DTOs til use case inputs (`OpretBogCommandDto`, `UdlånBogCommmandDto`)

**Eksempel på pre/post conditions:**
```csharp
// Pre-condition: Tjek om bog allerede er udlånt
if (ErUdlånt)
    throw new InvalidOperationException("Fejl: Bogen er allerede udlånt.");

// Handling
ErUdlånt = true;

// Post-condition
Debug.Assert(ErUdlånt, "ErUdlånt blev ikke sat til sandt");
```

**Kørsel:**
```bash
cd DemoKode/DetLilleBibliotek
dotnet build
```

### Bankkonto

**Beskrivelse:** Eksempler på bankkonti med arv, der demonstrerer polymorfi og virtual/override metoder. Dette er standalone kodeeksempler, ikke et komplet .NET projekt.

**Filer:**
- `Bankkonto.cs` - Grundlæggende Account hierarki med SavingsAccount og CheckingAccount
- `KundeBankkonto.cs` - Udvidet eksempel med Customer klasse og account type skift

**Vigtige koncepter:**
- **Arv:** `SavingsAccount` og `CheckingAccount` arver fra `Account`
- **Polymorfi:** Override af `Withdraw()` metoden med forskellige regler
- **Virtual/override:** Brug af `virtual` og `override` keywords
- **Abstrakte regler:** Forskellige minimumsbalancer for forskellige kontotyper
- **Composition:** Customer klasse har en Account (composition over inheritance eksempel)
- **Account type skift:** Demonstrerer hvordan en Customer kan skifte kontotype

**Eksempel:**
- `SavingsAccount`: Minimum balance = 0
- `CheckingAccount`: Minimum balance = -1000 (overtræk)
- `Account` konstruktør understøtter nu `openingBalance` parameter

**Bemærk:** Dette er standalone C# filer og ikke et .NET projekt. Koden kan kopieres ind i et console projekt eller køres via C# scripting.

### 01-Refactoring-opgave-OrderProcessor

**Beskrivelse:** Et order processing system designet som en refactoring øvelse. Demonstrerer hvordan man adskiller bekymringer og bruger dependency injection.

**Vigtige koncepter:**
- **Dependency Injection:** Constructor injection af interfaces
- **Interfaces:** `IOrderRepository`, `IPayment`, `IMail`
- **Separation of concerns:** Adskillelse af persistence, betaling og email
- **Testability:** Interfaces gør systemet testbart

**Arkitektur:**
- `OrderProcessor.Core/` - Core business logic med interfaces og implementeringer

**Kørsel:**
```bash
cd DemoKode/01-Refactoring-opgave-OrderProcessor/OrderProcessor.Core
dotnet build
```

### TestAfPrivateSet

**Beskrivelse:** Arkitektur tests, der sikrer at domain entities har private setters på deres properties. Demonstrerer hvordan man kan teste arkitekturregler automatisk.

**Vigtige koncepter:**
- **Architecture testing:** Automatisk validering af design constraints
- **NetArchTest:** Bibliotek til at teste arkitekturregler
- **Reflection:** Brug af reflection til at inspicere typer
- **Design constraints:** Sikring af encapsulation gennem tests

**Test eksempel:**
Testen tjekker at alle entities i `CodeToTest.Entity` namespace har private setters på deres public properties.

**Kørsel:**
```bash
cd DemoKode/TestAfPrivateSet/TestProject
dotnet test
```

## Brugsanvisninger

### Åbne et projekt

1. **Visual Studio:**
   - Åbn `.slnx` filen i Visual Studio
   - Restore NuGet packages (sker automatisk)
   - Build projektet (Ctrl+Shift+B)

2. **Visual Studio Code:**
   ```bash
   code DemoKode/[projekt-navn]/[projekt-navn].slnx
   ```

3. **Command Line:**
   ```bash
   cd DemoKode/[projekt-navn]
   dotnet restore
   dotnet build
   dotnet run  # Hvis projektet er et console application
   ```

### Bygge alle projekter

**Windows (PowerShell):**
```powershell
# Fra root mappen
Get-ChildItem -Path DemoKode -Recurse -Filter "*.slnx" | ForEach-Object { dotnet build $_.FullName }
```

**Linux/Mac:**
```bash
# Fra root mappen
find DemoKode -name "*.slnx" -exec dotnet build {} \;
```

**Alternativ (alle platforme):**
```bash
# Byg hvert projekt manuelt
dotnet build DemoKode/01-Lægehuset-dk-opgave-01/Lægehuset.slnx
dotnet build DemoKode/DetLilleBibliotek/DetLilleBibliotek.slnx
dotnet build DemoKode/01-Refactoring-opgave-OrderProcessor/OrderProcessor.slnx
dotnet build DemoKode/TestAfPrivateSet/TestAfPrivateSet.slnx
```

### Køre tests

For projekter med tests (fx. TestAfPrivateSet):
```bash
cd DemoKode/TestAfPrivateSet/TestProject
dotnet test
```

## Læringsmål

Disse demo-projekter dækker følgende vigtige koncepter:

- ✅ Domain-Driven Design principper
- ✅ Clean Architecture og lagdeling
- ✅ Object-Oriented Programming (arv, polymorfi, encapsulation)
- ✅ Design patterns (Repository, Use Case)
- ✅ Defensive programming (pre/post conditions, guard clauses)
- ✅ Dependency Injection og testability
- ✅ Architecture testing
- ✅ Best practices for C# og .NET

## Noter

- Alle projekter bruger .NET 10.0
- Projekterne er designet som læringseksempler og kan indeholde forenklede implementeringer
- Nogle projekter kan kræve yderligere konfiguration (fx. database connection strings) for at køre fuldt ud
- `Bankkonto` mappen indeholder standalone kodeeksempler, ikke et komplet .NET projekt
- Solution-filerne (`.slnx`) er Visual Studio 2022+ format og kan åbnes i Visual Studio eller kompatible IDEs