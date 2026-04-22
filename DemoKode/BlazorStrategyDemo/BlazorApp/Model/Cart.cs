namespace BlazorApp.Model;

public sealed class Cart
{
    private readonly List<CartLine> _lines = new();

    public IReadOnlyList<CartLine> Lines => _lines;

    public void Add(CartLine line) => _lines.Add(line);

    public decimal Subtotal() => _lines.Sum(l => l.LineTotal);
}