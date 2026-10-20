namespace Checkout;

public sealed class BulkPurchaseDiscount : IDiscountRule
{
    private const int MinimumQuantity = 10;
    private const decimal Rate = 0.10m;

    public string Name => "Bulk purchase";

    public decimal CalculateDiscount(Order order)
    {
        var bulkLinesTotal = order.Lines
            .Where(line => line.Quantity >= MinimumQuantity)
            .Sum(line => line.LineTotal);

        return Money.Round(bulkLinesTotal * Rate);
    }
}
