using Inventory.Abstractions;

namespace Inventory.ReadSide;

public sealed class GetLowStockProductsHandler : IQueryHandler<GetLowStockProducts, IReadOnlyList<StockLevelView>>
{
    private readonly IStockLevelReader _reader;

    public GetLowStockProductsHandler(IStockLevelReader reader)
    {
        _reader = reader;
    }

    public Task<IReadOnlyList<StockLevelView>> HandleAsync(GetLowStockProducts query, CancellationToken cancellationToken) =>
        _reader.ListLowStockAsync(cancellationToken);
}
