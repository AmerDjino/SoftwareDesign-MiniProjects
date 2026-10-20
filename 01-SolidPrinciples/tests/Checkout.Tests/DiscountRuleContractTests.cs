namespace Checkout.Tests;

public sealed class DiscountRuleContractTests
{
    private static readonly InMemoryCouponCatalog Coupons = new(
    [
        new Coupon("SAVE5", 5.00m, 50.00m),
        new Coupon("TENOFF", 10.00m, 0.00m),
    ]);

    private static readonly Dictionary<string, IDiscountRule> BuiltInRules = new()
    {
        [nameof(BulkPurchaseDiscount)] = new BulkPurchaseDiscount(),
        [nameof(LoyaltyDiscount)] = new LoyaltyDiscount(),
        [nameof(CouponDiscount)] = new CouponDiscount(Coupons),
    };

    private static readonly Dictionary<string, Order> EdgeCaseOrders = new()
    {
        ["empty order"] = new Order([], CustomerTier.Gold, "TENOFF"),
        ["below coupon minimum"] = new Order([new OrderLine("PEN", 1.99m, 3)], CustomerTier.Silver, "SAVE5"),
        ["coupon larger than subtotal"] = new Order([new OrderLine("PEN", 1.99m, 2)], CustomerTier.Standard, "TENOFF"),
        ["bulk quantity"] = new Order([new OrderLine("CLIP", 0.15m, 25)]),
        ["Gold tier"] = new Order([new OrderLine("LAMP", 33.33m, 1)], CustomerTier.Gold),
    };

    public static TheoryData<string, string> EveryRuleWithEveryOrder()
    {
        var cases = new TheoryData<string, string>();
        foreach (var ruleName in BuiltInRules.Keys)
        {
            foreach (var orderName in EdgeCaseOrders.Keys)
            {
                cases.Add(ruleName, orderName);
            }
        }

        return cases;
    }

    [Theory]
    [MemberData(nameof(EveryRuleWithEveryOrder))]
    public void DiscountRules_AnyOrder_HonourTheDiscountContract(string ruleName, string orderName)
    {
        var rule = BuiltInRules[ruleName];
        var order = EdgeCaseOrders[orderName];

        var amount = rule.CalculateDiscount(order);
        var repeatedAmount = rule.CalculateDiscount(order);

        var roundedToCents = decimal.Round(amount, 2);
        Assert.InRange(amount, 0.00m, order.Subtotal);
        Assert.Equal(roundedToCents, amount);
        Assert.Equal(amount, repeatedAmount);
    }
}
