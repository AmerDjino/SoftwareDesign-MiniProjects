using Microsoft.EntityFrameworkCore;

namespace Invoicing.Infrastructure;

public sealed class EfInvoiceRepository(InvoicingDbContext db) : IInvoiceRepository
{
    // Every read starts here: untracked invoices are detached copies, the same as the in-memory implementation returns.
    private IQueryable<Invoice> DetachedInvoices => db.Invoices.AsNoTracking();

    public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        DetachedInvoices.FirstOrDefaultAsync(invoice => invoice.Id == id, cancellationToken);

    public Task<Invoice?> GetByNumberAsync(string number, CancellationToken cancellationToken = default) =>
        DetachedInvoices.FirstOrDefaultAsync(invoice => invoice.Number == number, cancellationToken);

    public async Task<IReadOnlyList<Invoice>> ListOverdueAsync(DateOnly asOf, CancellationToken cancellationToken = default) =>
        await DetachedInvoices
            .Where(invoice => invoice.PaidOn == null && invoice.DueDate < asOf)
            .OrderBy(invoice => invoice.DueDate)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        db.Invoices.Update(invoice);
        await db.SaveChangesAsync(cancellationToken);
    }
}
