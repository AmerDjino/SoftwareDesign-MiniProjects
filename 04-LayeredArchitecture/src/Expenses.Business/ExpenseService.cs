using Expenses.DataAccess;

namespace Expenses.Business;

public sealed class ExpenseService(IExpenseRepository repository, TimeProvider timeProvider)
{
    public async Task<SubmitExpenseResult> SubmitAsync(NewExpense expense, CancellationToken cancellationToken = default)
    {
        var errors = ExpenseValidator.Validate(expense, Today());
        if (errors.Count > 0)
        {
            return SubmitExpenseResult.Invalid(errors);
        }

        var record = expense.ToRecord(Guid.NewGuid());
        await repository.AddAsync(record, cancellationToken);
        return SubmitExpenseResult.Success(record.ToExpense());
    }

    public async Task<MonthlySummary> GetMonthlySummaryAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var records = await repository.ListByMonthAsync(year, month, cancellationToken);

        var categories = records
            .Select(record => record.ToExpense())
            .GroupBy(expense => expense.Category)
            .Select(group => new CategoryTotal(group.Key, group.Sum(expense => expense.Amount), group.Count()))
            .OrderByDescending(total => total.Total)
            .ThenBy(total => total.Category)
            .ToList();

        return new MonthlySummary(year, month, categories, categories.Sum(total => total.Total));
    }

    private DateOnly Today() => DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
}
