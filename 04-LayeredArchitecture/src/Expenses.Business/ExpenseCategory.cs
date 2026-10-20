namespace Expenses.Business;

/// <summary>
/// Values start at 1, so an unset value (0) is never a valid category.
/// </summary>
public enum ExpenseCategory
{
    Travel = 1,
    Meals,
    Office,
    Training,
    Other,
}
