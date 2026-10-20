namespace Invoicing;

public sealed class InvoicePaymentService(IInvoiceRepository invoices)
{
    public async Task<PaymentResult> MarkPaidAsync(string invoiceNumber, DateOnly paidOn, CancellationToken cancellationToken = default)
    {
        var invoice = await invoices.GetByNumberAsync(invoiceNumber, cancellationToken);
        if (invoice is null)
        {
            return PaymentResult.NotFound;
        }

        if (invoice.IsPaid)
        {
            return PaymentResult.AlreadyPaid;
        }

        invoice.MarkPaid(paidOn);
        await invoices.UpdateAsync(invoice, cancellationToken);
        return PaymentResult.Paid;
    }
}
