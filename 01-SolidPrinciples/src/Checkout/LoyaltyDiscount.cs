namespace Checkout;

/// <summary>A percentage of the subtotal by customer tier; a tier without a rate gets no discount.</summary>
/// <param name="ratesByTier">The rate per tier as a fraction, for example 0.05 for 5%.</param>
public sealed class LoyaltyDiscount(IReadOnlyDictionary<CustomerTier, decimal> ratesByTier) : IDiscountRule
{
    private static readonly Dictionary<CustomerTier, decimal> DefaultRates = new()
    {
        [CustomerTier.Silver] = 0.03m,
        [CustomerTier.Gold] = 0.05m,
    };

    // A private copy: a caller that later changes its dictionary must not change this rule's results.
    private readonly Dictionary<CustomerTier, decimal> _ratesByTier = new(ratesByTier);

    /// <summary>Uses the default rates: Silver 3%, Gold 5%.</summary>
    public LoyaltyDiscount()
        : this(DefaultRates)
    {
    }

    public string Name => "Loyalty";

    public decimal CalculateDiscount(Order order) =>
        Money.Round(order.Subtotal * _ratesByTier.GetValueOrDefault(order.Tier));
}
