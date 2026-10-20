namespace Inventory.ReadSide;

/// <summary>
/// The read model: one row per product, shaped for the stock lookup and reorder screens.
/// </summary>
public sealed record StockLevelView(string Sku, string Name, int QuantityOnHand, int ReorderLevel, bool IsLowStock);
