namespace Expenses.Presentation;

public sealed record CreateExpenseRequest(string? Description, decimal Amount, string? Category, DateOnly IncurredOn);
