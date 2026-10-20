namespace Invoicing;

/// <summary>
/// The stored invoices, seen as a collection that answers billing questions. Implementations behave like a
/// database: every write is stored immediately, and every invoice they return is a detached copy, so changing
/// it has no effect until it is passed to <see cref="UpdateAsync"/>.
/// </summary>
public interface IInvoiceRepository
{
    /// <summary>Finds the invoice with this id, or returns <see langword="null"/>.</summary>
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Finds the invoice with exactly this number, or returns <see langword="null"/>.</summary>
    Task<Invoice?> GetByNumberAsync(string number, CancellationToken cancellationToken = default);

    /// <summary>Lists the unpaid invoices whose due date is before <paramref name="asOf"/>, oldest due date first.</summary>
    Task<IReadOnlyList<Invoice>> ListOverdueAsync(DateOnly asOf, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a new invoice. Later changes to the same object are not stored until it is passed to <see cref="UpdateAsync"/>.
    /// </summary>
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);

    /// <summary>Stores the current state of an invoice that was added before.</summary>
    Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default);
}
