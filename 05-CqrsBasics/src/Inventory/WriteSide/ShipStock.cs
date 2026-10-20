using Inventory.Abstractions;

namespace Inventory.WriteSide;

public sealed record ShipStock(string Sku, int Quantity) : ICommand;
