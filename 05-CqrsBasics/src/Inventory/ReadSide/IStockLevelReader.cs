namespace Inventory.ReadSide;

/// <summary>
/// Read access to the read model, for query handlers. SKUs are compared ordinal, ignoring case.
/// </summary>
public interface IStockLevelReader
{
    /// <summary>
    /// Returns the row for this SKU, or <see langword="null"/> if there is none.
    /// </summary>
    Task<StockLevelView?> FindAsync(string sku, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the rows with <see cref="StockLevelView.IsLowStock"/> set, lowest quantity on hand first, then by SKU.
    /// </summary>
    Task<IReadOnlyList<StockLevelView>> ListLowStockAsync(CancellationToken cancellationToken);
}
