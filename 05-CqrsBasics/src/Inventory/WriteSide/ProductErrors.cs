using System.Globalization;

namespace Inventory.WriteSide;

/// <summary>
/// The write side's failure messages, kept together so they can be checked against the spec's message table in one place.
/// </summary>
internal static class ProductErrors
{
    public static string AlreadyRegistered(string sku) => $"Product '{sku}' is already registered.";

    public static string NotRegistered(string sku) => $"Product '{sku}' is not registered.";

    public static string InsufficientStock(string sku, int requested, int onHand) =>
        string.Create(CultureInfo.InvariantCulture, $"Cannot ship {requested} of '{sku}': only {onHand} on hand.");
}
