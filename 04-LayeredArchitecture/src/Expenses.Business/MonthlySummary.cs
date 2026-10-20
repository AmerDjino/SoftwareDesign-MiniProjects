namespace Expenses.Business;

public sealed record MonthlySummary(int Year, int Month, IReadOnlyList<CategoryTotal> Categories, decimal GrandTotal);

public sealed record CategoryTotal(ExpenseCategory Category, decimal Total, int Count);
