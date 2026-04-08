# Dobbelt datahentning i Blazor *Interactive Server*



## Opgaver:

Kør programmet og hold øje med output konsol vinduet

Vælg "products" fra menuen

### Opgave 1 - Reflektionsopgave (individuel):

I konsol vinduet bliver ses det at der bliver hentet data fra databasen to gange. 

```cmd
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (2ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT "p"."Id", "p"."Category", "p"."Description", "p"."Name", "p"."Price"
      FROM "Products" AS "p"
info: 08-04-2026 18:00:53.064 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
      Executed DbCommand (2ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT "p"."Id", "p"."Category", "p"."Description", "p"."Name", "p"."Price"
      FROM "Products" AS "p"
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (0ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT "p"."Id", "p"."Category", "p"."Description", "p"."Name", "p"."Price"
      FROM "Products" AS "p"
info: 08-04-2026 18:00:53.298 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
      Executed DbCommand (0ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT "p"."Id", "p"."Category", "p"."Description", "p"."Name", "p"."Price"
      FROM "Products" AS "p"
```

**Hvorfor?**

- Forklar hvorfor 

  ```c#
  products = await db.Products.ToListAsync();
  ```

  kaldes flere gange.

- Hvorfor er det et problem?

- Hvilke løsningsmodeller kan du foreslå?



### Opgave 2  - Reflektionsopgave (gruppe):

Lav en fælles besvarelse på opgave 1 spørgsmål, og få valideret jeres svar ved underviseren.



### Opgave 3 - Realiser den bedste løsning og kontroller at den virker

```cmd
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (2ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT "p"."Id", "p"."Category", "p"."Description", "p"."Name", "p"."Price"
      FROM "Products" AS "p"
info: 08-04-2026 18:15:02.255 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
      Executed DbCommand (2ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT "p"."Id", "p"."Category", "p"."Description", "p"."Name", "p"."Price"
      FROM "Products" AS "p"
```

