using BlazorApp.Model;

namespace BlazorApp.Strategy;

public sealed class FixedAmountDiscount : IDiscountStrategy
{
    private readonly decimal _amount;
    private readonly decimal _threshold;

    public FixedAmountDiscount(decimal amount, decimal threshold)
    {
        _amount = amount;
        _threshold = threshold;
    }

    public decimal Calculate(Cart cart)
        => cart.Subtotal() >= _threshold ? _amount : 0m;
}