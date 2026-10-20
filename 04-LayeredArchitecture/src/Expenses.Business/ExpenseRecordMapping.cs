using Expenses.DataAccess;

namespace Expenses.Business;

/// <summary>
/// The Business ↔ Data Access boundary: the only code that knows how an expense is stored
/// (for example, that the category is saved as its name).
/// </summary>
internal static class ExpenseRecordMapping
{
    public static ExpenseRecord ToRecord(this NewExpense expense, Guid id) =>
        new(id, expense.Description, expense.Amount, expense.Category.ToString(), expense.IncurredOn);

    public static Expense ToExpense(this ExpenseRecord record) =>
        new(record.Id, record.Description, record.Amount, Enum.Parse<ExpenseCategory>(record.Category), record.IncurredOn);
}
