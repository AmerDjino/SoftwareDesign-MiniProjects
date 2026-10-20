namespace Checkout;

/// <summary>The shop's money rounding policy, in one place.</summary>
public static class Money
{
    /// <summary>Rounds to 2 decimals, midpoints away from zero (0.125 becomes 0.13).</summary>
    public static decimal Round(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}
