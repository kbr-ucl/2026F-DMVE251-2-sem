using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, World!");

var services = new ServiceCollection()
    .AddTransient<IGreetingService, GreetingService>()
    .AddTransient<MyFancyGreetingService>()
    .AddTransient<MyApplication>()
    .BuildServiceProvider();

var greetingService = services.GetRequiredService<MyApplication>();
greetingService.Run();

public interface IGreetingService
{
    void Greet(string name);
}

public class GreetingService : IGreetingService
{
    void IGreetingService.Greet(string name)
    {
        Console.WriteLine($"Hej, {name}");
    }
}

public class MyApplication
{
    private readonly IGreetingService _greetingService;

    public MyApplication(IGreetingService greetingService)
    {
        _greetingService = greetingService;
    }

    public void Run()
    {
        _greetingService.Greet("Datamatiker");
    }

    public string Run2()
    {
        return "Hello from IOC";
    }
}

public class MyFancyGreetingService(IGreetingService greetingService)
{
    public void Run()
    {
        greetingService.Greet("Datamatiker");
    }
}