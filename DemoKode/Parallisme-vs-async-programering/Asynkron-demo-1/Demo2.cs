using System.Diagnostics;

namespace Asynkron_demo_1;

internal class Demo2
{
    public static async Task Demo()
    {
        Console.WriteLine("--- I/O Simulator Startet ---");
        Console.WriteLine();

        var badTime = await BadCode();
        Console.WriteLine($"\n--- Simulator Færdig (Total tid: {badTime}s) ---");
        var niceButBadTime = await NiceButBadCode();
        Console.WriteLine($"\n--- Simulator Færdig (Total tid: {niceButBadTime}s) ---");
        var fastTime = await NiceAndFastCode();
        Console.WriteLine($"\n--- Simulator Færdig (Total tid: {fastTime}s) ---");


    }

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

    static async Task<int> NiceButBadCode()
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

    static async Task<int> NiceAndFastCode()
    {
        var sw = Stopwatch.StartNew();

        var t1 = IoSimulator.SimulateDatabaseCallAsync(101);
        var t2 = IoSimulator.SimulateDatabaseCallAsync(102);
        var t3 = IoSimulator.SimulateDatabaseCallAsync(103);
        var t4 = IoSimulator.SimulateDatabaseCallAsync(104);


        var results = await Task.WhenAll(t1, t2, t3, t4);

        foreach (var user in results)
            Console.WriteLine($"Færdig: {user}");

        sw.Stop();
        return sw.Elapsed.Seconds;
    }
}