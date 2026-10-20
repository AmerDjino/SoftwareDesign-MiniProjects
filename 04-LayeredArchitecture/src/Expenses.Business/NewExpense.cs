namespace Expenses.Business;

public sealed record NewExpense(string Description, decimal Amount, ExpenseCategory Category, DateOnly IncurredOn);
