using Inventory.Abstractions;

namespace Inventory.WriteSide;

public sealed record ReceiveStock(string Sku, int Quantity) : ICommand;
