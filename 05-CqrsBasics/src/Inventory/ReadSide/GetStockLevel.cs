using Inventory.Abstractions;

namespace Inventory.ReadSide;

public sealed record GetStockLevel(string Sku) : IQuery<StockLevelView?>;
