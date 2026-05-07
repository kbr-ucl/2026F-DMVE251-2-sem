# Mock-demo med Moq

Dette projekt demonstrerer, hvordan man kan teste en klasse uafhængigt af dens afhængigheder ved hjælp af et mock-objekt.

Eksemplet bruger:

- `BusinessLogic`: kode der skal testes.
- `BusinessLogic.Test`: unit tests skrevet med xUnit og Moq.
- `Moq`: et bibliotek til at oprette mock-objekter i C#.

## Hvad er et mock?

Et mock er et objekt, som opfører sig som en rigtig afhængighed, men hvor vi selv bestemmer, hvad det skal returnere.

Det bruges ofte i unit tests, når vi vil teste én klasse isoleret. I stedet for at bruge en rigtig service, database, fil, API eller tilfældig beregning, laver vi et mock-objekt og styrer dets svar.

Fordelen er, at testen bliver:

- mere forudsigelig
- hurtigere
- uafhængig af andre klasser
- nemmere at fejlsøge

## Demoens problem

`BeregnPrisService` beregner en pris efter rabat:

```csharp
var rabatProcent = _beregnRabatService.BeregnRabatProcent(pris);
var prisMedRabat = pris * (1 - rabatProcent / 100);
```

Servicen afhænger af `IBeregnRabatService`, som beregner rabatprocenten.

Den konkrete `BeregnRabatService` returnerer en tilfældig rabat mellem 0 og 80:

```csharp
return Random.Shared.Next(0, 81);
```

Det er et problem i en unit test, fordi testen ikke ved, hvilken rabat der kommer tilbage. Hvis rabatten er tilfældig, bliver resultatet også tilfældigt.

## Hvorfor bruger vi et interface?

`BeregnPrisService` modtager sin afhængighed gennem constructoren:

```csharp
public BeregnPrisService(IBeregnRabatService beregnRabatService)
{
    _beregnRabatService = beregnRabatService;
}
```

Det betyder, at testen kan give servicen en mock-version af `IBeregnRabatService` i stedet for den rigtige `BeregnRabatService`.

Det kaldes dependency injection.

## Testen med Moq

Testen ligger i `BusinessLogic.Test/BeregnPrisServiceTest.cs`.

I testen opretter vi et mock:

```csharp
var beregnRabatServiceMock = new Mock<IBeregnRabatService>();
```

Derefter bestemmer vi, hvad mocken skal returnere:

```csharp
beregnRabatServiceMock
    .Setup(service => service.BeregnRabatProcent(pris))
    .Returns(rabatProcent);
```

Hvis `BeregnPrisService` spørger efter rabat for prisen `200`, returnerer mocken `25`.

Det gør testen deterministisk:

- pris: `200`
- rabat: `25%`
- forventet pris efter rabat: `150`

## Arrange, Act, Assert

Testen er struktureret efter Arrange, Act og Assert:

```csharp
// Arrange
var pris = 200;
var rabatProcent = 25;
var expected = 150;

// Act
var actual = service.BeregnPrisMedRabat(pris, 0);

// Assert
Assert.Equal(expected, actual);
```

### Arrange

Her forbereder vi testen:

- inputdata
- forventet resultat
- mock-objekt
- den klasse der skal testes

### Act

Her kalder vi den metode, vi vil teste.

### Assert

Her kontrollerer vi, at resultatet er korrekt.

Testen verificerer også, at `BeregnPrisService` faktisk kalder rabatservicen:

```csharp
beregnRabatServiceMock.Verify(
    service => service.BeregnRabatProcent(pris),
    Times.Once);
```

Det betyder: metoden `BeregnRabatProcent` skal være kaldt præcis én gang.

## Hvad viser demoen?

Demoen viser, at `BeregnPrisService` kan testes uden at bruge den rigtige `BeregnRabatService`.

Det er vigtigt, fordi den rigtige rabatservice returnerer tilfældige værdier. Ved at mocke rabatservicen kan testen fokusere på det, den faktisk skal teste:

> Beregner `BeregnPrisService` prisen korrekt, når den får en bestemt rabatprocent?

## Kør testene

Kør alle tests fra projektets rodmappe:

```powershell
dotnet test
```

Forventet resultat er, at testen passerer.

## Visual Studio Test Explorer

Testprojektet indeholder også pakkerne `Microsoft.NET.Test.Sdk` og `xunit.runner.visualstudio`.

De gør det muligt for Visual Studio Test Explorer at finde og køre xUnit-testene direkte fra Visual Studio.
