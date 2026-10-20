namespace Checkout;

/// <summary>Read-only access to coupons, which is all a pricing rule needs.</summary>
public interface ICouponLookup
{
    /// <summary>Returns the coupon with the given code, or <see langword="null"/> when there is none.</summary>
    Coupon? Find(string code);
}
