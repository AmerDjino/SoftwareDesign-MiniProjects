using Invoicing.Infrastructure;

namespace Invoicing.Tests;

public sealed class InvoicePaymentServiceTests
{
    private static readonly DateOnly DueDate = new(2026, 9, 30);
    private static readonly DateOnly EarlierPaymentDate = new(2026, 10, 1);
    private static readonly DateOnly PaymentDate = new(2026, 10, 7);

    private readonly InMemoryInvoiceRepository _invoices = new(new InMemoryInvoiceStore());

    [Fact]
    public async Task MarkPaidAsync_UnpaidInvoice_MarksItPaidAndPersistsIt()
    {
        await _invoices.AddAsync(NewInvoice("INV-001"));
        var service = new InvoicePaymentService(_invoices);

        var result = await service.MarkPaidAsync("INV-001", PaymentDate);

        var stored = await _invoices.GetByNumberAsync("INV-001");
        Assert.Equal(PaymentResult.Paid, result);
        Assert.NotNull(stored);
        Assert.Equal(PaymentDate, stored.PaidOn);
    }

    [Theory]
    [InlineData("INV-404", PaymentResult.NotFound)]
    [InlineData("INV-002", PaymentResult.AlreadyPaid)]
    public async Task MarkPaidAsync_UnknownOrAlreadyPaidInvoice_ReturnsFailureAndChangesNothing(string invoiceNumber, PaymentResult expected)
    {
        var alreadyPaid = NewInvoice("INV-002");
        alreadyPaid.MarkPaid(EarlierPaymentDate);
        await _invoices.AddAsync(alreadyPaid);
        var service = new InvoicePaymentService(_invoices);

        var result = await service.MarkPaidAsync(invoiceNumber, PaymentDate);

        var stored = await _invoices.GetByNumberAsync("INV-002");
        Assert.Equal(expected, result);
        Assert.NotNull(stored);
        Assert.Equal(EarlierPaymentDate, stored.PaidOn);
    }

    private static Invoice NewInvoice(string number) => new(Guid.NewGuid(), number, "Acme d.o.o.", 100m, DueDate);
}
