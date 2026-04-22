using BlazorApp.Model;

namespace BlazorApp.Strategy;

public sealed class PercentageDiscount : IDiscountStrategy
{
    private readonly decimal _percentage;

    public PercentageDiscount(decimal percentage)
    {
        if (percentage < 0 || percentage > 1)
            throw new ArgumentOutOfRangeException(nameof(percentage),
                "Procenten skal angives som decimal mellem 0 og 1, fx 0.10 for 10 %.");

        _percentage = percentage;
    }

    public decimal Calculate(Cart cart) => cart.Subtotal() * _percentage;
}