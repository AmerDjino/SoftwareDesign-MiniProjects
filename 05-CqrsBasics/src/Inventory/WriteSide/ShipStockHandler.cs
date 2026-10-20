using Inventory.Abstractions;

namespace Inventory.WriteSide;

public sealed class ShipStockHandler : ICommandHandler<ShipStock>
{
    private readonly IProductRepository _products;
    private readonly StockLevelProjection _projection;

    public ShipStockHandler(IProductRepository products, StockLevelProjection projection)
    {
        _products = products;
        _projection = projection;
    }

    public async Task<CommandResult> HandleAsync(ShipStock command, CancellationToken cancellationToken)
    {
        var product = await _products.FindAsync(command.Sku, cancellationToken);
        if (product is null)
        {
            return CommandResult.Failure(ProductErrors.NotRegistered(command.Sku));
        }

        var result = product.Ship(command.Quantity);
        if (!result.IsSuccess)
        {
            return result;
        }

        await _products.SaveAsync(product, cancellationToken);
        await _projection.ProjectAsync(product, cancellationToken);
        return result;
    }
}
