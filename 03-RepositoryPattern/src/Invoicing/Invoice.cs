namespace Invoicing;

public sealed class Invoice(Guid id, string number, string customerName, decimal amount, DateOnly dueDate)
{
    public Guid Id { get; } = id;

    public string Number { get; } = number;

    public string CustomerName { get; } = customerName;

    public decimal Amount { get; } = amount;

    public DateOnly DueDate { get; } = dueDate;

    public DateOnly? PaidOn { get; private set; }

    public bool IsPaid => PaidOn is not null;

    public bool IsOverdueOn(DateOnly date) => !IsPaid && DueDate < date;

    public void MarkPaid(DateOnly paidOn)
    {
        if (IsPaid)
        {
            throw new InvalidOperationException($"Invoice {Number} is already paid.");
        }

        PaidOn = paidOn;
    }
}
