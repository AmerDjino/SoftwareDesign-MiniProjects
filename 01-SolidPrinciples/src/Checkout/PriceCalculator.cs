namespace Checkout;

/// <summary>
/// Prices an order: applies every rule in registration order, each to the original order (discounts add up,
/// they do not compound), and lists the rules that granted an amount greater than zero.
/// </summary>
public sealed class PriceCalculator(IEnumerable<IDiscountRule> rules)
{
    private readonly IDiscountRule[] _rules = [.. rules];

    public PriceBreakdown Calculate(Order order)
    {
        var discounts = new List<AppliedDiscount>();

        foreach (var rule in _rules)
        {
            var amount = rule.CalculateDiscount(order);
            if (amount > 0m)
            {
                discounts.Add(new AppliedDiscount(rule.Name, amount));
            }
        }

        return new PriceBreakdown(order.Subtotal, discounts);
    }
}
