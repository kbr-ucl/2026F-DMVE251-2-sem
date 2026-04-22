using BlazorApp.Model;

namespace BlazorApp.Strategy;

public sealed class NoDiscount : IDiscountStrategy
{
    public decimal Calculate(Cart cart) => 0m;
}