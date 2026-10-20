using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Expenses.Business;

public sealed class SubmitExpenseResult
{
    private SubmitExpenseResult(Expense? expense, IReadOnlyDictionary<string, string[]> errors)
    {
        Expense = expense;
        Errors = errors;
    }

    [MemberNotNullWhen(true, nameof(Expense))]
    public bool Succeeded => Expense is not null;

    public Expense? Expense { get; }

    /// <summary>Validation messages keyed by <see cref="NewExpense"/> property name; empty when the expense was stored.</summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    internal static SubmitExpenseResult Success(Expense expense) =>
        new(expense, ReadOnlyDictionary<string, string[]>.Empty);

    internal static SubmitExpenseResult Invalid(IReadOnlyDictionary<string, string[]> errors) =>
        new(null, errors);
}
