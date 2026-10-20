namespace Invoicing.Infrastructure;

public sealed class InMemoryInvoiceRepository(InMemoryInvoiceStore store) : IInvoiceRepository
{
    public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(store.Invoices.GetValueOrDefault(id)?.ToInvoice());

    public Task<Invoice?> GetByNumberAsync(string number, CancellationToken cancellationToken = default) =>
        Task.FromResult(store.Invoices.Values.FirstOrDefault(snapshot => snapshot.Number == number)?.ToInvoice());

    public Task<IReadOnlyList<Invoice>> ListOverdueAsync(DateOnly asOf, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Invoice> overdue = store.Invoices.Values
            .Select(snapshot => snapshot.ToInvoice())
            .Where(invoice => invoice.IsOverdueOn(asOf))
            .OrderBy(invoice => invoice.DueDate)
            .ToList();
        return Task.FromResult(overdue);
    }

    public Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        store.Invoices.Add(invoice.Id, InvoiceSnapshot.Of(invoice));
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        store.Invoices[invoice.Id] = InvoiceSnapshot.Of(invoice);
        return Task.CompletedTask;
    }
}
