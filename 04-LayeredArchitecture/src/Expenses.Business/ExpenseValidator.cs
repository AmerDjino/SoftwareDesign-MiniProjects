namespace Expenses.Business;

/// <summary>
/// Business rules BR-1 to BR-4. Every rule is checked, so all violations are reported at once.
/// Errors are keyed by <see cref="NewExpense"/> property name.
/// </summary>
internal static class ExpenseValidator
{
    private const int MaxDescriptionLength = 200;
    private const decimal MaxAmount = 5_000.00m;

    public static IReadOnlyDictionary<string, string[]> Validate(NewExpense expense, DateOnly today)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(expense.Description))
        {
            errors[nameof(NewExpense.Description)] = ["Description is required."];
        }
        else if (expense.Description.Length > MaxDescriptionLength)
        {
            errors[nameof(NewExpense.Description)] =
                [FormattableString.Invariant($"Description must be at most {MaxDescriptionLength} characters.")];
        }

        if (expense.Amount <= 0)
        {
            errors[nameof(NewExpense.Amount)] = ["Amount must be greater than 0."];
        }
        else if (expense.Amount > MaxAmount)
        {
            errors[nameof(NewExpense.Amount)] = [FormattableString.Invariant($"Amount must be at most {MaxAmount:0.00}.")];
        }

        if (!Enum.IsDefined(expense.Category))
        {
            errors[nameof(NewExpense.Category)] =
                [$"Category must be one of: {string.Join(", ", Enum.GetNames<ExpenseCategory>())}."];
        }

        if (expense.IncurredOn == default)
        {
            errors[nameof(NewExpense.IncurredOn)] = ["Date is required."];
        }
        else if (expense.IncurredOn > today)
        {
            errors[nameof(NewExpense.IncurredOn)] = ["Date must not be in the future."];
        }

        return errors;
    }
}
