using System.Diagnostics;

Console.WriteLine("Hello, World!");


Console.WriteLine($"Main-tråd: {Environment.CurrentManagedThreadId}");

var sw = Stopwatch.StartNew();

// Sekventielle version
var r1 = TælPrimtal(3_000_000);
var r2 = TælPrimtal(3_000_000);
var r3 = TælPrimtal(3_000_000);
Console.WriteLine($"Total tid: {sw.ElapsedMilliseconds} ms");
Console.WriteLine($"Resultater: {string.Join(", ", new[] {r1, r2, r3})}");

sw = Stopwatch.StartNew();

// Kør tre tunge beregninger parallelt
var t1 = Task.Run(() => TælPrimtal(3_000_000));
var t2 = Task.Run(() => TælPrimtal(3_000_000));
var t3 = Task.Run(() => TælPrimtal(3_000_000));

var resultater = await Task.WhenAll(t1, t2, t3);

Console.WriteLine($"Total tid: {sw.ElapsedMilliseconds} ms");
Console.WriteLine($"Resultater: {string.Join(", ", resultater)}");

int TælPrimtal(int øvre)
{
    Console.WriteLine($"  Starter beregning på tråd {Environment.CurrentManagedThreadId}");
    int antal = 0;
    for (int i = 2; i <= øvre; i++)
    {
        if (ErPrimtal(i)) antal++;
    }
    return antal;
}

bool ErPrimtal(int n)
{
    if (n < 2) return false;
    for (int i = 2; i * i <= n; i++)
        if (n % i == 0) return false;
    return true;
}