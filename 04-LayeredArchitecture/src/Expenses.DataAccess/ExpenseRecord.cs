namespace Expenses.DataAccess;

public sealed record ExpenseRecord(Guid Id, string Description, decimal Amount, string Category, DateOnly IncurredOn);
