using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ParallelEntityFrameworkConsoleApp.Data;
using ParallelEntityFrameworkConsoleApp.Dtos;
using ParallelEntityFrameworkConsoleApp.Tasks;

const int TaskCount = 100;

var dbPath = Path.Combine(AppContext.BaseDirectory, "Northwind_large.sqlite");
if (!File.Exists(dbPath))
{
    Console.Error.WriteLine($"Database not found: {dbPath}");
    return 1;
}

var connectionString = $"Data Source={dbPath}";

Console.WriteLine($"Database: {dbPath}");
Console.WriteLine($"Tasks per variant: {TaskCount}, MaxDegreeOfParallelism: {Environment.ProcessorCount}");
Console.WriteLine();

var variantASuccess = await RunSharedContextVariantAsync(connectionString);
Console.WriteLine();
var variantBSuccess = await RunFactoryVariantAsync(connectionString);

return variantBSuccess ? 0 : 1;

static async Task<bool> RunSharedContextVariantAsync(string connectionString)
{
    var services = new ServiceCollection();
    services.AddDbContext<NorthwindDbContext>(
        options => options.UseSqlite(connectionString),
        ServiceLifetime.Singleton);
    services.AddSingleton<CustomerStatsTask>();

    await using var provider = services.BuildServiceProvider();
    var task = provider.GetRequiredService<CustomerStatsTask>();

    return await RunParallelDemoAsync(
        "Variant A: Shared DbContext (singleton)",
        () => task.ExecuteAsync());
}

static async Task<bool> RunFactoryVariantAsync(string connectionString)
{
    var services = new ServiceCollection();
    services.AddDbContextFactory<NorthwindDbContext>(
        options => options.UseSqlite(connectionString));
    services.AddSingleton<CustomerStatsFactoryTask>();

    await using var provider = services.BuildServiceProvider();
    var task = provider.GetRequiredService<CustomerStatsFactoryTask>();

    return await RunParallelDemoAsync(
        "Variant B: DbContext Factory (one context per iteration)",
        () => task.ExecuteAsync());
}

static async Task<bool> RunParallelDemoAsync(
    string title,
    Func<Task<CustomerStatsDto>> execute)
{
    Console.WriteLine($"=== {title} ===");
    Console.WriteLine("All queries use AsNoTracking.");
    Console.WriteLine();

    Console.WriteLine("Warmup (single call on main thread)...");
    try
    {
        var warmup = await execute();
        Console.WriteLine($"  OK: {warmup.CustomerId} / {warmup.CompanyName} / orders={warmup.OrderCount} / sum={warmup.TotalOrderSum:N2}");
    }
    catch (Exception ex)
    {
        PrintException("Warmup failed", ex);
        return false;
    }
    Console.WriteLine();

    var results = new ConcurrentBag<CustomerStatsDto>();
    var errors = new ConcurrentBag<Exception>();

    var parallelTimer = Stopwatch.StartNew();
    Parallel.For(
        0,
        TaskCount,
        new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
        _ =>
        {
            try
            {
                var stats = execute().GetAwaiter().GetResult();
                results.Add(stats);
            }
            catch (Exception ex)
            {
                errors.Add(ex);
            }
        });
    parallelTimer.Stop();

    Console.WriteLine($"Parallel finished in {parallelTimer.ElapsedMilliseconds} ms");
    Console.WriteLine($"  Success: {results.Count}/{TaskCount}");
    Console.WriteLine($"  Failed:  {errors.Count}/{TaskCount}");
    Console.WriteLine();

    if (results.Count > 0)
    {
        Console.WriteLine("Sample results (first 5):");
        foreach (var r in results.Take(5))
            Console.WriteLine($"  {r.CustomerId,-6} {r.CompanyName,-30} orders={r.OrderCount,4} sum={r.TotalOrderSum,12:N2}");
        Console.WriteLine();
    }

    if (errors.Count > 0)
    {
        Console.WriteLine("Errors by type:");
        foreach (var group in errors.GroupBy(e => e.GetType().Name).OrderByDescending(g => g.Count()))
        {
            var sample = group.First();
            Console.WriteLine($"  {group.Key}: {group.Count()}");
            Console.WriteLine($"    {sample.Message}");
            var inner = sample.InnerException;
            while (inner != null)
            {
                Console.WriteLine($"    -> {inner.GetType().Name}: {inner.Message}");
                inner = inner.InnerException;
            }
        }
        Console.WriteLine();
    }

    return errors.Count == 0;
}

static void PrintException(string heading, Exception ex)
{
    Console.WriteLine(heading);
    for (var current = ex; current != null; current = current.InnerException)
        Console.WriteLine($"  {current.GetType().Name}: {current.Message}");
}
