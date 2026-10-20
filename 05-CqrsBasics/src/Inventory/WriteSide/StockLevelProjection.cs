using Inventory.ReadSide;

namespace Inventory.WriteSide;

/// <summary>
/// Keeps the read model in step with the write model by turning a product into its stock-level row.
/// </summary>
public sealed class StockLevelProjection
{
    private readonly IStockLevelWriter _writer;

    public StockLevelProjection(IStockLevelWriter writer)
    {
        _writer = writer;
    }

    public Task ProjectAsync(Product product, CancellationToken cancellationToken)
    {
        var view = new StockLevelView(
            product.Sku,
            product.Name,
            product.QuantityOnHand,
            product.ReorderLevel,
            IsLowStock: product.QuantityOnHand <= product.ReorderLevel);
        return _writer.UpsertAsync(view, cancellationToken);
    }
}
