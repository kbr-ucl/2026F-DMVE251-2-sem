namespace BlazorApp.Model;

public sealed record CartLine(string Product, decimal UnitPrice, int Quantity)
{
    public decimal LineTotal => UnitPrice * Quantity;
}