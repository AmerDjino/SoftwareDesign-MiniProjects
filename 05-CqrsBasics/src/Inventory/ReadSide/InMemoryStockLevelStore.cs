using System.Collections.Concurrent;

namespace Inventory.ReadSide;

public sealed class InMemoryStockLevelStore : IStockLevelReader, IStockLevelWriter
{
    private readonly ConcurrentDictionary<string, StockLevelView> _views = new(StringComparer.OrdinalIgnoreCase);

    public Task<StockLevelView?> FindAsync(string sku, CancellationToken cancellationToken) =>
        Task.FromResult(_views.GetValueOrDefault(sku));

    public Task<IReadOnlyList<StockLevelView>> ListLowStockAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<StockLevelView> lowStock = _views.Values
            .Where(view => view.IsLowStock)
            .OrderBy(view => view.QuantityOnHand)
            .ThenBy(view => view.Sku, StringComparer.OrdinalIgnoreCase)
            .ToList();
        return Task.FromResult(lowStock);
    }

    public Task UpsertAsync(StockLevelView view, CancellationToken cancellationToken)
    {
        _views[view.Sku] = view;
        return Task.CompletedTask;
    }
}
