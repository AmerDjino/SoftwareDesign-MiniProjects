namespace Checkout;

public sealed class InMemoryCouponCatalog(IEnumerable<Coupon> coupons) : ICouponLookup
{
    private readonly Dictionary<string, Coupon> _couponsByCode =
        coupons.ToDictionary(coupon => coupon.Code, StringComparer.OrdinalIgnoreCase);

    public Coupon? Find(string code) => _couponsByCode.GetValueOrDefault(code);
}
