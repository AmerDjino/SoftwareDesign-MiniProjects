using System.Collections.Concurrent;

namespace Inventory.WriteSide;

public sealed class InMemoryProductRepository : IProductRepository
{
    // Snapshots, not Product objects: a caller's changes reach the store only through SaveAsync, as with a database.
    private readonly ConcurrentDictionary<string, ProductSnapshot> _products = new(StringComparer.OrdinalIgnoreCase);

    public Task<Product?> FindAsync(string sku, CancellationToken cancellationToken) =>
        Task.FromResult(_products.GetValueOrDefault(sku)?.ToProduct());

    public Task SaveAsync(Product product, CancellationToken cancellationToken)
    {
        _products[product.Sku] = ProductSnapshot.Of(product);
        return Task.CompletedTask;
    }

    private sealed record ProductSnapshot(string Sku, string Name, int ReorderLevel, int QuantityOnHand)
    {
        public static ProductSnapshot Of(Product product) =>
            new(product.Sku, product.Name, product.ReorderLevel, product.QuantityOnHand);

        public Product ToProduct() => Product.Restore(Sku, Name, ReorderLevel, QuantityOnHand);
    }
}
