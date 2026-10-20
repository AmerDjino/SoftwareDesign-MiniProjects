namespace Checkout.Tests;

/// <summary>
/// Stub rule that grants the same amount for every order.
/// A test must pick an amount that stays within the discount contract for its order.
/// </summary>
internal sealed class FixedDiscountStub(string name, decimal amount) : IDiscountRule
{
    public string Name => name;

    public decimal CalculateDiscount(Order order) => amount;
}
