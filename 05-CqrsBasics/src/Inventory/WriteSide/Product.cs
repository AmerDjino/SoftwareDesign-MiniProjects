using Inventory.Abstractions;

namespace Inventory.WriteSide;

/// <summary>
/// The write model: a product and its stock. State changes only through methods that enforce the rules.
/// </summary>
public sealed class Product
{
    public Product(string sku, string name, int reorderLevel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegative(reorderLevel);
        Sku = sku;
        Name = name;
        ReorderLevel = reorderLevel;
    }

    /// <summary>
    /// Recreates a stored product as it was saved, without replaying the changes that led to its quantity on hand.
    /// </summary>
    internal static Product Restore(string sku, string name, int reorderLevel, int quantityOnHand)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(quantityOnHand);
        return new Product(sku, name, reorderLevel) { QuantityOnHand = quantityOnHand };
    }

    public string Sku { get; }

    public string Name { get; }

    public int ReorderLevel { get; }

    public int QuantityOnHand { get; private set; }

    /// <summary>
    /// Adds stock.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="quantity"/> is less than 1.</exception>
    public void Receive(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        QuantityOnHand += quantity;
    }

    /// <summary>
    /// Removes stock, or returns a failure and changes nothing when fewer than <paramref name="quantity"/> are on hand.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="quantity"/> is less than 1.</exception>
    public CommandResult Ship(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        if (quantity > QuantityOnHand)
        {
            return CommandResult.Failure(ProductErrors.InsufficientStock(Sku, quantity, QuantityOnHand));
        }

        QuantityOnHand -= quantity;
        return CommandResult.Success();
    }
}
