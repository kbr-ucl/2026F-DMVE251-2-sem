namespace ParallelEntityFrameworkConsoleApp.Models;

public class OrderDetail
{
    public string Id { get; set; } = null!;
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public double Discount { get; set; }

    public Order Order { get; set; } = null!;
}
