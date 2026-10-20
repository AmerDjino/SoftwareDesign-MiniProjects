using Inventory.Abstractions;

namespace Inventory.ReadSide;

public sealed class GetStockLevelHandler : IQueryHandler<GetStockLevel, StockLevelView?>
{
    private readonly IStockLevelReader _reader;

    public GetStockLevelHandler(IStockLevelReader reader)
    {
        _reader = reader;
    }

    public Task<StockLevelView?> HandleAsync(GetStockLevel query, CancellationToken cancellationToken) =>
        _reader.FindAsync(query.Sku, cancellationToken);
}
