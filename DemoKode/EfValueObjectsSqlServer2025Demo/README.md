# EF Core 10 Value Objects on SQL Server 2025

This demo console app shows how Entity Framework Core 10 maps an entity that contains a value object (`TimeInterval`) to SQL Server 2025 using three different strategies:

1. `OwnsOne` (owned entity type)
2. `ComplexProperty` (complex type persisted as table-splitting columns)
3. `ComplexProperty` + `ToJson()` (complex type persisted as a JSON column)

Each example uses its own database to keep schemas isolated:

* `EfValueObjectsSqlServer2025Demo_OwnsOne`
* `EfValueObjectsSqlServer2025Demo_ComplexProperty`
* `EfValueObjectsSqlServer2025Demo_ComplexPropertyJson`

## Prerequisites

* .NET 10 SDK
* SQL Server 2025 available from the machine running the console app
* Permission to create/drop databases (the program runs `EnsureDeleted()` + `EnsureCreated()` for each example database)

## Connection string

By default the program uses Windows auth and connects to `localhost`:

`Server=localhost;Database=EfValueObjectsSqlServer2025Demo;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False`

You can override it with an environment variable:

* `SQLSERVER_CONNECTION_STRING`

## How to run

From `EfComplexPropertyDemo/`:

```bash
dotnet run --project EfValueObjectsSqlServer2025Demo.Console/EfValueObjectsSqlServer2025Demo.Console.csproj
```

## Examples (with pros/cons)

### 1) OwnsOne

`OwnsOne` maps the value object as an owned type.

In SQL Server this results in extra columns on the entity table (table splitting). In this demo those columns are explicitly named `TimeInterval_From` and `TimeInterval_To`.

**Pros**

* Relational shape is explicit and familiar (real columns).
* Easy to query specific sub-properties with normal SQL and LINQ.

**Cons**

* EF models the value object as an *owned entity type* (not just a “value-shaped property”). Even though the database ends up with split columns, EF still has ownership semantics in the model, which can increase mental and configuration overhead in more advanced mappings.
* From a DDD perspective, a value object typically has no identity and no independent lifecycle. `OwnsOne` can feel a bit “entity-ish” because EF treats the owned type as part of the entity’s ownership graph with richer modeling rules than a pure complex value shape.
* Schema is still “shape-coupled”: if you change the value object’s structure, you generally need schema changes (and therefore migrations) because the sub-properties are columns.

### 2) ComplexProperty

`ComplexProperty` is EF Core 10’s mapping for value-semantics complex types (ideal for DDD value objects).

EF persists the value object as table-splitting columns on the entity table (but without treating it as an owned type).

**Pros**

* Fits the DDD value-object mental model (value semantics).
* Designed for EF Core 10 value-object workflows (supports LINQ filtering through the complex properties).

**Cons**

* Still “column-based” in the non-JSON case: the value object shape ends up in separate columns in the table.
* If your main goal is to reduce schema churn when the value object evolves, you’ll usually prefer the JSON variant (`ToJson()`).

### OwnsOne vs ComplexProperty (in more detail)

Both approaches will usually end up with table-splitting columns in SQL Server, so basic LINQ querying works in both cases. The difference is in how EF Core models the concept internally:

* Identity and lifecycle (DDD lens): value objects are typically identity-less and lifecycle-less; `ComplexProperty` matches that more directly than `OwnsOne`.
* EF model semantics (technical lens): `OwnsOne` uses EF’s owned-entity-type metadata/semantics, while `ComplexProperty` treats the value object as part of the entity’s value shape (a complex type).
* Configuration / evolution impact: both are affected by value-object shape changes, but owned-type semantics can make advanced configurations feel more “framework heavy” than complex-type mappings.
* Query translation: since both map sub-properties to real columns (in these examples), EF can translate filters on sub-properties in LINQ more naturally than JSON-only storage.

### 3) ComplexProperty as JSON (`ToJson()`)

`ComplexProperty(..., t => t.ToJson())` tells EF Core to store the value object as a JSON column (using SQL Server’s JSON capabilities when supported by the provider).

**Pros**

* Very compact schema: one column instead of multiple columns.
* Easier evolution when the value object shape changes (fewer schema migrations).

**Cons**

* Querying inside JSON is usually less straightforward than querying relational columns.
* Indexing and constraints on individual sub-properties can be more limited/complex than with real columns.
* JSON serialization/deserialization adds overhead.

## What the program prints

The program:

* creates the schema for each strategy (`EnsureDeleted()` + `EnsureCreated()`)
* inserts exactly one entity row containing one `TimeInterval` value object
* reads the row back and prints `From -> To`

So you can confirm that EF can materialize the value object correctly in all three mappings.

