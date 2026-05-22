# Parallel Entity Framework demo

Console-demo der tester om **EF Core** kan køre **100 parallelle læsekald** mod `Northwind_large.sqlite` på **én delt `DbContext`-instans**, når alle queries bruger **`AsNoTracking()`**.

## Formål

Undersøge om `AsNoTracking` er nok til parallel adgang på samme context — eller om EF kaster concurrency-fejl.

## Kørsel

```powershell
cd DemoKode\ParallelEntityFramework\ParallelEntityFrameworkConsoleApp
dotnet run
```

## Struktur

- `Models/` — Customer, Order, OrderDetail
- `Data/NorthwindDbContext.cs` — mapping mod tabellerne `Customer`, `Order`, `OrderDetail`
- `Tasks/CustomerStatsTask.cs` — `NorthwindDbContext` via constructor injection
- `Program.cs` — singleton `DbContext`, warmup, `Parallel.For` × 100

## Observeret resultat (typisk)

- **Warmup** på main thread: OK
- **Parallel (100):** `InvalidOperationException` — *A second operation was started on this context instance before a previous operation completed.*

`AsNoTracking` fjerner ikke kravet om én operation ad gangen per `DbContext`.

## Database

`Northwind_large.sqlite` i console-projektroden kopieres til `bin/` ved build. Skemaet bruger `Customer.Id` (ikke klassisk `CustomerID` / `Customers`).
