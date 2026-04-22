using BlazorApp.Model;

namespace BlazorApp.Strategy;

public interface IDiscountStrategy
{
    /// <summary>
    /// Beregner den rabat, som strategien tildeler den givne kurv.
    /// Returværdien er selve rabat-beløbet (ikke den endelige pris).
    /// </summary>
    decimal Calculate(Cart cart);
}