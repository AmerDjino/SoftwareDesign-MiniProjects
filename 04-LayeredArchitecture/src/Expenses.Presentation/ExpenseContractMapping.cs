using System.Text.Json;
using Expenses.Business;

namespace Expenses.Presentation;

/// <summary>The HTTP ↔ Business boundary: translates JSON contracts to Business models and back.</summary>
internal static class ExpenseContractMapping
{
    public static NewExpense ToNewExpense(this CreateExpenseRequest request) =>
        new(request.Description ?? "", request.Amount, ParseCategory(request.Category), request.IncurredOn);

    public static ExpenseResponse ToResponse(this Expense expense) =>
        new(expense.Id, expense.Description, expense.Amount, expense.Category.ToString(), expense.IncurredOn);

    public static MonthlySummaryResponse ToResponse(this MonthlySummary summary) =>
        new(
            summary.Year,
            summary.Month,
            [.. summary.Categories.Select(total => new CategoryTotalResponse(total.Category.ToString(), total.Total, total.Count))],
            summary.GrandTotal);

    /// <summary>Renames Business error keys (<see cref="NewExpense"/> property names) to the JSON names the client sent.</summary>
    public static Dictionary<string, string[]> ToHttpErrors(this IReadOnlyDictionary<string, string[]> errors) =>
        errors.ToDictionary(error => JsonNamingPolicy.CamelCase.ConvertName(error.Key), error => error.Value);

    /// <summary>
    /// Accepts category names only, case-insensitively. Anything else maps to 0, which is not a category,
    /// so the Business layer reports it. Enum.TryParse would also accept "3" or "Travel, Meals".
    /// </summary>
    private static ExpenseCategory ParseCategory(string? text) =>
        Enum.GetValues<ExpenseCategory>()
            .FirstOrDefault(category => string.Equals(category.ToString(), text, StringComparison.OrdinalIgnoreCase));
}
