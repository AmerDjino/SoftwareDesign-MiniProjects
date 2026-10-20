using Inventory.Abstractions;
using Inventory.Dispatching;
using Inventory.ReadSide;
using Inventory.WriteSide;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the inventory module: the stores as singletons because they hold the data,
    /// and the stateless handlers, projection and dispatcher as transient.
    /// </summary>
    public static IServiceCollection AddInventory(this IServiceCollection services)
    {
        services.AddSingleton<IProductRepository, InMemoryProductRepository>();

        // One store instance behind both interfaces, so the rows the projection writes are the rows the queries read.
        services.AddSingleton<InMemoryStockLevelStore>();
        services.AddSingleton<IStockLevelReader>(provider => provider.GetRequiredService<InMemoryStockLevelStore>());
        services.AddSingleton<IStockLevelWriter>(provider => provider.GetRequiredService<InMemoryStockLevelStore>());

        services.AddTransient<StockLevelProjection>();

        services.AddTransient<ICommandHandler<RegisterProduct>, RegisterProductHandler>();
        services.AddTransient<ICommandHandler<ReceiveStock>, ReceiveStockHandler>();
        services.AddTransient<ICommandHandler<ShipStock>, ShipStockHandler>();
        services.AddTransient<IQueryHandler<GetStockLevel, StockLevelView?>, GetStockLevelHandler>();
        services.AddTransient<IQueryHandler<GetLowStockProducts, IReadOnlyList<StockLevelView>>, GetLowStockProductsHandler>();

        services.AddTransient<IDispatcher, Dispatcher>();
        return services;
    }
}
