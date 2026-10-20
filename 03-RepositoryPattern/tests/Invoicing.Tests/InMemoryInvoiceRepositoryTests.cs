using Invoicing.Infrastructure;

namespace Invoicing.Tests;

public sealed class InMemoryInvoiceRepositoryTests : InvoiceRepositoryContract
{
    private readonly InMemoryInvoiceStore _store = new();

    protected override IInvoiceRepository CreateRepository() => new InMemoryInvoiceRepository(_store);
}
