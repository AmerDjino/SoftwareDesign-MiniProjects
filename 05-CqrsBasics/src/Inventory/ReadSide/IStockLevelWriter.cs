namespace Inventory.ReadSide;

/// <summary>
/// Write access to the read model. Only the projection uses it; query handlers never do.
/// </summary>
public interface IStockLevelWriter
{
    /// <summary>
    /// Adds the row, or replaces the row with the same SKU.
    /// </summary>
    Task UpsertAsync(StockLevelView view, CancellationToken cancellationToken);
}
