namespace Expenses.Presentation;

public sealed record ExpenseResponse(Guid Id, string Description, decimal Amount, string Category, DateOnly IncurredOn);
