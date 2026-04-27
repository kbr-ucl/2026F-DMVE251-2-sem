using BlazorApp.Model;

namespace BlazorApp.Strategy;

public interface IPriceCalculator
{
    decimal Calculate(Cart cart);
}

public sealed class PriceCalculator : IPriceCalculator
{
    private readonly IReadOnlyList<IDiscountStrategy> _strategies;

    public PriceCalculator(IEnumerable<IDiscountStrategy> strategies)
    {
        ArgumentNullException.ThrowIfNull(strategies);

        _strategies = strategies.ToList();

        if (_strategies.Count == 0)
            throw new ArgumentException(
                "Der skal være mindst én strategi.", nameof(strategies));
    }

    /// <summary>
    /// Kører alle strategier og returnerer den bedste (laveste) pris.
    /// </summary>
    public decimal Calculate(Cart cart)
    {
        ArgumentNullException.ThrowIfNull(cart);

        var subtotal = cart.Subtotal();
        //decimal bestDiscount = 0;
        //foreach (var strategy in _strategies)
        //{
        //    var discount = strategy.Calculate(cart);
        //    if (discount > bestDiscount) bestDiscount = discount;
        //}
        var bestDiscounts = _strategies
            .Select(s => s.Calculate(cart)).ToList();

        var bestDiscount = bestDiscounts.Max();

        return subtotal - bestDiscount;
    }
}