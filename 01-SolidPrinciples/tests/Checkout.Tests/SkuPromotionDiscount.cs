namespace Checkout.Tests;

/// <summary>
/// A pricing policy the Checkout library has never heard of: a percentage off every line of one product.
/// It lives in the test assembly to show that new rules plug in without editing the library.
/// </summary>
internal sealed class SkuPromotionDiscount(string sku, decimal rate) : IDiscountRule
{
    public string Name => $"{sku} promotion";

    public decimal CalculateDiscount(Order order)
    {
        var promotedTotal = order.Lines
            .Where(line => line.Sku == sku)
            .Sum(line => line.LineTotal);

        return Math.Round(promotedTotal * rate, 2, MidpointRounding.AwayFromZero);
    }
}
