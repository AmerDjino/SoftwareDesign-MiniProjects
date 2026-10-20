namespace Invoicing.Infrastructure;

/// <summary>
/// The stored form of an invoice in <see cref="InMemoryInvoiceStore"/>. It is immutable and is not an
/// <see cref="Invoice"/>, so nothing a caller does to an invoice object can change what is stored.
/// </summary>
internal sealed record InvoiceSnapshot(Guid Id, string Number, string CustomerName, decimal Amount, DateOnly DueDate, DateOnly? PaidOn)
{
    public static InvoiceSnapshot Of(Invoice invoice) =>
        new(invoice.Id, invoice.Number, invoice.CustomerName, invoice.Amount, invoice.DueDate, invoice.PaidOn);

    public Invoice ToInvoice()
    {
        var invoice = new Invoice(Id, Number, CustomerName, Amount, DueDate);
        if (PaidOn is { } paidOn)
        {
            invoice.MarkPaid(paidOn);
        }

        return invoice;
    }
}
