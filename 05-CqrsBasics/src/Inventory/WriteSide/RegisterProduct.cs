using Inventory.Abstractions;

namespace Inventory.WriteSide;

public sealed record RegisterProduct(string Sku, string Name, int ReorderLevel) : ICommand;
