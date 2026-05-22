namespace ParallelEntityFrameworkConsoleApp.Models;

public class Customer
{
    public string Id { get; set; } = null!;
    public string CompanyName { get; set; } = null!;

    public ICollection<Order> Orders { get; set; } = [];
}
