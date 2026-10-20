namespace Inventory.WriteSide;

/// <summary>
/// Stores the write model. SKUs are compared ordinal, ignoring case. Like a database, the store hands out
/// detached copies: a change to a product is stored only when the product is passed to <see cref="SaveAsync"/>.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Returns the product with this SKU, or <see langword="null"/> if it is not registered.
    /// </summary>
    Task<Product?> FindAsync(string sku, CancellationToken cancellationToken);

    /// <summary>
    /// Adds the product, or replaces the stored product with the same SKU.
    /// </summary>
    Task SaveAsync(Product product, CancellationToken cancellationToken);
}
