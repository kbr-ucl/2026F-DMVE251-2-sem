# Parallel Entity Framework demo

Console-demo der sammenligner **100 parallelle læsekald** mod `Northwind_large.sqlite` med to strategier — alle queries bruger **`AsNoTracking()`**.

## Formål

| Variant | Strategi | Forventet |
|---------|----------|-----------|
| **A** | Én delt `DbContext` (singleton) | Mange fejl — `InvalidOperationException` (context er ikke thread-safe) |
| **B** | `IDbContextFactory` — ny context per iteration | **100/100 OK** (EF-delen løst) |

`AsNoTracking` alene løser ikke parallel adgang på samme context. Factory giver hver tråd sin egen context og `await using` dispose efter hvert kald.

Under ekstrem parallelitet kan **SQLite** stadig give `database is locked` — det er en database-begrænsning, ikke EF tracking.

## Kørsel

```powershell
cd DemoKode\ParallelEntityFramework\ParallelEntityFrameworkConsoleApp
dotnet run
```

## Struktur

- `Models/` — Customer, Order, OrderDetail
- `Data/NorthwindDbContext.cs` — EF mapping
- `Queries/CustomerStatsQuery.cs` — fælles LINQ (AsNoTracking)
- `Tasks/CustomerStatsTask.cs` — variant A (delt context)
- `Tasks/CustomerStatsFactoryTask.cs` — variant B (factory)
- `Program.cs` — kører begge varianter i træk

## Database

`Northwind_large.sqlite` i console-projektroden kopieres til `bin/` ved build. Skema: tabeller `Customer`, `Order`, `OrderDetail` med `Customer.Id`.
