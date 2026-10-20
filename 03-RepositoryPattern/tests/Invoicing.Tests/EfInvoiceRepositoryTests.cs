using Invoicing.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Invoicing.Tests;

public sealed class EfInvoiceRepositoryTests : InvoiceRepositoryContract, IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly List<InvoicingDbContext> _contexts = [];
    private readonly DbContextOptions<InvoicingDbContext> _options;

    public EfInvoiceRepositoryTests()
    {
        _connection.Open();
        _options = new DbContextOptionsBuilder<InvoicingDbContext>().UseSqlite(_connection).Options;
        CreateContext().Database.EnsureCreated();
    }

    public void Dispose()
    {
        foreach (var context in _contexts)
        {
            context.Dispose();
        }

        _connection.Dispose();
    }

    protected override IInvoiceRepository CreateRepository() => new EfInvoiceRepository(CreateContext());

    private InvoicingDbContext CreateContext()
    {
        var context = new InvoicingDbContext(_options);
        _contexts.Add(context);
        return context;
    }
}
