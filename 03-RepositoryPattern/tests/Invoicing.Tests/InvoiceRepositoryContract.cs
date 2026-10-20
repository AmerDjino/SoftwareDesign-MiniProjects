namespace Invoicing.Tests;

/// <summary>
/// The behaviour every <see cref="IInvoiceRepository"/> implementation must share. Each step of a test
/// gets a fresh repository from <see cref="CreateRepository"/>; all of them work on the same underlying
/// store, so the tests prove what was stored rather than which objects happen to be in memory.
/// </summary>
public abstract class InvoiceRepositoryContract
{
    private static readonly DateOnly Today = new(2026, 10, 7);

    protected abstract IInvoiceRepository CreateRepository();

    [Fact]
    public async Task AddAsync_ThenLoadFromFreshRepository_ReturnsSameInvoiceByIdAndByNumber()
    {
        var invoice = new Invoice(Guid.NewGuid(), "INV-2026-0001", "Acme d.o.o.", 1250.50m, new DateOnly(2026, 10, 31));

        await CreateRepository().AddAsync(invoice);
        var byId = await CreateRepository().GetByIdAsync(invoice.Id);
        var byNumber = await CreateRepository().GetByNumberAsync(invoice.Number);

        AssertSameInvoice(invoice, byId);
        AssertSameInvoice(invoice, byNumber);
    }

    [Fact]
    public async Task ListOverdueAsync_ReturnsOnlyUnpaidInvoicesPastDueDate_OrderedByDueDate()
    {
        var overdueOneDay = NewInvoice("INV-001", Today.AddDays(-1));
        var dueToday = NewInvoice("INV-002", Today);
        var notYetDue = NewInvoice("INV-003", Today.AddDays(14));
        var paidLate = NewInvoice("INV-004", Today.AddDays(-30));
        paidLate.MarkPaid(Today.AddDays(-10));
        var overdueLongest = NewInvoice("INV-005", Today.AddDays(-45));
        var repository = CreateRepository();
        foreach (var invoice in new[] { overdueOneDay, dueToday, notYetDue, paidLate, overdueLongest })
        {
            await repository.AddAsync(invoice);
        }

        var overdue = await CreateRepository().ListOverdueAsync(Today);

        Assert.Equal(new[] { overdueLongest.Number, overdueOneDay.Number }, overdue.Select(invoice => invoice.Number));
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges_AndUnsavedChangesAreNotVisible()
    {
        var added = NewInvoice("INV-001", Today.AddDays(10));
        await CreateRepository().AddAsync(added);
        var loaded = await CreateRepository().GetByIdAsync(added.Id);
        Assert.NotNull(loaded);

        added.MarkPaid(Today);
        loaded.MarkPaid(Today);
        var beforeUpdate = await CreateRepository().GetByIdAsync(added.Id);
        await CreateRepository().UpdateAsync(loaded);
        var afterUpdate = await CreateRepository().GetByIdAsync(added.Id);

        Assert.NotNull(beforeUpdate);
        Assert.False(beforeUpdate.IsPaid);
        Assert.NotNull(afterUpdate);
        Assert.Equal(Today, afterUpdate.PaidOn);
    }

    private static Invoice NewInvoice(string number, DateOnly dueDate) =>
        new(Guid.NewGuid(), number, "Acme d.o.o.", 100m, dueDate);

    private static void AssertSameInvoice(Invoice expected, Invoice? actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Number, actual.Number);
        Assert.Equal(expected.CustomerName, actual.CustomerName);
        Assert.Equal(expected.Amount, actual.Amount);
        Assert.Equal(expected.DueDate, actual.DueDate);
        Assert.Equal(expected.PaidOn, actual.PaidOn);
    }
}
