using Inventory.Abstractions;
using Inventory.ReadSide;
using Inventory.WriteSide;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Tests;

public sealed class StockScenarioTests
{
    [Fact]
    public async Task ReceiveStock_ThenGetStockLevel_ReturnsUpdatedQuantity()
    {
        using var services = CreateServices();
        var dispatcher = services.GetRequiredService<IDispatcher>();
        await dispatcher.SendAsync(new RegisterProduct("BOLT-M6", "Hex bolt M6", ReorderLevel: 10));
        await dispatcher.SendAsync(new ReceiveStock("BOLT-M6", Quantity: 25));

        var result = await dispatcher.SendAsync(new ReceiveStock("BOLT-M6", Quantity: 15));
        var stockLevel = await dispatcher.QueryAsync(new GetStockLevel("BOLT-M6"));

        Assert.Equal(CommandResult.Success(), result);
        Assert.Equal(
            new StockLevelView("BOLT-M6", "Hex bolt M6", QuantityOnHand: 40, ReorderLevel: 10, IsLowStock: false),
            stockLevel);
    }

    [Fact]
    public async Task ShipStock_MoreThanOnHand_FailsAndLeavesStockUnchanged()
    {
        using var services = CreateServices();
        var dispatcher = services.GetRequiredService<IDispatcher>();
        var products = services.GetRequiredService<IProductRepository>();
        await dispatcher.SendAsync(new RegisterProduct("BOLT-M6", "Hex bolt M6", ReorderLevel: 10));
        await dispatcher.SendAsync(new ReceiveStock("BOLT-M6", Quantity: 5));

        var result = await dispatcher.SendAsync(new ShipStock("BOLT-M6", Quantity: 8));
        var product = await products.FindAsync("BOLT-M6", CancellationToken.None);
        var stockLevel = await dispatcher.QueryAsync(new GetStockLevel("BOLT-M6"));

        Assert.Equal(CommandResult.Failure("Cannot ship 8 of 'BOLT-M6': only 5 on hand."), result);
        Assert.Equal(5, product?.QuantityOnHand);
        Assert.Equal(5, stockLevel?.QuantityOnHand);
    }

    [Fact]
    public async Task GetLowStockProducts_ReturnsProductsAtOrBelowReorderLevel_LowestQuantityFirst()
    {
        using var services = CreateServices();
        var dispatcher = services.GetRequiredService<IDispatcher>();
        await dispatcher.SendAsync(new RegisterProduct("BOLT-M6", "Hex bolt M6", ReorderLevel: 10));
        await dispatcher.SendAsync(new ReceiveStock("BOLT-M6", Quantity: 11));
        await dispatcher.SendAsync(new RegisterProduct("NUT-M6", "Hex nut M6", ReorderLevel: 10));
        await dispatcher.SendAsync(new ReceiveStock("NUT-M6", Quantity: 10));
        await dispatcher.SendAsync(new RegisterProduct("WASHER-M6", "Flat washer M6", ReorderLevel: 20));
        await dispatcher.SendAsync(new ReceiveStock("WASHER-M6", Quantity: 30));
        await dispatcher.SendAsync(new ShipStock("WASHER-M6", Quantity: 27));
        await dispatcher.SendAsync(new RegisterProduct("SCREW-M4", "Wood screw M4", ReorderLevel: 5));

        var lowStock = await dispatcher.QueryAsync(new GetLowStockProducts());

        StockLevelView[] expected =
        [
            new("SCREW-M4", "Wood screw M4", QuantityOnHand: 0, ReorderLevel: 5, IsLowStock: true),
            new("WASHER-M6", "Flat washer M6", QuantityOnHand: 3, ReorderLevel: 20, IsLowStock: true),
            new("NUT-M6", "Hex nut M6", QuantityOnHand: 10, ReorderLevel: 10, IsLowStock: true),
        ];
        Assert.Equal(expected, lowStock);
    }

    [Theory]
    [InlineData("BOLT-M6")]
    [InlineData("bolt-m6")]
    public async Task RegisterProduct_DuplicateSku_Fails(string duplicateSku)
    {
        using var services = CreateServices();
        var dispatcher = services.GetRequiredService<IDispatcher>();
        await dispatcher.SendAsync(new RegisterProduct("BOLT-M6", "Hex bolt M6", ReorderLevel: 10));

        var result = await dispatcher.SendAsync(new RegisterProduct(duplicateSku, "Carriage bolt M6", ReorderLevel: 50));
        var stockLevel = await dispatcher.QueryAsync(new GetStockLevel("BOLT-M6"));

        Assert.Equal(CommandResult.Failure("Product 'BOLT-M6' is already registered."), result);
        Assert.Equal(
            new StockLevelView("BOLT-M6", "Hex bolt M6", QuantityOnHand: 0, ReorderLevel: 10, IsLowStock: true),
            stockLevel);
    }

    private static ServiceProvider CreateServices() =>
        new ServiceCollection()
            .AddInventory()
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
}
