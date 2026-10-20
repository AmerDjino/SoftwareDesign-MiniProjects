using Inventory.Abstractions;

namespace Inventory.ReadSide;

public sealed record GetLowStockProducts : IQuery<IReadOnlyList<StockLevelView>>;
