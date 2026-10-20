namespace Checkout;

public sealed record Order(
    IReadOnlyList<OrderLine> Lines,
    CustomerTier Tier = CustomerTier.Standard,
    string? CouponCode = null)
{
    public decimal Subtotal => Lines.Sum(line => line.LineTotal);
}
