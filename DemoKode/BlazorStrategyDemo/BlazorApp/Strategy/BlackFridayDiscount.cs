using BlazorApp.Model;

namespace BlazorApp.Strategy
{
    public sealed class BlackFridayDiscount : IDiscountStrategy
    {
        private readonly decimal _percentage;

        public BlackFridayDiscount(decimal percentage = 0.25m)
        {
            _percentage = percentage;
        }

        public decimal Calculate(Cart cart) => cart.Subtotal() * _percentage;
    }


    public sealed class KajSpecialDiscount : IDiscountStrategy
    {
        private readonly decimal _percentage;

        public KajSpecialDiscount(decimal percentage = 0.75m)
        {
            _percentage = percentage;
        }

        public decimal Calculate(Cart cart) => cart.Subtotal() * _percentage;
    }
}
