namespace Checkout.Tests;

public sealed class PriceCalculatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("WELCOME10")]
    [InlineData("SAVE5")]
    public void Calculate_NoApplicableRules_TotalEqualsSubtotal(string? couponCode)
    {
        var order = new Order(
            [new OrderLine("NOTEBOOK", 1.50m, 9), new OrderLine("LAMP", 25.00m, 1)],
            CustomerTier.Standard,
            couponCode);

        var breakdown = CreateCalculatorWithBuiltInRules().Calculate(order);

        Assert.Empty(breakdown.Discounts);
        Assert.Equal(0.00m, breakdown.TotalDiscount);
        Assert.Equal(38.50m, breakdown.Subtotal);
        Assert.Equal(breakdown.Subtotal, breakdown.Total);
    }

    [Fact]
    public void Calculate_SeveralApplicableRules_StacksDiscountsAndListsEachOne()
    {
        var order = new Order(
            [new OrderLine("PEN", 2.00m, 10), new OrderLine("LAMP", 80.00m, 1)],
            CustomerTier.Gold,
            "save5");

        var breakdown = CreateCalculatorWithBuiltInRules().Calculate(order);

        AppliedDiscount[] expected =
        [
            new("Loyalty", 5.00m),
            new("Bulk purchase", 2.00m),
            new("Coupon", 5.00m),
        ];
        Assert.Equal(expected, breakdown.Discounts);
        Assert.Equal(100.00m, breakdown.Subtotal);
        Assert.Equal(12.00m, breakdown.TotalDiscount);
        Assert.Equal(88.00m, breakdown.Total);
    }

    [Fact]
    public void Calculate_CustomRule_IsAppliedWithoutChangingTheCalculator()
    {
        var mugPromotion = new SkuPromotionDiscount("MUG", 0.20m);
        var calculator = new PriceCalculator([mugPromotion]);
        var order = new Order([new OrderLine("MUG", 12.50m, 2), new OrderLine("TEAPOT", 30.00m, 1)]);

        var breakdown = calculator.Calculate(order);

        var applied = Assert.Single(breakdown.Discounts);
        Assert.Equal(new AppliedDiscount(mugPromotion.Name, 5.00m), applied);
        Assert.Equal(50.00m, breakdown.Total);
    }

    [Fact]
    public void Calculate_DiscountsExceedSubtotal_TotalDiscountIsCappedAndTotalIsZero()
    {
        // Each rule stays within the contract (at most the subtotal); only their sum exceeds it.
        var calculator = new PriceCalculator(
            [new FixedDiscountStub("Voucher", 30.00m), new FixedDiscountStub("Gift card", 25.00m)]);
        var order = new Order([new OrderLine("BOOK", 40.00m, 1)]);

        var breakdown = calculator.Calculate(order);

        AppliedDiscount[] unchanged = [new("Voucher", 30.00m), new("Gift card", 25.00m)];
        Assert.Equal(unchanged, breakdown.Discounts);
        Assert.Equal(40.00m, breakdown.TotalDiscount);
        Assert.Equal(0.00m, breakdown.Total);
    }

    private static PriceCalculator CreateCalculatorWithBuiltInRules()
    {
        var coupons = new InMemoryCouponCatalog([new Coupon("SAVE5", 5.00m, 50.00m)]);
        return new PriceCalculator([new LoyaltyDiscount(), new BulkPurchaseDiscount(), new CouponDiscount(coupons)]);
    }
}
