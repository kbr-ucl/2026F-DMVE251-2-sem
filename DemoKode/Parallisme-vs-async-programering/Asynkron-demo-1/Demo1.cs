using System.Diagnostics;

namespace Asynkron_demo_1;

internal class Demo1
{
    public static async Task Demo()
    {
        var sw = Stopwatch.StartNew();
        Console.WriteLine($"Start  - Tråd {Environment.CurrentManagedThreadId}");
        await HentDataAsync(1);
        await HentDataAsync(2);
        await HentDataAsync(3);
        Console.WriteLine($"Total tid: {sw.ElapsedMilliseconds} ms");

        sw = Stopwatch.StartNew();
        Console.WriteLine($"Start  - Tråd {Environment.CurrentManagedThreadId}");
        var t4 = HentDataAsync(1);
        var t5 = HentDataAsync(2);
        var t6 = HentDataAsync(3);

        await Task.WhenAll(t4, t5, t6);

        Console.WriteLine($"Total tid: {sw.ElapsedMilliseconds} ms");



    }

    static async Task HentDataAsync(int id)
    {
        Console.WriteLine($"Start  {id} - Tråd {Environment.CurrentManagedThreadId}");
        await Task.Delay(
            2000); // siger til .NET: "Giv mig besked om 2 sekunder" — i mellemtiden kan tråden gå videre og lave andet.
        //Thread.Sleep(2000); // Thread.Sleep blokerer tråden.
        Console.WriteLine($"Færdig {id} - Tråd {Environment.CurrentManagedThreadId}");
    }
}