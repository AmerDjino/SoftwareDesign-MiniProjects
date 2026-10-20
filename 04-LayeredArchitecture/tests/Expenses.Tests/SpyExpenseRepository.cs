using Expenses.DataAccess;

namespace Expenses.Tests;

internal sealed class SpyExpenseRepository : IExpenseRepository
{
    public List<ExpenseRecord> Added { get; } = [];

    public Task AddAsync(ExpenseRecord record, CancellationToken cancellationToken = default)
    {
        Added.Add(record);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ExpenseRecord>> ListByMonthAsync(int year, int month, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ExpenseRecord>>([]);
}
