namespace Checkout;

public sealed record Coupon(string Code, decimal Amount, decimal MinimumSubtotal);
