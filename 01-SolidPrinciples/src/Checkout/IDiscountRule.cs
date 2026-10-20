namespace Checkout;

/// <summary>
/// One pricing policy that may grant a discount on an order.
/// </summary>
/// <remarks>
/// Every implementation must honour this contract, because <see cref="PriceCalculator"/> relies on it:
/// <list type="bullet">
/// <item>the amount is at least 0 and at most <see cref="Order.Subtotal"/>; 0 means the rule does not apply;</item>
/// <item>the amount is rounded with <see cref="Money.Round"/> (2 decimals, midpoints away from zero);</item>
/// <item>there are no side effects: the same order always gives the same amount.</item>
/// </list>
/// </remarks>
public interface IDiscountRule
{
    /// <summary>Display name shown to the customer in the price breakdown.</summary>
    string Name { get; }

    decimal CalculateDiscount(Order order);
}
