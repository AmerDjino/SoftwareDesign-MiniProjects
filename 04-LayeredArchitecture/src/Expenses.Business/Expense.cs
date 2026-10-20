namespace Expenses.Business;

public sealed record Expense(Guid Id, string Description, decimal Amount, ExpenseCategory Category, DateOnly IncurredOn);
