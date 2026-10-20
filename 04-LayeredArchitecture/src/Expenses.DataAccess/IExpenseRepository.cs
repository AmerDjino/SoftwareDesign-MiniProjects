namespace Expenses.DataAccess;

public interface IExpenseRepository
{
    Task AddAsync(ExpenseRecord record, CancellationToken cancellationToken = default);

    /// <summary>Returns the records whose <see cref="ExpenseRecord.IncurredOn"/> falls in the given year and month.</summary>
    Task<IReadOnlyList<ExpenseRecord>> ListByMonthAsync(int year, int month, CancellationToken cancellationToken = default);
}
