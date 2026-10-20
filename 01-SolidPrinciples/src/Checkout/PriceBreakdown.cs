namespace Checkout;

/// <summary>
/// The priced order. The totals are derived from the listed discounts, so they cannot disagree with them,
/// and the total discount is capped at the subtotal, so <see cref="Total"/> is never negative.
/// </summary>
public sealed record PriceBreakdown(decimal Subtotal, IReadOnlyList<AppliedDiscount> Discounts)
{
    public decimal TotalDiscount => Math.Min(Discounts.Sum(discount => discount.Amount), Subtotal);

    public decimal Total => Subtotal - TotalDiscount;
}
