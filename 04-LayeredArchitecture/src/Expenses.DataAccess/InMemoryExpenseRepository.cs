namespace Expenses.DataAccess;

public sealed class InMemoryExpenseRepository : IExpenseRepository
{
    private readonly List<ExpenseRecord> _records = [];
    private readonly Lock _gate = new();

    public Task AddAsync(ExpenseRecord record, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _records.Add(record);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ExpenseRecord>> ListByMonthAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            IReadOnlyList<ExpenseRecord> records =
                [.. _records.Where(record => record.IncurredOn.Year == year && record.IncurredOn.Month == month)];
            return Task.FromResult(records);
        }
    }
}
