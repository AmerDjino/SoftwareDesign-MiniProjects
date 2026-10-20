using Inventory.Abstractions;

namespace Inventory.WriteSide;

public sealed class ReceiveStockHandler : ICommandHandler<ReceiveStock>
{
    private readonly IProductRepository _products;
    private readonly StockLevelProjection _projection;

    public ReceiveStockHandler(IProductRepository products, StockLevelProjection projection)
    {
        _products = products;
        _projection = projection;
    }

    public async Task<CommandResult> HandleAsync(ReceiveStock command, CancellationToken cancellationToken)
    {
        var product = await _products.FindAsync(command.Sku, cancellationToken);
        if (product is null)
        {
            return CommandResult.Failure(ProductErrors.NotRegistered(command.Sku));
        }

        product.Receive(command.Quantity);
        await _products.SaveAsync(product, cancellationToken);
        await _projection.ProjectAsync(product, cancellationToken);
        return CommandResult.Success();
    }
}
