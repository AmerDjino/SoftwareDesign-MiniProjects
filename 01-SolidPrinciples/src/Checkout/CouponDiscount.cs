namespace Checkout;

public sealed class CouponDiscount(ICouponLookup coupons) : IDiscountRule
{
    public string Name => "Coupon";

    public decimal CalculateDiscount(Order order)
    {
        if (order.CouponCode is null)
        {
            return 0m;
        }

        var coupon = coupons.Find(order.CouponCode);
        if (coupon is null || order.Subtotal < coupon.MinimumSubtotal)
        {
            return 0m;
        }

        return Math.Min(coupon.Amount, order.Subtotal);
    }
}
