using Inventory.Abstractions;

namespace Inventory.WriteSide;

public sealed class RegisterProductHandler : ICommandHandler<RegisterProduct>
{
    private readonly IProductRepository _products;
    private readonly StockLevelProjection _projection;

    public RegisterProductHandler(IProductRepository products, StockLevelProjection projection)
    {
        _products = products;
        _projection = projection;
    }

    public async Task<CommandResult> HandleAsync(RegisterProduct command, CancellationToken cancellationToken)
    {
        var existing = await _products.FindAsync(command.Sku, cancellationToken);
        if (existing is not null)
        {
            return CommandResult.Failure(ProductErrors.AlreadyRegistered(existing.Sku));
        }

        var product = new Product(command.Sku, command.Name, command.ReorderLevel);
        await _products.SaveAsync(product, cancellationToken);
        await _projection.ProjectAsync(product, cancellationToken);
        return CommandResult.Success();
    }
}
