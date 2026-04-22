using System.Diagnostics;
using Asynkron_demo_1;

Console.WriteLine("--- I/O Simulator Startet ---");
Console.WriteLine();

var badTime = await BadCode();
Console.WriteLine($"\n--- Simulator Færdig (Total tid: {badTime}s) ---");
var niceTime = await NiceCode();
Console.WriteLine($"\n--- Simulator Færdig (Total tid: {niceTime}s) ---");
var fastTime = await FastCode();
Console.WriteLine($"\n--- Simulator Færdig (Total tid: {fastTime}s) ---");

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
    Console.WriteLine();
    totalTimer.Stop();

    return totalTimer.Elapsed.Seconds;
}

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
    return sw.Elapsed.Seconds;
}
