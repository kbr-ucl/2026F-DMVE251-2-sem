namespace ParallelEntityFrameworkConsoleApp.Models;

public class Order
{
    public int Id { get; set; }
    public string CustomerId { get; set; } = null!;

    public Customer Customer { get; set; } = null!;
    public ICollection<OrderDetail> OrderDetails { get; set; } = [];
}
