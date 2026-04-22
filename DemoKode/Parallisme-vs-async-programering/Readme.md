# Hvorfor er *BadCode* lige så hurtig som *NiceCode*?

Dette dokument forklarer, hvorfor to forskellige C#-metoder med `async/await`

kører **lige hurtigt**, selvom den ene ser “pænere” ud end den anden.

Eksemplet er velegnet som **eksamensfælde** og til at aflive en meget udbredt

misforståelse blandt studerende.

---

## Udgangspunkt

Vi har følgende to metoder:

### BadCode


```csharp
static async Task<int> BadCode()

{

    var totalTimer = Stopwatch.StartNew();

    var userData1 = await IoSimulator.SimulateDatabaseCallAsync(101);
    Console.WriteLine($"Færdig: {userData1}");

    var userData2 = await IoSimulator.SimulateDatabaseCallAsync(102);
    Console.WriteLine($"Færdig: {userData2}");

    var userData3 = await IoSimulator.SimulateDatabaseCallAsync(103);
    Console.WriteLine($"Færdig: {userData3}");

    var userData4 = await IoSimulator.SimulateDatabaseCallAsync(104);
    Console.WriteLine($"Færdig: {userData4}");

    totalTimer.Stop();

    return totalTimer.Elapsed.Seconds;
}
```


### NiceCode


```csharp
static async Task<int> NiceCode()

{

    var totalTimer = Stopwatch.StartNew();

    var userData1 = await IoSimulator.SimulateDatabaseCallAsync(101);
    var userData2 = await IoSimulator.SimulateDatabaseCallAsync(102);
    var userData3 = await IoSimulator.SimulateDatabaseCallAsync(103);
    var userData4 = await IoSimulator.SimulateDatabaseCallAsync(104);

    Console.WriteLine($"Færdig: {userData1}");
    Console.WriteLine($"Færdig: {userData2}");
    Console.WriteLine($"Færdig: {userData3}");
    Console.WriteLine($"Færdig: {userData4}");

    totalTimer.Stop();

    return totalTimer.Elapsed.Seconds;
}
```


💡 **Observation**
 Begge metoder tager ca. **samme tid** at køre.

------

## Hvorfor sker det?

### Den afgørende pointe

> **`async` betyder ikke parallel.**
>  **`await` betyder: “vent her, før du går videre”.**

Selvom koden er asynkron, bliver hvert databasekald udført **sekventielt**.

------

## Hvad gør `await` egentlig?

```c#
await Task.Delay(1200);
```
- Tråden bliver **ikke blokeret**
- Runtime må gerne bruge tråden til noget andet
- MEN: Metoden **venter stadig på resultatet**, før næste linje udføres

➡️ Du venter asynkront – men du venter stadig.

------

## Tidslinje for begge metoder

Hvert kald tager ca. 1,2 sekunder.

```
|--1.2s--|--1.2s--|--1.2s--|--1.2s--|
```

Samlet tid:

- **BadCode:** ~4,8 sek
- **NiceCode:** ~4,8 sek

➡️ Ingen forskel i performance.

------

## Den typiske studenter‑misforståelse

| Myte                                  | Virkelighed       |
| ------------------------------------- | ----------------- |
| `await` starter arbejdet i baggrunden | ❌ Nej             |
| async betyder multithreading          | ❌ Nej             |
| asynkron kode er altid hurtigere      | ❌ Nej             |
| Console.WriteLine gør det langsomt    | ❌ Ubetydeligt her |

------

## Hvornår bliver koden faktisk hurtigere?

Først når **alle tasks startes først**, og der ventes **samlet**.

### Korrekt parallel I/O med `Task.WhenAll`


```csharp
static async Task<int> FastCode()

{

  var sw = Stopwatch.StartNew();

  var tasks = new[]
  {
   IoSimulator.SimulateDatabaseCallAsync(101),
   IoSimulator.SimulateDatabaseCallAsync(102),
   IoSimulator.SimulateDatabaseCallAsync(103),
   IoSimulator.SimulateDatabaseCallAsync(104)
  };

  var results = await Task.WhenAll(tasks);

  foreach (var user in results)
    Console.WriteLine($"Færdig: {user}");

sw.Stop();

  return (int)sw.Elapsed.TotalSeconds;

}
```

### Ny tidslinje

```
|--------- 1.2s ---------|
```

✅ Alle kald overlapper
 ✅ Samlet tid ≈ **1.2 sekunder**

------

## Pædagogisk analogi

> Du bestiller fire pizzaer:
>
> - **Sekventielt:**
>    Du ringer, venter, spiser – ringer igen
> - **Parallelt:**
>    Du ringer til fire pizzeriaer på én gang og venter på alle

Asynkron kode hjælper med **ikke at blokere**,
 men kun **parallelt I/O** gør det hurtigere.

------

## Vigtig detalje (bonus)

Undgå:

```c#
totalTimer.Elapsed.Seconds;
```

Brug i stedet:

```c#
totalTimer.Elapsed.TotalSeconds;
```



Ellers mister du præcision.

------

## Kort konklusion (eksamensklar)

> *Async/await forbedrer skalerbarhed og responsivitet,
>  men giver kun performance‑gevinst, når I/O‑kald overlappes.*

