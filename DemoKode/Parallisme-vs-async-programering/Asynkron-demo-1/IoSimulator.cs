namespace Asynkron_demo_1;

public class IoSimulator
{
    public static async Task<string> SimulateDatabaseCallAsync(int id)
    {
        // Task.Delay blokerer ikke tråden
        await Task.Delay(1200);
        return $"Bruger #{id} (Navn: Anders And)";
    }

    public static async Task SimulateFileWriteAsync(string fileName)
    {
        var rng = new Random();
        var latency = rng.Next(500, 2500); // Tilfældig tid mellem 0.5 og 2.5 sekunder
        await Task.Delay(latency);
        Console.WriteLine($"Filen '{fileName}' blev gemt (Latency: {latency}ms)");
    }

    public static async Task SimulateApiCallAsync(string serviceName)
    {
        await Task.Delay(1500); // Fast ventetid
        Console.WriteLine($"- Svar modtaget fra {serviceName}");
    }
}