namespace Invoicing.Infrastructure;

/// <summary>
/// Plays the database for <see cref="InMemoryInvoiceRepository"/>: repositories created over the same store see
/// the same invoices. Not thread-safe; meant for tests and prototypes.
/// </summary>
public sealed class InMemoryInvoiceStore
{
    internal Dictionary<Guid, InvoiceSnapshot> Invoices { get; } = [];
}
